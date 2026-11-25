// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using JPEmuRenamer.Emulation;
using JPEmuRenamer.Models;

namespace JPEmuRenamer.Services;

/// <summary>
/// パッケージ・タイトル画面・ゲーム画面の画像を、RetroArch と同じ libretro の画像集（libretro-thumbnails）から取ってくる。
/// 画像集のファイル名は No-Intro の英語名なので、ROM の CRC から libretro-database の DAT で英語名を引いて探す。
/// 取ってきたものは data\thumbnails に保存し、次からはネットにつながずに表示する。
/// 画像の権利はゲームの作り手にあるので、アプリには入れず、利用者が表示を選んだときだけ取ってくる
/// </summary>
public static class Thumbnails
{
    public const string Boxart = "Boxart", Title = "Title", Snap = "Snap";

    private static string Folder(string kind) => kind switch { Boxart => "Named_Boxarts", Title => "Named_Titles", _ => "Named_Snaps" };

    public static string RootDir => Path.Combine(Storage.DataDir, "thumbnails");

    /// <summary>
    /// 機種 → libretro の機種名（画像集・DAT の名前）。先頭から順に探す。
    /// ここに無い機種は、使うコアのコア情報にある機種名を使う
    /// </summary>
    private static readonly Dictionary<string, string[]> Systems = new()
    {
        ["ファミコン"] = new[] { "Nintendo - Nintendo Entertainment System" },
        ["ディスクシステム"] = new[] { "Nintendo - Family Computer Disk System" },
        ["スーパーファミコン"] = new[] { "Nintendo - Super Nintendo Entertainment System" },
        ["サテラビュー"] = new[] { "Nintendo - Satellaview" },
        ["スーファミターボ"] = new[] { "Nintendo - Sufami Turbo" },
        ["スーパーゲームボーイ"] = new[] { "Nintendo - Game Boy" },
        ["NINTENDO64"] = new[] { "Nintendo - Nintendo 64" },
        ["64DD"] = new[] { "Nintendo - Nintendo 64DD" },
        ["ゲームボーイ"] = new[] { "Nintendo - Game Boy" },
        ["ゲームボーイカラー"] = new[] { "Nintendo - Game Boy Color" },
        ["ゲームボーイアドバンス"] = new[] { "Nintendo - Game Boy Advance" },
        ["バーチャルボーイ"] = new[] { "Nintendo - Virtual Boy" },
        ["ポケモンミニ"] = new[] { "Nintendo - Pokemon Mini" },
        ["ニンテンドーDS"] = new[] { "Nintendo - Nintendo DS" },
        ["ニンテンドーDSiウェア"] = new[] { "Nintendo - Nintendo DSi (Digital)", "Nintendo - Nintendo DSi" },
        ["ニンテンドー3DS"] = new[] { "Nintendo - Nintendo 3DS" },
        ["ゲームキューブ"] = new[] { "Nintendo - GameCube" },
        ["Wii"] = new[] { "Nintendo - Wii" },
        ["SG-1000"] = new[] { "Sega - SG-1000" },
        ["SC-3000"] = new[] { "Sega - SG-1000" },
        ["マスターシステム"] = new[] { "Sega - Master System - Mark III" },
        ["メガドライブ"] = new[] { "Sega - Mega Drive - Genesis" },
        ["スーパー32X"] = new[] { "Sega - 32X" },
        ["メガCD"] = new[] { "Sega - Mega-CD - Sega CD" },
        ["キッズコンピュータ ピコ"] = new[] { "Sega - PICO" },
        ["ゲームギア"] = new[] { "Sega - Game Gear" },
        ["セガサターン"] = new[] { "Sega - Saturn" },
        ["ドリームキャスト"] = new[] { "Sega - Dreamcast" },
        ["NAOMI"] = new[] { "Sega - Naomi" },
        ["NAOMI 2"] = new[] { "Sega - Naomi 2" },
        ["アトミスウェイブ"] = new[] { "Atomiswave" },
        ["PCエンジン"] = new[] { "NEC - PC Engine - TurboGrafx 16" },
        ["PCエンジン スーパーグラフィックス"] = new[] { "NEC - PC Engine SuperGrafx" },
        ["PCエンジン CD-ROM²"] = new[] { "NEC - PC Engine CD - TurboGrafx-CD" },
        ["ワンダースワン"] = new[] { "Bandai - WonderSwan" },
        ["ワンダースワンカラー"] = new[] { "Bandai - WonderSwan Color" },
        ["ネオジオポケット"] = new[] { "SNK - Neo Geo Pocket" },
        ["ネオジオポケットカラー"] = new[] { "SNK - Neo Geo Pocket Color" },
        ["ネオジオ"] = new[] { "FBNeo - Arcade Games" },
        ["アーケード"] = new[] { "FBNeo - Arcade Games", "MAME" },
        ["CPS1"] = new[] { "FBNeo - Arcade Games" },
        ["CPS2"] = new[] { "FBNeo - Arcade Games" },
        ["CPS3"] = new[] { "FBNeo - Arcade Games" },
        ["MSX"] = new[] { "Microsoft - MSX" },
        ["MSX2"] = new[] { "Microsoft - MSX2" },
        ["コモドール128"] = new[] { "Commodore - 64" },
        ["Amiga CD32"] = new[] { "Commodore - CD32" },
        ["CDTV"] = new[] { "Commodore - CDTV" },
        ["Atari 800"] = new[] { "Atari - 8-bit Family" },
        ["アタリ ジャガーCD"] = new[] { "Atari - Jaguar CD" },
        ["スペクトラビデオ"] = new[] { "Spectravideo - SVI-318 - SVI-328" },
        ["クリエイティビジョン"] = new[] { "VTech - CreatiVision" },
        ["ビデオパック+"] = new[] { "Philips - Videopac+" },
        ["アルカディア2001"] = new[] { "Emerson - Arcadia 2001" },
        ["VC 4000"] = new[] { "Interton - VC 4000" },
        ["TV-Computer"] = new[] { "Videoton - TV-Computer" },
        ["Enterprise 128"] = new[] { "Enterprise - 128" },
        ["コレコ アダム"] = new[] { "Coleco - ColecoVision" },
        ["マイビジョン"] = new[] { "Nichibutsu - My Vision" },
    };

