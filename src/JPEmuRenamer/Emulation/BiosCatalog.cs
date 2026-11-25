// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;

namespace JPEmuRenamer.Emulation;

/// <summary>
/// BIOSの1項目。Path は BIOSフォルダからの場所で、コアが決まったフォルダに探すものは「Mupen64plus/IPL.n64」のように書く。
/// Alternatives のどれかがあればよい。Note は Loc のキー。Folder は中身ごと置くフォルダ（blueMSX の Machines など）
/// </summary>
/// <summary>見つかったBIOS。Name はどの名前のBIOSとして見つかったか、ByContent は名前ではなく中身（指紋）で見つけたか</summary>
public sealed record BiosLocation(string Path, string Name, bool ByContent);

public sealed record BiosFile(string Path, bool Required, string[] Alternatives, string Note = "", bool Folder = false, bool AnyFileInside = false)
{
    public string FileName => System.IO.Path.GetFileName(Path);
    public IEnumerable<string> AllPaths => new[] { Path }.Concat(Alternatives);
}

/// <summary>
/// 機種ごとのBIOS。利用者は BIOSフォルダの直下に全部入れるだけでよい。
/// コアが決まったサブフォルダに探すものは、起動するときにアプリが直下からそこへ写す（Stage）。
/// ここに無い機種は、libretro のコア情報で「必須」とされているものを案内だけする（コア情報の必須は当てにならないので、起動は止めない）
/// </summary>
public static class BiosCatalog
{
    private sealed record Entry(string? Core, BiosFile File, bool Strict = true);

    private static Entry F(string path, params string[] alternatives) => new(null, new BiosFile(path, true, alternatives));
    private static Entry Opt(string path, params string[] alternatives) => new(null, new BiosFile(path, false, alternatives));
    private static Entry N(Entry e, string note) => e with { File = e.File with { Note = note } };
    private static Entry On(string core, Entry e) => e with { Core = core };
    private static Entry Dir(string path, string note, bool anyFile = false) => new(null, new BiosFile(path, true, Array.Empty<string>(), note, Folder: true, AnyFileInside: anyFile));

    private static readonly string[] TownsSets = { "geartowns/fmtowns.zip", "geartowns/fmtownsux.zip", "geartowns/fmtownshr.zip", "geartowns/fmtownsftv.zip", "geartowns/fmtownsmx.zip", "geartowns/fmtmarty.zip" };
    private static string[] Towns(string file) => new[] { "geartowns/fmtowns/" + file.ToLowerInvariant() }.Concat(TownsSets).ToArray();

