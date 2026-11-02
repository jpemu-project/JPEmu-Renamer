// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Diagnostics;
using System.Runtime.InteropServices;
using JPEmuRenamer.Models;
using Silk.NET.SDL;
using Thread = System.Threading.Thread;
using ThreadPriority = System.Threading.ThreadPriority;

namespace JPEmuRenamer.Emulation;

/// <summary>
/// キーボードとゲームパッドの状態を集める。
/// 専用スレッドで約250回/秒読み取り、最新の状態を保持する。パッドの認識はSDL2
/// （XInput・DirectInput・Switch Proコントローラー・DualSenseなど）。
/// キーボードはこのアプリが前面にあるときだけ読む。
/// </summary>
public sealed unsafe class InputManager : IDisposable
{
    public static InputManager Instance { get; } = new();

    public const int MaxButtons = 21;
    public const int MaxAxes = 6;

    /// <summary>パッド1台分の状態</summary>
    public sealed class Pad
    {
        public int InstanceId;
        public string Name = "";
        public bool IsController;
        public readonly bool[] Buttons = new bool[MaxButtons];
        public readonly short[] Axes = new short[MaxAxes];
        public bool[] JoyButtons = Array.Empty<bool>();
        public short[] JoyAxes = Array.Empty<short>();
        public byte[] JoyHats = Array.Empty<byte>();
        internal GameController* Controller;
        internal Joystick* Joystick;
    }

    private readonly object gate = new();
    private readonly bool[] keys = new bool[256];
    private List<Pad> pads = new();
    private Thread? thread;
    private volatile bool running;
    private Sdl? sdl;
    private int users;

    /// <summary>キーボードを読むかどうかの判定に使う（前面のウィンドウがこのプロセスか）</summary>
    private readonly int processId = Environment.ProcessId;

    public double DeadzoneRatio { get; set; } = 0.25;

    private InputManager() { }

    /// <summary>使い始める（参照カウント）。プレイ画面や割り当て画面が開いている間だけ動かす</summary>
    public void Acquire()
    {
        lock (gate)
        {
            users++;
            if (thread != null) return;
            running = true;
            thread = new Thread(Loop) { IsBackground = true, Name = "Input", Priority = ThreadPriority.AboveNormal };
            thread.Start();
        }
    }

    public void Release()
    {
        Thread? t;
        lock (gate)
        {
            if (--users > 0) return;
            users = 0;
            running = false;
            t = thread;
            thread = null;
        }
        t?.Join(1000);
    }

    public void Dispose() => Release();

    private void Loop()
    {
        try
        {
            sdl = Sdl.GetApi();
            sdl.SetHint(Sdl.HintJoystickAllowBackgroundEvents, "1");
            sdl.SetHint("SDL_JOYSTICK_HIDAPI_PS4_RUMBLE", "1");
            sdl.SetHint("SDL_JOYSTICK_HIDAPI_PS5_RUMBLE", "1");
            if (sdl.Init(Sdl.InitGamecontroller | Sdl.InitJoystick) != 0)
                Debug.WriteLine("SDL_Init failed: " + sdl.GetErrorS());
        }
        catch (Exception ex)
        {
            // SDL2.dll が読めなくてもキーボードは使えるようにする
            Debug.WriteLine(ex);
            sdl = null;
        }

        var lastScan = Stopwatch.StartNew();
        bool first = true;
        while (running)
        {
            if (sdl != null)
            {
                sdl.PumpEvents();
                sdl.FlushEvents(0, uint.MaxValue);
                if (first || lastScan.ElapsedMilliseconds > 1000)
                {
                    Rescan();
                    lastScan.Restart();
                    first = false;
                }
                ReadPads();
            }
            ReadKeyboard();
            Thread.Sleep(4);
        }

        if (sdl != null)
        {
            lock (gate)
            {
                foreach (var p in pads) ClosePad(p);
                pads = new List<Pad>();
            }
            sdl.Quit();
        }
    }