    /// <summary>この機種の libretro の機種名（候補の順。分からなければ空）</summary>
    public static List<string> DatabasesFor(SystemConfig sys)
    {
        var key = CoreCatalog.SystemKeyOf(sys);
        if (key != null && Systems.TryGetValue(key, out var list)) return list.ToList();
        var core = CoreCatalog.CoreNameFor(sys);
        return string.IsNullOrEmpty(core) ? new() : (CoreInfoDb.Get(core)?.Databases ?? Array.Empty<string>()).Take(1).ToList();
    }

    // ---------- 名前 ----------

    /// <summary>画像集のファイル名で使えない文字（RetroArch と同じ置き換え）</summary>
    public static string SafeName(string name) => Regex.Replace(name, "[&*/:`<>?\\\\|\"]", "_");

    /// <summary>（）や［］の付記を除いた、比べるための名前</summary>
    private static string Base(string name) => Regex.Replace(Regex.Replace(name, @"\s*[\(\[][^\)\]]*[\)\]]", ""), @"\s+", " ").Trim().ToLowerInvariant();

    private static bool IsAscii(string s) => s.Length > 0 && s.All(c => c < 0x80);

    /// <summary>このゲームの、画像集で探す名前の候補（先のものほど確か）</summary>
    private static async Task<List<string>> NamesAsync(SystemConfig sys, GameItem item, string db, CancellationToken ct)
    {
        var names = new List<string>();
        var crcs = new[] { item.Dat?.Crc, item.Rom?.Crc }.Where(c => c is > 0).Select(c => c!.Value).Distinct();
        var index = await IndexAsync(db, ct);
        foreach (var crc in crcs)
            if (index.TryGetValue(crc, out var n)) names.Add(n);
        // アーケードは DAT のゲーム名（英語）、No-Intro の DAT を使っている人は DAT の名前そのもの
        if (item.Dat is { } d)
        {
            if (IsAscii(d.Description)) names.Add(d.Description);
            if (IsAscii(d.Title)) names.Add(d.Title);
        }
        if (item.Rom != null && IsAscii(Path.GetFileNameWithoutExtension(item.Rom.Path))) names.Add(Path.GetFileNameWithoutExtension(item.Rom.Path));
        return names.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    // ---------- CRC → 英語名（libretro-database の DAT） ----------

    private static readonly Dictionary<string, Dictionary<uint, string>> Indexes = new();
    private static readonly SemaphoreSlim IndexLock = new(1, 1);

    private static async Task<Dictionary<uint, string>> IndexAsync(string db, CancellationToken ct)
    {
        await IndexLock.WaitAsync(ct);
        try
        {
            if (Indexes.TryGetValue(db, out var cached)) return cached;
            var file = Path.Combine(RootDir, SafeName(db), "crc-index.tsv");
            var map = new Dictionary<uint, string>();
            if (!File.Exists(file))
            {
                foreach (var dir in new[] { "metadat/no-intro", "dat" })
                {
                    var text = await GetStringAsync($"https://raw.githubusercontent.com/libretro/libretro-database/master/{dir}/{Uri.EscapeDataString(db)}.dat", ct);
                    if (text == null) continue;
                    ParseDat(text, map);
                    if (map.Count > 0) break;
                }
                // 取れなかったときもファイルは作らない（次の起動でもう一度試す）
                if (map.Count > 0)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                    File.WriteAllLines(file, map.Select(kv => kv.Key.ToString("X8") + "\t" + kv.Value));
                }
            }
            else
            {
                foreach (var line in File.ReadLines(file))
                {
                    var p = line.Split('\t');
                    if (p.Length == 2 && uint.TryParse(p[0], System.Globalization.NumberStyles.HexNumber, null, out var crc)) map[crc] = p[1];
                }
            }
            Indexes[db] = map;
            return map;
        }
        finally { IndexLock.Release(); }
    }