    private static readonly Dictionary<string, Entry[]> Table = new()
    {
        ["ディスクシステム"] = new[] { F("disksys.rom") },
        ["ゲームボーイアドバンス"] = new[] { Opt("gba_bios.bin") },
        ["ニンテンドーDS"] = new[] { Opt("bios7.bin"), Opt("bios9.bin"), Opt("firmware.bin") },
        ["ニンテンドーDSiウェア"] = new[]
        {
            F("bios7.bin"), F("bios9.bin"), F("dsi_bios7.bin"), F("dsi_bios9.bin"),
            Opt("dsi_firmware.bin"), Opt("dsi_nand.bin"),
        },
        ["メガCD"] = new[] { F("bios_CD_J.bin"), N(Opt("bios_CD_U.bin"), "BiosNoteOverseas"), N(Opt("bios_CD_E.bin"), "BiosNoteOverseas") },
        ["セガサターン"] = new[] { On("mednafen_saturn", F("sega_101.bin", "mpr-17933.bin")) },
        ["PCエンジン CD-ROM²"] = new[] { F("syscard3.pce") },
        ["ネオジオ"] = new[] { N(F("neogeo.zip", "fbneo/neogeo.zip"), "BiosNoteRomFolder") },
        ["プレイステーション"] = new[] { Opt("scph5500.bin", "scph5501.bin", "scph5502.bin") },
        ["サテラビュー"] = new[] { F("BS-X.bin") },
        ["ポケモンミニ"] = new[] { Opt("bios.min") },
        ["PC-FX"] = new[] { F("pcfx.rom") },
        ["スーパーカセットビジョン"] = new[] { F("upd7801g.s01") },
        ["3DO"] = new[] { F("panafz10.bin", "panafz10-norsa.bin", "panafz1.bin", "panafz1j.bin", "panafz1j-norsa.bin", "goldstar.bin", "sanyotry.bin", "3do_arcade_saot.bin") },
        ["アタリ5200"] = new[] { F("5200.rom") },
        ["アタリ7800"] = new[] { Opt("7800 BIOS (U).rom") },
        ["アタリ リンクス"] = new[] { F("lynxboot.img") },
        ["コレコビジョン"] = new[] { F("colecovision.rom") },
        ["インテレビジョン"] = new[] { F("exec.bin"), F("grom.bin") },
        ["オデッセイ²"] = new[] { F("o2rom.bin") },
        ["ビデオパック+"] = new[] { F("g7400.bin", "jopac.bin") },
        ["チャンネルF"] = new[] { F("sl31254.bin"), F("sl31253.bin", "sl90025.bin") },
        ["MSX"] = new[]
        {
            On("fmsx", F("MSX.ROM")),
            On("bluemsx", Dir("Machines", "BiosNoteBlueMsx")), On("bluemsx", Dir("Databases", "BiosNoteBlueMsx")),
        },
        ["MSX2"] = new[]
        {
            On("fmsx", F("MSX2.ROM")), On("fmsx", F("MSX2EXT.ROM")),
            On("bluemsx", Dir("Machines", "BiosNoteBlueMsx")), On("bluemsx", Dir("Databases", "BiosNoteBlueMsx")),
        },
        ["スペクトラビデオ"] = new[] { Dir("Machines", "BiosNoteBlueMsx"), Dir("Databases", "BiosNoteBlueMsx") },
        ["PC-8801"] = new[] { F("quasi88/N88.ROM"), F("quasi88/N88_0.ROM"), Opt("quasi88/N88SUB.ROM"), Opt("quasi88/N88_1.ROM"), Opt("quasi88/N88_2.ROM"), Opt("quasi88/N88_3.ROM") },
        ["PC-9801"] = new[] { Opt("np2kai/bios.rom"), Opt("np2kai/font.bmp"), Opt("np2kai/itf.rom"), Opt("np2kai/sound.rom") },
        ["X68000"] = new[] { F("keropi/iplrom.dat"), F("keropi/cgrom.dat") },
        ["X1"] = new[] { Opt("xmil/IPLROM.X1"), Opt("xmil/FNT0808.X1"), Opt("xmil/FNT0816.X1"), Opt("xmil/FNT1616.X1") },
        ["Amiga"] = new[] { Opt("kick34005.A500") },
        ["Amiga CD32"] = new[] { F("kick40060.CD32"), F("kick40060.CD32.ext") },
        ["CDTV"] = new[] { F("kick34005.CDTV") },
        ["ネオジオCD"] = new[]
        {
            F("neocd/neocd_f.rom", "neocd/neocd_sf.rom", "neocd/front-sp1.bin", "neocd/neocd_t.rom", "neocd/neocd_st.rom", "neocd/top-sp1.bin",
              "neocd/neocd_z.rom", "neocd/neocd_sz.rom", "neocd/neocd.bin", "neocd/uni-bioscd.rom"),
            F("neocd/000-lo.lo", "neocd/ng-lo.rom"),
        },
        ["Atari 800"] = new[] { Opt("ATARIXL.ROM"), Opt("ATARIBAS.ROM") },
        ["FM TOWNS"] = new[] { F("geartowns/FMT_SYS.ROM", Towns("FMT_SYS.ROM")), F("geartowns/FMT_DOS.ROM", Towns("FMT_DOS.ROM")), F("geartowns/FMT_FNT.ROM", Towns("FMT_FNT.ROM")), F("geartowns/FMT_DIC.ROM", Towns("FMT_DIC.ROM")) },
        // ParaLLEl N64 は IPL が無くても動く（あれば使う）。Mupen64Plus-Next を自分で選んだときは Mupen64plus\IPL.n64 が要る
        ["64DD"] = new[] { On("parallel_n64", Opt("64DD_IPL.bin")), On("mupen64plus_next", N(F(Ipl64dd), "BiosNoteIpl")) },
        ["CD-i"] = new[] { F("same_cdi/bios/cdimono1.zip") },
        ["ドリームキャスト"] = new[] { Opt("dc/dc_boot.bin"), Opt("dc/dc_flash.bin") },
        ["NAOMI"] = new[] { F("dc/naomi.zip") },
        ["NAOMI 2"] = new[] { F("dc/naomi2.zip") },
        ["アトミスウェイブ"] = new[] { F("dc/awbios.zip") },
        ["アーケード"] = new[] { N(Opt("neogeo.zip", "fbneo/neogeo.zip"), "BiosNoteArcadeBoard"), N(Opt("pgm.zip", "fbneo/pgm.zip"), "BiosNoteArcadeBoard") },
        ["Atari ST"] = new[] { F("tos.img") },
        ["Atari STE"] = new[] { N(F("tos.img"), "BiosNoteTos") },
        ["Atari TT"] = new[] { N(F("tos.img"), "BiosNoteTos") },
        ["Atari Falcon"] = new[] { N(F("tos.img"), "BiosNoteTos") },
        ["Palm OS"] = new[] { F("palmos41-en-m515.rom"), Opt("bootloader-dbvz.rom") },
        ["マッキントッシュ"] = new[] { F("MacII.ROM") },
        ["BK-0010"] = new[] { Opt("bk/MONIT10.ROM"), Opt("bk/FOCAL10.ROM"), Opt("bk/BASIC10.ROM") },
        ["コレコ アダム"] = new[] { F("colecovision.rom"), Opt("eos.rom"), Opt("writer.rom") },
        ["スーファミターボ"] = new[] { F("STBIOS.bin") },
        ["スーパーゲームボーイ"] = new[] { Opt("sgb_bios.bin") },
        ["ポケットステーション"] = new[] { Opt("pocketstation.bin") },
        ["クリエイティビジョン"] = new[] { F("bioscv.rom") },
        ["アタリ ジャガーCD"] = new[] { Opt("[BIOS] Atari Jaguar CD (World).j64") },
        ["ゲームキューブ"] = new[] { Opt("dolphin-emu/Sys/GC/JAP/IPL.bin", "dolphin-emu/Sys/GC/USA/IPL.bin", "dolphin-emu/Sys/GC/EUR/IPL.bin") },
        ["ハイパースキャン"] = new[] { F("spg290.bin"), F("hyperscan.bin") },
        ["Commander X16"] = new[] { F("x16/rom.bin") },
        ["TI-83"] = new[] { F("ti83se.rom", "ti83plus.rom", "ti83.rom") },
        ["TV-Computer"] = new[] { Opt("ep128emu/roms/tvc22_sys.rom"), Opt("ep128emu/roms/tvc22_ext.rom") },
        ["Galaksija"] = new[] { Opt("galaksija/ROM1.BIN"), Opt("galaksija/ROM2.BIN"), Opt("galaksija/CHRGEN.BIN") },
        ["プレイステーション2"] = new[] { On("pcsx2", Dir("pcsx2/bios", "BiosNotePs2", anyFile: true)) },
    };

