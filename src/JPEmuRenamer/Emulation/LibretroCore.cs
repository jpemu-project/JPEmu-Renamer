// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace JPEmuRenamer.Emulation;

/// <summary>コアが宣言した設定項目（SET_VARIABLES）</summary>
public class CoreOption
{
    public string Key { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Values { get; set; } = new();
    public string Default => Values.Count > 0 ? Values[0] : "";
}

/// <summary>コアからの呼び出しを受け取る側（EmulatorSession が実装する）</summary>
public interface IRetroHost
{
    string SystemDirectory { get; }
    string SaveDirectory { get; }
    bool FastForwarding { get; }
    void OnVideo(IntPtr data, uint width, uint height, nuint pitch);
    void OnAudio(IntPtr frames, int count);
    void OnInputPoll();
    short OnInputState(uint port, uint device, uint index, uint id);
    void OnPixelFormat(uint format);
    void OnGeometry(RetroGameGeometry geometry);
    void OnAvInfo(RetroSystemAvInfo info);
    void OnRotation(uint rotation);
    void OnMessage(string text, int frames);
    bool OnRumble(uint port, int effect, ushort strength);
    string? GetOptionValue(string key);
    bool ConsumeOptionsChanged();
    void OnOptionsDeclared(IReadOnlyList<CoreOption> options);
    bool OnHwRender(ref RetroHwRenderCallback hw);
    IntPtr HwGetCurrentFramebuffer();
    IntPtr HwGetProcAddress(string name);
    void OnShutdown();
}

/// <summary>
/// libretroコア（DLL）を読み込み、関数を呼び出す。
/// コアは関数ポインタで呼び返してくるが文脈を渡さないため、同時に動かせるコアは1つだけ（Current）。
/// コアの呼び出しはすべて同じスレッド（エミュレーションスレッド）から行うこと。
/// </summary>
public sealed unsafe class LibretroCore : IDisposable
{
    private static LibretroCore? current;

    private readonly IntPtr lib;
    private readonly IRetroHost host;
    private readonly List<IntPtr> allocations = new();
    private readonly Dictionary<string, IntPtr> variableValues = new();
    private readonly List<CoreOption> options = new();
    private IntPtr systemDirPtr, saveDirPtr, corePathPtr;
    private bool initialized, gameLoaded;

    private readonly delegate* unmanaged[Cdecl]<delegate* unmanaged[Cdecl]<uint, void*, byte>, void> setEnvironment;
    private readonly delegate* unmanaged[Cdecl]<delegate* unmanaged[Cdecl]<void*, uint, uint, nuint, void>, void> setVideoRefresh;
    private readonly delegate* unmanaged[Cdecl]<delegate* unmanaged[Cdecl]<short, short, void>, void> setAudioSample;
    private readonly delegate* unmanaged[Cdecl]<delegate* unmanaged[Cdecl]<short*, nuint, nuint>, void> setAudioSampleBatch;
    private readonly delegate* unmanaged[Cdecl]<delegate* unmanaged[Cdecl]<void>, void> setInputPoll;
    private readonly delegate* unmanaged[Cdecl]<delegate* unmanaged[Cdecl]<uint, uint, uint, uint, short>, void> setInputState;
    private readonly delegate* unmanaged[Cdecl]<void> init;
    private readonly delegate* unmanaged[Cdecl]<void> deinit;
    private readonly delegate* unmanaged[Cdecl]<uint> apiVersion;
    private readonly delegate* unmanaged[Cdecl]<RetroSystemInfo*, void> getSystemInfo;
    private readonly delegate* unmanaged[Cdecl]<RetroSystemAvInfo*, void> getSystemAvInfo;
    private readonly delegate* unmanaged[Cdecl]<uint, uint, void> setControllerPortDevice;
    private readonly delegate* unmanaged[Cdecl]<void> reset;
    private readonly delegate* unmanaged[Cdecl]<void> run;
    private readonly delegate* unmanaged[Cdecl]<nuint> serializeSize;
    private readonly delegate* unmanaged[Cdecl]<void*, nuint, byte> serialize;
    private readonly delegate* unmanaged[Cdecl]<void*, nuint, byte> unserialize;
    private readonly delegate* unmanaged[Cdecl]<RetroGameInfo*, byte> loadGame;
    private readonly delegate* unmanaged[Cdecl]<void> unloadGame;
    private readonly delegate* unmanaged[Cdecl]<uint, void*> getMemoryData;
    private readonly delegate* unmanaged[Cdecl]<uint, nuint> getMemorySize;

