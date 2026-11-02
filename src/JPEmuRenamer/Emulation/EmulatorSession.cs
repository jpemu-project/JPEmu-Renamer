// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;

namespace JPEmuRenamer.Emulation;

/// <summary>内蔵コアで1本のゲームを遊ぶための情報</summary>
public record SessionRequest(
    string CoreName,
    string RomPath,
    string? InnerName,
    SystemConfig System,
    string GameKey,
    string Title,
    PlaySettings Settings,
    string BiosDir);

/// <summary>
/// 内蔵コアでゲームを動かす。専用スレッドでコアを毎フレーム実行し、映像・音声・入力・セーブを受け持つ。
/// 画面（PlayWindow）とのやりとりは、フレームの取得（TryGetFrame）とイベント・コマンドで行う。
/// </summary>
public sealed unsafe class EmulatorSession : IRetroHost, IDisposable
{
    private readonly SessionRequest req;
    private readonly ConcurrentQueue<Action> commands = new();
    private readonly InputManager input = InputManager.Instance;
    private Thread? thread;
    private volatile bool running;
    private LibretroCore? core;
    private AudioOutput? audio;
    private GlContext? gl;
    private RetroHwRenderCallback hwCallback;
    private bool useHw;
    private string? tempDir;

    private PlaySettings settings;
    private volatile bool optionsChanged;
    private uint pixelFormat = Retro.Pixel0Rgb1555;

    // 映像（エミュレーションスレッドで書き、UIスレッドで読む）
    private readonly object frameLock = new();
    private int[] backBuffer = Array.Empty<int>();
    private int[] frontBuffer = Array.Empty<int>();
    private int frontW, frontH;
    private long frameSerial;

    // 入力
    private readonly ushort[] padMask = new ushort[4];
    private readonly short[,] analog = new short[4, 4];
    private readonly Dictionary<string, bool> hotkeyPrev = new();
    private long frameCount;

    // セーブ
    private byte[]? lastSram;
    private readonly Stopwatch sramTimer = new();

    public string SaveDir { get; private set; } = "";
    public string StateDir { get; private set; } = "";
    public string ScreenshotDir { get; private set; } = "";
    public string RomBaseName { get; }

    public int Slot { get; private set; }
    public bool Paused { get; private set; }
    public bool MenuPaused { get; set; }
    public bool FastForwarding { get; private set; }
    public double Fps { get; private set; } = 60;
    public double AspectRatio { get; private set; } = 4.0 / 3.0;
    public int BaseWidth { get; private set; } = 256;
    public int BaseHeight { get; private set; } = 240;
    public uint Rotation { get; private set; }
    public string CoreDisplayName { get; private set; } = "";

    /// <summary>コアの読み込みが終わって動き始めた</summary>
    public event Action? Started;
    /// <summary>起動に失敗した（理由）</summary>
    public event Action<string>? Failed;
    /// <summary>画面に短く出すメッセージ</summary>
    public event Action<string>? Message;
    /// <summary>画面側で処理するホットキー（Menu / Fullscreen / Screenshot / Exit）</summary>
    public event Action<string>? Hotkey;
    /// <summary>映像の大きさや縦横比が変わった</summary>
    public event Action? GeometryChanged;
    /// <summary>エミュレーションスレッドが終わった</summary>
    public event Action? Stopped;

    public EmulatorSession(SessionRequest req)
    {
        this.req = req;
        settings = req.Settings.Clone();
        RomBaseName = System.IO.Path.GetFileNameWithoutExtension(req.RomPath);
        ResolveDirectories();
    }

    public PlaySettings Settings => settings;
    public SessionRequest Request => req;

    // ---------- 保存先 ----------