    private void Rescan()
    {
        var s = sdl!;
        int n = s.NumJoysticks();
        var current = new List<Pad>();
        var old = pads;
        for (int i = 0; i < n; i++)
        {
            int id = s.JoystickGetDeviceInstanceID(i);
            var existing = old.FirstOrDefault(p => p.InstanceId == id);
            if (existing != null)
            {
                current.Add(existing);
                continue;
            }
            var pad = new Pad { InstanceId = id };
            if (s.IsGameController(i) == SdlBool.True)
            {
                pad.Controller = s.GameControllerOpen(i);
                if (pad.Controller == null) continue;
                pad.IsController = true;
                pad.Joystick = s.GameControllerGetJoystick(pad.Controller);
                pad.Name = s.GameControllerNameS(pad.Controller) ?? "Controller";
            }
            else
            {
                pad.Joystick = s.JoystickOpen(i);
                if (pad.Joystick == null) continue;
                pad.Name = s.JoystickNameS(pad.Joystick) ?? "Joystick";
            }
            pad.JoyButtons = new bool[Math.Max(0, s.JoystickNumButtons(pad.Joystick))];
            pad.JoyAxes = new short[Math.Max(0, s.JoystickNumAxes(pad.Joystick))];
            pad.JoyHats = new byte[Math.Max(0, s.JoystickNumHats(pad.Joystick))];
            current.Add(pad);
        }
        foreach (var p in old.Where(o => !current.Contains(o))) ClosePad(p);
        lock (gate) pads = current;
    }

    private void ClosePad(Pad p)
    {
        if (sdl == null) return;
        if (p.Controller != null) sdl.GameControllerClose(p.Controller);
        else if (p.Joystick != null) sdl.JoystickClose(p.Joystick);
        p.Controller = null;
        p.Joystick = null;
    }

    private void ReadPads()
    {
        var s = sdl!;
        List<Pad> list;
        lock (gate) list = pads;
        foreach (var p in list)
        {
            bool[] b = new bool[MaxButtons];
            short[] a = new short[MaxAxes];
            if (p.Controller != null)
            {
                for (int i = 0; i < MaxButtons; i++) b[i] = s.GameControllerGetButton(p.Controller, (GameControllerButton)i) != 0;
                for (int i = 0; i < MaxAxes; i++) a[i] = s.GameControllerGetAxis(p.Controller, (GameControllerAxis)i);
            }
            var jb = new bool[p.JoyButtons.Length];
            var ja = new short[p.JoyAxes.Length];
            var jh = new byte[p.JoyHats.Length];
            if (p.Joystick != null)
            {
                for (int i = 0; i < jb.Length; i++) jb[i] = s.JoystickGetButton(p.Joystick, i) != 0;
                for (int i = 0; i < ja.Length; i++) ja[i] = s.JoystickGetAxis(p.Joystick, i);
                for (int i = 0; i < jh.Length; i++) jh[i] = s.JoystickGetHat(p.Joystick, i);
            }
            lock (gate)
            {
                Array.Copy(b, p.Buttons, MaxButtons);
                Array.Copy(a, p.Axes, MaxAxes);
                p.JoyButtons = jb;
                p.JoyAxes = ja;
                p.JoyHats = jh;
            }
        }
    }

    [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int vKey);
    [DllImport("user32.dll")] private static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    private void ReadKeyboard()
    {
        var fg = GetForegroundWindow();
        GetWindowThreadProcessId(fg, out var pid);
        bool active = pid == processId;
        var k = new bool[256];
        if (active)
            for (int i = 1; i < 256; i++) k[i] = (GetAsyncKeyState(i) & 0x8000) != 0;
        lock (gate) Array.Copy(k, keys, 256);
    }

    // ---------- 状態の読み取り ----------

    /// <summary>パッドの一覧（接続順。プレイヤーNはN番目のパッド）</summary>
    public IReadOnlyList<Pad> Pads
    {
        get { lock (gate) return pads.ToList(); }
    }

    public bool KeyDown(int vk)
    {
        if (vk <= 0 || vk > 255) return false;
        lock (gate) return keys[vk];
    }