    public string CorePath { get; }
    public string LibraryName { get; private set; } = "";
    public string LibraryVersion { get; private set; } = "";
    public HashSet<string> ValidExtensions { get; } = new(StringComparer.OrdinalIgnoreCase);
    public bool NeedFullpath { get; private set; }
    public RetroSystemAvInfo AvInfo { get; private set; }
    public IReadOnlyList<CoreOption> Options => options;

    public LibretroCore(string corePath, IRetroHost host)
    {
        if (current != null) throw new InvalidOperationException("Another core is already running.");
        CorePath = corePath;
        this.host = host;
        lib = NativeLibrary.Load(corePath);
        try
        {
            setEnvironment = (delegate* unmanaged[Cdecl]<delegate* unmanaged[Cdecl]<uint, void*, byte>, void>)Export("retro_set_environment");
            setVideoRefresh = (delegate* unmanaged[Cdecl]<delegate* unmanaged[Cdecl]<void*, uint, uint, nuint, void>, void>)Export("retro_set_video_refresh");
            setAudioSample = (delegate* unmanaged[Cdecl]<delegate* unmanaged[Cdecl]<short, short, void>, void>)Export("retro_set_audio_sample");
            setAudioSampleBatch = (delegate* unmanaged[Cdecl]<delegate* unmanaged[Cdecl]<short*, nuint, nuint>, void>)Export("retro_set_audio_sample_batch");
            setInputPoll = (delegate* unmanaged[Cdecl]<delegate* unmanaged[Cdecl]<void>, void>)Export("retro_set_input_poll");
            setInputState = (delegate* unmanaged[Cdecl]<delegate* unmanaged[Cdecl]<uint, uint, uint, uint, short>, void>)Export("retro_set_input_state");
            init = (delegate* unmanaged[Cdecl]<void>)Export("retro_init");
            deinit = (delegate* unmanaged[Cdecl]<void>)Export("retro_deinit");
            apiVersion = (delegate* unmanaged[Cdecl]<uint>)Export("retro_api_version");
            getSystemInfo = (delegate* unmanaged[Cdecl]<RetroSystemInfo*, void>)Export("retro_get_system_info");
            getSystemAvInfo = (delegate* unmanaged[Cdecl]<RetroSystemAvInfo*, void>)Export("retro_get_system_av_info");
            setControllerPortDevice = (delegate* unmanaged[Cdecl]<uint, uint, void>)Export("retro_set_controller_port_device");
            reset = (delegate* unmanaged[Cdecl]<void>)Export("retro_reset");
            run = (delegate* unmanaged[Cdecl]<void>)Export("retro_run");
            serializeSize = (delegate* unmanaged[Cdecl]<nuint>)Export("retro_serialize_size");
            serialize = (delegate* unmanaged[Cdecl]<void*, nuint, byte>)Export("retro_serialize");
            unserialize = (delegate* unmanaged[Cdecl]<void*, nuint, byte>)Export("retro_unserialize");
            loadGame = (delegate* unmanaged[Cdecl]<RetroGameInfo*, byte>)Export("retro_load_game");
            unloadGame = (delegate* unmanaged[Cdecl]<void>)Export("retro_unload_game");
            getMemoryData = (delegate* unmanaged[Cdecl]<uint, void*>)Export("retro_get_memory_data");
            getMemorySize = (delegate* unmanaged[Cdecl]<uint, nuint>)Export("retro_get_memory_size");
        }
        catch
        {
            NativeLibrary.Free(lib);
            throw;
        }
        current = this;
    }

    private IntPtr Export(string name) =>
        NativeLibrary.TryGetExport(lib, name, out var p) ? p : throw new EntryPointNotFoundException(name);