    private void ResolveDirectories()
    {
        string sysName = Sanitize(req.System.Name);
        string game = Sanitize(RomBaseName);
        string Apply(string? configured, string fallback)
        {
            var root = string.IsNullOrWhiteSpace(configured) ? fallback : configured!;
            return settings.FolderMode switch
            {
                "Shared" => root,
                "PerGame" => System.IO.Path.Combine(root, sysName, game),
                _ => System.IO.Path.Combine(root, sysName)
            };
        }
        SaveDir = settings.SaveNextToRom == true
            ? System.IO.Path.GetDirectoryName(req.RomPath) ?? Storage.DefaultSaveDir
            : Apply(settings.SaveDir, Storage.DefaultSaveDir);
        StateDir = Apply(settings.StateDir, Storage.DefaultStateDir);
        ScreenshotDir = Apply(settings.ScreenshotDir, Storage.DefaultScreenshotDir);
    }

    private static string Sanitize(string s) => Matcher.SafeFileName(s);

    private string SramPath => System.IO.Path.Combine(SaveDir, RomBaseName + ".srm");
    public string StatePath(int slot) => System.IO.Path.Combine(StateDir, RomBaseName + (slot == 0 ? ".state" : ".state" + slot));

    string IRetroHost.SystemDirectory => req.BiosDir;
    string IRetroHost.SaveDirectory => SaveDir;

    // ---------- 開始・終了 ----------

    public void Start()
    {
        CleanupStaleTemp();
        running = true;
        input.Acquire();
        thread = new Thread(ThreadMain) { IsBackground = true, Name = "Emulation", Priority = ThreadPriority.AboveNormal };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
    }

    public void Stop()
    {
        running = false;
        if (thread != null && thread != Thread.CurrentThread) thread.Join(5000);
    }

    public void Dispose() => Stop();

    [DllImport("winmm.dll")] private static extern uint timeBeginPeriod(uint ms);
    [DllImport("winmm.dll")] private static extern uint timeEndPeriod(uint ms);
    [DllImport("dwmapi.dll")] private static extern int DwmFlush();

    private void ThreadMain()
    {
        timeBeginPeriod(1);
        try
        {
            if (!Load(out var error))
            {
                Failed?.Invoke(error);
                return;
            }
            Started?.Invoke();
            RunLoop();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            Failed?.Invoke(ex.Message);
        }
        finally
        {
            Unload();
            timeEndPeriod(1);
            input.Release();
            Stopped?.Invoke();
        }
    }

    private bool Load(out string error)
    {
        error = "";
        var dll = CoreCatalog.DllPath(req.CoreName);
        if (!File.Exists(dll))
        {
            error = Loc.F("CoreMissing", req.CoreName);
            return false;
        }
        Directory.CreateDirectory(SaveDir);
        Directory.CreateDirectory(req.BiosDir);

        core = new LibretroCore(dll, this);
        core.Init();
        CoreDisplayName = string.IsNullOrEmpty(core.LibraryName) ? req.CoreName : core.LibraryName;

        // ZIPに対応していないコアには、中のROMを一時フォルダに取り出して渡す。
        // 多くのコアは日本語を含むパスを開けないため、一時ファイルは英数字だけの名前にする
        string loadPath = req.RomPath;
        bool isZip = System.IO.Path.GetExtension(req.RomPath).Equals(".zip", StringComparison.OrdinalIgnoreCase);
        if (isZip && !core.ValidExtensions.Contains("zip"))
        {
            // 古いZIPは中のファイル名がShift-JISのことがあるので、UTF-8の印が無い名前はShift-JISとして読む
            using var zip = ZipFile.Open(req.RomPath, ZipArchiveMode.Read, ShiftJis);
            var entry = zip.Entries.Where(e => !string.IsNullOrEmpty(e.Name))
                                   .OrderByDescending(e => core.ValidExtensions.Contains(System.IO.Path.GetExtension(e.Name).TrimStart('.')))
                                   .ThenByDescending(e => e.FullName == req.InnerName)
                                   .ThenByDescending(e => e.Length).FirstOrDefault();
            if (entry == null)
            {
                error = Loc.T("ZipEmpty");
                return false;
            }
            loadPath = TempContentPath(System.IO.Path.GetExtension(entry.Name));
            entry.ExtractToFile(loadPath, true);
        }
        else if (core.NeedFullpath && req.RomPath.Any(c => c > 0x7E))
        {
            loadPath = TempContentPath(System.IO.Path.GetExtension(req.RomPath));
            File.Copy(req.RomPath, loadPath, true);
        }

        byte[]? data = core.NeedFullpath ? null : File.ReadAllBytes(loadPath);
        if (!core.LoadGame(loadPath, data))
        {
            error = useHw && gl == null ? Loc.T("HwNotSupported") : Loc.F("LoadGameFailed", CoreDisplayName);
            return false;
        }

        for (uint p = 0; p < 4; p++) core.SetControllerPortDevice(p, Retro.DeviceJoypad);
        ApplyAvInfo(core.AvInfo);

        if (useHw)
        {
            if (gl == null)
            {
                error = Loc.T("HwNotSupported");
                return false;
            }
            gl.Resize((int)Math.Max(core.AvInfo.Geometry.MaxWidth, core.AvInfo.Geometry.BaseWidth),
                      (int)Math.Max(core.AvInfo.Geometry.MaxHeight, core.AvInfo.Geometry.BaseHeight));
            if (hwCallback.ContextReset != IntPtr.Zero)
                ((delegate* unmanaged[Cdecl]<void>)hwCallback.ContextReset)();
        }

        LoadSram();
        CreateAudio();
        ApplyRuntimeSettings();
        return true;
    }

