// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Reflection;

namespace JPEmuRenamer.Models;

/// <summary>
/// 内蔵コアで遊ぶときの設定の1層分。「全体→機種→ゲーム」の順に重ね、null の項目は下の層の値を使う。
/// </summary>
public class PlaySettings
{
    // ---------- 画面 ----------
    /// <summary>Window / Fullscreen</summary>
    public string? WindowMode { get; set; }
    /// <summary>1 / 2 / 3 / Fit</summary>
    public string? ScaleMode { get; set; }
    /// <summary>Original / 4:3 / Stretch</summary>
    public string? Aspect { get; set; }
    public bool? IntegerScale { get; set; }
    /// <summary>Sharp / Smooth</summary>
    public string? Filter { get; set; }
    /// <summary>None / Scanlines / Crt</summary>
    public string? Shader { get; set; }
    public bool? VSync { get; set; }
    /// <summary>全画面時の画面モード。"" はデスクトップのまま、"1920x1080@60" の形で指定</summary>
    public string? FullscreenMode { get; set; }

    // ---------- 音声 ----------
    public int? Volume { get; set; }
    public bool? Mute { get; set; }
    /// <summary>出力デバイスのID（"" は既定のデバイス）</summary>
    public string? AudioDevice { get; set; }
    public int? SampleRate { get; set; }
    public bool? Exclusive { get; set; }
    public int? LatencyMs { get; set; }
    /// <summary>音ずれ補正（再生速度をわずかに調整して音切れを防ぐ）</summary>
    public bool? RateControl { get; set; }

    // ---------- 保存先 ----------
    public string? SaveDir { get; set; }
    public string? StateDir { get; set; }
    public string? ScreenshotDir { get; set; }
    /// <summary>Shared / PerSystem / PerGame</summary>
    public string? FolderMode { get; set; }
    public bool? SaveNextToRom { get; set; }
    public int? StateSlots { get; set; }

    // ---------- 入力 ----------
    public int? Deadzone { get; set; }
    /// <summary>連射の速さ（何フレームごとに押す/離すを切り替えるか）</summary>
    public int? TurboRate { get; set; }
    public InputProfile? Input { get; set; }

    // ---------- コア固有 ----------
    public Dictionary<string, string>? CoreOptions { get; set; }

    public static PlaySettings Defaults() => new()
    {
        WindowMode = "Window",
        ScaleMode = "3",
        Aspect = "Original",
        IntegerScale = false,
        Filter = "Sharp",
        Shader = "None",
        VSync = true,
        FullscreenMode = "",
        Volume = 80,
        Mute = false,
        AudioDevice = "",
        SampleRate = 48000,
        Exclusive = false,
        LatencyMs = 64,
        RateControl = true,
        SaveDir = "",
        StateDir = "",
        ScreenshotDir = "",
        FolderMode = "PerSystem",
        SaveNextToRom = false,
        StateSlots = 10,
        Deadzone = 25,
        TurboRate = 3,
        Input = InputProfile.CreateDefault(),
        CoreOptions = new Dictionary<string, string>()
    };

    /// <summary>重ね合わせの対象になる項目（CoreOptions は別扱い）</summary>
    public static readonly PropertyInfo[] ScalarProperties = typeof(PlaySettings).GetProperties()
        .Where(p => p.CanWrite && p.Name != nameof(CoreOptions)).ToArray();

    /// <summary>下の層から順に重ねた、実際に使う設定を作る</summary>
    public static PlaySettings Resolve(params PlaySettings?[] layersFromBottom)
    {
        var result = Defaults();
        foreach (var layer in layersFromBottom)
        {
            if (layer == null) continue;
            foreach (var p in ScalarProperties)
            {
                var v = p.GetValue(layer);
                if (v != null) p.SetValue(result, v is InputProfile ip ? ip.Clone() : v);
            }
            if (layer.CoreOptions != null)
                foreach (var kv in layer.CoreOptions) result.CoreOptions![kv.Key] = kv.Value;
        }
        return result;
    }

    public PlaySettings Clone()
    {
        var c = (PlaySettings)MemberwiseClone();
        c.Input = Input?.Clone();
        c.CoreOptions = CoreOptions == null ? null : new Dictionary<string, string>(CoreOptions);
        return c;
    }

