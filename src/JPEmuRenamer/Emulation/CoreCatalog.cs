// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.IO.Compression;
using System.Net.Http;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;

namespace JPEmuRenamer.Emulation;

/// <summary>内蔵コアの候補（libretro公式の配布サイトにあるもの）</summary>
public record CoreInfo(string Name, string DisplayName, bool HardwareRendered = false)
{
    public override string ToString() => HardwareRendered ? DisplayName + " (OpenGL)" : DisplayName;
}

/// <summary>
/// 機種ごとのコアの候補と、コアのダウンロード。
/// コアはGPLのものが多いためアプリに同梱せず、利用者の操作で libretro 公式の配布サイトから取得する。
/// </summary>
public static class CoreCatalog
{
    public const string BuildbotBase = "https://buildbot.libretro.com/nightly/windows/x86_64/latest/";

    private static readonly Dictionary<string, CoreInfo> Cores = new CoreInfo[]
    {
        new("mesen", "Mesen"),
        new("fceumm", "FCEUmm"),
        new("nestopia", "Nestopia UE"),
        new("snes9x", "Snes9x"),
        new("bsnes", "bsnes"),
        new("mesen-s", "Mesen-S"),
        new("mupen64plus_next", "Mupen64Plus-Next", true),
        new("parallel_n64", "ParaLLEl N64", true),
        new("gambatte", "Gambatte"),
        new("sameboy", "SameBoy"),
        new("mgba", "mGBA"),
        new("gpsp", "gpSP"),
        new("mednafen_vb", "Beetle VB"),
        new("melonds", "melonDS"),
        new("desmume", "DeSmuME"),
        new("melondsds", "melonDS DS"),
        new("azahar", "Azahar", true),
        new("genesis_plus_gx", "Genesis Plus GX"),
        new("picodrive", "PicoDrive"),
        new("mednafen_saturn", "Beetle Saturn"),
        new("yabause", "Yabause"),
        new("mednafen_pce_fast", "Beetle PCE FAST"),
        new("mednafen_pce", "Beetle PCE"),
        new("mednafen_supergrafx", "Beetle SuperGrafx"),
        new("mednafen_wswan", "Beetle WonderSwan"),
        new("mednafen_ngp", "Beetle NeoPop"),
        new("fbneo", "FinalBurn Neo"),
        new("pcsx_rearmed", "PCSX ReARMed"),
        new("swanstation", "SwanStation"),
        new("mednafen_pcfx", "Beetle PC-FX"),
        new("opera", "Opera"),
        new("emuscv", "EmuSCV"),
        new("pokemini", "PokeMini"),
        new("stella", "Stella"),
        new("a5200", "a5200"),
        new("prosystem", "ProSystem"),
        new("handy", "Handy"),
        new("mednafen_lynx", "Beetle Lynx"),
        new("virtualjaguar", "Virtual Jaguar"),
        new("gearcoleco", "Gearcoleco"),
        new("freeintv", "FreeIntv"),
        new("vecx", "vecx"),
        new("o2em", "O2EM"),
        new("freechaf", "FreeChaF"),
        new("sameduck", "SameDuck"),
        new("potator", "Potator"),
        // パソコン系（キーボードをそのままゲームに送る）
        new("fmsx", "fMSX"),
        new("bluemsx", "blueMSX"),
        new("quasi88", "QUASI88"),
        new("np2kai", "Neko Project II kai"),
        new("px68k", "PX68k"),
        new("x1", "X Millennium"),
        new("fuse", "Fuse"),
        new("vice_x64", "VICE x64"),
        new("puae", "PUAE"),
        new("cap32", "Caprice32"),
        new("atari800", "Atari800"),
        new("geartowns", "GearTowns"),
        new("neocd", "NeoCD"),
        new("same_cdi", "SAME CDi"),
        new("flycast", "Flycast", true),
        new("ppsspp", "PPSSPP", true),
        // 2026-11-21 に追加（パソコン系・小さな機種）
        new("hatari", "Hatari"),
        new("vice_x128", "VICE x128"),
        new("vice_xvic", "VICE xvic"),
        new("vice_xplus4", "VICE xplus4"),
        new("vice_xpet", "VICE xpet"),
        new("vice_xcbm2", "VICE xcbm2"),
        new("81", "EightyOne"),
        new("theodore", "Theodore"),
        new("uzem", "Uzem"),
        new("gw", "Game & Watch"),
        new("vemulator", "VeMUlator"),
        new("mu", "Mu"),
        new("minivmac", "Mini vMac"),
        new("bk", "bk"),
        new("dosbox_pure", "DOSBox Pure"),
        // 2026-11-21 に追加（コア情報ファイルから選んだ機種）
        new("applewin", "AppleWin"),
        new("amiarcadia", "AmiArcadia"),
        new("pd777", "PD777"),
        new("playdiaemu", "PlaydiaEmu"),
        new("pokket", "Pokket"),
        new("ep128emu_core", "ep128emu"),
        new("m2000", "M2000"),
        new("hyperscanemu", "HyperScanEmu"),
        new("nuance", "Nuance", true),
        new("retro8", "Retro8"),
        new("ardens", "Ardens"),
        new("arduous", "Arduous"),
        new("jaxe", "JAXE"),
        new("b2", "b2"),
        new("piece", "piece-emu"),
        new("jollycv", "JollyCV"),
        new("x16emu", "x16emu"),
        new("tamalibretro", "TamaLIBretro"),
        // 重い3Dの機種（OpenGL で描画する）
        new("dolphin", "Dolphin", true),
        new("pcsx2", "LRPS2", true),
        new("play", "Play!", true),
        new("supermodel", "Supermodel", true),
        new("numero", "Numero"),
        new("vice_xcbm5x0", "VICE xcbm5x0"),
        new("gametank", "GameTank"),
        new("galaksija", "Galaksija"),
        new("virtualxt", "VirtualXT"),
        new("mojozork", "MojoZork"),
        new("squirreljme", "SquirrelJME"),
        new("dirksimple", "DirkSimple"),
        new("dingooemu", "DingooEmu"),
    }.ToDictionary(c => c.Name);

