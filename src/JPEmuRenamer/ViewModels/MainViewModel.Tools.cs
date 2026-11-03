// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.IO;
using CommunityToolkit.Mvvm.Input;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;

namespace JPEmuRenamer.ViewModels;

/// <summary>ヘッダーの「ツール」：ゲームリストの保存と、ROMの一括解凍・一括圧縮</summary>
public partial class MainViewModel
{
    /// <summary>保存先を選ぶ（候補のファイル名を渡し、選んだパスか null が返る）</summary>
    public Func<string, string?>? SaveListDialog { get; set; }

    /// <summary>"All" / "Owned" / "Missing" のタイトル一覧をテキストで保存する</summary>
    [RelayCommand]
    private void SaveGameList(string kind)
    {
        var sys = SelectedSystem?.Config;
        if (sys == null) return;
        var titles = Items
            .Where(i => kind switch { "Owned" => i.IsOwned, "Missing" => i.Status == MatchStatus.Missing, _ => true })
            .Select(i => i.Title)
            .Distinct()
            .ToList();
        if (titles.Count == 0)
        {
            Info?.Invoke(Loc.T("NothingToProcess"));
            return;
        }
        var label = kind switch { "Owned" => Loc.T("FilterOwned"), "Missing" => Loc.T("FilterMissing"), _ => Loc.T("FilterAll") };
        var path = SaveListDialog?.Invoke(Matcher.SafeFileName($"{sys.Name} - {label}") + ".txt");
        if (path == null) return;
        try
        {
            File.WriteAllLines(path, titles, new System.Text.UTF8Encoding(true));
            StatusText = Loc.F("ListSaved", titles.Count, Path.GetFileName(path));
        }
        catch (Exception ex)
        {
            Info?.Invoke(ex.Message);
        }
    }

    [RelayCommand]
    private Task ExtractAllRoms() => ArchiveAll(extract: true);

    [RelayCommand]
    private Task CompressAllRoms() => ArchiveAll(extract: false);

    private async Task ArchiveAll(bool extract)
    {
        var sys = SelectedSystem?.Config;
        if (sys == null || IsBusy) return;
        var targets = Items.Where(i => i.Rom != null).Select(i => i.Rom!.Path).Distinct()
            .Where(p => extract ? RomArchiver.CanExtract(p) : RomArchiver.CanCompress(p))
            .ToList();
        if (targets.Count == 0)
        {
            Info?.Invoke(Loc.T("NothingToProcess"));
            return;
        }
        if (Confirm?.Invoke(Loc.F(extract ? "ConfirmExtractAll" : "ConfirmCompressAll", sys.Name, targets.Count)) != true) return;

        IsBusy = true;
        int done = 0, skipped = 0, n = 0;
        var newExts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var errors = new List<string>();
        var progress = new Progress<int>(i => StatusText = Loc.F(extract ? "ExtractProgress" : "CompressProgress", i, targets.Count));
        await Task.Run(() =>
        {
            foreach (var path in targets)
            {
                ((IProgress<int>)progress).Report(++n);
                try
                {
                    var made = extract ? RomArchiver.Extract(path) : RomArchiver.Compress(path);
                    if (made == null) { skipped++; continue; }
                    done++;
                    newExts.Add(Path.GetExtension(made));
                }
                catch (Exception ex)
                {
                    skipped++;
                    errors.Add($"{Path.GetFileName(path)}: {ex.Message}");
                }
            }
        });

        // できたファイルの拡張子が機種の読み込み対象に無ければ足す（無いと一覧から消えてしまうため）
        var missing = newExts.Where(e => !sys.Extensions.Contains(e, StringComparer.OrdinalIgnoreCase)).ToList();
        if (missing.Count > 0)
        {
            sys.Extensions.AddRange(missing.Select(e => e.ToLowerInvariant()));
            Save();
        }
        IsBusy = false;

        var msg = Loc.F(extract ? "ExtractDone" : "CompressDone", done, skipped);
        if (missing.Count > 0) msg += "\n" + Loc.F("ExtensionsAdded", string.Join(", ", missing));
        if (errors.Count > 0) msg += "\n\n" + string.Join("\n", errors.Take(10));
        Info?.Invoke(msg);
        await ReloadAsync();
    }
}
