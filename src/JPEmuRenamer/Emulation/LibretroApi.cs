// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Runtime.InteropServices;

namespace JPEmuRenamer.Emulation;

// libretro.h の必要な部分だけを C# に写したもの。
// 構造体のレイアウトは x64 Windows の C と同じになるよう、bool は byte で持つ。

public static class Retro
{
    public const uint ApiVersion = 1;

    // 環境コマンド
    public const uint EnvExperimental = 0x10000;
    public const uint EnvPrivate = 0x20000;
    public const uint SetRotation = 1;
    public const uint GetOverscan = 2;
    public const uint GetCanDupe = 3;
    public const uint SetMessage = 6;
    public const uint Shutdown = 7;
    public const uint SetPerformanceLevel = 8;
    public const uint GetSystemDirectory = 9;
    public const uint SetPixelFormat = 10;
    public const uint SetInputDescriptors = 11;
    public const uint SetKeyboardCallback = 12;
    public const uint SetDiskControlInterface = 13;
    public const uint SetHwRender = 14;
    public const uint GetVariable = 15;
    public const uint SetVariables = 16;
    public const uint GetVariableUpdate = 17;
    public const uint SetSupportNoGame = 18;
    public const uint GetLibretroPath = 19;
    public const uint SetFrameTimeCallback = 21;
    public const uint SetAudioCallback = 22;
    public const uint GetRumbleInterface = 23;
    public const uint GetInputDeviceCapabilities = 24;
    public const uint GetLogInterface = 27;
    public const uint GetPerfInterface = 28;
    public const uint GetCoreAssetsDirectory = 30;
    public const uint GetSaveDirectory = 31;
    public const uint SetSystemAvInfo = 32;
    public const uint SetProcAddressCallback = 33;
    public const uint SetSubsystemInfo = 34;
    public const uint SetControllerInfo = 35;
    public const uint SetMemoryMaps = 36;
    public const uint SetGeometry = 37;
    public const uint GetUsername = 38;
    public const uint GetLanguage = 39;
    public const uint GetCurrentSoftwareFramebuffer = 40;
    public const uint GetHwRenderInterface = 41;
    public const uint SetSupportAchievements = 42;
    public const uint SetHwRenderContextNegotiationInterface = 43;
    public const uint SetSerializationQuirks = 44;
    public const uint GetAudioVideoEnable = 47;
    public const uint GetFastForwarding = 49;
    public const uint GetTargetRefreshRate = 50;
    public const uint GetInputBitmasks = 51;
    public const uint GetCoreOptionsVersion = 52;
    public const uint SetCoreOptions = 53;
    public const uint SetCoreOptionsIntl = 54;
    public const uint SetCoreOptionsDisplay = 55;
    public const uint GetPreferredHwRender = 56;
    public const uint GetDiskControlInterfaceVersion = 57;
    public const uint GetMessageInterfaceVersion = 59;
    public const uint SetMessageExt = 60;
    public const uint GetInputMaxUsers = 61;
    public const uint SetAudioBufferStatusCallback = 62;
    public const uint SetMinimumAudioLatency = 63;
    public const uint SetFastForwardingOverride = 64;
    public const uint SetContentInfoOverride = 65;
    public const uint GetGameInfoExt = 66;
    public const uint SetCoreOptionsV2 = 67;
    public const uint SetCoreOptionsV2Intl = 68;
    public const uint SetCoreOptionsUpdateDisplayCallback = 69;
    public const uint SetVariable = 70;

    // ピクセル形式
    public const uint Pixel0Rgb1555 = 0;
    public const uint PixelXrgb8888 = 1;
    public const uint PixelRgb565 = 2;

    // 入力デバイス
    public const uint DeviceNone = 0;
    public const uint DeviceJoypad = 1;
    public const uint DeviceMouse = 2;
    public const uint DeviceKeyboard = 3;
    public const uint DeviceLightgun = 4;
    public const uint DeviceAnalog = 5;
    public const uint DevicePointer = 6;
    public const uint DeviceIdJoypadMask = 256;