    /// <summary>キーボードをそのままゲームに送るコア（パソコン系）。このときキーボードのホットキーは F11（全画面）と F12（メニュー）だけ</summary>
    private static readonly HashSet<string> KeyboardCores = new(StringComparer.OrdinalIgnoreCase)
    {
        "fmsx", "bluemsx", "quasi88", "np2kai", "px68k", "x1", "fuse", "vice_x64", "puae", "cap32", "atari800", "geartowns",
        "hatari", "vice_x128", "vice_xvic", "vice_xplus4", "vice_xpet", "vice_xcbm2", "81", "theodore", "minivmac", "bk", "dosbox_pure",
        "applewin", "ep128emu_core", "m2000", "b2", "x16emu", "vice_xcbm5x0", "galaksija", "virtualxt", "mojozork"
    };

    public static bool UsesKeyboard(string coreName) => KeyboardCores.Contains(coreName);

    /// <summary>マウスで操作するゲームが多いコア。ゲーム画面をクリックすると、マウスをゲームに渡す</summary>
    private static readonly HashSet<string> MouseCores = new(StringComparer.OrdinalIgnoreCase) { "puae", "px68k", "np2kai", "geartowns", "hatari", "minivmac", "dosbox_pure", "virtualxt" };

    public static bool UsesMouse(string coreName) => MouseCores.Contains(coreName);