    /// <summary>実際に使うBIOSフォルダ（設定が空ならデータフォルダの system）</summary>
    public static string DirOf(AppSettings app) => string.IsNullOrWhiteSpace(app.BiosDir) ? Storage.DefaultBiosDir : app.BiosDir;

    /// <summary>BIOSフォルダが無ければ作る（作れなくても何もしない）</summary>
    public static void EnsureDir(string dir) { try { Directory.CreateDirectory(dir); } catch { } }

    /// <summary>表に載っている機種（確かめ用）</summary>
    public static IEnumerable<string> Systems => Table.Keys;

    /// <summary>この機種とコアで使うBIOS（要らなければ空）</summary>
    public static List<BiosFile> For(SystemConfig sys, string? coreName = null) => Entries(sys, coreName).Select(e => e.File).ToList();

    private static List<Entry> Entries(SystemConfig sys, string? coreName)
    {
        var core = string.IsNullOrEmpty(coreName) ? CoreCatalog.CoreNameFor(sys) : coreName;
        var key = CoreCatalog.SystemKeyOf(sys);
        if (key != null && Table.TryGetValue(key, out var list))
            return list.Where(e => e.Core == null || string.Equals(e.Core, core, StringComparison.OrdinalIgnoreCase)).ToList();
        // 表に無い機種は、コア情報の「必須」を案内だけする（部品を自動で取ってくるコアは除く）
        if (string.IsNullOrEmpty(core) || CoreCatalog.HasCoreAssets(core)) return new();
        return (CoreInfoDb.Get(core)?.Firmware ?? Array.Empty<CoreFirmware>())
            .Where(f => !f.Optional)
            .Select(f => new Entry(core, new BiosFile(f.Path, true, Array.Empty<string>()), Strict: false))
            .ToList();
    }

