// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using JPEmuRenamer.Models;

namespace JPEmuRenamer.Services;

/// <summary>ROMのCRC32をDATと照合して一覧の行を作る</summary>
public static class Matcher
{
    public static List<GameItem> Match(List<DatEntry> dat, List<RomFile> roms, SystemConfig sys)
    {
        var byCrc = new Dictionary<uint, DatEntry>();
        foreach (var d in dat.Where(d => d.Crc.HasValue))
            byCrc.TryAdd(d.Crc!.Value, d);

        var items = new List<GameItem>();
        var matchedTitles = new HashSet<string>(StringComparer.Ordinal);
        var takenTargets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 既に正しい名前になっているファイルを先に確定させ、重複判定で優先する
        var ordered = roms
            .Select(r => (rom: r, dat: Find(r, byCrc, sys.HeaderMode)))
            .OrderBy(x => x.dat != null && IsAlreadyNamed(x.rom, x.dat) ? 0 : 1)
            .ThenBy(x => x.rom.Path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var (rom, d) in ordered)
        {
            if (d == null)
            {
                items.Add(new GameItem(MatchStatus.NotInDat, System.IO.Path.GetFileName(rom.Path), null, rom, ""));
                continue;
            }

            var dir = System.IO.Path.GetDirectoryName(rom.Path) ?? "";
            var target = SafeFileName(d.Title) + System.IO.Path.GetExtension(rom.Path);
            var targetFull = System.IO.Path.Combine(dir, target);
            bool firstForTitle = matchedTitles.Add(d.Title);

            if (sys.DisplayRenameOnly)
            {
                items.Add(new GameItem(MatchStatus.Match, d.Title, d, rom, target));
            }
            else if (IsAlreadyNamed(rom, d))
            {
                takenTargets.Add(targetFull);
                items.Add(new GameItem(firstForTitle ? MatchStatus.Match : MatchStatus.Duplicate, d.Title, d, rom, target));
            }
            else if (!firstForTitle || takenTargets.Contains(targetFull) ||
                     (File.Exists(targetFull) && !SamePath(targetFull, rom.Path)))
            {
                items.Add(new GameItem(MatchStatus.Duplicate, d.Title, d, rom, target));
            }
            else
            {
                takenTargets.Add(targetFull);
                items.Add(new GameItem(MatchStatus.NameDiff, d.Title, d, rom, target));
            }
        }

        // DATにあるが手元に無いタイトル（タイトル単位で1行）
        foreach (var d in dat.GroupBy(d => d.Title).Select(g => g.First()))
        {
            if (!matchedTitles.Contains(d.Title))
                items.Add(new GameItem(MatchStatus.Missing, d.Title, d, null, ""));
        }

        // DATの収録順（なければタイトル順）に並べ、DATに無いファイルは最後
        var order = new Dictionary<string, int>();
        for (int i = 0; i < dat.Count; i++) order.TryAdd(dat[i].Title, i);
        return items
            .OrderBy(i => i.Status == MatchStatus.NotInDat ? 1 : 0)
            .ThenBy(i => i.Dat != null && order.TryGetValue(i.Dat.Title, out var o) ? o : int.MaxValue)
            .ThenBy(i => i.Title, StringComparer.CurrentCulture)
            .ToList();
    }

    private static DatEntry? Find(RomFile rom, Dictionary<uint, DatEntry> byCrc, string headerMode)
    {
        switch (headerMode)
        {
            case "Include":
                return byCrc.GetValueOrDefault(rom.Crc);
            case "Exclude":
                return byCrc.GetValueOrDefault(rom.CrcNoHeader ?? rom.Crc);
            default:
                if (byCrc.TryGetValue(rom.Crc, out var a)) return a;
                if (rom.CrcNoHeader.HasValue && byCrc.TryGetValue(rom.CrcNoHeader.Value, out var b)) return b;
                return null;
        }
    }

    private static bool IsAlreadyNamed(RomFile rom, DatEntry d) =>
        string.Equals(System.IO.Path.GetFileNameWithoutExtension(rom.Path), SafeFileName(d.Title), StringComparison.Ordinal);

    private static bool SamePath(string a, string b) =>
        string.Equals(System.IO.Path.GetFullPath(a), System.IO.Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);

    /// <summary>Windowsで使えない文字を全角に置き換える</summary>
    public static string SafeFileName(string title)
    {
        var sb = new System.Text.StringBuilder(title.Length);
        foreach (var ch in title)
        {
            sb.Append(ch switch
            {
                '\\' => '＼',
                '/' => '／',
                ':' => '：',
                '*' => '＊',
                '?' => '？',
                '"' => '”',
                '<' => '＜',
                '>' => '＞',
                '|' => '｜',
                _ when ch < 0x20 => ' ',
                _ => ch
            });
        }
        var s = sb.ToString().Trim().TrimEnd('.');
        return s.Length == 0 ? "_" : s;
    }
}