    /// <summary>機種名 → コアの候補（先頭が既定）と、必要なBIOSの説明</summary>
    private static readonly Dictionary<string, (string[] cores, string bios)> BySystem = new()
    {
        ["ファミコン"] = (new[] { "mesen", "fceumm", "nestopia" }, ""),
        ["ディスクシステム"] = (new[] { "mesen", "fceumm", "nestopia" }, "disksys.rom"),
        ["スーパーファミコン"] = (new[] { "snes9x", "bsnes", "mesen-s" }, ""),
        ["NINTENDO64"] = (new[] { "mupen64plus_next", "parallel_n64" }, ""),
        ["ゲームボーイ"] = (new[] { "gambatte", "sameboy", "mgba" }, ""),
        ["ゲームボーイカラー"] = (new[] { "gambatte", "sameboy", "mgba" }, ""),
        ["ゲームボーイアドバンス"] = (new[] { "mgba", "gpsp" }, "gba_bios.bin?"),
        ["バーチャルボーイ"] = (new[] { "mednafen_vb" }, ""),
        ["ニンテンドーDS"] = (new[] { "melonds", "desmume", "melondsds" }, "bios7.bin / bios9.bin / firmware.bin?"),
        // DSiWare は melonDS DS が DSi モードで NAND に入れてから起動する（DSi の吸い出しファイルが必要。ファームウェアと NAND は名前自由）
        ["ニンテンドーDSiウェア"] = (new[] { "melondsds" }, "bios7.bin / bios9.bin / dsi_bios7.bin / dsi_bios9.bin / DSi firmware / DSi NAND"),
        // 3DS は復号済みのROMのみ
        ["ニンテンドー3DS"] = (new[] { "azahar" }, ""),
        ["SG-1000"] = (new[] { "genesis_plus_gx" }, ""),
        ["マスターシステム"] = (new[] { "genesis_plus_gx", "picodrive" }, ""),
        ["メガドライブ"] = (new[] { "genesis_plus_gx", "picodrive" }, ""),
        ["スーパー32X"] = (new[] { "picodrive" }, ""),
        ["ゲームギア"] =(new[] { "genesis_plus_gx" }, ""),
        ["メガCD"] = (new[] { "genesis_plus_gx", "picodrive" }, "bios_CD_J.bin"),
        ["セガサターン"] = (new[] { "mednafen_saturn", "yabause" }, "sega_101.bin"),
        ["PCエンジン"] = (new[] { "mednafen_pce_fast", "mednafen_pce" }, ""),
        ["PCエンジン スーパーグラフィックス"] = (new[] { "mednafen_supergrafx", "mednafen_pce" }, ""),
        ["PCエンジン CD-ROM²"] =(new[] { "mednafen_pce_fast", "mednafen_pce" }, "syscard3.pce"),
        ["ワンダースワン"] = (new[] { "mednafen_wswan" }, ""),
        ["ワンダースワンカラー"] = (new[] { "mednafen_wswan" }, ""),
        ["ネオジオポケット"] = (new[] { "mednafen_ngp" }, ""),
        ["ネオジオポケットカラー"] = (new[] { "mednafen_ngp" }, ""),
        ["ネオジオ"] = (new[] { "fbneo" }, "neogeo.zip"),
        ["プレイステーション"] = (new[] { "pcsx_rearmed", "swanstation" }, "scph5500.bin / scph5501.bin / scph5502.bin?"),
        // ---- No-Intro・Redump にDATがある、ゲームパッドだけで遊べる機種（2026-11 に追加） ----
        ["サテラビュー"] = (new[] { "snes9x", "bsnes" }, "BS-X.bin"),
        ["ポケモンミニ"] = (new[] { "pokemini" }, "bios.min?"),
        ["キッズコンピュータ ピコ"] = (new[] { "picodrive" }, ""),
        ["PC-FX"] = (new[] { "mednafen_pcfx" }, "pcfx.rom"),
        ["スーパーカセットビジョン"] = (new[] { "emuscv" }, "upd7801g.s01"),
        ["3DO"] = (new[] { "opera" }, "panafz10.bin"),
        ["アタリ2600"] = (new[] { "stella" }, ""),
        ["アタリ5200"] = (new[] { "a5200" }, "5200.rom"),
        ["アタリ7800"] = (new[] { "prosystem" }, "7800 BIOS (U).rom?"),
        ["アタリ リンクス"] = (new[] { "handy", "mednafen_lynx" }, "lynxboot.img"),
        ["アタリ ジャガー"] = (new[] { "virtualjaguar" }, ""),
        ["コレコビジョン"] = (new[] { "gearcoleco" }, "colecovision.rom"),
        ["インテレビジョン"] = (new[] { "freeintv" }, "exec.bin / grom.bin"),
        ["ベクトレックス"] = (new[] { "vecx" }, ""),
        ["オデッセイ²"] = (new[] { "o2em" }, "o2rom.bin"),
        ["チャンネルF"] = (new[] { "freechaf" }, "sl31253.bin / sl31254.bin"),
        ["メガダック"] = (new[] { "sameduck" }, ""),
        ["スーパービジョン"] = (new[] { "potator" }, ""),
        // ---- パソコン系（キーボードで操作する。BIOSはコアごとのフォルダに置くものがある） ----
        ["MSX"] = (new[] { "fmsx", "bluemsx" }, "MSX.ROM（fMSX） / Machines・Databases フォルダ（blueMSX）"),
        ["MSX2"] = (new[] { "fmsx", "bluemsx" }, "MSX2.ROM / MSX2EXT.ROM（fMSX） / Machines・Databases フォルダ（blueMSX）"),
        ["PC-8801"] = (new[] { "quasi88" }, "quasi88\\N88.ROM / N88SUB.ROM / N88_0.ROM〜N88_3.ROM"),
        ["PC-9801"] = (new[] { "np2kai" }, "np2kai\\bios.rom / font.bmp / itf.rom / sound.rom"),
        ["X68000"] = (new[] { "px68k" }, "keropi\\iplrom.dat / cgrom.dat"),
        ["X1"] = (new[] { "x1" }, "xmil\\IPLROM.X1 / FNT0808.X1 / FNT0816.X1 / FNT1616.X1"),
        ["ZX Spectrum"] = (new[] { "fuse" }, ""),
        ["コモドール64"] = (new[] { "vice_x64" }, ""),
        ["Amiga"] = (new[] { "puae" }, "kick34005.A500?"),
        ["Amstrad CPC"] = (new[] { "cap32" }, ""),
        // ---- 2026-11-20 に追加 ----
        ["ネオジオCD"] = (new[] { "neocd" }, "neocd\\neocd_f.rom（など） / 000-lo.lo"),
        ["SC-3000"] = (new[] { "genesis_plus_gx", "bluemsx" }, ""),
        ["Atari 800"] = (new[] { "atari800" }, "ATARIXL.ROM / ATARIBAS.ROM?"),
        ["FM TOWNS"] = (new[] { "geartowns" }, "geartowns\\FMT_SYS.ROM / FMT_DOS.ROM / FMT_FNT.ROM / FMT_DIC.ROM"),
        ["64DD"] = (new[] { "mupen64plus_next" }, "Mupen64plus\\IPL.n64"),
        ["CD-i"] = (new[] { "same_cdi" }, "same_cdi\\bios\\cdimono1.zip"),
        // 3D の機種（OpenGL で描画する）。Flycast は BIOS が無くても動く。PPSSPP の部品（フォントなど）は初めて使うときに自動で取ってくる
        ["ドリームキャスト"] = (new[] { "flycast" }, "dc\\dc_boot.bin / dc_flash.bin?"),
        ["PSP"] = (new[] { "ppsspp" }, ""),
        // ---- 2026-11-21 に追加（パソコン系・小さな機種） ----
        ["Atari ST"] = (new[] { "hatari" }, "tos.img"),
        ["Atari Falcon"] = (new[] { "hatari" }, "tos.img（Falcon 用の TOS 4.0x）"),
        ["コモドール128"] = (new[] { "vice_x128" }, ""),
        ["VIC-20"] = (new[] { "vice_xvic" }, ""),
        ["Plus/4"] = (new[] { "vice_xplus4" }, ""),
        ["PET"] = (new[] { "vice_xpet" }, ""),
        ["CBM-II"] = (new[] { "vice_xcbm2" }, ""),
        ["ZX81"] = (new[] { "81" }, ""),
        ["スペクトラビデオ"] = (new[] { "bluemsx" }, "Machines・Databases フォルダ（blueMSX）"),
        ["Thomson MO/TO"] = (new[] { "theodore" }, ""),
        ["Uzebox"] = (new[] { "uzem" }, ""),
        ["ゲーム&ウオッチ"] = (new[] { "gw" }, ""),
        ["ビジュアルメモリ"] = (new[] { "vemulator" }, ""),
        ["Palm OS"] = (new[] { "mu" }, "palmos41-en-m515.rom / bootloader-dbvz.rom"),
        ["マッキントッシュ"] = (new[] { "minivmac" }, "MacII.ROM"),
        ["BK-0010"] = (new[] { "bk" }, "bk\\MONIT10.ROM / FOCAL10.ROM など"),
        ["MS-DOS"] = (new[] { "dosbox_pure" }, ""),
        // ---- 2026-11-21 に追加（コア情報ファイルから選んだ機種。説明が空のものはコア情報から自動で出す） ----
        ["Apple II"] = (new[] { "applewin" }, ""),
        ["Amiga CD32"] = (new[] { "puae" }, "kick40060.CD32 / kick40060.CD32.ext"),
        ["コレコ アダム"] = (new[] { "gearcoleco" }, "colecovision.rom / eos.rom / writer.rom"),
        ["アルカディア2001"] = (new[] { "amiarcadia" }, ""),
        ["カセットビジョン"] = (new[] { "pd777" }, ""),
        ["スーファミターボ"] = (new[] { "snes9x" }, "STBIOS.bin"),
        ["プレイディア"] = (new[] { "playdiaemu" }, ""),
        ["ポケットステーション"] = (new[] { "pokket" }, "pocketstation.bin?"),
        ["Enterprise 128"] = (new[] { "ep128emu_core" }, ""),
        ["P2000T"] = (new[] { "m2000" }, ""),
        ["ハイパースキャン"] = (new[] { "hyperscanemu" }, ""),
        ["NUON"] = (new[] { "nuance" }, ""),
        ["PICO-8"] = (new[] { "retro8" }, ""),
        ["Arduboy"] = (new[] { "ardens", "arduous" }, ""),
        ["CHIP-8"] = (new[] { "jaxe" }, ""),
        ["BBC Micro"] = (new[] { "b2" }, ""),
        ["P/ECE"] = (new[] { "piece" }, ""),
        ["マイビジョン"] = (new[] { "jollycv" }, ""),
        ["クリエイティビジョン"] = (new[] { "jollycv" }, "bioscv.rom"),
        ["アタリ ジャガーCD"] = (new[] { "virtualjaguar" }, "[BIOS] Atari Jaguar CD (World).j64?"),
        ["Commander X16"] = (new[] { "x16emu" }, ""),
        ["たまごっち"] = (new[] { "tamalibretro" }, ""),
        // Dolphin の Sys フォルダと LRPS2 の部品は、初めて使うときに自動で取ってくる。
        // LRPS2 は PS2 のBIOS（名前は自由）を pcsx2\bios に置く。Play! はBIOSが無くても動く（試作段階）
        ["ゲームキューブ"] = (new[] { "dolphin" }, "dolphin-emu\\Sys\\GC\\JAP\\IPL.bin?"),
        ["Wii"] = (new[] { "dolphin" }, ""),
        // ---- アーケード。NAOMI・アトミスウェイブの BIOS は Flycast が BIOSフォルダの dc に探す ----
        ["アーケード"] = (new[] { "fbneo" }, "neogeo.zip・pgm.zip など（それを使うゲームだけ。ROMと同じフォルダでも可）?"),
        ["CPS1"] = (new[] { "fbneo" }, ""),
        ["CPS2"] = (new[] { "fbneo" }, ""),
        ["CPS3"] = (new[] { "fbneo" }, ""),
        ["NAOMI"] = (new[] { "flycast" }, "dc\\naomi.zip"),
        ["NAOMI 2"] = (new[] { "flycast" }, "dc\\naomi2.zip"),
        ["アトミスウェイブ"] = (new[] { "flycast" }, "dc\\awbios.zip"),
        ["Model 3"] = (new[] { "supermodel" }, ""),
        // ---- 同じコアで、初期設定（SystemCoreOptions）を替えて動かす機種 ----
        ["ビデオパック+"] = (new[] { "o2em" }, "g7400.bin"),
        ["CDTV"] = (new[] { "puae" }, "kick34005.CDTV"),
        ["Atari STE"] = (new[] { "hatari" }, "tos.img（STE 用の TOS 1.06 以降）"),
        ["Atari TT"] = (new[] { "hatari" }, "tos.img（TT 用の TOS 3.0x）"),
        ["スーパーゲームボーイ"] = (new[] { "mgba" }, "sgb_bios.bin?"),
        // ---- コアを登録するだけで動く機種 ----
        ["TI-83"] = (new[] { "numero" }, "ti83se.rom / ti83plus.rom / ti83.rom のどれか"),
        ["VC 4000"] = (new[] { "amiarcadia" }, ""),
        ["TV-Computer"] = (new[] { "ep128emu_core" }, "ep128emu\\roms\\tvc22_sys.rom / tvc22_ext.rom?"),
        ["CBM-500"] = (new[] { "vice_xcbm5x0" }, ""),
        ["GameTank"] = (new[] { "gametank" }, ""),
        ["Galaksija"] = (new[] { "galaksija" }, "galaksija\\ROM1.BIN / ROM2.BIN / CHRGEN.BIN?"),
        ["PC/XT"] = (new[] { "virtualxt" }, ""),
        ["Z-Machine"] = (new[] { "mojozork" }, ""),
        ["Java の携帯ゲーム"] = (new[] { "squirreljme" }, ""),
        ["レーザーディスク"] = (new[] { "dirksimple" }, ""),
        ["Dingoo A320"] = (new[] { "dingooemu" }, ""),
        // ---- 内蔵コアでは動かない機種（外部のエミュレーターで起動する）。候補のコアは無い ----
        ["プレイステーション3"] = (Array.Empty<string>(), ""),
        ["PS Vita"] = (Array.Empty<string>(), ""),
        ["Xbox"] = (Array.Empty<string>(), ""),
        ["Xbox 360"] = (Array.Empty<string>(), ""),
        ["Wii U"] = (Array.Empty<string>(), ""),
        ["プレイステーション2"] = (new[] { "pcsx2", "play" }, "pcsx2\\bios\\（PS2 のBIOS。ファイル名は自由。Play! では不要）"),
    };

