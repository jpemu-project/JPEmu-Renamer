// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using JPEmuRenamer.Models;

namespace JPEmuRenamer.Services;

/// <summary>機種追加時の候補（拡張子・アイコン・色を自動で入れる）。日本語以外の表示では英語名を使う</summary>
public record SystemPreset(string Name, string NameEn, string Extensions, string Icon, string Color, bool DisplayRenameOnly = false)
{
    public string DisplayName => Loc.Instance.Language == "ja" ? Name : NameEn;
    public override string ToString() => DisplayName;
}

public static class SystemPresets
{
    public static readonly IReadOnlyList<SystemPreset> All = new List<SystemPreset>
    {
        new("ファミコン", "Famicom / NES", ".nes;.zip", "cart", "#F06B70"),
        new("ディスクシステム", "Famicom Disk System", ".fds;.zip", "diskcard", "#F5A524"),
        new("スーパーファミコン", "Super Famicom / SNES", ".sfc;.smc;.zip", "cart", "#A78BFA"),
        new("NINTENDO64", "Nintendo 64", ".z64;.n64;.v64;.zip", "cart", "#5B9BFF"),
        new("ゲームボーイ", "Game Boy", ".gb;.zip", "handV", "#7CC96A"),
        new("ゲームボーイカラー", "Game Boy Color", ".gbc;.zip", "handV", "#4FC3D9"),
        new("ゲームボーイアドバンス", "Game Boy Advance", ".gba;.zip", "handH", "#8B9CFF"),
        new("バーチャルボーイ", "Virtual Boy", ".vb;.zip", "pad", "#E8D35C"),
        new("ニンテンドーDS", "Nintendo DS", ".nds;.zip", "dual", "#C58BF0"),
        new("SG-1000", "SG-1000", ".sg;.zip", "cart", "#B8C2D1"),
        new("マスターシステム", "Master System", ".sms;.zip", "cart", "#F06B70"),
        new("メガドライブ", "Mega Drive / Genesis", ".md;.gen;.bin;.zip", "cart", "#5B9BFF"),
        new("ゲームギア", "Game Gear", ".gg;.zip", "handH", "#4FC3D9"),
        new("メガCD", "Mega-CD / Sega CD", ".cue;.chd", "disc", "#8B9CFF"),
        new("セガサターン", "Sega Saturn", ".cue;.chd", "disc", "#B8C2D1"),
        new("PCエンジン", "PC Engine / TurboGrafx-16", ".pce;.zip", "cart", "#FF8A5B"),
        new("PCエンジン CD-ROM²", "PC Engine CD-ROM²", ".cue;.chd", "disc", "#FF8A5B"),
        new("ワンダースワン", "WonderSwan", ".ws;.zip", "handH", "#B8C2D1"),
        new("ワンダースワンカラー", "WonderSwan Color", ".wsc;.zip", "handH", "#4FC3D9"),
        new("ネオジオポケット", "Neo Geo Pocket", ".ngp;.zip", "handH", "#E8D35C"),
        new("ネオジオポケットカラー", "Neo Geo Pocket Color", ".ngc;.zip", "handH", "#F5A524"),
        new("ネオジオ", "Neo Geo", ".zip", "arcade", "#FF8A5B", true),
        new("プレイステーション", "PlayStation", ".cue;.chd;.pbp", "disc", "#B8C2D1"),
        new("その他", "Other", ".zip", "pad", "#E8D35C"),
    };

    /// <summary>候補の名前（日本語・英語のどちらでも）から、日本語の名前を返す</summary>
    public static string? JapaneseNameOf(string name) =>
        All.FirstOrDefault(p => p.Name == name || p.NameEn.Equals(name, StringComparison.OrdinalIgnoreCase))?.Name;

    public static readonly IReadOnlyList<string> IconKinds = new[]
    {
        "cart", "diskcard", "disc", "handV", "handH", "dual", "arcade", "pc", "pad"
    };

    public static List<string> ParseExtensions(string text) =>
        text.Split(new[] { ';', ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(e => e.Trim().ToLowerInvariant())
            .Select(e => e.StartsWith('.') ? e : "." + e)
            .Distinct()
            .ToList();
}
