// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JPEmuRenamer.Models;

namespace JPEmuRenamer.Services;

/// <summary>
/// DATを読む。次の形式に対応する。
/// - OfflineList 形式（XML。日本語DATなど）
/// - Logiqx 形式（XML。No-Intro の DAT-o-MATIC、Redump などが配っている clrmamepro 用のDAT）
/// - clrmamepro のテキスト形式（game ( name ... rom ( ... crc ... ) )）
/// ZIP に入ったままのDAT（中の .dat / .xml）も読める。
/// </summary>
public static class DatParser
{
    public static List<DatEntry> Parse(string path)
    {
        // Shift-JIS の古いDATにも対応できるよう、XML宣言の encoding に従って読む
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        using var stream = OpenDat(path);
        using var buffered = new BufferedStream(stream);
        return LooksLikeXml(buffered) ? ParseXml(buffered) : ParseClrMamePro(buffered);
    }

    /// <summary>DATのファイルを開く。ZIP なら中の最初の .dat / .xml を開く</summary>
    private static Stream OpenDat(string path)
    {
        if (!path.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            return File.OpenRead(path);
        var zip = ZipFile.OpenRead(path);
        var entry = zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".dat", StringComparison.OrdinalIgnoreCase) || e.Name.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    ?? throw new InvalidDataException("ZIP の中に .dat / .xml がありません");
        // ZIP を閉じるとエントリーも読めなくなるので、メモリに写してから閉じる
        var ms = new MemoryStream();
        using (var s = entry.Open()) s.CopyTo(ms);
        zip.Dispose();
        ms.Position = 0;
        return ms;
    }

    /// <summary>先頭（BOM と空白を除く）が「&lt;」なら XML</summary>
    private static bool LooksLikeXml(BufferedStream s)
    {
        var head = new byte[64];
        int n = s.Read(head, 0, head.Length);
        s.Seek(0, SeekOrigin.Begin);
        int i = 0;
        if (n >= 3 && head[0] == 0xEF && head[1] == 0xBB && head[2] == 0xBF) i = 3;
        else if (n >= 2 && (head[0] == 0xFF && head[1] == 0xFE || head[0] == 0xFE && head[1] == 0xFF)) return true;
        while (i < n && (head[i] == ' ' || head[i] == '\t' || head[i] == '\r' || head[i] == '\n')) i++;
        return i < n && head[i] == '<';
    }

    // ---------- XML（OfflineList / Logiqx） ----------

    private static List<DatEntry> ParseXml(Stream stream)
    {
        var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
        XDocument doc;
        using (var reader = XmlReader.Create(stream, settings))
            doc = XDocument.Load(reader);

        var list = new List<DatEntry>();
        foreach (var game in doc.Descendants().Where(e => Is(e, "game") || Is(e, "machine")))
        {
            // OfflineList は <title> 要素、Logiqx は name 属性にタイトルがある
            if (Child(game, "title") != null) ParseOfflineListGame(game, list);
            else ParseLogiqxGame(game, list);
        }
        return list;
    }

    private static void ParseOfflineListGame(XElement game, List<DatEntry> list)
    {
        var title = Child(game, "title")?.Value.Trim() ?? "";
        if (title.Length == 0) return;
        var publisher = Child(game, "publisher")?.Value.Trim() ?? "";
        var release = Child(game, "releaseNumber")?.Value.Trim() ?? "";
        var comment = Child(game, "comment")?.Value.Trim() ?? "";
        int? location = int.TryParse(Child(game, "location")?.Value.Trim(), out var loc) ? loc : null;
        // 日本のDAT配布サイトの形式では、画像CRC欄に発売日と価格、sourceRom欄にジャンルが入っている
        var im1 = Child(game, "im1CRC")?.Value.Trim() ?? "";
        var im2 = Child(game, "im2CRC")?.Value.Trim() ?? "";
        bool infoInImageFields = (im1.Length > 0 && !IsHex(im1)) || (im2.Length > 0 && !IsHex(im2));
        var genre = infoInImageFields ? Child(game, "sourceRom")?.Value.Trim() ?? "" : "";
        var date = infoInImageFields && !IsHex(im1) ? im1 : "";
        var price = infoInImageFields && !IsHex(im2) ? im2 : "";
        DatEntry Entry() => new()
        {
            Title = title, Publisher = publisher, ReleaseNumber = release,
            Genre = genre, ReleaseDate = date, Price = price, Comment = comment, Location = location
        };

        bool any = false;
        var files = Child(game, "files");
        if (files != null)
        {
            foreach (var c in files.Elements().Where(e => Is(e, "romCRC")))
            {
                if (TryCrc(c.Value, out var crc))
                {
                    var entry = Entry();
                    entry.Crc = crc;
                    entry.Extension = (Attr(c, "extension") ?? "").ToLowerInvariant();
                    list.Add(entry);
                    any = true;
                }
            }
        }
        if (!any)
            list.Add(Entry());
    }