    /// <summary>拡張子から推測するときの対応（機種名が候補と違う場合）</summary>
    private static readonly (string ext, string system)[] ByExtension =
    {
        (".nes", "ファミコン"), (".fds", "ディスクシステム"), (".sfc", "スーパーファミコン"), (".smc", "スーパーファミコン"),
        (".z64", "NINTENDO64"), (".n64", "NINTENDO64"), (".v64", "NINTENDO64"), (".gb", "ゲームボーイ"), (".gbc", "ゲームボーイカラー"),
        (".gba", "ゲームボーイアドバンス"), (".vb", "バーチャルボーイ"), (".nds", "ニンテンドーDS"),
        (".3ds", "ニンテンドー3DS"), (".cci", "ニンテンドー3DS"), (".cxi", "ニンテンドー3DS"), (".3dsx", "ニンテンドー3DS"), (".sg", "SG-1000"),
        (".sms", "マスターシステム"), (".md", "メガドライブ"), (".gen", "メガドライブ"), (".gg", "ゲームギア"), (".pce", "PCエンジン"),
        (".32x", "スーパー32X"), (".sgx", "PCエンジン スーパーグラフィックス"),
        (".ws", "ワンダースワン"), (".wsc", "ワンダースワンカラー"), (".ngp", "ネオジオポケット"), (".ngc", "ネオジオポケットカラー"),
        (".pbp", "プレイステーション"),
        (".bs", "サテラビュー"), (".min", "ポケモンミニ"), (".a26", "アタリ2600"), (".a52", "アタリ5200"), (".a78", "アタリ7800"),
        (".lnx", "アタリ リンクス"), (".j64", "アタリ ジャガー"), (".jag", "アタリ ジャガー"), (".col", "コレコビジョン"),
        (".int", "インテレビジョン"), (".vec", "ベクトレックス"), (".chf", "チャンネルF"), (".sv", "スーパービジョン"),
        (".mx1", "MSX"), (".mx2", "MSX2"), (".d98", "PC-9801"), (".fdi", "PC-9801"), (".hdi", "PC-9801"), (".dim", "X68000"), (".xdf", "X68000"),
        (".2d", "X1"), (".tzx", "ZX Spectrum"), (".z80", "ZX Spectrum"), (".d64", "コモドール64"), (".t64", "コモドール64"),
        (".adf", "Amiga"), (".cdt", "Amstrad CPC"),
        (".ndd", "64DD"), (".atr", "Atari 800"), (".xex", "Atari 800"), (".xfd", "Atari 800"), (".sc", "SC-3000"),
        (".gdi", "ドリームキャスト"), (".cdi", "ドリームキャスト"), (".cso", "PSP"),
        (".msa", "Atari ST"), (".stx", "Atari ST"), (".d71", "コモドール128"), (".d81", "コモドール128"), (".t81", "ZX81"),
        (".sap", "Thomson MO/TO"), (".k7", "Thomson MO/TO"), (".uze", "Uzebox"), (".mgw", "ゲーム&ウオッチ"), (".vms", "ビジュアルメモリ"),
        (".prc", "Palm OS"), (".hfv", "マッキントッシュ"), (".dosz", "MS-DOS"),
        (".woz", "Apple II"), (".nib", "Apple II"), (".do", "Apple II"), (".po", "Apple II"), (".ddp", "コレコ アダム"),
        (".bin777", "カセットビジョン"), (".gme", "ポケットステーション"), (".nuon", "NUON"), (".p8", "PICO-8"),
        (".hex", "Arduboy"), (".arduboy", "Arduboy"), (".ch8", "CHIP-8"), (".sc8", "CHIP-8"), (".xo8", "CHIP-8"),
        (".ssd", "BBC Micro"), (".dsd", "BBC Micro"), (".pfi", "P/ECE"), (".myv", "マイビジョン"),
        (".gcm", "ゲームキューブ"), (".gcz", "ゲームキューブ"), (".wbfs", "Wii"), (".wad", "Wii"), (".zso", "プレイステーション2"),
    };