    private static readonly Regex GameRe = new(@"game\s*\(\s*name\s+""(?<n>[^""]+)""(?<body>.*?)\n\)", RegexOptions.Singleline);
    private static readonly Regex CrcRe = new(@"\bcrc\s+(?<c>[0-9A-Fa-f]{8})\b");

    internal static void ParseDat(string text, Dictionary<uint, string> map)
    {
        foreach (Match g in GameRe.Matches(text.Replace("\r\n", "\n")))
            foreach (Match c in CrcRe.Matches(g.Groups["body"].Value))
                map.TryAdd(Convert.ToUInt32(c.Groups["c"].Value, 16), g.Groups["n"].Value);
    }

    // ---------- 画像集にあるファイルの一覧（似た名前を探すため） ----------

    private static readonly Dictionary<string, Dictionary<string, List<string>>?> Trees = new();

    /// <summary>画像集のファイル名の一覧（種類ごと）。取れなければ null（そのときは名前をそのまま試す）</summary>
    private static async Task<Dictionary<string, List<string>>?> TreeAsync(string db, CancellationToken ct)
    {
        lock (Trees) if (Trees.TryGetValue(db, out var t)) return t;
        var file = Path.Combine(RootDir, SafeName(db), "files.txt");
        var result = new Dictionary<string, List<string>>();
        if (!File.Exists(file) || File.GetLastWriteTimeUtc(file) < DateTime.UtcNow.AddDays(-30))
        {
            var json = await GetStringAsync($"https://api.github.com/repos/libretro-thumbnails/{RepoName(db)}/git/trees/master?recursive=1", ct);
            if (json != null)
            {
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var paths = doc.RootElement.GetProperty("tree").EnumerateArray()
                        .Select(e => e.GetProperty("path").GetString() ?? "").Where(p => p.StartsWith("Named_") && p.EndsWith(".png")).ToList();
                    Directory.CreateDirectory(Path.GetDirectoryName(file)!);
                    File.WriteAllLines(file, paths);
                }
                catch { }
            }
        }
        if (File.Exists(file))
        {
            foreach (var p in File.ReadLines(file))
            {
                var i = p.IndexOf('/');
                if (i <= 0) continue;
                var folder = p[..i];
                if (!result.TryGetValue(folder, out var list)) result[folder] = list = new();
                list.Add(Path.GetFileNameWithoutExtension(p[(i + 1)..]));
            }
        }
        var final = result.Count > 0 ? result : null;
        lock (Trees) Trees[db] = final;
        return final;
    }

    private static string RepoName(string db) => db.Replace(' ', '_');

    /// <summary>候補の名前から、画像集に実際にあるファイル名を決める（無ければ一覧に似た名前があるか見る）</summary>
    public static string? Pick(IReadOnlyList<string> names, List<string>? files)
    {
        if (files == null) return names.Count > 0 ? SafeName(names[0]) : null;
        var set = new HashSet<string>(files, StringComparer.OrdinalIgnoreCase);
        foreach (var n in names)
            if (set.Contains(SafeName(n))) return files.First(f => string.Equals(f, SafeName(n), StringComparison.OrdinalIgnoreCase));
        // 版や地域の違いだけなら、付記を除いた名前で探す（日本版を先に）
        foreach (var n in names)
        {
            var b = Base(SafeName(n));
            var hits = files.Where(f => Base(f) == b).ToList();
            if (hits.Count == 0) continue;
            return hits.FirstOrDefault(f => f.Contains("(Japan", StringComparison.OrdinalIgnoreCase)) ?? hits[0];
        }
        return null;
    }

    // ---------- 取ってくる ----------

    private static readonly HashSet<string> NotFound = new();

    /// <summary>もう保存してある画像（無ければ null）。ネットにはつながない</summary>
    public static string? Cached(SystemConfig sys, GameItem item, string kind)
    {
        foreach (var db in DatabasesFor(sys))
        {
            var dir = Path.Combine(RootDir, SafeName(db), Folder(kind));
            var key = KeyOf(item);
            var map = Path.Combine(dir, "_games.tsv");
            if (key == null || !File.Exists(map)) continue;
            foreach (var line in File.ReadLines(map))
            {
                var p = line.Split('\t');
                if (p.Length == 2 && p[0] == key)
                {
                    var path = Path.Combine(dir, p[1] + ".png");
                    if (File.Exists(path)) return path;
                }
            }
        }
        return null;
    }

    /// <summary>このゲームを見分ける印（CRC か、無ければ名前）</summary>
    private static string? KeyOf(GameItem item) =>
        (item.Dat?.Crc ?? item.Rom?.Crc) is uint crc and > 0 ? crc.ToString("X8")
        : item.Dat?.SetName is { Length: > 0 } set ? "set:" + set
        : item.Rom != null ? "file:" + Path.GetFileName(item.Rom.Path) : null;

    /// <summary>画像を探して、保存した場所を返す（見つからなければ null）</summary>
    public static async Task<string?> GetAsync(SystemConfig sys, GameItem item, string kind, CancellationToken ct)
    {
        if (Cached(sys, item, kind) is string hit) return hit;
        var key = KeyOf(item);
        if (key == null) return null;
        foreach (var db in DatabasesFor(sys))
        {
            var miss = db + "|" + kind + "|" + key;
            lock (NotFound) if (NotFound.Contains(miss)) continue;
            var names = await NamesAsync(sys, item, db, ct);
            if (names.Count == 0) { lock (NotFound) NotFound.Add(miss); continue; }
            var tree = await TreeAsync(db, ct);
            var name = Pick(names, tree == null ? null : tree.GetValueOrDefault(Folder(kind)) ?? new List<string>());
            if (name == null) { lock (NotFound) NotFound.Add(miss); continue; }
            var data = await GetBytesAsync($"https://raw.githubusercontent.com/libretro-thumbnails/{RepoName(db)}/master/{Folder(kind)}/{Uri.EscapeDataString(name)}.png", ct)
                       ?? await GetBytesAsync($"https://thumbnails.libretro.com/{Uri.EscapeDataString(db)}/{Folder(kind)}/{Uri.EscapeDataString(name)}.png", ct);
            if (data == null || data.Length < 8 || data[0] != 0x89) { lock (NotFound) NotFound.Add(miss); continue; }
            var dir = Path.Combine(RootDir, SafeName(db), Folder(kind));
            Directory.CreateDirectory(dir);
            var path = Path.Combine(dir, name + ".png");
            await File.WriteAllBytesAsync(path, data, ct);
            await File.AppendAllTextAsync(Path.Combine(dir, "_games.tsv"), key + "\t" + name + Environment.NewLine, ct);
            return path;
        }
        return null;
    }

    private static readonly HttpClient Http = CreateHttp();

    private static HttpClient CreateHttp()
    {
        var h = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        h.DefaultRequestHeaders.UserAgent.ParseAdd("JPEmuRenamer/1.0");
        return h;
    }

    private static async Task<string?> GetStringAsync(string url, CancellationToken ct)
    {
        try
        {
            using var r = await Http.GetAsync(url, ct);
            return r.IsSuccessStatusCode ? await r.Content.ReadAsStringAsync(ct) : null;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch { return null; }
    }

    private static async Task<byte[]?> GetBytesAsync(string url, CancellationToken ct)
    {
        try
        {
            using var r = await Http.GetAsync(url, ct);
            return r.IsSuccessStatusCode ? await r.Content.ReadAsByteArrayAsync(ct) : null;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch { return null; }
    }
}
