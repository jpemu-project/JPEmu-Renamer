// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JPEmuRenamer.Emulation;

/// <summary>コアが使うBIOSなどのファイル（libretro のコア情報ファイルの firmware 欄）</summary>
public sealed record CoreFirmware(
    [property: JsonPropertyName("p")] string Path,
    [property: JsonPropertyName("d")] string Description,
    [property: JsonPropertyName("o")] bool Optional);

/// <summary>libretro 公式のコア情報ファイル（*.info）1つ分。tools/gen-coreinfo.ps1 で CoreInfo.json にまとめてアプリに入れてある</summary>
public sealed record CoreInfoEntry(
    [property: JsonPropertyName("n")] string Name,
    [property: JsonPropertyName("d")] string DisplayName,
    [property: JsonPropertyName("s")] string SystemName,
    [property: JsonPropertyName("x")] string[] Extensions,
    [property: JsonPropertyName("hw")] string HwApi,
    [property: JsonPropertyName("db")] string[] Databases,
    [property: JsonPropertyName("fw")] CoreFirmware[] Firmware)
{
    /// <summary>OpenGL で描画するコアか</summary>
    public bool UsesOpenGl => HwApi.Contains("OpenGL", StringComparison.OrdinalIgnoreCase);

    /// <summary>Vulkan・Direct3D だけで描画するコア（JPEmu は OpenGL にしか対応していないので動かない）</summary>
    public bool Unsupported => HwApi.Length > 0 && !UsesOpenGl;
}

public static class CoreInfoDb
{
    private sealed record Root([property: JsonPropertyName("cores")] CoreInfoEntry[] Cores);

    private static readonly Lazy<Dictionary<string, CoreInfoEntry>> Data = new(Load);

    private static Dictionary<string, CoreInfoEntry> Load()
    {
        try
        {
            using var s = typeof(CoreInfoDb).Assembly.GetManifestResourceStream("JPEmuRenamer.Emulation.CoreInfo.json");
            if (s == null) return new();
            var root = JsonSerializer.Deserialize<Root>(s);
            return (root?.Cores ?? Array.Empty<CoreInfoEntry>()).ToDictionary(c => c.Name, StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new();
        }
    }

    public static IReadOnlyCollection<CoreInfoEntry> All => Data.Value.Values;

    public static CoreInfoEntry? Get(string coreName) => Data.Value.GetValueOrDefault(coreName);
}