    /// <summary>手で登録したコア（コアの一覧の画面に出すもの）</summary>
    public static IReadOnlyCollection<CoreInfo> All => Cores.Values;

    /// <summary>コア情報ファイルにだけあるコア（その他の機種で選べる）。Vulkan・Direct3D だけで描画するコアは除く</summary>
    private static readonly Lazy<Dictionary<string, CoreInfo>> InfoOnlyCores = new(() =>
        CoreInfoDb.All.Where(i => !i.Unsupported && !Cores.ContainsKey(i.Name))
            .ToDictionary(i => i.Name, i => new CoreInfo(i.Name, i.DisplayName, i.UsesOpenGl), StringComparer.OrdinalIgnoreCase));

    public static CoreInfo? Get(string name) => Cores.GetValueOrDefault(name) ?? InfoOnlyCores.Value.GetValueOrDefault(name);

    /// <summary>この機種の候補の日本語名（分からなければ null）</summary>
    public static string? SystemKeyOf(SystemConfig sys) => SystemKey(sys);

    private static string? SystemKey(SystemConfig sys)
    {
        if (BySystem.ContainsKey(sys.Name)) return sys.Name;
        // 候補の英語名（Famicom / NES など）で作った機種も同じ扱いにする
        if (SystemPresets.JapaneseNameOf(sys.Name) is string ja && BySystem.ContainsKey(ja)) return ja;
        foreach (var (ext, system) in ByExtension)
            if (sys.Extensions.Contains(ext, StringComparer.OrdinalIgnoreCase)) return system;
        return null;
    }