    /// <summary>そのBIOSがあるか。決まった場所のほか、BIOSフォルダの直下（ファイル名だけ）と、昔の置き場所（コア名のフォルダ）も探す</summary>
    public static bool Found(BiosFile f, string biosDir, string core, IEnumerable<string>? extraDirs = null) =>
        Locate(f, biosDir, core, extraDirs) != null;

    /// <summary>
    /// そのBIOSの場所。名前で見つからなければ、BIOSフォルダの直下にある違う名前のファイルを中身（指紋）で見分ける。
    /// Name は見つかったものがどの名前のBIOSか、ByContent は名前ではなく中身で見つけたか
    /// </summary>
    public static BiosLocation? Locate(BiosFile f, string biosDir, string core, IEnumerable<string>? extraDirs = null)
    {
        if (f.Path == Ipl64dd)
            return FindIpl(biosDir, core) is var (ipl, _)
                ? new BiosLocation(ipl, f.FileName, !string.Equals(System.IO.Path.GetFileName(ipl), f.FileName, StringComparison.OrdinalIgnoreCase))
                : null;
        var dirs = new List<string> { biosDir };
        if (!string.IsNullOrEmpty(core)) dirs.Add(System.IO.Path.Combine(biosDir, core));
        dirs.Add(System.IO.Path.Combine(biosDir, "melonDS DS"));
        if (extraDirs != null) dirs.AddRange(extraDirs);
        foreach (var rel in f.AllPaths)
        {
            var local = rel.Replace('/', System.IO.Path.DirectorySeparatorChar);
            if (f.Folder)
            {
                var dir = System.IO.Path.Combine(biosDir, local);
                if (f.AnyFileInside ? HasAnyFile(dir) : DirExists(dir)) return new BiosLocation(dir, rel, false);
                continue;
            }
            var name = System.IO.Path.GetFileName(local);
            foreach (var d in dirs)
            {
                foreach (var candidate in new[] { System.IO.Path.Combine(d, local), System.IO.Path.Combine(d, name) })
                    if (FileExists(candidate)) return new BiosLocation(candidate, name, false);
            }
        }
        if (f.Folder) return null;
        return BiosHashes.FindByContent(f.AllPaths, dirs.Take(2)) is var (path, matched) ? new BiosLocation(path, matched, true) : null;
    }

