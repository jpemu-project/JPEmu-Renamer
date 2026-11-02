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
        new("genesis_plus_gx", "Genesis Plus GX"),
        new("picodrive", "PicoDrive"),
        new("mednafen_saturn", "Beetle Saturn"),
        new("yabause", "Yabause"),
        new("mednafen_pce_fast", "Beetle PCE FAST"),
        new("mednafen_pce", "Beetle PCE"),
        new("mednafen_wswan", "Beetle WonderSwan"),
        new("mednafen_ngp", "Beetle NeoPop"),
        new("fbneo", "FinalBurn Neo"),
        new("pcsx_rearmed", "PCSX ReARMed"),
        new("swanstation", "SwanStation"),
    }.ToDictionary(c => c.Name);

    /// <summary>機種名 → コアの候補（先頭が既定）と、必要なBIOSの説明</summary>
    private static readonly Dictionary<string, (string[] cores, string bios)> BySystem = new()
    {
        ["ファミコン"] = (new[] { "mesen", "fceumm", "nestopia" }, ""),
        ["ディスクシステム"] = (new[] { "mesen", "fceumm", "nestopia" }, "disksys.rom"),
        ["スーパーファミコン"] = (new[] { "snes9x", "bsnes", "mesen-s" }, ""),
        ["NINTENDO64"] = (new[] { "mupen64plus_next", "parallel_n64" }, ""),
        ["ゲームボーイ"] = (new[] { "gambatte", "sameboy", "mgba" }, ""),
        ["ゲームボーイカラー"] = (new[] { "gambatte", "sameboy", "mgba" }, ""),
        ["ゲームボーイアドバンス"] = (new[] { "mgba", "gpsp" }, "gba_bios.bin（任意）"),
        ["バーチャルボーイ"] = (new[] { "mednafen_vb" }, ""),
        ["ニンテンドーDS"] = (new[] { "melonds", "desmume" }, "bios7.bin / bios9.bin / firmware.bin（melonDSは任意）"),
        ["SG-1000"] = (new[] { "genesis_plus_gx" }, ""),
        ["マスターシステム"] = (new[] { "genesis_plus_gx", "picodrive" }, ""),
        ["メガドライブ"] = (new[] { "genesis_plus_gx", "picodrive" }, ""),
        ["ゲームギア"] = (new[] { "genesis_plus_gx" }, ""),
        ["メガCD"] = (new[] { "genesis_plus_gx", "picodrive" }, "bios_CD_J.bin"),
        ["セガサターン"] = (new[] { "mednafen_saturn", "yabause" }, "sega_101.bin"),
        ["PCエンジン"] = (new[] { "mednafen_pce_fast", "mednafen_pce" }, ""),
        ["PCエンジン CD-ROM²"] = (new[] { "mednafen_pce_fast", "mednafen_pce" }, "syscard3.pce"),
        ["ワンダースワン"] = (new[] { "mednafen_wswan" }, ""),
        ["ワンダースワンカラー"] = (new[] { "mednafen_wswan" }, ""),
        ["ネオジオポケット"] = (new[] { "mednafen_ngp" }, ""),
        ["ネオジオポケットカラー"] = (new[] { "mednafen_ngp" }, ""),
        ["ネオジオ"] = (new[] { "fbneo" }, "neogeo.zip"),
        ["プレイステーション"] = (new[] { "pcsx_rearmed", "swanstation" }, "scph5500.bin など（PCSX ReARMedは任意）"),
    };

    /// <summary>拡張子から推測するときの対応（機種名が候補と違う場合）</summary>
    private static readonly (string ext, string system)[] ByExtension =
    {
        (".nes", "ファミコン"), (".fds", "ディスクシステム"), (".sfc", "スーパーファミコン"), (".smc", "スーパーファミコン"),
        (".z64", "NINTENDO64"), (".n64", "NINTENDO64"), (".v64", "NINTENDO64"), (".gb", "ゲームボーイ"), (".gbc", "ゲームボーイカラー"),
        (".gba", "ゲームボーイアドバンス"), (".vb", "バーチャルボーイ"), (".nds", "ニンテンドーDS"), (".sg", "SG-1000"),
        (".sms", "マスターシステム"), (".md", "メガドライブ"), (".gen", "メガドライブ"), (".gg", "ゲームギア"), (".pce", "PCエンジン"),
        (".ws", "ワンダースワン"), (".wsc", "ワンダースワンカラー"), (".ngp", "ネオジオポケット"), (".ngc", "ネオジオポケットカラー"),
        (".pbp", "プレイステーション"),
    };

    public static IReadOnlyCollection<CoreInfo> All => Cores.Values;

    public static CoreInfo? Get(string name) => Cores.GetValueOrDefault(name);

    private static string? SystemKey(SystemConfig sys)
    {
        if (BySystem.ContainsKey(sys.Name)) return sys.Name;
        foreach (var (ext, system) in ByExtension)
            if (sys.Extensions.Contains(ext, StringComparer.OrdinalIgnoreCase)) return system;
        return null;
    }

    /// <summary>この機種で選べるコア（候補が無い機種はすべて）</summary>
    public static List<CoreInfo> CandidatesFor(SystemConfig sys)
    {
        var key = SystemKey(sys);
        return key != null ? BySystem[key].cores.Select(n => Cores[n]).ToList() : Cores.Values.OrderBy(c => c.DisplayName).ToList();
    }

    public static string BiosHintFor(SystemConfig sys)
    {
        var key = SystemKey(sys);
        return key != null ? BySystem[key].bios : "";
    }

    /// <summary>この機種で使うコア（設定が空なら候補の先頭）</summary>
    public static string CoreNameFor(SystemConfig sys)
    {
        if (!string.IsNullOrWhiteSpace(sys.CoreName)) return sys.CoreName;
        var key = SystemKey(sys);
        return key != null ? BySystem[key].cores[0] : "";
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

    /// <summary>コアをダウンロードして cores フォルダに置く（既にあれば更新）</summary>
    public static async Task DownloadAsync(string coreName, IProgress<double>? progress, CancellationToken ct)
    {
        if (!Cores.ContainsKey(coreName)) throw new ArgumentException(coreName);
        Directory.CreateDirectory(Storage.CoresDir);
        var url = BuildbotBase + coreName + "_libretro.dll.zip";
        var tmpZip = System.IO.Path.Combine(Storage.CoresDir, coreName + ".download");

        using (var resp = await Http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct))
        {
            resp.EnsureSuccessStatusCode();
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

        try
        {
            using var zip = ZipFile.OpenRead(tmpZip);
            var entry = zip.Entries.FirstOrDefault(e => e.Name.Equals(coreName + "_libretro.dll", StringComparison.OrdinalIgnoreCase))
                        ?? zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                        ?? throw new InvalidDataException("No DLL in the downloaded archive.");
            var tmpDll = DllPath(coreName) + ".new";
            entry.ExtractToFile(tmpDll, true);
            File.Move(tmpDll, DllPath(coreName), true);
        }
        finally
        {
            try { File.Delete(tmpZip); } catch { }
        }
    }
}