    /// <summary>1つの割り当てが押されているか。player はパッドを選ぶのに使う（キーは誰でも）</summary>
    public bool IsActive(InputBinding b, int player)
    {
        lock (gate)
        {
            if (b.Kind == "Key")
            {
                if (b.Code <= 0 || b.Code > 255 || !keys[b.Code]) return false;
                return b.With == 0 || (b.With is > 0 and < 256 && keys[b.With]);
            }
            if (player < 0 || player >= pads.Count) return false;
            var p = pads[player];
            short threshold = (short)(32767 * Math.Clamp(DeadzoneRatio, 0.05, 0.95));
            switch (b.Kind)
            {
                case "Button":
                    return b.Code is >= 0 and < MaxButtons && p.IsController && p.Buttons[b.Code];
                case "Axis":
                    if (!p.IsController || b.Code is < 0 or >= MaxAxes) return false;
                    return b.Dir >= 0 ? p.Axes[b.Code] > threshold : p.Axes[b.Code] < -threshold;
                case "JoyButton":
                    return b.Code >= 0 && b.Code < p.JoyButtons.Length && p.JoyButtons[b.Code];
                case "JoyAxis":
                    if (b.Code < 0 || b.Code >= p.JoyAxes.Length) return false;
                    return b.Dir >= 0 ? p.JoyAxes[b.Code] > threshold : p.JoyAxes[b.Code] < -threshold;
                case "JoyHat":
                {
                    int hat = b.Code >> 4;
                    return hat >= 0 && hat < p.JoyHats.Length && (p.JoyHats[hat] & b.Dir) != 0;
                }
            }
            return false;
        }
    }

    public bool AnyActive(IEnumerable<InputBinding>? list, int player) => list != null && list.Any(b => IsActive(b, player));

    /// <summary>スティックの値（-32768〜32767）。デッドゾーン内は0</summary>
    public short Stick(int player, int axis)
    {
        lock (gate)
        {
            if (player < 0 || player >= pads.Count || !pads[player].IsController || axis is < 0 or >= MaxAxes) return 0;
            var v = pads[player].Axes[axis];
            var dz = 32767 * Math.Clamp(DeadzoneRatio, 0, 0.95);
            if (Math.Abs((int)v) < dz) return 0;
            return v;
        }
    }

    public void Rumble(int player, ushort low, ushort high, uint ms)
    {
        var s = sdl;
        if (s == null) return;
        Pad? p;
        lock (gate) p = player >= 0 && player < pads.Count ? pads[player] : null;
        if (p != null && p.Controller != null) s.GameControllerRumble(p.Controller, low, high, ms);
    }

    // ---------- 割り当て画面用：次に押された入力を取る ----------

    /// <summary>今押されているものを除き、新たに押された入力を1つ返す（無ければ null）</summary>
    public sealed class Capture
    {
        private readonly InputManager m;
        private readonly HashSet<string> baseline;

        internal Capture(InputManager m)
        {
            this.m = m;
            baseline = m.ActiveSignatures().Select(x => x.sig).ToHashSet();
        }

        public InputBinding? Poll()
        {
            var now = m.ActiveSignatures();
            // 離されたものは基準から外す（同じボタンを押し直しても取れるように）
            baseline.IntersectWith(now.Select(x => x.sig));
            var fresh = now.FirstOrDefault(x => !baseline.Contains(x.sig));
            if (fresh.binding == null) return null;
            var b = fresh.binding;
            if (b.Kind == "Key")
            {
                // 押しっぱなしのキーがあれば同時押しとして扱う（修飾キーを優先）
                var held = now.Where(x => x.binding.Kind == "Key" && x.binding.Code != b.Code && baseline.Contains(x.sig))
                              .Select(x => x.binding.Code).ToList();
                var modifier = held.FirstOrDefault(c => c is 0x10 or 0x11 or 0x12 or 0xA0 or 0xA1 or 0xA2 or 0xA3 or 0xA4 or 0xA5);
                if (modifier == 0 && held.Count > 0) modifier = held[0];
                b.With = modifier;
            }
            return b;
        }
    }

    public Capture StartCapture() => new(this);