    private static void ParseLogiqxGame(XElement game, List<DatEntry> list)
    {
        var title = (Attr(game, "name") ?? Child(game, "description")?.Value ?? "").Trim();
        if (title.Length == 0) return;
        var publisher = Child(game, "manufacturer")?.Value.Trim() ?? Child(game, "publisher")?.Value.Trim() ?? "";
        var year = Child(game, "year")?.Value.Trim() ?? "";
        var roms = game.Elements().Where(e => Is(e, "rom")).ToList();
        bool any = false;
        foreach (var rom in roms)
        {
            if (!TryCrc(Attr(rom, "crc"), out var crc)) continue;
            list.Add(NewLogiqxEntry(title, publisher, year, Attr(rom, "name"), crc));
            any = true;
        }
        if (!any)
            list.Add(NewLogiqxEntry(title, publisher, year, null, null));
    }

    private static DatEntry NewLogiqxEntry(string title, string publisher, string year, string? romName, uint? crc) => new()
    {
        Title = title,
        Publisher = publisher,
        ReleaseDate = year,
        Crc = crc,
        Extension = romName != null ? System.IO.Path.GetExtension(romName).ToLowerInvariant() : "",
        Location = LocationFromName(title)
    };

    // ---------- clrmamepro のテキスト形式 ----------

