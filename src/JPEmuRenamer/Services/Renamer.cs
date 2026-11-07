// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Diagnostics;
using System.IO.Compression;
using JPEmuRenamer.Models;

namespace JPEmuRenamer.Services;

/// <summary>リネームの実行と取り消し</summary>
public static class Renamer
{
    public record Result(RenameRun Run, int Skipped, List<string> Errors)
    {
        /// <summary>移動したDATに無いファイル（移動先のパス）</summary>
        public List<string> Moved { get; } = new();
    }

    public static Result Execute(IEnumerable<GameItem> items, SystemConfig sys, bool renameInsideZip)
    {
        var run = new RenameRun { SystemId = sys.Id };
        var errors = new List<string>();
        int skipped = 0;

        foreach (var item in items.Where(i => i.IsChecked && i.CanRename && i.Rom != null && i.Dat != null))
        {
            var from = item.Rom!.Path;
            var dir = System.IO.Path.GetDirectoryName(from) ?? "";
            var to = System.IO.Path.Combine(dir, item.TargetFileName);
            try
            {
                bool caseOnly = string.Equals(from, to, StringComparison.OrdinalIgnoreCase);
                if (!File.Exists(from) || (!caseOnly && File.Exists(to)))
                {
                    skipped++;
                    errors.Add(System.IO.Path.GetFileName(from) + "：" + Loc.T("AlreadyExists"));
                    continue;
                }

                var record = new RenameRecord { From = from, To = to };
                File.Move(from, to);

                if (renameInsideZip && item.Rom.InnerName != null &&
                    System.IO.Path.GetExtension(to).Equals(".zip", StringComparison.OrdinalIgnoreCase))
                {
                    var newInner = Matcher.SafeFileName(item.Dat!.Title) + System.IO.Path.GetExtension(item.Rom.InnerName);
                    if (!string.Equals(newInner, item.Rom.InnerName, StringComparison.Ordinal) &&
                        RenameZipEntry(to, item.Rom.InnerName, newInner))
                    {
                        record.InnerFrom = item.Rom.InnerName;
                        record.InnerTo = newInner;
                    }
                }

                run.Records.Add(record);
            }
            catch (Exception ex)
            {
                skipped++;
                errors.Add(System.IO.Path.GetFileName(from) + "：" + ex.Message);
            }
        }
        return new Result(run, skipped, errors);
    }

    /// <summary>
    /// ファイルをフォルダへ移動し、同じ実行の記録に足す（取り消しで元の場所に戻せる）。
    /// 移動先に同じ名前があるものは飛ばす
    /// </summary>
    public static void MoveFiles(IEnumerable<string> paths, string folder, Result result)
    {
        try { Directory.CreateDirectory(folder); }
        catch (Exception ex)
        {
            result.Errors.Add(folder + "：" + ex.Message);
            return;
        }
        foreach (var from in paths)
        {
            var to = System.IO.Path.Combine(folder, System.IO.Path.GetFileName(from));
            try
            {
                if (!File.Exists(from)) continue;
                // 移動先に同じ名前があるとき（別の機種で同じファイル名を移動済みなど）は、
                // 「名前 (2).zip」のように番号を付けて移動する
                if (File.Exists(to))
                {
                    var stem = System.IO.Path.GetFileNameWithoutExtension(from);
                    var ext = System.IO.Path.GetExtension(from);
                    for (int n = 2; File.Exists(to); n++)
                        to = System.IO.Path.Combine(folder, $"{stem} ({n}){ext}");
                }
                File.Move(from, to);
                result.Run.Records.Add(new RenameRecord { From = from, To = to });
                result.Moved.Add(to);
            }
            catch (Exception ex)
            {
                result.Errors.Add(System.IO.Path.GetFileName(from) + "：" + ex.Message);
            }
        }
    }

    public static int Undo(RenameRun run, List<string> errors)
    {
        int count = 0;
        foreach (var r in Enumerable.Reverse(run.Records))
        {
            try
            {
                if (!File.Exists(r.To) || (File.Exists(r.From) && !string.Equals(r.From, r.To, StringComparison.OrdinalIgnoreCase)))
                {
                    errors.Add(System.IO.Path.GetFileName(r.To));
                    continue;
                }
                File.Move(r.To, r.From);
                if (r.InnerFrom != null && r.InnerTo != null)
                    RenameZipEntry(r.From, r.InnerTo, r.InnerFrom);
                count++;
            }
            catch (Exception ex)
            {
                errors.Add(System.IO.Path.GetFileName(r.To) + "：" + ex.Message);
            }
        }
        return count;
    }

    /// <summary>ZIP内のエントリ名を変える（新しいエントリにコピーして古いものを消す）</summary>
    private static bool RenameZipEntry(string zipPath, string oldName, string newName)
    {
        try
        {
            using var zip = ZipFile.Open(zipPath, ZipArchiveMode.Update);
            var old = zip.GetEntry(oldName);
            if (old == null || zip.GetEntry(newName) != null) return false;
            var created = zip.CreateEntry(newName, CompressionLevel.Optimal);
            created.LastWriteTime = old.LastWriteTime;
            using (var src = old.Open())
            using (var dst = created.Open())
                src.CopyTo(dst);
            old.Delete();
            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return false;
        }
    }
}

/// <summary>外部exe（エミュレーター）の起動</summary>
public static class Launcher
{
    public static void LaunchExternal(SystemConfig sys, string romPath)
    {
        var args = (sys.EmulatorArgs ?? "\"{rom}\"")
            .Replace("{rom}", romPath)
            .Replace("{romdir}", System.IO.Path.GetDirectoryName(romPath) ?? "")
            .Replace("{romname}", System.IO.Path.GetFileNameWithoutExtension(romPath));
        var workDir = !string.IsNullOrWhiteSpace(sys.EmulatorWorkDir)
            ? sys.EmulatorWorkDir
            : System.IO.Path.GetDirectoryName(sys.EmulatorPath) ?? "";
        Process.Start(new ProcessStartInfo
        {
            FileName = sys.EmulatorPath,
            Arguments = args,
            WorkingDirectory = workDir,
            UseShellExecute = false
        });
    }

    public static void OpenFolder(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = "/select,\"" + path + "\"",
            UseShellExecute = true
        });
    }
}