    /// <summary>
    /// この機種で選べるコア。候補が無い機種は、拡張子に対応しているコアをコア情報ファイルから探す（見つからなければすべて）
    /// </summary>
    public static List<CoreInfo> CandidatesFor(SystemConfig sys)
    {
        var key = SystemKey(sys);
        if (key != null) return BySystem[key].cores.Select(n => Cores[n]).ToList();
        var all = Cores.Values.Concat(InfoOnlyCores.Value.Values).OrderBy(c => c.DisplayName, StringComparer.OrdinalIgnoreCase).ToList();
        // zip は多くのコアが対応しているので、機種を見分ける手がかりにしない
        var exts = sys.Extensions.Select(e => e.TrimStart('.').ToLowerInvariant()).Where(e => e is not ("zip" or "7z")).ToHashSet();
        if (exts.Count == 0) return all;
        var matched = all.Where(c => CoreInfoDb.Get(c.Name)?.Extensions.Any(exts.Contains) == true).ToList();
        return matched.Count > 0 ? matched : all;
    }

    /// <summary>
    /// 必要なBIOSの説明。手で書いた説明が無い機種は、使うコアのコア情報ファイルから「無いと動かない」ファイルを出す
    /// </summary>
    public static string BiosHintFor(SystemConfig sys, string? coreName = null)
    {
        var key = SystemKey(sys);
        var bios = key != null ? BySystem[key].bios : "";
        if (bios.Length == 0) bios = BiosHintFromCoreInfo(string.IsNullOrEmpty(coreName) ? CoreNameFor(sys) : coreName);
        // 末尾の ? は「無くても動く（任意）」の印
        return bios.EndsWith('?') ? Loc.F("BiosOptional", bios.TrimEnd('?')) : bios;
    }

    private static string BiosHintFromCoreInfo(string coreName)
    {
        // 部品を自動で取ってくるコアは、説明を出さない
        if (string.IsNullOrEmpty(coreName) || CoreAssets.ContainsKey(coreName)) return "";
        var required = CoreInfoDb.Get(coreName)?.Firmware.Where(f => !f.Optional).Select(f => f.Path.Replace('/', '\\')).ToList();
        if (required == null || required.Count == 0) return "";
        // 同じフォルダのものは、2つめからフォルダ名を省く（例: nuance\bios.cof / minibios.cof）
        var parts = new List<string>();
        string? lastDir = null;
        foreach (var p in required)
        {
            var dir = Path.GetDirectoryName(p);
            parts.Add(!string.IsNullOrEmpty(dir) && dir == lastDir ? Path.GetFileName(p) : p);
            lastDir = dir;
        }
        return string.Join(" / ", parts);
    }

