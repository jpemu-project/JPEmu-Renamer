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
        ["ゲームボーイアドバンス"] = (new[] { "mgba", "gpsp" }, "gba_bios.bin?"),
        ["バーチャルボーイ"] = (new[] { "mednafen_vb" }, ""),
        ["ニンテンドーDS"] = (new[] { "melonds", "desmume" }, "bios7.bin / bios9.bin / firmware.bin?"),
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
        ["プレイステーション"] = (new[] { "pcsx_rearmed", "swanstation" }, "scph5500.bin / scph5501.bin / scph5502.bin?"),
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
        // 候補の英語名（Famicom / NES など）で作った機種も同じ扱いにする
        if (SystemPresets.JapaneseNameOf(sys.Name) is string ja && BySystem.ContainsKey(ja)) return ja;
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
        var bios = key != null ? BySystem[key].bios : "";
        // 末尾の ? は「無くても動く（任意）」の印
        return bios.EndsWith('?') ? Loc.F("BiosOptional", bios.TrimEnd('?')) : bios;
    }

    /// <summary>機種名の表示用（日本語以外では候補の英語名にする）</summary>
    public static string SystemDisplayName(string japaneseKey) =>
        SystemPresets.All.FirstOrDefault(p => p.Name == japaneseKey)?.DisplayName ?? japaneseKey;

    /// <summary>この機種で使うコア（設定が空なら候補の先頭）</summary>
    public static string CoreNameFor(SystemConfig sys)
    {
        if (!string.IsNullOrWhiteSpace(sys.CoreName)) return sys.CoreName;
        var key = SystemKey(sys);
        return key != null ? BySystem[key].cores[0] : "";
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

    /// <summary>コアをダウンロードして cores フォルダに置く（既にあれば更新）</summary>
    public static async Task DownloadAsync(string coreName, IProgress<double>? progress, CancellationToken ct)
    {
        if (!Cores.ContainsKey(coreName)) throw new ArgumentException(coreName);
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
