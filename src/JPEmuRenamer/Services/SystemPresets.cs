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
        new("サテラビュー", "Satellaview", ".bs;.zip", "cart", "#A78BFA"),
        new("NINTENDO64", "Nintendo 64", ".z64;.n64;.v64;.zip", "cart", "#5B9BFF"),
        new("ゲームボーイ", "Game Boy", ".gb;.zip", "handV", "#7CC96A"),
        new("ゲームボーイカラー", "Game Boy Color", ".gbc;.zip", "handV", "#4FC3D9"),
        new("ゲームボーイアドバンス", "Game Boy Advance", ".gba;.zip", "handH", "#8B9CFF"),
        new("バーチャルボーイ", "Virtual Boy", ".vb;.zip", "pad", "#E8D35C"),
        new("ポケモンミニ", "Pokémon mini", ".min;.zip", "handV", "#F5A524"),
        new("ニンテンドーDS", "Nintendo DS", ".nds;.zip", "dual", "#C58BF0"),
        new("ニンテンドーDSiウェア", "Nintendo DSiWare", ".nds;.zip", "dual", "#8B9CFF"),
        new("ニンテンドー3DS", "Nintendo 3DS", ".3ds;.cci;.cxi;.3dsx", "dual", "#F06B70"),
        new("SG-1000", "SG-1000", ".sg;.zip", "cart", "#B8C2D1"),
        new("マスターシステム", "Master System", ".sms;.zip", "cart", "#F06B70"),
        new("メガドライブ", "Mega Drive / Genesis", ".md;.gen;.bin;.zip", "cart", "#5B9BFF"),
        new("キッズコンピュータ ピコ", "Sega Pico", ".md;.bin;.zip", "pad", "#7CC96A"),
        new("スーパー32X", "Super 32X", ".32x;.zip", "cart", "#E8D35C"),
        new("ゲームギア","Game Gear", ".gg;.zip", "handH", "#4FC3D9"),
        new("メガCD", "Mega-CD / Sega CD", ".cue;.chd", "disc", "#8B9CFF"),
        new("セガサターン", "Sega Saturn", ".cue;.chd", "disc", "#B8C2D1"),
        new("PCエンジン", "PC Engine / TurboGrafx-16", ".pce;.zip", "cart", "#FF8A5B"),
        new("PCエンジン スーパーグラフィックス", "PC Engine SuperGrafx", ".sgx;.pce;.zip", "cart", "#B8C2D1"),
        new("PCエンジン CD-ROM²","PC Engine CD-ROM²", ".cue;.chd", "disc", "#FF8A5B"),
        new("PC-FX", "PC-FX", ".cue;.chd", "disc", "#B8C2D1"),
        new("ワンダースワン", "WonderSwan", ".ws;.zip", "handH", "#B8C2D1"),
        new("ワンダースワンカラー", "WonderSwan Color", ".wsc;.zip", "handH", "#4FC3D9"),
        new("ネオジオポケット", "Neo Geo Pocket", ".ngp;.zip", "handH", "#E8D35C"),
        new("ネオジオポケットカラー", "Neo Geo Pocket Color", ".ngc;.zip", "handH", "#F5A524"),
        new("ネオジオ", "Neo Geo", ".zip", "arcade", "#FF8A5B", true),
        new("プレイステーション", "PlayStation", ".cue;.chd;.pbp", "disc", "#B8C2D1"),
        new("スーパーカセットビジョン", "Super Cassette Vision", ".bin;.rom;.0;.zip", "cart", "#F06B70"),
        new("3DO", "3DO", ".cue;.chd;.iso", "disc", "#E8D35C"),
        new("アタリ2600", "Atari 2600", ".a26;.bin;.zip", "cart", "#F5A524"),
        new("アタリ5200", "Atari 5200", ".a52;.bin;.zip", "cart", "#B8C2D1"),
        new("アタリ7800", "Atari 7800", ".a78;.bin;.zip", "cart", "#F06B70"),
        new("アタリ リンクス", "Atari Lynx", ".lnx;.zip", "handH", "#E8D35C"),
        new("アタリ ジャガー", "Atari Jaguar", ".j64;.jag;.zip", "cart", "#F06B70"),
        new("コレコビジョン", "ColecoVision", ".col;.zip", "cart", "#5B9BFF"),
        new("インテレビジョン", "Intellivision", ".int;.bin;.zip", "cart", "#E8D35C"),
        new("ベクトレックス", "Vectrex", ".vec;.zip", "pad", "#B8C2D1"),
        new("オデッセイ²", "Odyssey² / Videopac", ".bin;.zip", "cart", "#8B9CFF"),
        new("チャンネルF", "Channel F", ".chf;.bin;.zip", "cart", "#FF8A5B"),
        new("メガダック", "Mega Duck", ".bin;.zip", "handV", "#4FC3D9"),
        new("スーパービジョン", "Supervision", ".sv;.bin;.zip", "handV", "#7CC96A"),
        new("MSX", "MSX", ".rom;.mx1;.dsk;.cas;.m3u;.zip", "pc", "#5B9BFF"),
        new("MSX2", "MSX2", ".rom;.mx2;.dsk;.m3u;.zip", "pc", "#A78BFA"),
        new("PC-8801", "PC-8801", ".d88;.88d;.t88;.cmt;.m3u;.zip", "pc", "#7CC96A"),
        new("PC-9801", "PC-9801", ".d98;.98d;.fdi;.hdi;.hdm;.fdd;.nhd;.d88;.m3u;.zip", "pc", "#B8C2D1"),
        new("X68000", "X68000", ".dim;.xdf;.hdm;.2hd;.d88;.hdf;.m3u;.zip", "pc", "#F06B70"),
        new("X1", "X1", ".2d;.2hd;.d88;.88d;.hdm;.tfd;.m3u;.zip", "pc", "#F5A524"),
        new("ZX Spectrum", "ZX Spectrum", ".tzx;.tap;.z80;.sna;.dsk;.zip", "pc", "#4FC3D9"),
        new("コモドール64", "Commodore 64", ".d64;.t64;.prg;.crt;.tap;.m3u;.zip", "pc", "#8B9CFF"),
        new("Amiga", "Amiga", ".adf;.hdf;.ipf;.lha;.m3u;.zip", "pc", "#FF8A5B"),
        new("Amstrad CPC", "Amstrad CPC", ".dsk;.sna;.cdt;.zip", "pc", "#E8D35C"),
        new("ネオジオCD", "Neo Geo CD", ".cue;.chd", "disc", "#FF8A5B"),
        new("SC-3000", "SC-3000", ".sc;.sg;.bin;.zip", "pc", "#B8C2D1"),
        new("Atari 800", "Atari 8-bit", ".atr;.xex;.xfd;.atx;.cas;.car;.rom;.zip", "pc", "#F5A524"),
        new("FM TOWNS", "FM Towns", ".cue;.chd;.iso;.d88;.m3u", "pc", "#5B9BFF"),
        new("64DD", "Nintendo 64DD", ".ndd;.zip", "diskcard", "#5B9BFF"),
        new("CD-i", "Philips CD-i", ".chd;.iso;.cue", "disc", "#A78BFA"),
        new("その他", "Other", ".zip", "pad", "#E8D35C"),
    };

    /// <summary>
    /// 候補の名前（日本語・英語のどちらでも）から、日本語の名前を返す。
    /// 書き方の違い（全角・半角、「²」と「2」、大文字・小文字、空白・「-」「・」の有無）は同じとみなす。
    /// 英語名は「Famicom / NES」の「NES」のように、「/」で区切ったどちらでもよい。
    /// </summary>
    public static string? JapaneseNameOf(string name)
    {
        var n = NormalizeName(name);
        if (n.Length == 0) return null;
        return All.FirstOrDefault(p => NormalizeName(p.Name) == n || NormalizeName(p.NameEn) == n
                                       || p.NameEn.Split('/').Any(e => NormalizeName(e) == n))?.Name;
    }

    private static string NormalizeName(string s) =>
        new string(s.Normalize(System.Text.NormalizationForm.FormKC).ToLowerInvariant()
                    .Where(c => !char.IsWhiteSpace(c) && c is not ('-' or '・' or '.' or '_')).ToArray());

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
