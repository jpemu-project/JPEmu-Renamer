// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using JPEmuRenamer.Models;

namespace JPEmuRenamer.Services;

/// <summary>OfflineList形式のDAT（XML）を読む</summary>
public static class DatParser
{
    public static List<DatEntry> Parse(string path)
    {
        // Shift-JIS の古いDATにも対応できるよう、XML宣言の encoding に従って読む
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        var settings = new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null };
        XDocument doc;
        using (var reader = XmlReader.Create(path, settings))
            doc = XDocument.Load(reader);

        var list = new List<DatEntry>();
        foreach (var game in doc.Descendants().Where(e => Is(e, "game")))
        {
            var title = Child(game, "title")?.Value.Trim() ?? "";
            if (title.Length == 0) continue;
            var publisher = Child(game, "publisher")?.Value.Trim() ?? "";
            var release = Child(game, "releaseNumber")?.Value.Trim() ?? "";
            var comment = Child(game, "comment")?.Value.Trim() ?? "";
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
                Genre = genre, ReleaseDate = date, Price = price, Comment = comment
            };

            bool any = false;
            var files = Child(game, "files");
            if (files != null)
            {
                foreach (var c in files.Elements().Where(e => Is(e, "romCRC")))
                {
                    if (uint.TryParse(c.Value.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var crc))
                    {
                        var entry = Entry();
                        entry.Crc = crc;
                        entry.Extension = ((string?)c.Attributes().FirstOrDefault(a => a.Name.LocalName.Equals("extension", StringComparison.OrdinalIgnoreCase)) ?? "").ToLowerInvariant();
                        list.Add(entry);
                        any = true;
                    }
                }
            }
            if (!any)
                list.Add(Entry());
        }
        return list;
    }

    private static bool IsHex(string s) =>
        s.Length == 8 && uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _);

    private static bool Is(XElement e, string name) => e.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase);
    private static XElement? Child(XElement e, string name) => e.Elements().FirstOrDefault(c => Is(c, name));
}