    /// <summary>無いと起動できないBIOSのうち、見つからないもの（ファイル名を「 / 」でつなぐ。そろっていれば null）</summary>
    public static string? Missing(SystemConfig sys, string biosDir)
    {
        var core = CoreCatalog.CoreNameFor(sys);
        var extra = CoreCatalog.SystemKeyOf(sys) is "ネオジオ" or "アーケード" ? sys.RomFolders : null;
        var missing = Entries(sys, core)
            .Where(e => e.Strict && e.File.Required && !Found(e.File, biosDir, core, extra))
            .Select(e => e.File.Folder ? e.File.Path.Replace('/', '\\') + "\\" : e.File.FileName)
            .ToList();
        return missing.Count > 0 ? string.Join(" / ", missing) : null;
    }

    /// <summary>
    /// コアが決まったサブフォルダ（Mupen64plus\ など）に探すBIOSを、BIOSフォルダの直下から写す。
    /// 写し先にもうあるものは触らない。書き込めないフォルダなどで失敗しても、起動は続ける
    /// </summary>
    public static int Stage(SystemConfig sys, string core, string biosDir)
    {
        int copied = 0;
        if (For(sys, core).Any(f => f.Path == Ipl64dd) && StageIpl(biosDir, core)) copied++;
        foreach (var f in For(sys, core).Where(f => !f.Folder && f.Path != Ipl64dd))
        {
            // 違う名前で置いてあるものは、中身で見分けて、正しい名前で写す（元のファイルは残す）
            if (Locate(f, biosDir, core) is { ByContent: true } byContent)
            {
                try
                {
                    var rel = f.AllPaths.First(p => string.Equals(System.IO.Path.GetFileName(p), byContent.Name, StringComparison.OrdinalIgnoreCase));
                    var target = System.IO.Path.Combine(biosDir, rel.Replace('/', System.IO.Path.DirectorySeparatorChar));
                    if (!File.Exists(target))
                    {
                        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target)!);
                        File.Copy(byContent.Path, target, false);
                        copied++;
                    }
                }
                catch { }
                continue;
            }
            foreach (var rel in f.AllPaths.Where(p => p.Contains('/')))
            {
                try
                {
                    var target = System.IO.Path.Combine(biosDir, rel.Replace('/', System.IO.Path.DirectorySeparatorChar));
                    var flat = System.IO.Path.Combine(biosDir, System.IO.Path.GetFileName(target));
                    if (File.Exists(target) || !File.Exists(flat)) continue;
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target)!);
                    File.Copy(flat, target, false);
                    copied++;
                }
                catch { }
            }
        }
        return copied;
    }

    // ---------- 64DD の IPL（BIOS） ----------
    // Mupen64Plus は Mupen64plus\IPL.n64 を、ビッグエンディアン（.z64 と同じ並び）で読む。
    // 吸い出し器やツールによって、名前（64DD_IPL.bin など）やバイトの並び（.v64・.n64 の並び）が違うので、
    // 4MB のファイルの中身を見て IPL か確かめ、並びを直して決まった場所に置く

    private const string Ipl64dd = "Mupen64plus/IPL.n64";
    private const int IplSize = 4 * 1024 * 1024;

    /// <summary>IPL のバイトの並び（0: IPL ではない, 1: ビッグエンディアン, 2: 2バイトずつ入れ替え（.v64）, 3: 4バイトずつ逆（リトルエンディアン））</summary>
    public static int IplOrder(byte[] head)
    {
        if (head.Length < 0x28) return 0;
        string Text(int from) => System.Text.Encoding.ASCII.GetString(head, from, 8);
        var t = Text(0x20);
        if (t == "64DD IPL" || (head[0] == 0x80 && head[1] == 0x27 && head[2] == 0x07 && head[3] == 0x40)) return 1;
        if (t == "46DDI LP" || (head[0] == 0x27 && head[1] == 0x80 && head[2] == 0x40 && head[3] == 0x07)) return 2;
        if (t == "DD46LPI " || (head[0] == 0x40 && head[1] == 0x07 && head[2] == 0x27 && head[3] == 0x80)) return 3;
        return 0;
    }

    private static int IplOrderOf(string path)
    {
        try
        {
            var info = new FileInfo(path);
            if (!info.Exists || info.Length != IplSize) return 0;
            var head = new byte[0x28];
            using var fs = File.OpenRead(path);
            return fs.Read(head, 0, head.Length) == head.Length ? IplOrder(head) : 0;
        }
        catch { return 0; }
    }

    /// <summary>IPL を探す（決まった場所の IPL.n64 を先に。無ければ BIOSフォルダの直下などの 4MB のファイルを中身で見分ける）</summary>
    private static (string path, int order)? FindIpl(string biosDir, string core)
    {
        var fixedPath = System.IO.Path.Combine(biosDir, "Mupen64plus", "IPL.n64");
        if (IplOrderOf(fixedPath) is int o and > 0) return (fixedPath, o);
        var dirs = new List<string> { biosDir, System.IO.Path.Combine(biosDir, "Mupen64plus") };
        if (!string.IsNullOrEmpty(core)) dirs.Add(System.IO.Path.Combine(biosDir, core));
        foreach (var dir in dirs)
        {
            IEnumerable<string> files;
            try { files = Directory.Exists(dir) ? Directory.EnumerateFiles(dir).ToList() : Enumerable.Empty<string>(); } catch { continue; }
            foreach (var file in files.OrderBy(x => System.IO.Path.GetFileName(x).Contains("ipl", StringComparison.OrdinalIgnoreCase) ? 0 : 1))
                if (IplOrderOf(file) is int order and > 0) return (file, order);
        }
        return null;
    }

    /// <summary>バイトの並びをビッグエンディアンに直す</summary>
    public static byte[] ToBigEndian(byte[] data, int order)
    {
        var r = (byte[])data.Clone();
        if (order == 2)
            for (int i = 0; i + 1 < r.Length; i += 2) (r[i], r[i + 1]) = (r[i + 1], r[i]);
        else if (order == 3)
            for (int i = 0; i + 3 < r.Length; i += 4) { (r[i], r[i + 3]) = (r[i + 3], r[i]); (r[i + 1], r[i + 2]) = (r[i + 2], r[i + 1]); }
        return r;
    }

    /// <summary>
    /// IPL を Mupen64plus\IPL.n64 にビッグエンディアンで置く。もう正しく置いてあれば何もしない。
    /// 決まった場所に並びの違う IPL.n64 があったときは、IPL.n64.bak に残してから置き直す
    /// </summary>
    private static bool StageIpl(string biosDir, string core)
    {
        try
        {
            var found = FindIpl(biosDir, core);
            if (found is not var (src, order)) return false;
            var target = System.IO.Path.Combine(biosDir, "Mupen64plus", "IPL.n64");
            if (order == 1 && string.Equals(System.IO.Path.GetFullPath(src), System.IO.Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase)) return false;
            var data = ToBigEndian(File.ReadAllBytes(src), order);
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target)!);
            if (File.Exists(target))
            {
                var bak = target + ".bak";
                for (int n = 2; File.Exists(bak); n++) bak = target + ".bak" + n;
                File.Move(target, bak);
            }
            File.WriteAllBytes(target, data);
            return true;
        }
        catch { return false; }
    }

    private static bool FileExists(string p) { try { return File.Exists(p); } catch { return false; } }
    private static bool DirExists(string p) { try { return Directory.Exists(p); } catch { return false; } }
    private static bool HasAnyFile(string dir)
    {
        try { return Directory.Exists(dir) && Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories).Any(); } catch { return false; }
    }
}