    private static List<DatEntry> ParseClrMamePro(Stream stream)
    {
        string text;
        using (var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            text = reader.ReadToEnd();

        var list = new List<DatEntry>();
        var tokens = Tokenize(text).ToList();
        int i = 0;
        while (i < tokens.Count)
        {
            var t = tokens[i];
            // game ( ... ) / machine ( ... ) / resource ( ... ) を読む。clrmamepro ( ... ) などの見出しは飛ばす
            if ((t.Equals("game", StringComparison.OrdinalIgnoreCase) || t.Equals("machine", StringComparison.OrdinalIgnoreCase))
                && i + 1 < tokens.Count && tokens[i + 1] == "(")
            {
                i += 2;
                string title = "", desc = "", publisher = "", year = "";
                var roms = new List<(string? name, uint? crc)>();
                while (i < tokens.Count && tokens[i] != ")")
                {
                    var key = tokens[i];
                    if (key.Equals("rom", StringComparison.OrdinalIgnoreCase) && i + 1 < tokens.Count && tokens[i + 1] == "(")
                    {
                        i += 2;
                        string? romName = null; uint? crc = null;
                        while (i < tokens.Count && tokens[i] != ")")
                        {
                            var k = tokens[i];
                            var v = i + 1 < tokens.Count ? tokens[i + 1] : "";
                            if (k.Equals("name", StringComparison.OrdinalIgnoreCase)) { romName = v; i += 2; }
                            else if (k.Equals("crc", StringComparison.OrdinalIgnoreCase)) { if (TryCrc(v, out var c)) crc = c; i += 2; }
                            else i += 2;
                        }
                        i++; // ")"
                        roms.Add((romName, crc));
                    }
                    else if (i + 1 < tokens.Count && tokens[i + 1] == "(")
                    {
                        i = SkipGroup(tokens, i + 1);
                    }
                    else
                    {
                        var v = i + 1 < tokens.Count ? tokens[i + 1] : "";
                        if (key.Equals("name", StringComparison.OrdinalIgnoreCase)) title = v;
                        else if (key.Equals("description", StringComparison.OrdinalIgnoreCase)) desc = v;
                        else if (key.Equals("manufacturer", StringComparison.OrdinalIgnoreCase) || key.Equals("publisher", StringComparison.OrdinalIgnoreCase)) publisher = v;
                        else if (key.Equals("year", StringComparison.OrdinalIgnoreCase)) year = v;
                        i += 2;
                    }
                }
                i++; // ")"
                if (title.Length == 0) title = desc;
                if (title.Length == 0) continue;
                bool any = false;
                foreach (var (romName, crc) in roms)
                {
                    if (crc == null) continue;
                    list.Add(NewLogiqxEntry(title, publisher, year, romName, crc));
                    any = true;
                }
                if (!any) list.Add(NewLogiqxEntry(title, publisher, year, null, null));
            }
            else if (i + 1 < tokens.Count && tokens[i + 1] == "(")
            {
                i = SkipGroup(tokens, i + 1);
            }
            else i++;
        }
        return list;
    }

    /// <summary>「(」の位置から、対応する「)」の次の位置を返す</summary>
    private static int SkipGroup(List<string> tokens, int open)
    {
        int depth = 0, i = open;
        for (; i < tokens.Count; i++)
        {
            if (tokens[i] == "(") depth++;
            else if (tokens[i] == ")" && --depth == 0) return i + 1;
        }
        return i;
    }

    /// <summary>「(」「)」、引用符で囲んだ文字列、空白で区切った語に分ける</summary>
    private static IEnumerable<string> Tokenize(string text)
    {
        int i = 0;
        while (i < text.Length)
        {
            char c = text[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }
            if (c == '(' || c == ')') { yield return c.ToString(); i++; continue; }
            if (c == '"')
            {
                int start = ++i;
                while (i < text.Length && text[i] != '"') i++;
                yield return text[start..Math.Min(i, text.Length)];
                i++;
                continue;
            }
            int s = i;
            while (i < text.Length && !char.IsWhiteSpace(text[i]) && text[i] != '(' && text[i] != ')') i++;
            yield return text[s..i];
        }
    }

    // ---------- 地域（国旗） ----------

    /// <summary>OfflineList の地域番号（FlagFactory と同じ番号）</summary>
    private static readonly Dictionary<string, int> RegionCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Europe"] = 0, ["USA"] = 1, ["Germany"] = 2, ["China"] = 3, ["Spain"] = 4, ["France"] = 5, ["Italy"] = 6,
        ["Japan"] = 7, ["Netherlands"] = 8, ["Sweden"] = 9, ["Denmark"] = 10, ["Finland"] = 11, ["Norway"] = 12,
        ["Poland"] = 13, ["Portugal"] = 14, ["UK"] = 15, ["United Kingdom"] = 15, ["Australia"] = 19, ["Brazil"] = 21, ["Korea"] = 22,
    };

    /// <summary>
    /// No-Intro の名前「Title (Japan)」「Title (USA, Europe)」の最初のかっこから地域を読み、国旗の番号にする。
    /// 2〜3地域の組み合わせは OfflineList の組み合わせ番号に、ほかは最初の地域にする。
    /// </summary>
    public static int? LocationFromName(string name)
    {
        int open = name.IndexOf('(');
        if (open < 0) return null;
        int close = name.IndexOf(')', open);
        if (close < 0) return null;
        var parts = name[(open + 1)..close].Split(',').Select(p => p.Trim()).ToList();
        if (parts.Count == 1 && parts[0].Equals("World", StringComparison.OrdinalIgnoreCase)) return 17;
        var codes = parts.Where(RegionCodes.ContainsKey).Select(p => RegionCodes[p]).Distinct().ToList();
        if (codes.Count == 0) return null;
        var set = codes.ToHashSet();
        bool Is(params int[] c) => set.SetEquals(c);
        if (Is(0, 1, 7)) return 17;
        if (Is(0, 1)) return 16;
        if (Is(1, 7)) return 18;
        if (Is(21, 0)) return 23;
        if (Is(21, 0, 1)) return 24;
        if (Is(21, 1)) return 25;
        return codes[0];
    }

    // ---------- 共通 ----------

    private static bool TryCrc(string? s, out uint crc)
    {
        crc = 0;
        if (string.IsNullOrWhiteSpace(s)) return false;
        return uint.TryParse(s.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out crc);
    }

    private static bool IsHex(string s) =>
        s.Length == 8 && uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _);

    private static bool Is(XElement e, string name) => e.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase);
    private static XElement? Child(XElement e, string name) => e.Elements().FirstOrDefault(c => Is(c, name));
    private static string? Attr(XElement e, string name) =>
        e.Attributes().FirstOrDefault(a => a.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value;
}