    /// <summary>コアを初期化し、システム情報を読む</summary>
    public void Init()
    {
        if (apiVersion() != Retro.ApiVersion) throw new NotSupportedException("Unsupported libretro API version.");
        systemDirPtr = Utf8(host.SystemDirectory);
        saveDirPtr = Utf8(host.SaveDirectory);
        corePathPtr = Utf8(CorePath);

        // RetroArch と同じ順番にする（retro_init の中で準備したものに、あとからコールバックを渡すコアがある）
        setEnvironment(&EnvironmentCallback);

        RetroSystemInfo info;
        getSystemInfo(&info);
        LibraryName = Marshal.PtrToStringUTF8(info.LibraryName) ?? "";
        LibraryVersion = Marshal.PtrToStringUTF8(info.LibraryVersion) ?? "";
        foreach (var e in (Marshal.PtrToStringUTF8(info.ValidExtensions) ?? "").Split('|', StringSplitOptions.RemoveEmptyEntries))
            ValidExtensions.Add(e.Trim().TrimStart('.'));
        NeedFullpath = info.NeedFullpath != 0;

        init();
        initialized = true;

        setVideoRefresh(&VideoRefreshCallback);
        setAudioSample(&AudioSampleCallback);
        setAudioSampleBatch(&AudioSampleBatchCallback);
        setInputPoll(&InputPollCallback);
        setInputState(&InputStateCallback);
    }

    /// <summary>ゲームを読み込む。data が null のときはパスだけを渡す（need_fullpath のコア）</summary>
    public bool LoadGame(string path, byte[]? data)
    {
        var pathPtr = Utf8(path);
        RetroGameInfo gi = default;
        gi.Path = pathPtr;
        bool ok;
        if (data != null)
        {
            // コアが読み込み中にデータを参照するので、アンロードまで固定しておく
            var mem = Marshal.AllocHGlobal(data.Length);
            allocations.Add(mem);
            Marshal.Copy(data, 0, mem, data.Length);
            gi.Data = mem;
            gi.Size = (nuint)data.Length;
        }
        ok = loadGame(&gi) != 0;
        Trace?.Invoke($"load_game {(ok ? "ok" : "failed")}: {path} data={data?.Length ?? -1} exts={string.Join("|", ValidExtensions)} fullpath={NeedFullpath}");
        if (!ok) return false;
        gameLoaded = true;

        RetroSystemAvInfo av;
        getSystemAvInfo(&av);
        AvInfo = av;
        return true;
    }

    public void SetControllerPortDevice(uint port, uint device) => setControllerPortDevice(port, device);
    public void Run() => run();
    public void Reset() => reset();

    public byte[]? SaveState()
    {
        var size = serializeSize();
        if (size == 0) return null;
        var buf = new byte[(int)size];
        fixed (byte* p = buf)
            return serialize(p, size) != 0 ? buf : null;
    }

    public bool LoadState(byte[] state)
    {
        fixed (byte* p = state)
            return unserialize(p, (nuint)state.Length) != 0;
    }

    /// <summary>バッテリーバックアップなどのメモリ領域（無ければ空）</summary>
    public Span<byte> Memory(uint id)
    {
        var data = getMemoryData(id);
        var size = getMemorySize(id);
        return data == null || size == 0 ? Span<byte>.Empty : new Span<byte>(data, (int)size);
    }

    public void RefreshAvInfo()
    {
        RetroSystemAvInfo av;
        getSystemAvInfo(&av);
        AvInfo = av;
    }