    private static readonly System.Text.Encoding ShiftJis = CreateShiftJis();

    private static System.Text.Encoding CreateShiftJis()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        return System.Text.Encoding.GetEncoding(932);
    }

    /// <summary>前回までに消せなかった一時フォルダ（異常終了したときなど）を消す</summary>
    private static void CleanupStaleTemp()
    {
        try
        {
            var root = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "JPEmuRenamer");
            if (!Directory.Exists(root)) return;
            foreach (var dir in Directory.GetDirectories(root))
            {
                try
                {
                    if (Directory.GetLastWriteTimeUtc(dir) < DateTime.UtcNow.AddHours(-1)) Directory.Delete(dir, true);
                }
                catch { }
            }
        }
        catch { }
    }

    /// <summary>一時フォルダ内の、英数字だけのパス（拡張子は元のまま）</summary>
    private string TempContentPath(string ext)
    {
        tempDir ??= System.IO.Path.Combine(System.IO.Path.GetTempPath(), "JPEmuRenamer", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);
        var safeExt = new string(ext.Where(c => c < 0x7F && (char.IsLetterOrDigit(c) || c == '.')).ToArray());
        return System.IO.Path.Combine(tempDir, "content" + safeExt.ToLowerInvariant());
    }

    private void Unload()
    {
        try
        {
            if (core != null)
            {
                SaveSram(force: true);
                if (useHw && hwCallback.ContextDestroy != IntPtr.Zero)
                    ((delegate* unmanaged[Cdecl]<void>)hwCallback.ContextDestroy)();
            }
        }
        catch (Exception ex) { Debug.WriteLine(ex); }
        core?.Dispose();
        core = null;
        gl?.Dispose();
        gl = null;
        audio?.Dispose();
        audio = null;
        for (int i = 0; i < 4; i++) input.Rumble(i, 0, 0, 0);
        if (tempDir != null)
        {
            try { Directory.Delete(tempDir, true); } catch { }
        }
    }

    private void CreateAudio()
    {
        audio?.Dispose();
        audio = null;
        if (core == null) return;
        audio = new AudioOutput(core.AvInfo.Timing.SampleRate, settings.AudioDevice ?? "", settings.SampleRate ?? 48000,
            settings.Exclusive == true, settings.LatencyMs ?? 64);
    }

    private void ApplyAvInfo(RetroSystemAvInfo av)
    {
        Fps = av.Timing.Fps > 1 ? av.Timing.Fps : 60;
        audio?.SetInputRate(av.Timing.SampleRate);
        ApplyGeometry(av.Geometry);
        if (gl != null)
            gl.Resize((int)Math.Max(av.Geometry.MaxWidth, av.Geometry.BaseWidth), (int)Math.Max(av.Geometry.MaxHeight, av.Geometry.BaseHeight));
    }

    private void ApplyGeometry(RetroGameGeometry g)
    {
        if (g.BaseWidth > 0) BaseWidth = (int)g.BaseWidth;
        if (g.BaseHeight > 0) BaseHeight = (int)g.BaseHeight;
        AspectRatio = g.AspectRatio > 0 ? g.AspectRatio : (double)BaseWidth / Math.Max(1, BaseHeight);
        GeometryChanged?.Invoke();
    }

    // ---------- メインループ ----------

    private void RunLoop()
    {
        var sw = Stopwatch.StartNew();
        double next = sw.Elapsed.TotalSeconds;
        sramTimer.Restart();
        double refresh = DisplayInfo.CurrentRefreshRate();

        while (running)
        {
            while (commands.TryDequeue(out var cmd))
            {
                try { cmd(); }
                catch (Exception ex) { Debug.WriteLine(ex); }
            }

            HandleHotkeys();

            if (Paused || MenuPaused)
            {
                audio?.Clear();
                Thread.Sleep(10);
                next = sw.Elapsed.TotalSeconds;
                continue;
            }

            core!.Run();
            frameCount++;

            if (sramTimer.ElapsedMilliseconds > 10_000)
            {
                SaveSram(force: false);
                sramTimer.Restart();
            }

            if (FastForwarding)
            {
                next = sw.Elapsed.TotalSeconds;
                continue;
            }

            bool vsync = settings.VSync == true && Math.Abs(refresh - Fps) / Fps < 0.01;
            if (vsync)
            {
                // 画面の更新に合わせる（音のずれは音ずれ補正で吸収する）
                if (DwmFlush() != 0) WaitFrame(sw, ref next);
                else next = sw.Elapsed.TotalSeconds;
            }
            else if (audio is { IsPlaying: true } && settings.RateControl == true)
            {
                // 音の残量で速度を合わせる（残りが目標を超えている間は待つ）
                var limit = sw.Elapsed.TotalSeconds + 0.1;
                while (running && audio.BufferedMs > audio.TargetLatencyMs && sw.Elapsed.TotalSeconds < limit)
                    Thread.Sleep(1);
                next = sw.Elapsed.TotalSeconds;
            }
            else
            {
                WaitFrame(sw, ref next);
            }
        }
    }

    private void WaitFrame(Stopwatch sw, ref double next)
    {
        next += 1.0 / Fps;
        double now = sw.Elapsed.TotalSeconds;
        if (next < now - 0.1) next = now; // 大きく遅れたら追いつこうとしない
        while (true)
        {
            double remain = next - sw.Elapsed.TotalSeconds;
            if (remain <= 0) break;
            if (remain > 0.002) Thread.Sleep(1);
            else Thread.SpinWait(50);
        }
    }

    // ---------- 画面側からの操作（エミュレーションスレッドで実行） ----------

    private void Post(Action a) => commands.Enqueue(a);

    public void RequestSaveState() => Post(() => SaveState(Slot));
    public void RequestLoadState() => Post(() => LoadState(Slot));
    public void RequestReset() => Post(() => { core?.Reset(); Message?.Invoke(Loc.T("MsgReset")); });

    public void ChangeSlot(int delta) => Post(() =>
    {
        int slots = Math.Max(1, settings.StateSlots ?? 10);
        Slot = ((Slot + delta) % slots + slots) % slots;
        Message?.Invoke(Loc.F("MsgSlot", Slot));
    });

    public void SetPaused(bool paused) => Post(() => Paused = paused);

    /// <summary>設定を変えたときに呼ぶ（その場で反映）</summary>
    public void ApplySettings(PlaySettings newSettings)
    {
        var copy = newSettings.Clone();
        Post(() =>
        {
            var old = settings;
            settings = copy;
            bool optionsDiffer = !DictEquals(old.CoreOptions, copy.CoreOptions);
            if (optionsDiffer) optionsChanged = true;
            if (old.AudioDevice != copy.AudioDevice || old.SampleRate != copy.SampleRate ||
                old.Exclusive != copy.Exclusive || old.LatencyMs != copy.LatencyMs)
                CreateAudio();
            if (old.SaveDir != copy.SaveDir || old.StateDir != copy.StateDir || old.ScreenshotDir != copy.ScreenshotDir ||
                old.FolderMode != copy.FolderMode || old.SaveNextToRom != copy.SaveNextToRom)
            {
                SaveSram(force: true);
                ResolveDirectories();
            }
            ApplyRuntimeSettings();
        });
    }

    private void ApplyRuntimeSettings()
    {
        if (audio != null)
        {
            audio.Volume = settings.Mute == true ? 0 : Math.Clamp(settings.Volume ?? 80, 0, 100) / 100f;
            audio.RateControl = settings.RateControl == true;
        }
        input.DeadzoneRatio = Math.Clamp(settings.Deadzone ?? 25, 0, 95) / 100.0;
    }

    private static bool DictEquals(Dictionary<string, string>? a, Dictionary<string, string>? b)
    {
        a ??= new();
        b ??= new();
        return a.Count == b.Count && a.All(kv => b.TryGetValue(kv.Key, out var v) && v == kv.Value);
    }

    // ---------- ステートセーブ ----------

    private void SaveState(int slot)
    {
        if (core == null) return;
        var data = core.SaveState();
        if (data == null)
        {
            Message?.Invoke(Loc.T("MsgStateUnsupported"));
            return;
        }
        Directory.CreateDirectory(StateDir);
        File.WriteAllBytes(StatePath(slot), data);
        Message?.Invoke(Loc.F("MsgStateSaved", slot));
    }

    private void LoadState(int slot)
    {
        if (core == null) return;
        var path = StatePath(slot);
        if (!File.Exists(path))
        {
            Message?.Invoke(Loc.F("MsgStateNone", slot));
            return;
        }
        bool ok = core.LoadState(File.ReadAllBytes(path));
        audio?.Clear();
        Message?.Invoke(ok ? Loc.F("MsgStateLoaded", slot) : Loc.T("MsgStateFailed"));
    }

    // ---------- バッテリーバックアップ ----------

    private void LoadSram()
    {
        if (core == null || !File.Exists(SramPath)) return;
        var mem = core.Memory(Retro.MemorySaveRam);
        if (mem.IsEmpty) return;
        try
        {
            var data = File.ReadAllBytes(SramPath);
            data.AsSpan(0, Math.Min(data.Length, mem.Length)).CopyTo(mem);
            lastSram = mem.ToArray();
        }
        catch (Exception ex) { Debug.WriteLine(ex); }
    }

    private void SaveSram(bool force)
    {
        if (core == null) return;
        var mem = core.Memory(Retro.MemorySaveRam);
        if (mem.IsEmpty) return;
        if (!force && lastSram != null && mem.SequenceEqual(lastSram)) return;
        // まだ一度も書き込まれていない（全部0）ならファイルを作らない
        if (lastSram == null && !File.Exists(SramPath) && mem.IndexOfAnyExcept((byte)0) < 0) return;
        try
        {
            Directory.CreateDirectory(SaveDir);
            var data = mem.ToArray();
            var tmp = SramPath + ".tmp";
            File.WriteAllBytes(tmp, data);
            File.Move(tmp, SramPath, true);
            lastSram = data;
        }
        catch (Exception ex) { Debug.WriteLine(ex); }
    }

    // ---------- ホットキー ----------

    private void HandleHotkeys()
    {
        var hk = settings.Input?.Hotkeys;
        if (hk == null) return;
        foreach (var name in InputProfile.HotkeyNames)
        {
            bool now = hk.TryGetValue(name, out var list) && input.AnyActive(list, 0);
            hotkeyPrev.TryGetValue(name, out var before);
            hotkeyPrev[name] = now;

            if (name == "FastForward")
            {
                if (FastForwarding != now && !MenuPaused)
                {
                    FastForwarding = now;
                    if (!now) audio?.Clear();
                }
                continue;
            }
            if (!now || before) continue;
            if (MenuPaused && name != "Menu" && name != "Exit") continue;

            switch (name)
            {
                case "SaveState": SaveState(Slot); break;
                case "LoadState": LoadState(Slot); break;
                case "SlotPrev": ChangeSlotNow(-1); break;
                case "SlotNext": ChangeSlotNow(1); break;
                case "Reset": core?.Reset(); Message?.Invoke(Loc.T("MsgReset")); break;
                case "Pause":
                    Paused = !Paused;
                    Message?.Invoke(Paused ? Loc.T("MsgPaused") : Loc.T("MsgResumed"));
                    break;
                default: Hotkey?.Invoke(name); break;
            }
        }
    }

    private void ChangeSlotNow(int delta)
    {
        int slots = Math.Max(1, settings.StateSlots ?? 10);
        Slot = ((Slot + delta) % slots + slots) % slots;
        Message?.Invoke(Loc.F("MsgSlot", Slot));
    }

    // ---------- 映像 ----------

    /// <summary>新しいフレームがあれば dst に写す（UIスレッドから呼ぶ）</summary>
    public bool TryGetFrame(ref int[] dst, out int width, out int height, ref long serial)
    {
        lock (frameLock)
        {
            width = frontW;
            height = frontH;
            if (serial == frameSerial || frontW == 0) return false;
            if (dst.Length < frontW * frontH) dst = new int[frontW * frontH];
            Array.Copy(frontBuffer, dst, frontW * frontH);
            serial = frameSerial;
            return true;
        }
    }

    public void OnPixelFormat(uint format) => pixelFormat = format;

    public void OnVideo(IntPtr data, uint width, uint height, nuint pitch)
    {
        if (data == IntPtr.Zero || width == 0 || height == 0) return; // 前のフレームをそのまま使う
        int w = (int)width, h = (int)height;
        if (backBuffer.Length < w * h) backBuffer = new int[w * h];

        if (data == Retro.HwFrameBufferValid)
        {
            gl?.ReadFrame(w, h, backBuffer, w);
        }
        else
        {
            byte* src = (byte*)data;
            fixed (int* dstBase = backBuffer)
            {
                for (int y = 0; y < h; y++)
                {
                    byte* row = src + (long)y * (long)pitch;
                    int* d = dstBase + y * w;
                    switch (pixelFormat)
                    {
                        case Retro.PixelXrgb8888:
                        {
                            uint* s = (uint*)row;
                            for (int x = 0; x < w; x++) d[x] = (int)(s[x] | 0xFF000000);
                            break;
                        }
                        case Retro.PixelRgb565:
                        {
                            ushort* s = (ushort*)row;
                            for (int x = 0; x < w; x++)
                            {
                                int c = s[x];
                                int r = (c >> 11) & 0x1F, g = (c >> 5) & 0x3F, b = c & 0x1F;
                                d[x] = unchecked((int)0xFF000000) | ((r << 3 | r >> 2) << 16) | ((g << 2 | g >> 4) << 8) | (b << 3 | b >> 2);
                            }
                            break;
                        }
                        default:
                        {
                            ushort* s = (ushort*)row;
                            for (int x = 0; x < w; x++)
                            {
                                int c = s[x];
                                int r = (c >> 10) & 0x1F, g = (c >> 5) & 0x1F, b = c & 0x1F;
                                d[x] = unchecked((int)0xFF000000) | ((r << 3 | r >> 2) << 16) | ((g << 3 | g >> 2) << 8) | (b << 3 | b >> 2);
                            }
                            break;
                        }
                    }
                }
            }
        }

        lock (frameLock)
        {
            (frontBuffer, backBuffer) = (backBuffer, frontBuffer);
            frontW = w;
            frontH = h;
            frameSerial++;
        }
    }

    public void OnGeometry(RetroGameGeometry geometry) => ApplyGeometry(geometry);

    public void OnAvInfo(RetroSystemAvInfo info) => ApplyAvInfo(info);

    public void OnRotation(uint rotation)
    {
        Rotation = rotation & 3;
        GeometryChanged?.Invoke();
    }

    public void OnMessage(string text, int frames)
    {
        if (!string.IsNullOrWhiteSpace(text)) Message?.Invoke(text);
    }

    public void OnShutdown() => running = false;

    // ---------- 音声 ----------

    public void OnAudio(IntPtr frames, int count)
    {
        if (FastForwarding || audio == null) return;
        audio.Write((short*)frames, count);
    }

    // ---------- 入力 ----------

    public void OnInputPoll()
    {
        var profile = settings.Input;
        if (profile == null) return;
        int turboRate = Math.Max(1, settings.TurboRate ?? 3);
        bool turboOn = frameCount / turboRate % 2 == 0;
        for (int p = 0; p < 4; p++)
        {
            ushort mask = 0;
            if (p < profile.Players.Count)
            {
                var map = profile.Players[p];
                for (int id = 0; id < InputProfile.Controls.Length; id++)
                {
                    var name = InputProfile.Controls[id];
                    if (!map.Buttons.TryGetValue(name, out var list) || !input.AnyActive(list, p)) continue;
                    if (map.Turbo.Contains(name) && !turboOn) continue;
                    mask |= (ushort)(1 << ControlToRetro(id));
                }
            }
            padMask[p] = mask;
            analog[p, 0] = input.Stick(p, InputProfile.AxisLeftX);
            analog[p, 1] = input.Stick(p, InputProfile.AxisLeftY);
            analog[p, 2] = input.Stick(p, InputProfile.AxisRightX);
            analog[p, 3] = input.Stick(p, InputProfile.AxisRightY);
        }
    }

    /// <summary>InputProfile.Controls の並び → libretro のボタン番号</summary>
    private static int ControlToRetro(int index) => index switch
    {
        0 => Retro.JoyUp, 1 => Retro.JoyDown, 2 => Retro.JoyLeft, 3 => Retro.JoyRight,
        4 => Retro.JoyA, 5 => Retro.JoyB, 6 => Retro.JoyX, 7 => Retro.JoyY,
        8 => Retro.JoyL, 9 => Retro.JoyR, 10 => Retro.JoyL2, 11 => Retro.JoyR2,
        12 => Retro.JoyL3, 13 => Retro.JoyR3, 14 => Retro.JoySelect, _ => Retro.JoyStart
    };

    public short OnInputState(uint port, uint device, uint index, uint id)
    {
        if (port >= 4 || MenuPaused) return 0;
        switch (device & 0xFF)
        {
            case Retro.DeviceJoypad:
                if (id == Retro.DeviceIdJoypadMask) return (short)padMask[port];
                return id < 16 && (padMask[port] & (1 << (int)id)) != 0 ? (short)1 : (short)0;
            case Retro.DeviceAnalog:
                if (index == Retro.AnalogLeft && id <= 1) return analog[port, (int)id];
                if (index == Retro.AnalogRight && id <= 1) return analog[port, 2 + (int)id];
                if (index == 2 && id < 16) return (padMask[port] & (1 << (int)id)) != 0 ? short.MaxValue : (short)0;
                return 0;
        }
        return 0;
    }

    private readonly ushort[,] rumble = new ushort[4, 2];

    public bool OnRumble(uint port, int effect, ushort strength)
    {
        if (port >= 4 || effect is < 0 or > 1) return false;
        if (rumble[port, effect] == strength) return true;
        rumble[port, effect] = strength;
        input.Rumble((int)port, rumble[port, 0], rumble[port, 1], strength == 0 && rumble[port, 1 - effect] == 0 ? 0u : 5000u);
        return true;
    }

    // ---------- コア固有の設定 ----------

    public string? GetOptionValue(string key) =>
        settings.CoreOptions != null && settings.CoreOptions.TryGetValue(key, out var v) ? v : null;

    public bool ConsumeOptionsChanged()
    {
        if (!optionsChanged) return false;
        optionsChanged = false;
        return true;
    }

    public IReadOnlyList<CoreOption> DeclaredOptions { get; private set; } = Array.Empty<CoreOption>();

    public void OnOptionsDeclared(IReadOnlyList<CoreOption> options)
    {
        DeclaredOptions = options.ToList();
        Storage.SaveCoreOptions(req.CoreName, options);
    }

    // ---------- ハードウェア描画 ----------

    public bool OnHwRender(ref RetroHwRenderCallback hw)
    {
        if (hw.ContextType is not (Retro.HwContextOpenGl or Retro.HwContextOpenGlCore))
            return false;
        useHw = true;
        try
        {
            gl?.Dispose();
            gl = new GlContext(hw, 1024, 1024);
            hwCallback = hw;
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            gl = null;
            return false;
        }
    }

    public IntPtr HwGetCurrentFramebuffer() => (IntPtr)(gl?.Framebuffer ?? 0);

    public IntPtr HwGetProcAddress(string name) => gl?.Proc(name) ?? IntPtr.Zero;
}