    // ジョイパッドのボタン
    public const int JoyB = 0, JoyY = 1, JoySelect = 2, JoyStart = 3, JoyUp = 4, JoyDown = 5, JoyLeft = 6, JoyRight = 7,
        JoyA = 8, JoyX = 9, JoyL = 10, JoyR = 11, JoyL2 = 12, JoyR2 = 13, JoyL3 = 14, JoyR3 = 15;

    // アナログ
    public const uint AnalogLeft = 0, AnalogRight = 1, AnalogX = 0, AnalogY = 1;

    // メモリ
    public const uint MemorySaveRam = 0;
    public const uint MemoryRtc = 1;

    // ハードウェア描画
    public const uint HwContextNone = 0;
    public const uint HwContextOpenGl = 1;
    public const uint HwContextOpenGlEs2 = 2;
    public const uint HwContextOpenGlCore = 3;
    public const uint HwContextOpenGlEs3 = 4;
    public const uint HwContextOpenGlEsVersion = 5;
    public const uint HwContextVulkan = 6;
    public static readonly IntPtr HwFrameBufferValid = new(-1);

    // 言語
    public const uint LanguageEnglish = 0;
    public const uint LanguageJapanese = 1;

    // ログ
    public const int LogDebug = 0, LogInfo = 1, LogWarn = 2, LogError = 3;

    // 振動
    public const int RumbleStrong = 0, RumbleWeak = 1;
}

[StructLayout(LayoutKind.Sequential)]
public struct RetroSystemInfo
{
    public IntPtr LibraryName;
    public IntPtr LibraryVersion;
    public IntPtr ValidExtensions;
    public byte NeedFullpath;
    public byte BlockExtract;
}

[StructLayout(LayoutKind.Sequential)]
public struct RetroGameGeometry
{
    public uint BaseWidth;
    public uint BaseHeight;
    public uint MaxWidth;
    public uint MaxHeight;
    public float AspectRatio;
}

[StructLayout(LayoutKind.Sequential)]
public struct RetroSystemTiming
{
    public double Fps;
    public double SampleRate;
}

[StructLayout(LayoutKind.Sequential)]
public struct RetroSystemAvInfo
{
    public RetroGameGeometry Geometry;
    public RetroSystemTiming Timing;
}

[StructLayout(LayoutKind.Sequential)]
public struct RetroGameInfo
{
    public IntPtr Path;
    public IntPtr Data;
    public UIntPtr Size;
    public IntPtr Meta;
}

[StructLayout(LayoutKind.Sequential)]
public struct RetroVariable
{
    public IntPtr Key;
    public IntPtr Value;
}

[StructLayout(LayoutKind.Sequential)]
public struct RetroMessage
{
    public IntPtr Msg;
    public uint Frames;
}

[StructLayout(LayoutKind.Sequential)]
public struct RetroMessageExt
{
    public IntPtr Msg;
    public uint Duration;
    public uint Priority;
    public int Level;
    public int Target;
    public int Type;
    public sbyte Progress;
}

[StructLayout(LayoutKind.Sequential)]
public struct RetroHwRenderCallback
{
    public uint ContextType;
    public IntPtr ContextReset;
    public IntPtr GetCurrentFramebuffer;
    public IntPtr GetProcAddress;
    public byte Depth;
    public byte Stencil;
    public byte BottomLeftOrigin;
    public uint VersionMajor;
    public uint VersionMinor;
    public byte CacheContext;
    public IntPtr ContextDestroy;
    public byte DebugContext;
}

[StructLayout(LayoutKind.Sequential)]
public struct RetroGameInfoExt
{
    public IntPtr FullPath;
    public IntPtr ArchivePath;
    public IntPtr ArchiveFile;
    public IntPtr Dir;
    public IntPtr Name;
    public IntPtr Ext;
    public IntPtr Meta;
    public IntPtr Data;
    public UIntPtr Size;
    public byte FileInArchive;
    public byte Persistent;
}