    public void Dispose()
    {
        try
        {
            if (gameLoaded) unloadGame();
            if (initialized) deinit();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        gameLoaded = initialized = false;
        if (current == this) current = null;
        try { NativeLibrary.Free(lib); } catch { }
        foreach (var p in allocations) Marshal.FreeHGlobal(p);
        allocations.Clear();
        foreach (var p in variableValues.Values) Marshal.FreeCoTaskMem(p);
        variableValues.Clear();
        foreach (var p in utf8Allocations) Marshal.FreeCoTaskMem(p);
        utf8Allocations.Clear();
    }

    // ---------- 文字列 ----------

    private IntPtr Utf8(string s)
    {
        var p = Marshal.StringToCoTaskMemUTF8(s);
        utf8Allocations.Add(p);
        return p;
    }

    private readonly List<IntPtr> utf8Allocations = new();

    // ---------- コールバック（コアから呼ばれる） ----------

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static byte EnvironmentCallback(uint cmd, void* data)
    {
        var self = current;
        if (self == null) return 0;
        try
        {
            return self.Environment(cmd, data) ? (byte)1 : (byte)0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return 0;
        }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void VideoRefreshCallback(void* data, uint width, uint height, nuint pitch)
    {
        try { current?.host.OnVideo((IntPtr)data, width, height, pitch); }
        catch (Exception ex) { Debug.WriteLine(ex); }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void AudioSampleCallback(short left, short right)
    {
        var self = current;
        if (self == null) return;
        short* buf = stackalloc short[2];
        buf[0] = left;
        buf[1] = right;
        try { self.host.OnAudio((IntPtr)buf, 1); }
        catch (Exception ex) { Debug.WriteLine(ex); }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static nuint AudioSampleBatchCallback(short* data, nuint frames)
    {
        try { current?.host.OnAudio((IntPtr)data, (int)frames); }
        catch (Exception ex) { Debug.WriteLine(ex); }
        return frames;
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void InputPollCallback()
    {
        try { current?.host.OnInputPoll(); }
        catch (Exception ex) { Debug.WriteLine(ex); }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static short InputStateCallback(uint port, uint device, uint index, uint id)
    {
        try { return current?.host.OnInputState(port, device, index, id) ?? 0; }
        catch (Exception ex) { Debug.WriteLine(ex); return 0; }
    }

    // 可変長引数の printf 形式だが、書式文字列だけを読む（x64 では残りの引数を無視しても安全）
    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static void LogCallback(int level, byte* fmt)
    {
        try
        {
            var s = Marshal.PtrToStringUTF8((IntPtr)fmt);
            if (!string.IsNullOrEmpty(s)) Debug.Write("[core] " + s);
        }
        catch { }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static byte RumbleCallback(uint port, int effect, ushort strength)
    {
        try { return current?.host.OnRumble(port, effect, strength) == true ? (byte)1 : (byte)0; }
        catch { return 0; }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static nuint HwGetCurrentFramebufferCallback()
    {
        try { return (nuint)(current?.host.HwGetCurrentFramebuffer() ?? IntPtr.Zero); }
        catch { return 0; }
    }

    [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
    private static IntPtr HwGetProcAddressCallback(byte* sym)
    {
        try
        {
            var name = Marshal.PtrToStringUTF8((IntPtr)sym);
            return name == null ? IntPtr.Zero : current?.host.HwGetProcAddress(name) ?? IntPtr.Zero;
        }
        catch { return IntPtr.Zero; }
    }

    /// <summary>調査用：コアからの環境コマンドを書き出す</summary>
    public static Action<string>? Trace { get; set; }

    private bool Environment(uint cmd, void* data)
    {
        Trace?.Invoke($"env {cmd & 0xFFFF}{((cmd & Retro.EnvExperimental) != 0 ? "|EXP" : "")}");
        switch (cmd & ~(Retro.EnvExperimental | Retro.EnvPrivate))
        {
            case Retro.SetRotation:
                host.OnRotation(*(uint*)data);
                return true;
            case Retro.GetOverscan:
                *(byte*)data = 0;
                return true;
            case Retro.GetCanDupe:
                *(byte*)data = 1;
                return true;
            case Retro.SetMessage:
            {
                var m = (RetroMessage*)data;
                host.OnMessage(Marshal.PtrToStringUTF8(m->Msg) ?? "", (int)m->Frames);
                return true;
            }
            case Retro.SetMessageExt:
            {
                var m = (RetroMessageExt*)data;
                host.OnMessage(Marshal.PtrToStringUTF8(m->Msg) ?? "", (int)Math.Max(60, m->Duration * 60 / 1000));
                return true;
            }
            case Retro.GetMessageInterfaceVersion:
                *(uint*)data = 1;
                return true;
            case Retro.Shutdown:
                host.OnShutdown();
                return true;
            case Retro.SetPerformanceLevel:
            case Retro.SetInputDescriptors:
            case Retro.SetSupportNoGame:
            case Retro.SetControllerInfo:
            case Retro.SetMemoryMaps:
            case Retro.SetSupportAchievements:
            case Retro.SetSerializationQuirks:
            case Retro.SetCoreOptionsDisplay:
            case Retro.SetSubsystemInfo:
            case Retro.SetMinimumAudioLatency:
            case Retro.SetCoreOptionsUpdateDisplayCallback:
                return true;
            case Retro.GetSystemDirectory:
                *(IntPtr*)data = systemDirPtr;
                return true;
            case Retro.GetSaveDirectory:
                *(IntPtr*)data = saveDirPtr;
                return true;
            case Retro.GetCoreAssetsDirectory:
                *(IntPtr*)data = systemDirPtr;
                return true;
            case Retro.GetLibretroPath:
                *(IntPtr*)data = corePathPtr;
                return true;
            case Retro.SetPixelFormat:
            {
                var fmt = *(uint*)data;
                if (fmt > Retro.PixelRgb565) return false;
                host.OnPixelFormat(fmt);
                return true;
            }
            case Retro.GetVariable:
            {
                var v = (RetroVariable*)data;
                var key = Marshal.PtrToStringUTF8(v->Key);
                if (key == null) return false;
                var opt = options.FirstOrDefault(o => o.Key == key);
                var value = host.GetOptionValue(key);
                if (opt != null && (value == null || !opt.Values.Contains(value))) value = opt.Default;
                if (value == null) { v->Value = IntPtr.Zero; return false; }
                if (variableValues.TryGetValue(key, out var old)) Marshal.FreeCoTaskMem(old);
                var p = Marshal.StringToCoTaskMemUTF8(value);
                variableValues[key] = p;
                v->Value = p;
                return true;
            }
            case Retro.SetVariables:
            {
                options.Clear();
                for (var v = (RetroVariable*)data; v->Key != IntPtr.Zero; v++)
                {
                    var key = Marshal.PtrToStringUTF8(v->Key) ?? "";
                    var desc = Marshal.PtrToStringUTF8(v->Value) ?? "";
                    var semi = desc.IndexOf(';');
                    var opt = new CoreOption { Key = key, Description = semi >= 0 ? desc[..semi].Trim() : key };
                    if (semi >= 0)
                        opt.Values.AddRange(desc[(semi + 1)..].Trim().Split('|'));
                    options.Add(opt);
                }
                host.OnOptionsDeclared(options);
                return true;
            }
            case Retro.GetVariableUpdate:
                *(byte*)data = host.ConsumeOptionsChanged() ? (byte)1 : (byte)0;
                return true;
            case Retro.GetCoreOptionsVersion:
                // 0 を返すと、コアは SET_VARIABLES（キーと選択肢の一覧）で設定項目を伝えてくる
                *(uint*)data = 0;
                return true;
            case Retro.GetLogInterface:
                *(IntPtr*)data = (IntPtr)(delegate* unmanaged[Cdecl]<int, byte*, void>)&LogCallback;
                return true;
            case Retro.GetRumbleInterface:
                *(IntPtr*)data = (IntPtr)(delegate* unmanaged[Cdecl]<uint, int, ushort, byte>)&RumbleCallback;
                return true;
            case Retro.GetLanguage:
                // libretro の言語番号（コアのメニューやゲームの言語選択に使われる）
                *(uint*)data = Services.Loc.Instance.Language switch
                {
                    "ja" => Retro.LanguageJapanese,
                    "fr" => 2u,
                    "es" => 3u,
                    "it" => 5u,
                    "pt" => 7u,   // ブラジルのポルトガル語
                    "ru" => 9u,
                    "ko" => 10u,
                    "zh" => 12u,  // 簡体字
                    "zh-Hant" => 11u, // 繁体字
                    "de" => 4u,
                    "pl" => 14u,
                    "vi" => 15u,
                    "tr" => 18u,
                    "id" => 24u,
                    // タイ語は libretro に無いので英語
                    _ => Retro.LanguageEnglish
                };
                return true;
            case Retro.GetInputBitmasks:
                return true;
            case Retro.GetInputMaxUsers:
                *(uint*)data = 4;
                return true;
            case Retro.GetAudioVideoEnable:
                *(int*)data = 3;
                return true;
            case Retro.GetFastForwarding:
                *(byte*)data = host.FastForwarding ? (byte)1 : (byte)0;
                return true;
            case Retro.SetGeometry:
                host.OnGeometry(*(RetroGameGeometry*)data);
                return true;
            case Retro.SetSystemAvInfo:
            {
                var av = *(RetroSystemAvInfo*)data;
                AvInfo = av;
                host.OnAvInfo(av);
                return true;
            }
            case Retro.GetPreferredHwRender:
                *(uint*)data = Retro.HwContextOpenGl;
                return true;
            case Retro.SetHwRender:
            {
                var hw = (RetroHwRenderCallback*)data;
                if (!host.OnHwRender(ref *hw)) return false;
                hw->GetCurrentFramebuffer = (IntPtr)(delegate* unmanaged[Cdecl]<nuint>)&HwGetCurrentFramebufferCallback;
                hw->GetProcAddress = (IntPtr)(delegate* unmanaged[Cdecl]<byte*, IntPtr>)&HwGetProcAddressCallback;
                return true;
            }
            default:
                return false;
        }
    }
}
