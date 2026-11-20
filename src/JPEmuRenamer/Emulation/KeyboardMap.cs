// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Runtime.InteropServices;
using System.Text;

namespace JPEmuRenamer.Emulation;

/// <summary>
/// Windows の仮想キー（VK）と libretro のキー番号（RETROK_*）の対応。パソコン系の機種でキーボードをそのまま使うときに使う。
/// 記号のキーは配列で位置が違うので、日本語キーボード（106/109）と英語キーボード（101/104）で対応を変える。
/// </summary>
public static class KeyboardMap
{
    // libretro の修飾キーの印（retro_mod）
    public const ushort ModShift = 0x01, ModCtrl = 0x02, ModAlt = 0x04, ModMeta = 0x08,
                        ModNumLock = 0x10, ModCapsLock = 0x20, ModScrollLock = 0x40;

    private static readonly Dictionary<int, uint> vkToRetro = Build();
    private static readonly Dictionary<uint, int> retroToVk = vkToRetro.GroupBy(kv => kv.Value).ToDictionary(g => g.Key, g => g.First().Key);

    /// <summary>VK → RETROK（対応が無ければ 0 = RETROK_UNKNOWN）</summary>
    public static uint ToRetro(int vk) => vkToRetro.TryGetValue(vk, out var r) ? r : 0;

    /// <summary>RETROK → VK（対応が無ければ 0）</summary>
    public static int ToVk(uint retro) => retroToVk.TryGetValue(retro, out var v) ? v : 0;

    /// <summary>キーボードの対応表に載っている VK（押された・離されたを調べる対象）</summary>
    public static IEnumerable<int> MappedKeys => vkToRetro.Keys;

    [DllImport("user32.dll")] private static extern IntPtr GetKeyboardLayout(uint thread);
    [DllImport("user32.dll")] private static extern short GetKeyState(int vk);
    [DllImport("user32.dll")] private static extern uint MapVirtualKey(uint code, uint mapType);
    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int ToUnicodeEx(uint vk, uint scan, byte[] state, StringBuilder buf, int bufSize, uint flags, IntPtr layout);

    private static bool IsJapaneseLayout => ((long)GetKeyboardLayout(0) & 0xFFFF) == 0x0411;

    private static Dictionary<int, uint> Build()
    {
        var m = new Dictionary<int, uint>
        {
            [0x08] = 8,   // Backspace
            [0x09] = 9,   // Tab
            [0x0C] = 12,  // Clear
            [0x0D] = 13,  // Enter
            [0x13] = 19,  // Pause
            [0x1B] = 27,  // Esc
            [0x20] = 32,  // Space
            [0x2E] = 127, // Delete
            [0x26] = 273, [0x28] = 274, [0x27] = 275, [0x25] = 276, // ↑ ↓ → ←
            [0x2D] = 277, [0x24] = 278, [0x23] = 279, [0x21] = 280, [0x22] = 281, // Insert Home End PageUp PageDown
            [0x6E] = 266, [0x6F] = 267, [0x6A] = 268, [0x6D] = 269, [0x6B] = 270, // テンキーの . / * - +
            [0x90] = 300, [0x14] = 301, [0x91] = 302, // NumLock CapsLock ScrollLock
            [0xA1] = 303, [0xA0] = 304, [0xA3] = 305, [0xA2] = 306, [0xA5] = 307, [0xA4] = 308, // 右左 Shift Ctrl Alt
            [0x5B] = 311, [0x5C] = 312, [0x5D] = 319, // 左右 Windows、アプリケーション
            [0x2C] = 316, // PrintScreen
            [0xBC] = 44, [0xBD] = 45, [0xBE] = 46, [0xBF] = 47, // , - . /
            [0xDB] = 91, [0xDD] = 93, // [ ]
        };
        for (int i = 0; i < 26; i++) m['A' + i] = (uint)('a' + i);
        for (int i = 0; i < 10; i++) m['0' + i] = (uint)('0' + i);
        for (int i = 0; i < 10; i++) m[0x60 + i] = (uint)(256 + i);   // テンキーの 0〜9
        for (int i = 0; i < 15; i++) m[0x70 + i] = (uint)(282 + i);   // F1〜F15

        if (IsJapaneseLayout)
        {
            m[0xBA] = 58;  // :（＊）
            m[0xBB] = 59;  // ;（＋）
            m[0xC0] = 64;  // @（｀）
            m[0xDE] = 94;  // ^（～）
            m[0xDC] = 92;  // ￥（｜）
            m[0xE2] = 95;  // ＼（＿）ろ
        }
        else
        {
            m[0xBA] = 59;  // ;
            m[0xBB] = 61;  // =
            m[0xC0] = 96;  // `
            m[0xDE] = 39;  // '
            m[0xDC] = 92;  // \
            m[0xE2] = 92;  // 102キーの \
        }
        return m;
    }

    /// <summary>いま押されているキー（keys[vk]）から、libretro の修飾キーの印を作る</summary>
    public static ushort Modifiers(Func<int, bool> down)
    {
        ushort mods = 0;
        if (down(0xA0) || down(0xA1)) mods |= ModShift;
        if (down(0xA2) || down(0xA3)) mods |= ModCtrl;
        if (down(0xA4) || down(0xA5)) mods |= ModAlt;
        if (down(0x5B) || down(0x5C)) mods |= ModMeta;
        if ((GetKeyState(0x90) & 1) != 0) mods |= ModNumLock;
        if ((GetKeyState(0x14) & 1) != 0) mods |= ModCapsLock;
        if ((GetKeyState(0x91) & 1) != 0) mods |= ModScrollLock;
        return mods;
    }

    /// <summary>押されたキーが打つ文字（Shift などを考えた Unicode。文字にならないキーは 0）</summary>
    public static uint Character(int vk, Func<int, bool> down)
    {
        try
        {
            var state = new byte[256];
            for (int i = 1; i < 256; i++) if (down(i)) state[i] = 0x80;
            if (down(0xA0) || down(0xA1)) state[0x10] = 0x80;
            if (down(0xA2) || down(0xA3)) state[0x11] = 0x80;
            if ((GetKeyState(0x14) & 1) != 0) state[0x14] |= 1;
            var sb = new StringBuilder(4);
            // flags 0x4：キーボードの状態（デッドキーなど）を変えない
            int n = ToUnicodeEx((uint)vk, MapVirtualKey((uint)vk, 0), state, sb, sb.Capacity, 0x4, GetKeyboardLayout(0));
            return n == 1 && sb[0] >= 0x20 && sb[0] != 0x7F ? (uint)sb[0] : 0u;
        }
        catch { return 0; }
    }
}