/// <summary>画面の情報</summary>
public static class DisplayInfo
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmDeviceName;
        public short dmSpecVersion, dmDriverVersion, dmSize, dmDriverExtra;
        public int dmFields, dmPositionX, dmPositionY, dmDisplayOrientation, dmDisplayFixedOutput;
        public short dmColor, dmDuplex, dmYResolution, dmTTOption, dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel, dmPelsWidth, dmPelsHeight, dmDisplayFlags, dmDisplayFrequency;
        public int dmICMMethod, dmICMIntent, dmMediaType, dmDitherType, dmReserved1, dmReserved2, dmPanningWidth, dmPanningHeight;
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern bool EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int ChangeDisplaySettingsEx(string? deviceName, ref DEVMODE devMode, IntPtr hwnd, int flags, IntPtr param);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int ChangeDisplaySettingsEx(string? deviceName, IntPtr devMode, IntPtr hwnd, int flags, IntPtr param);

    private const int ENUM_CURRENT_SETTINGS = -1, CDS_FULLSCREEN = 4, DISP_CHANGE_SUCCESSFUL = 0;
    private const int DM_PELSWIDTH = 0x80000, DM_PELSHEIGHT = 0x100000, DM_DISPLAYFREQUENCY = 0x400000;

    public static double CurrentRefreshRate()
    {
        var dm = new DEVMODE { dmSize = (short)Marshal.SizeOf<DEVMODE>() };
        return EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm) && dm.dmDisplayFrequency > 1 ? dm.dmDisplayFrequency : 60;
    }

    /// <summary>主画面で使える画面モード（"1920x1080@60" の形）</summary>
    public static List<string> Modes()
    {
        var set = new SortedSet<(int w, int h, int f)>();
        var dm = new DEVMODE { dmSize = (short)Marshal.SizeOf<DEVMODE>() };
        for (int i = 0; EnumDisplaySettings(null, i, ref dm); i++)
            if (dm.dmBitsPerPel >= 24 && dm.dmPelsWidth >= 640) set.Add((dm.dmPelsWidth, dm.dmPelsHeight, dm.dmDisplayFrequency));
        return set.Reverse().Select(m => $"{m.w}x{m.h}@{m.f}").ToList();
    }

    /// <summary>全画面のあいだだけ画面モードを変える（成功したら true）</summary>
    public static bool ChangeMode(string mode)
    {
        var m = System.Text.RegularExpressions.Regex.Match(mode ?? "", @"^(\d+)x(\d+)@(\d+)$");
        if (!m.Success) return false;
        var dm = new DEVMODE { dmSize = (short)Marshal.SizeOf<DEVMODE>() };
        if (!EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm)) return false;
        dm.dmPelsWidth = int.Parse(m.Groups[1].Value);
        dm.dmPelsHeight = int.Parse(m.Groups[2].Value);
        dm.dmDisplayFrequency = int.Parse(m.Groups[3].Value);
        dm.dmFields = DM_PELSWIDTH | DM_PELSHEIGHT | DM_DISPLAYFREQUENCY;
        return ChangeDisplaySettingsEx(null, ref dm, IntPtr.Zero, CDS_FULLSCREEN, IntPtr.Zero) == DISP_CHANGE_SUCCESSFUL;
    }

    public static void RestoreMode() => ChangeDisplaySettingsEx(null, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero);
}