    private List<(string sig, InputBinding binding)> ActiveSignatures()
    {
        var list = new List<(string, InputBinding)>();
        lock (gate)
        {
            for (int i = 1; i < 256; i++)
            {
                // 左右を区別しない Shift/Ctrl/Alt（0x10〜0x12）は、左右別のコードと重複するので除く
                if (i is 0x10 or 0x11 or 0x12) continue;
                if (keys[i]) list.Add(("K" + i, InputBinding.Key(i)));
            }
            short threshold = 32767 / 2;
            for (int pi = 0; pi < pads.Count; pi++)
            {
                var p = pads[pi];
                if (p.IsController)
                {
                    for (int i = 0; i < MaxButtons; i++)
                        if (p.Buttons[i]) list.Add(($"B{pi}:{i}", InputBinding.Button(i)));
                    for (int i = 0; i < MaxAxes; i++)
                    {
                        if (p.Axes[i] > threshold) list.Add(($"A{pi}:{i}+", InputBinding.Axis(i, 1)));
                        // トリガーは負側に振れないので、負側は取らない
                        else if (p.Axes[i] < -threshold && i < 4) list.Add(($"A{pi}:{i}-", InputBinding.Axis(i, -1)));
                    }
                }
                else
                {
                    for (int i = 0; i < p.JoyButtons.Length; i++)
                        if (p.JoyButtons[i]) list.Add(($"JB{pi}:{i}", new InputBinding { Kind = "JoyButton", Code = i }));
                    for (int i = 0; i < p.JoyAxes.Length; i++)
                    {
                        if (p.JoyAxes[i] > threshold) list.Add(($"JA{pi}:{i}+", new InputBinding { Kind = "JoyAxis", Code = i, Dir = 1 }));
                        else if (p.JoyAxes[i] < -threshold) list.Add(($"JA{pi}:{i}-", new InputBinding { Kind = "JoyAxis", Code = i, Dir = -1 }));
                    }
                    for (int i = 0; i < p.JoyHats.Length; i++)
                        foreach (var d in new[] { 1, 2, 4, 8 })
                            if ((p.JoyHats[i] & d) != 0) list.Add(($"JH{pi}:{i}:{d}", new InputBinding { Kind = "JoyHat", Code = i << 4, Dir = d }));
                }
            }
        }
        return list;
    }

    // ---------- 表示用 ----------

    private static readonly string[] PadButtonNames =
        { "A", "B", "X", "Y", "Back", "Guide", "Start", "LS", "RS", "LB", "RB", "↑", "↓", "←", "→", "Misc", "P1", "P2", "P3", "P4", "Touchpad" };
    private static readonly string[] PadAxisNames = { "左スティック横", "左スティック縦", "右スティック横", "右スティック縦", "LT", "RT" };

    public static string Describe(InputBinding b)
    {
        switch (b.Kind)
        {
            case "Key":
                var name = KeyName(b.Code);
                return b.With != 0 ? KeyName(b.With) + "+" + name : name;
            case "Button":
                return "🎮 " + (b.Code >= 0 && b.Code < PadButtonNames.Length ? PadButtonNames[b.Code] : "#" + b.Code);
            case "Axis":
                if (b.Code is 4 or 5) return "🎮 " + PadAxisNames[b.Code];
                var axis = b.Code >= 0 && b.Code < PadAxisNames.Length ? PadAxisNames[b.Code] : "#" + b.Code;
                return "🎮 " + axis + (b.Dir >= 0 ? "+" : "−");
            case "JoyButton":
                return "🕹 " + (b.Code + 1);
            case "JoyAxis":
                return "🕹 軸" + (b.Code + 1) + (b.Dir >= 0 ? "+" : "−");
            case "JoyHat":
                return "🕹 " + b.Dir switch { 1 => "↑", 2 => "→", 4 => "↓", 8 => "←", _ => "?" };
        }
        return "?";
    }

    private static string KeyName(int vk)
    {
        try
        {
            var key = System.Windows.Input.KeyInterop.KeyFromVirtualKey(vk);
            return key switch
            {
                System.Windows.Input.Key.Return => "Enter",
                System.Windows.Input.Key.Escape => "Esc",
                System.Windows.Input.Key.Up => "↑",
                System.Windows.Input.Key.Down => "↓",
                System.Windows.Input.Key.Left => "←",
                System.Windows.Input.Key.Right => "→",
                System.Windows.Input.Key.LeftShift => "左Shift",
                System.Windows.Input.Key.RightShift => "右Shift",
                System.Windows.Input.Key.LeftCtrl => "左Ctrl",
                System.Windows.Input.Key.RightCtrl => "右Ctrl",
                System.Windows.Input.Key.LeftAlt or System.Windows.Input.Key.System => "Alt",
                System.Windows.Input.Key.RightAlt => "右Alt",
                _ => key == System.Windows.Input.Key.None ? "VK" + vk : key.ToString()
            };
        }
        catch
        {
            return "VK" + vk;
        }
    }
}
