// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Text.RegularExpressions;
using JPEmuRenamer.Models;

namespace JPEmuRenamer.Services;

/// <summary>
/// アーケードの照合。アーケードのROMは、中身ではなく「ROMセットのZIPの名前」（例：sf2.zip）で決まるので、
/// ファイル名をDATのセット名（game の name）と照らし合わせる。表示名はDATの説明（description）を使い、ファイル名は変えない。
/// </summary>
public static class ArcadeMatcher
{
    /// <summary>
    /// 1つのDAT（FinalBurn Neo や MAME の全アーケード）から、その機種の基板のゲームだけを選ぶ。
    /// DATの sourcefile（どのドライバーで動くか。例：capcom/d_cps1.cpp）で見分ける。キーは候補の日本語名
    /// </summary>
    private static readonly Dictionary<string, Regex> Boards = new()
    {
        ["CPS1"] = new(@"(^|/)(d_)?cps1\.cpp$", RegexOptions.IgnoreCase),
        ["CPS2"] = new(@"(^|/)(d_)?cps2(_custom)?\.cpp$", RegexOptions.IgnoreCase),
        ["CPS3"] = new(@"(^|/)(d_)?cps3\.cpp$", RegexOptions.IgnoreCase),
        ["ネオジオ"] = new(@"(^|/)(d_)?neogeo\.cpp$", RegexOptions.IgnoreCase),
    };

    /// <summary>この機種の基板のゲームだけにする（DATに基板の情報が無い、または1本も当てはまらないときは、そのまま）</summary>
    public static List<DatEntry> FilterBoard(List<DatEntry> dat, SystemConfig sys)
    {
        var key = SystemPresets.JapaneseNameOf(sys.Name);
        if (key == null || !Boards.TryGetValue(key, out var board)) return dat;
        var picked = dat.Where(d => d.SourceFile.Length > 0 && board.IsMatch(d.SourceFile)).ToList();
        return picked.Count > 0 ? picked : dat;
    }

    /// <summary>ROMセットの名前（拡張子を除いたファイル名）</summary>
    public static string SetNameOf(string path) => System.IO.Path.GetFileNameWithoutExtension(path);

    public static List<GameItem> Match(List<DatEntry> dat, List<RomFile> roms, SystemConfig sys)
    {
        dat = FilterBoard(dat, sys);
        // 1セット＝1行。DATは ROM ごとに行があるので、セットごとに最初の1つを使う
        var sets = new Dictionary<string, DatEntry>(StringComparer.OrdinalIgnoreCase);
        var order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var d in dat)
        {
            var name = d.SetName.Length > 0 ? d.SetName : d.Title;
            if (sets.TryAdd(name, d)) order[name] = order.Count;
        }

        var items = new List<GameItem>();
        var owned = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var rom in roms.OrderBy(r => r.Path, StringComparer.OrdinalIgnoreCase))
        {
            var name = SetNameOf(rom.Path);
            if (sets.TryGetValue(name, out var d))
            {
                // 同じセットのZIPが2つ以上あるとき（別のフォルダにもあるなど）は、2つめからを重複にする
                var status = owned.Add(name) ? MatchStatus.Match : MatchStatus.Duplicate;
                items.Add(new GameItem(status, DisplayName(d), d, rom, System.IO.Path.GetFileName(rom.Path)));
            }
            else
            {
                items.Add(new GameItem(MatchStatus.NotInDat, System.IO.Path.GetFileName(rom.Path), null, rom, ""));
            }
        }

        // 手元に無いゲーム。クローン（地域違い・版違い）と BIOS は数が多いので出さない（持っていれば上で出る）
        foreach (var (name, d) in sets)
        {
            if (!owned.Contains(name) && d.CloneOf.Length == 0 && !d.IsBios)
                items.Add(new GameItem(MatchStatus.Missing, DisplayName(d), d, null, ""));
        }

        return items
            .OrderBy(i => i.Status == MatchStatus.NotInDat ? 1 : 0)
            .ThenBy(i => i.Dat != null && order.TryGetValue(i.Dat.SetName.Length > 0 ? i.Dat.SetName : i.Dat.Title, out var o) ? o : int.MaxValue)
            .ThenBy(i => i.Title, StringComparer.CurrentCulture)
            .ToList();
    }

    /// <summary>一覧に出す名前（説明があれば説明。例：sf2 → Street Fighter II: The World Warrior）</summary>
    private static string DisplayName(DatEntry d) => d.Description.Length > 0 ? d.Description : d.Title;
}
