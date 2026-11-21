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
        new("ドリームキャスト", "Dreamcast", ".gdi;.cdi;.chd;.cue;.m3u", "disc", "#F5A524"),
        new("PSP", "PSP", ".iso;.cso;.pbp;.chd", "handH", "#5B9BFF"),
        new("ゲームキューブ", "GameCube", ".iso;.gcm;.gcz;.rvz;.ciso;.wia;.m3u", "disc", "#A78BFA"),
        new("Wii", "Wii", ".iso;.wbfs;.rvz;.wia;.gcz;.wad;.m3u", "disc", "#B8C2D1"),
        new("プレイステーション2", "PlayStation 2", ".iso;.chd;.cso;.zso;.cue;.bin;.gz;.m3u", "disc", "#5B9BFF"),
        new("Atari ST", "Atari ST", ".st;.msa;.stx;.dim;.ipf;.m3u;.zip", "pc", "#B8C2D1"),
        new("Atari Falcon", "Atari Falcon", ".st;.msa;.stx;.ipf;.m3u;.zip", "pc", "#8B9CFF"),
        new("コモドール128", "Commodore 128", ".d64;.d71;.d81;.prg;.t64;.crt;.tap;.m3u;.zip", "pc", "#5B9BFF"),
        new("VIC-20", "Commodore VIC-20", ".prg;.d64;.crt;.tap;.20;.40;.60;.a0;.b0;.zip", "pc", "#F5A524"),
        new("Plus/4", "Commodore Plus/4", ".prg;.d64;.t64;.tap;.crt;.zip", "pc", "#B8C2D1"),
        new("PET", "Commodore PET", ".prg;.d64;.tap;.zip", "pc", "#7CC96A"),
        new("CBM-II", "Commodore CBM-II", ".prg;.d80;.d82;.crt;.zip", "pc", "#B8C2D1"),
        new("ZX81", "ZX81", ".p;.tzx;.t81;.zip", "pc", "#B8C2D1"),
        new("スペクトラビデオ", "Spectravideo", ".cas;.rom;.dsk;.zip", "pc", "#F06B70"),
        new("Thomson MO/TO", "Thomson MO/TO", ".fd;.sap;.k7;.m5;.m7;.rom;.zip", "pc", "#5B9BFF"),
        new("Uzebox", "Uzebox", ".uze;.zip", "pad", "#7CC96A"),
        new("ゲーム&ウオッチ", "Game & Watch", ".mgw;.zip", "handH", "#E8D35C"),
        new("ビジュアルメモリ", "Sega VMU", ".vms;.dci;.bin", "handV", "#B8C2D1"),
        new("Palm OS", "Palm OS", ".prc;.pqa;.img;.zip", "handV", "#7CC96A"),
        new("マッキントッシュ", "Macintosh", ".dsk;.img;.hfv;.zip", "pc", "#B8C2D1"),
        new("BK-0010", "Elektronika BK", ".bin;.zip", "pc", "#F06B70"),
        new("MS-DOS", "MS-DOS", ".zip;.dosz;.exe;.com;.bat;.iso;.img;.cue;.m3u", "pc", "#5B9BFF"),
        // 2026-11-21 に追加（拡張子は libretro のコア情報ファイルから）
        new("Apple II", "Apple II", ".dsk;.do;.po;.nib;.woz;.2mg;.hdv;.zip", "pc", "#7CC96A"),
        new("Amiga CD32", "Amiga CD32", ".cue;.chd;.iso;.m3u", "disc", "#FF8A5B"),
        new("コレコ アダム", "Coleco ADAM", ".ddp;.dsk;.m3u;.zip", "pc", "#5B9BFF"),
        new("アルカディア2001", "Arcadia 2001", ".bin;.tvc;.zip", "cart", "#4FC3D9"),
        new("カセットビジョン", "Cassette Vision", ".bin777;.zip", "cart", "#F06B70"),
        new("スーファミターボ", "Sufami Turbo", ".st;.zip", "cart", "#A78BFA"),
        new("プレイディア", "Playdia", ".cue;.iso;.bin;.zip", "disc", "#F5A524"),
        new("ポケットステーション", "PocketStation", ".gme;.mcd;.mcr;.mcs;.pss;.bin;.zip", "handV", "#B8C2D1"),
        new("Enterprise 128", "Enterprise 128", ".img;.dsk;.tap;.com;.trn;.128;.bas;.zip", "pc", "#5B9BFF"),
        new("P2000T", "Philips P2000T", ".cas;.zip", "pc", "#B8C2D1"),
        new("ハイパースキャン", "HyperScan", ".cue;.bin;.zip", "disc", "#7CC96A"),
        new("NUON", "NUON", ".cue;.iso;.chd;.run;.cof;.nuon", "disc", "#A78BFA"),
        new("PICO-8", "PICO-8", ".p8;.png", "pad", "#F06B70"),
        new("Arduboy", "Arduboy", ".hex;.arduboy;.zip", "handH", "#B8C2D1"),
        new("CHIP-8", "CHIP-8", ".ch8;.sc8;.xo8;.zip", "pc", "#7CC96A"),
        new("BBC Micro", "BBC Micro", ".ssd;.dsd;.zip", "pc", "#F06B70"),
        new("P/ECE", "P/ECE", ".pfi;.zip", "handH", "#4FC3D9"),
        new("マイビジョン", "My Vision", ".myv;.bin;.zip", "pad", "#E8D35C"),
        new("クリエイティビジョン", "CreatiVision", ".rom;.bin;.zip", "cart", "#5B9BFF"),
        new("アタリ ジャガーCD", "Atari Jaguar CD", ".cue;.cdi;.chd", "disc", "#F06B70"),
        new("Commander X16", "Commander X16", ".prg;.img;.bas;.crt;.zip", "pc", "#8B9CFF"),
        new("たまごっち", "Tamagotchi", ".b;.bin;.rom;.zip", "handV", "#F5A524"),
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
