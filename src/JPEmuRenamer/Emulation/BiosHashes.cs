// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Security.Cryptography;

namespace JPEmuRenamer.Emulation;

/// <summary>
/// BIOSの指紋（md5）とファイル名の表。libretro-database の System.dat から作った BiosHashes.txt（CC BY-SA 4.0）をアプリに入れてある。
/// 利用者が違う名前で置いたBIOSを中身で見分けるのと、名前は合っているのに中身が違うものに気づくのに使う
/// </summary>
public static class BiosHashes
{
    private sealed class Db
    {
        public readonly Dictionary<string, HashSet<string>> NamesByMd5 = new();
        public readonly Dictionary<string, HashSet<string>> Md5sByName = new(StringComparer.OrdinalIgnoreCase);
        public readonly HashSet<long> Sizes = new();
    }

    private static readonly Lazy<Db> Data = new(Load);

    private static Db Load()
    {
        var db = new Db();
        try
        {
            using var s = typeof(BiosHashes).Assembly.GetManifestResourceStream("JPEmuRenamer.Emulation.BiosHashes.txt");
            if (s == null) return db;
            using var r = new StreamReader(s);
            while (r.ReadLine() is string line)
            {
                if (line.Length == 0 || line[0] == '#') continue;
                var p = line.Split('\t');
                if (p.Length < 3 || !long.TryParse(p[1], out var size)) continue;
                var name = Path.GetFileName(p[2]);
                Add(db.NamesByMd5, p[0], name);
                Add(db.Md5sByName, name, p[0]);
                db.Sizes.Add(size);
            }
        }
        catch { }
        return db;
    }

    private static void Add(Dictionary<string, HashSet<string>> d, string key, string value)
    {
        if (!d.TryGetValue(key, out var set)) d[key] = set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        set.Add(value);
    }

    /// <summary>表に載っているファイルの数（確かめ用）</summary>
    public static int Count => Data.Value.NamesByMd5.Count;

    /// <summary>このファイル名の正しい中身が表に載っているか</summary>
    public static bool Knows(string fileName) => Data.Value.Md5sByName.ContainsKey(Path.GetFileName(fileName));

    // 一度計算した指紋は、ファイルの大きさと更新日時が同じあいだ覚えておく
    private static readonly Dictionary<string, (long len, DateTime time, string md5)> Cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>ファイルの指紋（表に載っている大きさのファイルだけ計算する。それ以外・読めないときは null）</summary>
    public static string? Md5Of(string path)
    {
        try
        {
            var info = new FileInfo(path);
            if (!info.Exists || !Data.Value.Sizes.Contains(info.Length)) return null;
            lock (Cache)
                if (Cache.TryGetValue(path, out var c) && c.len == info.Length && c.time == info.LastWriteTimeUtc) return c.md5;
            string md5;
            using (var fs = File.OpenRead(path)) md5 = Convert.ToHexString(MD5.HashData(fs)).ToLowerInvariant();
            lock (Cache) Cache[path] = (info.Length, info.LastWriteTimeUtc, md5);
            return md5;
        }
        catch { return null; }
    }

    /// <summary>このファイルの中身が、表でこの名前のものとして載っているか</summary>
    public static bool Matches(string path, string fileName) =>
        Md5Of(path) is string md5 && Data.Value.NamesByMd5.TryGetValue(md5, out var names) && names.Contains(Path.GetFileName(fileName));

    /// <summary>
    /// 名前は合っているが、中身が表のどれとも違う（壊れている・別の版の可能性がある）か。
    /// 表にその名前が載っていない・ZIP（作り方で中身が変わる）は、分からないので false
    /// </summary>
    public static bool LooksWrong(string path, string fileName)
    {
        var name = Path.GetFileName(fileName);
        if (!Knows(name) || name.EndsWith(".zip", StringComparison.OrdinalIgnoreCase)) return false;
        try
        {
            if (!File.Exists(path)) return false;
            var md5 = Md5Of(path);
            return md5 == null || !Data.Value.Md5sByName[name].Contains(md5);
        }
        catch { return false; }
    }

    /// <summary>
    /// 中身で探す。dirs の直下のファイルのうち、表で names のどれかとして載っているものを返す（見つからなければ null）。
    /// 返すのは（見つけたファイル, どの名前のものだったか）
    /// </summary>
    public static (string path, string name)? FindByContent(IEnumerable<string> names, IEnumerable<string> dirs)
    {
        var wanted = names.Select(Path.GetFileName).Where(n => !string.IsNullOrEmpty(n) && Knows(n!)).Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (wanted.Count == 0) return null;
        foreach (var dir in dirs.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            IEnumerable<string> files;
            try { files = Directory.Exists(dir) ? Directory.EnumerateFiles(dir).ToList() : Enumerable.Empty<string>(); } catch { continue; }
            foreach (var file in files)
            {
                if (Md5Of(file) is not string md5 || !Data.Value.NamesByMd5.TryGetValue(md5, out var known)) continue;
                var hit = wanted.FirstOrDefault(known.Contains);
                if (hit != null) return (file, hit);
            }
        }
        return null;
    }
}
