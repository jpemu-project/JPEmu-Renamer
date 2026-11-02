// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.ComponentModel;
using System.Windows;
using JPEmuRenamer.Emulation;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;

namespace JPEmuRenamer.Views;

/// <summary>
/// 機種内のROMを順に裏で起動し、指定秒数の時点の画面をスナップショットとして保存する。
/// 音・入力なしで、待たずに最速で動かす。
/// </summary>
public partial class SnapshotBatchWindow : Window
{
    private readonly AppSettings app;
    private readonly SystemConfig system;
    private readonly List<GameItem> items;
    private readonly Action saveApp;
    private CancellationTokenSource? cts;

    public SnapshotBatchWindow(AppSettings app, SystemConfig system, IEnumerable<GameItem> items, Action saveApp)
    {
        InitializeComponent();
        this.app = app;
        this.system = system;
        this.saveApp = saveApp;
        // 同じROMが複数行に出ることはないが、念のためパスで重複を除く
        this.items = items.Where(i => i.Rom != null).GroupBy(i => i.Rom!.Path).Select(g => g.First()).ToList();

        var coreName = CoreCatalog.CoreNameFor(system);
        IntroText.Text = Loc.F("SnapBatchIntro", system.Name, this.items.Count, app.Preview.SnapshotSeconds,
            CoreCatalog.Get(coreName)?.DisplayName ?? coreName);
        StartButton.Content = Loc.T("SnapBatchStart");
        StopButton.Content = Loc.T("SnapBatchStop");
        CloseButton.Content = Loc.T("Close");
        ShotText.Text = Loc.T("SnapBatchNoImage");
        ProgressText.Text = Loc.F("SnapBatchCount", this.items.Count(i => Snapshots.Exists(system, i)), this.items.Count);

        if (!CoreCatalog.IsInstalled(coreName))
        {
            StartButton.IsEnabled = false;
            ResultText.Text = Loc.T("SnapBatchNoCore");
        }
    }

    private async void OnStart(object sender, RoutedEventArgs e)
    {
        if (cts != null) return;
        cts = new CancellationTokenSource();
        StartButton.Visibility = Visibility.Collapsed;
        StopButton.Visibility = Visibility.Visible;
        SkipExisting.IsEnabled = false;
        ResultText.Text = "";

        bool skip = SkipExisting.IsChecked == true;
        var targets = items.Where(i => !skip || !Snapshots.Exists(system, i)).ToList();
        int saved = 0, failed = 0;
        var coreName = CoreCatalog.CoreNameFor(system);

        for (int n = 0; n < targets.Count && !cts.IsCancellationRequested; n++)
        {
            var item = targets[n];
            ProgressText.Text = Loc.F("SnapBatchProgress", n + 1, targets.Count, item.Title);
            Progress.Value = (double)n / Math.Max(1, targets.Count);
            if (await CaptureAsync(coreName, item, cts.Token)) saved++;
            else if (!cts.IsCancellationRequested) failed++;
        }

        bool cancelled = cts.IsCancellationRequested;
        cts.Dispose();
        cts = null;
        Progress.Value = cancelled ? Progress.Value : 1;
        StopButton.Visibility = Visibility.Collapsed;
        StartButton.Visibility = Visibility.Visible;
        SkipExisting.IsEnabled = true;
        ProgressText.Text = Loc.F("SnapBatchCount", items.Count(i => Snapshots.Exists(system, i)), items.Count);
        ResultText.Text = cancelled ? Loc.F("SnapBatchCancelled", saved) : Loc.F("SnapBatchDone", saved, failed);
    }

    /// <summary>1本を起動し、指定秒数ぶん進めた画面を保存する</summary>
    private async Task<bool> CaptureAsync(string coreName, GameItem item, CancellationToken ct)
    {
        var path = Snapshots.PathFor(system, item);
        if (path == null) return false;
        var key = item.Rom!.Crc.ToString("X8");
        var context = new PlayContext(app, system, key, item.Title, saveApp);
        var request = new SessionRequest(coreName, item.Rom.Path, item.Rom.InnerName, system, key, item.Title,
            context.Resolve(), context.BiosDir, new PreviewOptions(0, Math.Clamp(app.Preview.SnapshotSeconds, 1, 600), 0));

        var session = new EmulatorSession(request);
        bool failed = false;
        session.Failed += _ => failed = true;
        int[] frame = Array.Empty<int>();
        long serial = 0;
        int w = 0, h = 0;
        try
        {
            session.Start();
            var deadline = DateTime.Now.AddSeconds(30);
            // 冒頭を飛ばし終えて最初の映像が届いたら、それを保存する
            while (!ct.IsCancellationRequested && !failed && DateTime.Now < deadline)
            {
                if (session.Publishing && session.TryGetFrame(ref frame, out w, out h, ref serial)) break;
                await Task.Delay(30, CancellationToken.None);
            }
            if (serial == 0 || w == 0) return false;
            Snapshots.Save(path, frame, w, h);
            ShotImage.Source = Snapshots.Load(path);
            ShotText.Text = "";
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex);
            return false;
        }
        finally
        {
            // 止めるのを待つあいだ画面が固まらないよう、別のスレッドで待つ
            await Task.Run(() => session.Stop());
            session.Dispose();
        }
    }

    private void OnStop(object sender, RoutedEventArgs e) => cts?.Cancel();

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (cts == null) return;
        // 実行中は止めてから閉じる（コアが動いたまま次の画面で別のコアを起動しないように）
        e.Cancel = true;
        cts.Cancel();
        Dispatcher.BeginInvoke(async () =>
        {
            while (cts != null) await Task.Delay(50);
            Close();
        });
    }
}