    /// <summary>
    /// この機種で必要なBIOSが見つからなければ、足りないファイル名を返す（そろっている・不要なら null）。
    /// 中身は BiosCatalog にある。BIOSフォルダの直下にファイル名だけで置いてあっても見つかる
    /// </summary>
    public static string? MissingBios(SystemConfig sys, string biosDir) => BiosCatalog.Missing(sys, biosDir);

    /// <summary>機種名の表示用（日本語以外では候補の英語名にする）</summary>
    public static string SystemDisplayName(string japaneseKey) =>
        SystemPresets.All.FirstOrDefault(p => p.Name == japaneseKey)?.DisplayName ?? japaneseKey;

    /// <summary>この機種で使うコア（設定が空なら候補の先頭）</summary>
    public static string CoreNameFor(SystemConfig sys)
    {
        if (!string.IsNullOrWhiteSpace(sys.CoreName)) return sys.CoreName;
        var key = SystemKey(sys);
        return key != null && BySystem[key].cores.Length > 0 ? BySystem[key].cores[0] : "";
    }

    /// <summary>
    /// 機種ごとのコアの初期設定。同じコアで別の機種を動かすためのもので、利用者がプレイ設定で選んだ値があればそちらが優先。
    /// キーは候補の日本語名。値は（そのコアのときだけ使う, 設定名 → 値）。設定名と値は各コアの libretro_core_options に合わせる
    /// </summary>
    private static readonly Dictionary<string, (string core, Dictionary<string, string> options)> SystemCoreOptions = new()
    {
        ["ビデオパック+"] = ("o2em", new() { ["o2em_bios"] = "g7400.bin" }),
        ["オデッセイ²"] = ("o2em", new() { ["o2em_bios"] = "o2rom.bin" }),
        ["CDTV"] = ("puae", new() { ["puae_model"] = "CDTV" }),
        ["Amiga CD32"] = ("puae", new() { ["puae_model"] = "CD32" }),
        ["Atari ST"] = ("hatari", new() { ["hatari_machinetype"] = "st" }),
        ["Atari STE"] = ("hatari", new() { ["hatari_machinetype"] = "ste" }),
        ["Atari TT"] = ("hatari", new() { ["hatari_machinetype"] = "tt" }),
        ["Atari Falcon"] = ("hatari", new() { ["hatari_machinetype"] = "falcon" }),
        ["スーパーゲームボーイ"] = ("mgba", new() { ["mgba_gb_model"] = "Super Game Boy", ["mgba_sgb_borders"] = "ON" }),
    };

    /// <summary>この機種とコアの組み合わせでの、設定の初期値（決まっていなければ null）</summary>
    public static string? DefaultCoreOption(SystemConfig sys, string coreName, string key)
    {
        var name = SystemKey(sys);
        return name != null && SystemCoreOptions.TryGetValue(name, out var o)
               && string.Equals(o.core, coreName, StringComparison.OrdinalIgnoreCase)
               && o.options.TryGetValue(key, out var v) ? v : null;
    }

    /// <summary>このコアを候補に持つ機種の名前（表示用）</summary>
    public static List<string> SystemsUsing(string coreName) =>
        BySystem.Where(kv => kv.Value.cores.Contains(coreName)).Select(kv => kv.Key).ToList();

