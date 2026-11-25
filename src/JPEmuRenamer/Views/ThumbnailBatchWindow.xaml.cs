// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.ComponentModel;
using System.Windows;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;

namespace JPEmuRenamer.Views;

/// <summary>
/// 登録した全機種の、所有しているROMのパッケージ・タイトル画面・ゲーム画面の画像を、まとめて取ってくる。
/// 所有しているかは前回の読み込みの結果（キャッシュ）で決める。すでにある画像は取り直さない。
/// 相手のサーバーに負担をかけないよう、1枚ずつ順に取る
/// </summary>
public partial class ThumbnailBatchWindow : Window
{
    private readonly List<SystemConfig> systems;
    private List<(SystemConfig sys, GameItem item)> games = new();
    private CancellationTokenSource? cts;

    public ThumbnailBatchWindow(IEnumerable<SystemConfig> systems)
    {
        InitializeComponent();
        this.systems = systems.ToList();
        StartButton.Content = Loc.T("ThumbBatchStart");
        StopButton.Content = Loc.T("ThumbBatchStop");
        CloseButton.Content = Loc.T("Close");
        IntroText.Text = Loc.T("ThumbBatchCounting");
        Loaded += async (_, _) => await CountAsync();
    }

    /// <summary>所有しているROMを、各機種のDATとキャッシュから集める（ROMフォルダは読み直さない）</summary>
    private async Task CountAsync()
    {
        var list = await Task.Run(() =>
        {
            var result = new List<(SystemConfig, GameItem)>();
            foreach (var sys in systems)
            {
                try
                {
                    var dat = string.IsNullOrWhiteSpace(sys.DatPath) ? new List<DatEntry>() : DatParser.Parse(sys.DatPath);
                    var roms = Storage.LoadCache(sys.Id).Values.ToList();
                    foreach (var item in Matcher.Match(dat, roms, sys).Where(i => i.IsOwned))
                        result.Add((sys, item));
                }
                catch { /* DATが読めない機種は飛ばす */ }
            }
            return result;
        });
        games = list;
        IntroText.Text = Loc.F("ThumbBatchIntro", games.Count, games.Select(g => g.sys.Id).Distinct().Count());
        StartButton.IsEnabled = games.Count > 0;
    }

    private async void OnStart(object sender, RoutedEventArgs e)
    {
        if (cts != null) return;
        var kinds = new List<string>();
        if (BoxartCheck.IsChecked == true) kinds.Add(Thumbnails.Boxart);
        if (TitleCheck.IsChecked == true) kinds.Add(Thumbnails.Title);
        if (SnapCheck.IsChecked == true) kinds.Add(Thumbnails.Snap);
        if (kinds.Count == 0) return;

        cts = new CancellationTokenSource();
        StartButton.Visibility = Visibility.Collapsed;
        StopButton.Visibility = Visibility.Visible;
        BoxartCheck.IsEnabled = TitleCheck.IsEnabled = SnapCheck.IsEnabled = false;
        ResultText.Text = "";

        int total = games.Count * kinds.Count, done = 0, got = 0, had = 0, missing = 0;
        try
        {
            foreach (var (sys, item) in games)
            {
                foreach (var kind in kinds)
                {
                    cts.Token.ThrowIfCancellationRequested();
                    ProgressText.Text = Loc.F("ThumbBatchProgress", done + 1, total, item.Title);
                    if (Thumbnails.Cached(sys, item, kind) is string existing)
                    {
                        had++;
                        if (kind == kinds[0]) ShotImage.Source = Snapshots.Load(existing);
                    }
                    else if (await Thumbnails.GetAsync(sys, item, kind, cts.Token) is string path)
                    {
                        got++;
                        ShotImage.Source = Snapshots.Load(path);
                    }
                    else missing++;
                    done++;
                    Progress.Value = (double)done / total;
                }
            }
            ResultText.Text = Loc.F("ThumbBatchDone", got, had, missing);
        }
        catch (OperationCanceledException)
        {
            ResultText.Text = Loc.T("ThumbBatchStopped") + " " + Loc.F("ThumbBatchDone", got, had, missing);
        }
        finally
        {
            cts = null;
            StopButton.Visibility = Visibility.Collapsed;
            StartButton.Visibility = Visibility.Visible;
            BoxartCheck.IsEnabled = TitleCheck.IsEnabled = SnapCheck.IsEnabled = true;
        }
    }

    private void OnStop(object sender, RoutedEventArgs e) => cts?.Cancel();

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnClosing(object? sender, CancelEventArgs e) => cts?.Cancel();
}
