// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace JPEmuRenamer.Services;

/// <summary>
/// ROMの一括解凍・一括圧縮。1つのZIPに1ファイルの形だけを扱い、できあがったら元のファイルはごみ箱に移す（元に戻せるように）。
/// 同じ名前のファイルがすでにあるときや、中身が複数あるZIPは飛ばす。
/// </summary>
public static class RomArchiver
{
    // 解凍・圧縮しない拡張子（複数ファイルで1本になる形式や、すでに圧縮済みの形式）
    private static readonly HashSet<string> SkipCompress = new(StringComparer.OrdinalIgnoreCase)
        { ".zip", ".7z", ".rar", ".cue", ".bin", ".gdi", ".m3u", ".chd", ".cso", ".pbp", ".iso" };

    private static readonly System.Text.Encoding ShiftJis = CreateShiftJis();

    private static System.Text.Encoding CreateShiftJis()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        return System.Text.Encoding.GetEncoding(932);
    }

    public static bool CanExtract(string path) => Path.GetExtension(path).Equals(".zip", StringComparison.OrdinalIgnoreCase);

    public static bool CanCompress(string path) => !SkipCompress.Contains(Path.GetExtension(path));

    /// <summary>ZIPを同じフォルダに解凍する。できたファイルのパス（飛ばしたときは null）</summary>
    public static string? Extract(string zipPath)
    {
        string dir = Path.GetDirectoryName(zipPath)!;
        string tmp;
        string target;
        // 古いZIPは中のファイル名がShift-JISのことがあるので、UTF-8の印が無い名前はShift-JISとして読む
        using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Read, ShiftJis))
        {
            var entries = zip.Entries.Where(e => !string.IsNullOrEmpty(e.Name)).ToList();
            if (entries.Count != 1) return null;
            var entry = entries[0];
            target = Path.Combine(dir, Path.GetFileNameWithoutExtension(zipPath) + Path.GetExtension(entry.Name));
            if (File.Exists(target)) return null;
            tmp = target + ".jpemu-tmp";
            entry.ExtractToFile(tmp, true);
            if (new FileInfo(tmp).Length != entry.Length)
            {
                File.Delete(tmp);
                return null;
            }
        }
        File.Move(tmp, target);
        File.SetLastWriteTimeUtc(target, File.GetLastWriteTimeUtc(zipPath));
        MoveToRecycleBin(zipPath);
        return target;
    }

    /// <summary>ROMを同じフォルダの同じ名前のZIPにする。できたZIPのパス（飛ばしたときは null）</summary>
    public static string? Compress(string romPath)
    {
        string target = Path.ChangeExtension(romPath, ".zip");
        if (File.Exists(target)) return null;
        string tmp = target + ".jpemu-tmp";
        using (var zip = ZipFile.Open(tmp, ZipArchiveMode.Create))
            zip.CreateEntryFromFile(romPath, Path.GetFileName(romPath), CompressionLevel.Optimal);
        // 書けたかを確かめてから元を消す
        using (var check = ZipFile.OpenRead(tmp))
        {
            if (check.Entries.Count != 1 || check.Entries[0].Length != new FileInfo(romPath).Length)
            {
                check.Dispose();
                File.Delete(tmp);
                return null;
            }
        }
        File.Move(tmp, target);
        MoveToRecycleBin(romPath);
        return target;
    }

    // ---------- ごみ箱へ移す ----------

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ShFileOpStruct
    {
        public IntPtr hwnd;
        public uint wFunc;
        public string pFrom;
        public string? pTo;
        public ushort fFlags;
        public bool fAnyOperationsAborted;
        public IntPtr hNameMappings;
        public string? lpszProgressTitle;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHFileOperation(ref ShFileOpStruct op);

    private const uint FoDelete = 3;
    private const ushort FofSilent = 0x4, FofNoConfirmation = 0x10, FofAllowUndo = 0x40, FofNoErrorUi = 0x400;

    private static void MoveToRecycleBin(string path)
    {
        var op = new ShFileOpStruct
        {
            wFunc = FoDelete,
            pFrom = path + "\0\0",
            fFlags = FofAllowUndo | FofNoConfirmation | FofSilent | FofNoErrorUi
        };
        int r = SHFileOperation(ref op);
        if (r != 0) throw new IOException(Loc.F("RecycleFailed", Path.GetFileName(path)));
    }
}
