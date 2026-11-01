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

            bool any = false;
            var files = Child(game, "files");
            if (files != null)
            {
                foreach (var c in files.Elements().Where(e => Is(e, "romCRC")))
                {
                    if (uint.TryParse(c.Value.Trim(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var crc))
                    {
                        list.Add(new DatEntry
                        {
                            Title = title,
                            Crc = crc,
                            Extension = ((string?)c.Attributes().FirstOrDefault(a => a.Name.LocalName.Equals("extension", StringComparison.OrdinalIgnoreCase)) ?? "").ToLowerInvariant(),
                            Publisher = publisher,
                            ReleaseNumber = release
                        });
                        any = true;
                    }
                }
            }
            if (!any)
                list.Add(new DatEntry { Title = title, Publisher = publisher, ReleaseNumber = release });
        }
        return list;
    }

    private static bool Is(XElement e, string name) => e.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase);
    private static XElement? Child(XElement e, string name) => e.Elements().FirstOrDefault(c => Is(c, name));
}