    [System.Text.Json.Serialization.JsonIgnore]
    public bool IsEmpty =>
        ScalarProperties.All(p => p.GetValue(this) == null) && (CoreOptions == null || CoreOptions.Count == 0);
}

/// <summary>1つの入力（キー、パッドのボタン・スティック・ハット）</summary>
public class InputBinding
{
    /// <summary>Key / Button / Axis / JoyButton / JoyAxis / JoyHat</summary>
    public string Kind { get; set; } = "Key";
    /// <summary>キーは仮想キーコード、パッドはボタン・軸・ハットの番号</summary>
    public int Code { get; set; }
    /// <summary>軸は +1/-1、ハットは方向のビット（1=上 2=右 4=下 8=左）</summary>
    public int Dir { get; set; }
    /// <summary>同時押し（キーボードのみ）。0 なら単独</summary>
    public int With { get; set; }

    public InputBinding Clone() => (InputBinding)MemberwiseClone();

    public static InputBinding Key(int vk, int with = 0) => new() { Kind = "Key", Code = vk, With = with };
    public static InputBinding Button(int b) => new() { Kind = "Button", Code = b };
    public static InputBinding Axis(int a, int dir) => new() { Kind = "Axis", Code = a, Dir = dir };

    public bool SameAs(InputBinding o) => Kind == o.Kind && Code == o.Code && Dir == o.Dir && With == o.With;
}

/// <summary>プレイヤー1人分の割り当て</summary>
public class PlayerMap
{
    /// <summary>操作名（Up, A, Start など）→ 割り当て</summary>
    public Dictionary<string, List<InputBinding>> Buttons { get; set; } = new();
    /// <summary>連射にする操作</summary>
    public List<string> Turbo { get; set; } = new();

    public PlayerMap Clone() => new()
    {
        Buttons = Buttons.ToDictionary(kv => kv.Key, kv => kv.Value.Select(b => b.Clone()).ToList()),
        Turbo = new List<string>(Turbo)
    };
}

/// <summary>入力の割り当て一式（プレイヤー1〜4とホットキー）</summary>
public class InputProfile
{
    public List<PlayerMap> Players { get; set; } = new();
    public Dictionary<string, List<InputBinding>> Hotkeys { get; set; } = new();

    /// <summary>libretro の RetroPad の操作（表示順）</summary>
    public static readonly string[] Controls =
        { "Up", "Down", "Left", "Right", "A", "B", "X", "Y", "L", "R", "L2", "R2", "L3", "R3", "Select", "Start" };

    public static readonly string[] HotkeyNames =
        { "Menu", "FastForward", "SaveState", "LoadState", "SlotPrev", "SlotNext", "Screenshot", "Fullscreen", "Pause", "Reset", "Exit" };

    // 仮想キーコード
    private const int VK_TAB = 0x09, VK_RETURN = 0x0D, VK_SHIFT = 0x10, VK_ESCAPE = 0x1B, VK_SPACE = 0x20,
        VK_LEFT = 0x25, VK_UP = 0x26, VK_RIGHT = 0x27, VK_DOWN = 0x28,
        VK_F1 = 0x70, VK_F2 = 0x71, VK_F4 = 0x73, VK_F6 = 0x75, VK_F7 = 0x76, VK_F10 = 0x79, VK_F11 = 0x7A, VK_F12 = 0x7B,
        VK_RSHIFT = 0xA1;

    // SDL GameController のボタン・軸の番号
    public const int PadA = 0, PadB = 1, PadX = 2, PadY = 3, PadBack = 4, PadGuide = 5, PadStart = 6, PadLStick = 7, PadRStick = 8,
        PadLShoulder = 9, PadRShoulder = 10, PadUp = 11, PadDown = 12, PadLeft = 13, PadRight = 14;
    public const int AxisLeftX = 0, AxisLeftY = 1, AxisRightX = 2, AxisRightY = 3, AxisTriggerLeft = 4, AxisTriggerRight = 5;

