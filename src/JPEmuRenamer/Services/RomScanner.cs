// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.IO.Compression;
using JPEmuRenamer.Models;

namespace JPEmuRenamer.Services;

/// <summary>ROMフォルダを走査し、CRC32を計算する（前回から変わっていないファイルはキャッシュを使う）</summary>
public static class RomScanner
{
    public static List<RomFile> Scan(SystemConfig sys, bool subfolders, Dictionary<string, RomFile> cache,
        IProgress<(int done, int total)>? progress, CancellationToken ct)
    {
        var exts = new HashSet<string>(sys.Extensions, StringComparer.OrdinalIgnoreCase);
        var files = new List<string>();
        foreach (var folder in sys.RomFolders.Where(Directory.Exists))
        {
            try
            {
                files.AddRange(Directory.EnumerateFiles(folder, "*", new EnumerationOptions
                {
                    RecurseSubdirectories = subfolders,
                    IgnoreInaccessible = true
                }).Where(f => exts.Contains(System.IO.Path.GetExtension(f))));
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
        files = files.Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        var result = new List<RomFile>(files.Count);
        for (int i = 0; i < files.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var path = files[i];
            try
            {
                var fi = new FileInfo(path);
                if (cache.TryGetValue(path, out var cached) && cached.Size == fi.Length && cached.LastWriteTicks == fi.LastWriteTimeUtc.Ticks)
                {
                    result.Add(cached);
                }
                else
                {
                    var rom = Compute(fi, exts);
                    if (rom != null) result.Add(rom);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
            catch (InvalidDataException) { }

            if (i % 8 == 0 || i == files.Count - 1) progress?.Report((i + 1, files.Count));
        }
        return result;
    }

    private static RomFile? Compute(FileInfo fi, HashSet<string> exts)
    {
        var rom = new RomFile
        {
            Path = fi.FullName,
            Size = fi.Length,
            LastWriteTicks = fi.LastWriteTimeUtc.Ticks
        };

        if (fi.Extension.Equals(".zip", StringComparison.OrdinalIgnoreCase))
        {
            using var zip = ZipFile.OpenRead(fi.FullName);
            var entries = zip.Entries.Where(e => !string.IsNullOrEmpty(e.Name)).ToList();
            if (entries.Count == 0) return null;
            // 対象拡張子のROMを優先し、無ければ一番大きいファイル
            var entry = entries.Where(e => !System.IO.Path.GetExtension(e.Name).Equals(".zip", StringComparison.OrdinalIgnoreCase)
                                            && exts.Contains(System.IO.Path.GetExtension(e.Name)))
                               .OrderByDescending(e => e.Length).FirstOrDefault()
                        ?? entries.OrderByDescending(e => e.Length).First();
            using var s = entry.Open();
            (rom.Crc, rom.CrcNoHeader) = Crc(s);
            rom.InnerName = entry.FullName;
            rom.RomSize = entry.Length;
        }
        else
        {
            using var s = new FileStream(fi.FullName, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16);
            (rom.Crc, rom.CrcNoHeader) = Crc(s);
            rom.RomSize = fi.Length;
        }
        return rom;
    }

    /// <summary>
    /// ファイル全体のCRC32と、16バイトのヘッダー（iNES "NES\x1A"、FDS "FDS\x1A"）を除いたCRC32を同時に計算する。
    /// </summary>
    public static (uint full, uint? noHeader) Crc(Stream s)
    {
        var full = new System.IO.Hashing.Crc32();
        System.IO.Hashing.Crc32? noHeader = null;
        var buf = new byte[1 << 16];

        int n = s.ReadAtLeast(buf, 16, throwOnEndOfStream: false);
        if (n > 0)
        {
            var span = buf.AsSpan(0, n);
            full.Append(span);
            if (n >= 16 && buf[3] == 0x1A &&
                ((buf[0] == (byte)'N' && buf[1] == (byte)'E' && buf[2] == (byte)'S') ||
                 (buf[0] == (byte)'F' && buf[1] == (byte)'D' && buf[2] == (byte)'S')))
            {
                noHeader = new System.IO.Hashing.Crc32();
                noHeader.Append(span[16..]);
            }
        }
        while ((n = s.Read(buf, 0, buf.Length)) > 0)
        {
            var span = buf.AsSpan(0, n);
            full.Append(span);
            noHeader?.Append(span);
        }
        return (full.GetCurrentHashAsUInt32(), noHeader?.GetCurrentHashAsUInt32());
    }
}
