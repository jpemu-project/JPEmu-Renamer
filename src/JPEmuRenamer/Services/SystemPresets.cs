// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using JPEmuRenamer.Models;

namespace JPEmuRenamer.Services;

/// <summary>機種追加時の候補（拡張子・アイコン・色を自動で入れる）</summary>
public record SystemPreset(string Name, string Extensions, string Icon, string Color, bool DisplayRenameOnly = false)
{
    public override string ToString() => Name;
}

public static class SystemPresets
{
    public static readonly IReadOnlyList<SystemPreset> All = new List<SystemPreset>
    {
        new("ファミコン", ".nes;.zip", "cart", "#F06B70"),
        new("ディスクシステム", ".fds;.zip", "diskcard", "#F5A524"),
        new("スーパーファミコン", ".sfc;.smc;.zip", "cart", "#A78BFA"),
        new("NINTENDO64", ".z64;.n64;.v64;.zip", "cart", "#5B9BFF"),
        new("ゲームボーイ", ".gb;.zip", "handV", "#7CC96A"),
        new("ゲームボーイカラー", ".gbc;.zip", "handV", "#4FC3D9"),
        new("ゲームボーイアドバンス", ".gba;.zip", "handH", "#8B9CFF"),
        new("バーチャルボーイ", ".vb;.zip", "pad", "#E8D35C"),
        new("ニンテンドーDS", ".nds;.zip", "dual", "#C58BF0"),
        new("SG-1000", ".sg;.zip", "cart", "#B8C2D1"),
        new("マスターシステム", ".sms;.zip", "cart", "#F06B70"),
        new("メガドライブ", ".md;.gen;.bin;.zip", "cart", "#5B9BFF"),
        new("ゲームギア", ".gg;.zip", "handH", "#4FC3D9"),
        new("メガCD", ".cue;.chd", "disc", "#8B9CFF"),
        new("セガサターン", ".cue;.chd", "disc", "#B8C2D1"),
        new("PCエンジン", ".pce;.zip", "cart", "#FF8A5B"),
        new("PCエンジン CD-ROM²", ".cue;.chd", "disc", "#FF8A5B"),
        new("ワンダースワン", ".ws;.zip", "handH", "#B8C2D1"),
        new("ワンダースワンカラー", ".wsc;.zip", "handH", "#4FC3D9"),
        new("ネオジオポケット", ".ngp;.zip", "handH", "#E8D35C"),
        new("ネオジオポケットカラー", ".ngc;.zip", "handH", "#F5A524"),
        new("ネオジオ", ".zip", "arcade", "#FF8A5B", true),
        new("プレイステーション", ".cue;.chd;.pbp", "disc", "#B8C2D1"),
        new("その他", ".zip", "pad", "#E8D35C"),
    };

    public static readonly IReadOnlyList<string> IconKinds = new[]
    {
        "cart", "diskcard", "disc", "handV", "handH", "dual", "arcade", "pc", "pad"
    };

    public static SystemConfig DefaultFamicom() => new()
    {
        Name = "ファミコン",
        IconKind = "cart",
        Color = "#F06B70",
        Extensions = new List<string> { ".nes", ".zip" },
        HeaderMode = "Auto",
        LaunchMode = "Core"
    };

    public static List<string> ParseExtensions(string text) =>
        text.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLowerInvariant())
            .Select(e => e.StartsWith('.') ? e : "." + e)
            .Distinct()
            .ToList();
}