    public static InputProfile CreateDefault()
    {
        var p = new InputProfile();
        for (int i = 0; i < 4; i++) p.Players.Add(DefaultPlayer(i == 0));
        p.Hotkeys["Menu"] = new() { InputBinding.Key(VK_F1), InputBinding.Button(PadGuide) };
        p.Hotkeys["FastForward"] = new() { InputBinding.Key(VK_TAB) };
        p.Hotkeys["SaveState"] = new() { InputBinding.Key(VK_F2) };
        p.Hotkeys["LoadState"] = new() { InputBinding.Key(VK_F4) };
        p.Hotkeys["SlotPrev"] = new() { InputBinding.Key(VK_F6) };
        p.Hotkeys["SlotNext"] = new() { InputBinding.Key(VK_F7) };
        p.Hotkeys["Screenshot"] = new() { InputBinding.Key(VK_F12) };
        p.Hotkeys["Fullscreen"] = new() { InputBinding.Key(VK_F11), InputBinding.Key(VK_RETURN, 0x12) };
        p.Hotkeys["Pause"] = new() { InputBinding.Key('P') };
        p.Hotkeys["Reset"] = new() { InputBinding.Key(VK_F10) };
        p.Hotkeys["Exit"] = new() { InputBinding.Key(VK_ESCAPE) };
        return p;
    }

    /// <summary>機種共通の標準配置（任天堂系の並び：右=A、下=B）</summary>
    private static PlayerMap DefaultPlayer(bool withKeyboard)
    {
        var m = new PlayerMap();
        void Add(string control, params InputBinding[] b) => m.Buttons[control] = b.ToList();

        Add("Up", InputBinding.Button(PadUp), InputBinding.Axis(AxisLeftY, -1));
        Add("Down", InputBinding.Button(PadDown), InputBinding.Axis(AxisLeftY, 1));
        Add("Left", InputBinding.Button(PadLeft), InputBinding.Axis(AxisLeftX, -1));
        Add("Right", InputBinding.Button(PadRight), InputBinding.Axis(AxisLeftX, 1));
        Add("A", InputBinding.Button(PadB));
        Add("B", InputBinding.Button(PadA));
        Add("X", InputBinding.Button(PadY));
        Add("Y", InputBinding.Button(PadX));
        Add("L", InputBinding.Button(PadLShoulder));
        Add("R", InputBinding.Button(PadRShoulder));
        Add("L2", InputBinding.Axis(AxisTriggerLeft, 1));
        Add("R2", InputBinding.Axis(AxisTriggerRight, 1));
        Add("L3", InputBinding.Button(PadLStick));
        Add("R3", InputBinding.Button(PadRStick));
        Add("Select", InputBinding.Button(PadBack));
        Add("Start", InputBinding.Button(PadStart));

        if (withKeyboard)
        {
            m.Buttons["Up"].Add(InputBinding.Key(VK_UP));
            m.Buttons["Down"].Add(InputBinding.Key(VK_DOWN));
            m.Buttons["Left"].Add(InputBinding.Key(VK_LEFT));
            m.Buttons["Right"].Add(InputBinding.Key(VK_RIGHT));
            m.Buttons["A"].Add(InputBinding.Key('X'));
            m.Buttons["B"].Add(InputBinding.Key('Z'));
            m.Buttons["X"].Add(InputBinding.Key('S'));
            m.Buttons["Y"].Add(InputBinding.Key('A'));
            m.Buttons["L"].Add(InputBinding.Key('Q'));
            m.Buttons["R"].Add(InputBinding.Key('W'));
            m.Buttons["L2"].Add(InputBinding.Key('E'));
            m.Buttons["R2"].Add(InputBinding.Key('R'));
            m.Buttons["Select"].Add(InputBinding.Key(VK_RSHIFT));
            m.Buttons["Start"].Add(InputBinding.Key(VK_RETURN));
        }
        return m;
    }

    public InputProfile Clone() => new()
    {
        Players = Players.Select(p => p.Clone()).ToList(),
        Hotkeys = Hotkeys.ToDictionary(kv => kv.Key, kv => kv.Value.Select(b => b.Clone()).ToList())
    };
}

/// <summary>ゲーム単位の上書き（data\games\{CRC}.json）</summary>
public class GameOverrides
{
    public PlaySettings Play { get; set; } = new();
}