    /// <summary>配布サイト上のファイルの大きさと更新日時（分からなければ null）。ダウンロードはしない</summary>
    public static async Task<(long? size, DateTimeOffset? modified)> GetRemoteInfoAsync(string coreName, CancellationToken ct)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Head, BuildbotBase + coreName + "_libretro.dll.zip");
            using var resp = await Http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return (null, null);
            return (resp.Content.Headers.ContentLength, resp.Content.Headers.LastModified);
        }
        catch
        {
            return (null, null);
        }
    }

    /// <summary>
    /// 配布サイトに新しい版があるか。ダウンロードしたコアのファイル日時は配布サイトの日時に合わせてあるので、それと比べる
    /// </summary>
    public static bool HasUpdate(string coreName, DateTimeOffset? remoteModified)
    {
        if (remoteModified == null || !IsInstalled(coreName)) return false;
        var local = File.GetLastWriteTimeUtc(DllPath(coreName));
        return remoteModified.Value.UtcDateTime > local.AddMinutes(1);
    }

    public static string DllPath(string coreName) => System.IO.Path.Combine(Storage.CoresDir, coreName + "_libretro.dll");

    public static bool IsInstalled(string coreName) => !string.IsNullOrEmpty(coreName) && File.Exists(DllPath(coreName));

    private static readonly HttpClient Http = CreateHttp();

    private static HttpClient CreateHttp()
    {
        var h = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        h.DefaultRequestHeaders.UserAgent.ParseAdd("JPEmuRenamer/1.0");
        return h;
    }

    /// <summary>
    /// コアが動くのに必要な部品（libretro 公式の配布サイトの assets/system にあるもの）。
    /// コアの名前 → （BIOSフォルダの中の、これが無ければ取ってくるフォルダ, 配布サイトのZIPの名前）
    /// </summary>
    private static readonly Dictionary<string, (string check, string zip)> CoreAssets = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ppsspp"] = ("PPSSPP", "PPSSPP"),
        ["dolphin"] = ("dolphin-emu\\Sys", "Dolphin"),
        ["pcsx2"] = ("pcsx2\\resources", "LRPS2"),
    };

    /// <summary>部品を配布サイトから取ってくるコアか</summary>
    public static bool HasCoreAssets(string coreName) => CoreAssets.ContainsKey(coreName);

    /// <summary>このコアに必要な部品が、まだ BIOSフォルダに無いか（あれば取ってくる必要がある）</summary>
    public static bool NeedsCoreAssets(string coreName, string systemDir) =>
        CoreAssets.TryGetValue(coreName, out var asset) && !Directory.Exists(System.IO.Path.Combine(systemDir, asset.check));

    /// <summary>このコアに必要な部品が BIOSフォルダに無ければ、配布サイトから取ってきて展開する（無い・要らないときは何もしない）</summary>
    public static async Task EnsureCoreAssetsAsync(string coreName, string systemDir, CancellationToken ct)
    {
        if (!CoreAssets.TryGetValue(coreName, out var asset)) return;
        if (Directory.Exists(System.IO.Path.Combine(systemDir, asset.check))) return;
        Directory.CreateDirectory(systemDir);
        var tmpZip = System.IO.Path.Combine(systemDir, asset.zip + ".download");
        var tmpDir = System.IO.Path.Combine(systemDir, asset.zip + ".extract");
        try
        {
            using (var resp = await Http.GetAsync("https://buildbot.libretro.com/assets/system/" + asset.zip + ".zip", HttpCompletionOption.ResponseHeadersRead, ct))
            {
                resp.EnsureSuccessStatusCode();
                await using var src = await resp.Content.ReadAsStreamAsync(ct);
                await using var dst = File.Create(tmpZip);
                await src.CopyToAsync(dst, ct);
            }
            // ZIP の中は「PPSSPP/…」「dolphin-emu/Sys/…」のように BIOSフォルダからの形になっている。
            // 途中で止まっても半端なフォルダが残らないよう、別の場所に展開してから移す。利用者が置いたファイルは上書きしない
            if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true);
            ZipFile.ExtractToDirectory(tmpZip, tmpDir);
            MergeInto(tmpDir, systemDir);
        }
        finally
        {
            try { File.Delete(tmpZip); } catch { }
            try { if (Directory.Exists(tmpDir)) Directory.Delete(tmpDir, true); } catch { }
        }
    }

    private static void MergeInto(string from, string to)
    {
        Directory.CreateDirectory(to);
        foreach (var dir in Directory.GetDirectories(from))
        {
            var target = System.IO.Path.Combine(to, System.IO.Path.GetFileName(dir));
            if (!Directory.Exists(target)) Directory.Move(dir, target);
            else MergeInto(dir, target);
        }
        foreach (var file in Directory.GetFiles(from))
        {
            var target = System.IO.Path.Combine(to, System.IO.Path.GetFileName(file));
            if (!File.Exists(target)) File.Move(file, target);
        }
    }

    /// <summary>コアをダウンロードして cores フォルダに置く（既にあれば更新）</summary>
    public static async Task DownloadAsync(string coreName, IProgress<double>? progress, CancellationToken ct)
    {
        if (Get(coreName) == null) throw new ArgumentException(coreName);
        Directory.CreateDirectory(Storage.CoresDir);
        var url = BuildbotBase + coreName + "_libretro.dll.zip";
        var tmpZip = System.IO.Path.Combine(Storage.CoresDir, coreName + ".download");
        DateTimeOffset? remoteModified;

        try
        {
            using var resp = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
            resp.EnsureSuccessStatusCode();
            remoteModified = resp.Content.Headers.LastModified;
            var total = resp.Content.Headers.ContentLength ?? -1;
            await using var src = await resp.Content.ReadAsStreamAsync(ct);
            await using var dst = File.Create(tmpZip);
            var buf = new byte[1 << 16];
            long done = 0;
            int n;
            while ((n = await src.ReadAsync(buf, ct)) > 0)
            {
                await dst.WriteAsync(buf.AsMemory(0, n), ct);
                done += n;
                if (total > 0) progress?.Report((double)done / total);
            }
        }
        catch
        {
            // 途中で止まったときは書きかけのファイルを残さない
            try { File.Delete(tmpZip); } catch { }
            throw;
        }

        try
        {
            using var zip = ZipFile.OpenRead(tmpZip);
            var entry = zip.Entries.FirstOrDefault(e => e.Name.Equals(coreName + "_libretro.dll", StringComparison.OrdinalIgnoreCase))
                        ?? zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        ?? throw new InvalidDataException("No DLL in the downloaded archive.");
            var tmpDll = DllPath(coreName) + ".new";
            entry.ExtractToFile(tmpDll, true);
            File.Move(tmpDll, DllPath(coreName), true);
            // 更新の確認に使うので、ファイル日時を配布サイトの日時にそろえる
            if (remoteModified != null) File.SetLastWriteTimeUtc(DllPath(coreName), remoteModified.Value.UtcDateTime);
        }
        finally
        {
            try { File.Delete(tmpZip); } catch { }
        }
    }
}
