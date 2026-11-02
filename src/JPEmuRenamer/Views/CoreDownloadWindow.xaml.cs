// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JPEmuRenamer.Emulation;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;

namespace JPEmuRenamer.Views;

/// <summary>
/// 内蔵コアの一括ダウンロードと一括更新。
/// 配布サイトの大きさと日時を読み（ダウンロードはしない）、選んだコアを1つずつ取得する。
/// </summary>
public partial class CoreDownloadWindow : Window
{
    private sealed class Row
    {
        public required CoreInfo Core;
        public required CheckBox Check;
        public required TextBlock Size;
        public required TextBlock Status;
        public long? RemoteSize;
        public DateTimeOffset? RemoteModified;
        public bool Failed;
    }

    private readonly List<Row> rows = new();
    private readonly HashSet<string> registeredDefaults;
    private CancellationTokenSource? cts;
    private bool infoLoaded;

    public CoreDownloadWindow(IEnumerable<SystemConfig> systems)
    {
        InitializeComponent();
        registeredDefaults = systems.Select(CoreCatalog.CoreNameFor).Where(n => !string.IsNullOrEmpty(n)).ToHashSet();

        IntroText.Text = Loc.F("BulkCoreIntro", CoreCatalog.BuildbotBase);
        SelectRegistered.Content = Loc.T("BulkSelectRegistered");
        SelectMissing.Content = Loc.T("BulkSelectMissing");
        SelectUpdates.Content = Loc.T("BulkSelectUpdates");
        SelectAll.Content = Loc.T("BulkSelectAll");
        SelectNone.Content = Loc.T("BulkSelectNone");
        UpdateAllButton.Content = Loc.T("BulkUpdateAll");
        CancelButton.Content = Loc.T("Cancel");
        CloseButton.Content = Loc.T("Close");

        BuildRows();
        // 最初は「登録済みの機種で使う、まだ無いコア」を選んでおく
        foreach (var r in rows) r.Check.IsChecked = registeredDefaults.Contains(r.Core.Name) && !CoreCatalog.IsInstalled(r.Core.Name);
        UpdateSummary();
        Loaded += async (_, _) => await LoadRemoteInfoAsync();
    }

    private void BuildRows()
    {
        CoreGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        CoreGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(230) });
        CoreGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        CoreGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
        CoreGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });

        var muted = (Brush)FindResource("MutedBrush");
        void Header(int col, string text)
        {
            var tb = new TextBlock { Text = text, Foreground = muted, FontSize = 12.5, Margin = new Thickness(4, 4, 8, 6) };
            Grid.SetColumn(tb, col);
            CoreGrid.Children.Add(tb);
        }
        CoreGrid.RowDefinitions.Add(new RowDefinition());
        Header(1, Loc.T("BulkColCore"));
        Header(2, Loc.T("BulkColSystems"));
        Header(3, Loc.T("BulkColSize"));
        Header(4, Loc.T("BulkColStatus"));

        // 登録済みの機種で使うコアを上に並べる
        var cores = CoreCatalog.All
            .OrderByDescending(c => registeredDefaults.Contains(c.Name))
            .ThenBy(c => CoreCatalog.SystemsUsing(c.Name).Count == 0)
            .ThenBy(c => c.DisplayName, StringComparer.OrdinalIgnoreCase);
        int row = 1;
        foreach (var core in cores)
        {
            CoreGrid.RowDefinitions.Add(new RowDefinition());
            var check = new CheckBox { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 6, 10, 6) };
            check.Checked += (_, _) => UpdateSummary();
            check.Unchecked += (_, _) => UpdateSummary();
            var name = new TextBlock
            {
                Text = core.ToString(),
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = registeredDefaults.Contains(core.Name) ? FontWeights.SemiBold : FontWeights.Normal,
                Margin = new Thickness(4, 0, 8, 0)
            };
            var systems = new TextBlock
            {
                Text = string.Join("、", CoreCatalog.SystemsUsing(core.Name)),
                Foreground = muted,
                FontSize = 12.5,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 8, 0)
            };
            var size = new TextBlock { Text = "…", Foreground = muted, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 8, 0) };
            var status = new TextBlock { VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(4, 0, 4, 0) };

            // 行のどこをクリックしてもチェックを切り替えられるようにする
            var hit = new Border { Background = Brushes.Transparent };
            hit.MouseLeftButtonUp += (_, _) => { if (check.IsEnabled) check.IsChecked = check.IsChecked != true; };
            Grid.SetRow(hit, row);
            Grid.SetColumn(hit, 1);
            Grid.SetColumnSpan(hit, 4);
            CoreGrid.Children.Add(hit);

            foreach (var (el, col) in new (UIElement, int)[] { (check, 0), (name, 1), (systems, 2), (size, 3), (status, 4) })
            {
                Grid.SetRow(el, row);
                Grid.SetColumn(el, col);
                if (el is TextBlock t) t.IsHitTestVisible = false;
                CoreGrid.Children.Add(el);
            }
            rows.Add(new Row { Core = core, Check = check, Size = size, Status = status });
            RefreshStatus(rows[^1]);
            row++;
        }
    }

    private void RefreshStatus(Row r)
    {
        bool installed = CoreCatalog.IsInstalled(r.Core.Name);
        bool update = CoreCatalog.HasUpdate(r.Core.Name, r.RemoteModified);
        r.Status.Text = !installed ? Loc.T("BulkNotInstalled") : update ? Loc.T("BulkHasUpdate") : Loc.T("BulkInstalled");
        r.Status.Foreground = (Brush)FindResource(!installed ? "MutedBrush" : update ? "WarnFgBrush" : "OkFgBrush");
    }

    /// <summary>配布サイトの大きさと日時を並行して読む（HEADだけで、ダウンロードはしない）</summary>
    private async Task LoadRemoteInfoAsync()
    {
        SummaryText.Text = Loc.T("BulkCheckingUpdates");
        using var throttle = new SemaphoreSlim(6);
        var tasks = rows.Select(async r =>
        {
            await throttle.WaitAsync();
            try
            {
                var (size, modified) = await CoreCatalog.GetRemoteInfoAsync(r.Core.Name, CancellationToken.None);
                r.RemoteSize = size;
                r.RemoteModified = modified;
            }
            finally
            {
                throttle.Release();
            }
            await Dispatcher.InvokeAsync(() =>
            {
                r.Size.Text = r.RemoteSize is long s ? FormatSize(s) : "—";
                RefreshStatus(r);
            });
        }).ToList();
        await Task.WhenAll(tasks);
        infoLoaded = true;
        UpdateSummary();
    }

    private static string FormatSize(long bytes) =>
        bytes >= 1024 * 1024 ? (bytes / 1024d / 1024).ToString("0.0") + "MB" : Math.Max(1, bytes / 1024) + "KB";

    private void UpdateSummary()
    {
        if (cts != null) return;
        var selected = rows.Where(r => r.Check.IsChecked == true).ToList();
        long total = selected.Sum(r => r.RemoteSize ?? 0);
        SummaryText.Text = selected.Count == 0
            ? (infoLoaded ? Loc.F("BulkUpdatesFound", rows.Count(r => CoreCatalog.HasUpdate(r.Core.Name, r.RemoteModified))) : Loc.T("BulkCheckingUpdates"))
            : Loc.F("BulkSelected", selected.Count, FormatSize(total));
        DownloadButton.Content = Loc.F("BulkDownload", selected.Count);
        DownloadButton.IsEnabled = selected.Count > 0;
        int updates = rows.Count(r => CoreCatalog.HasUpdate(r.Core.Name, r.RemoteModified));
        UpdateAllButton.IsEnabled = infoLoaded && updates > 0;
    }

    // ---------- 選択 ----------

    private void Select(Func<Row, bool> predicate)
    {
        foreach (var r in rows) r.Check.IsChecked = predicate(r);
    }

    private void OnSelectRegistered(object sender, RoutedEventArgs e) => Select(r => registeredDefaults.Contains(r.Core.Name));
    private void OnSelectMissing(object sender, RoutedEventArgs e) => Select(r => !CoreCatalog.IsInstalled(r.Core.Name));
    private void OnSelectUpdates(object sender, RoutedEventArgs e) => Select(r => CoreCatalog.HasUpdate(r.Core.Name, r.RemoteModified));
    private void OnSelectAll(object sender, RoutedEventArgs e) => Select(_ => true);
    private void OnSelectNone(object sender, RoutedEventArgs e) => Select(_ => false);

    // ---------- ダウンロード ----------

    private async void OnUpdateAll(object sender, RoutedEventArgs e)
    {
        Select(r => CoreCatalog.HasUpdate(r.Core.Name, r.RemoteModified));
        await RunAsync();
    }

    private async void OnDownload(object sender, RoutedEventArgs e) => await RunAsync();

    private async Task RunAsync()
    {
        var targets = rows.Where(r => r.Check.IsChecked == true).ToList();
        if (targets.Count == 0 || cts != null) return;

        cts = new CancellationTokenSource();
        SetBusy(true);
        int done = 0, failed = 0;
        foreach (var r in targets)
        {
            if (cts.IsCancellationRequested) break;
            r.Status.Foreground = (Brush)FindResource("SubBrush");
            int index = done + failed;
            var progress = new Progress<double>(p =>
            {
                r.Status.Text = Loc.F("BulkDownloading", (int)(p * 100));
                Progress.Value = (index + p) / targets.Count;
            });
            SummaryText.Text = Loc.F("BulkProgress", index + 1, targets.Count, r.Core.DisplayName);
            r.Status.Text = Loc.F("BulkDownloading", 0);
            try
            {
                await CoreCatalog.DownloadAsync(r.Core.Name, progress, cts.Token);
                r.Failed = false;
                r.Check.IsChecked = false;
                RefreshStatus(r);
                done++;
            }
            catch (OperationCanceledException)
            {
                RefreshStatus(r);
                break;
            }
            catch (Exception ex)
            {
                r.Failed = true;
                r.Status.Text = Loc.F("BulkFailed", ex.Message);
                r.Status.Foreground = (Brush)FindResource("ErrFgBrush");
                failed++;
            }
        }
        bool cancelled = cts.IsCancellationRequested;
        cts.Dispose();
        cts = null;
        SetBusy(false);
        UpdateSummary();
        SummaryText.Text = cancelled
            ? Loc.F("BulkCancelled", done)
            : failed > 0 ? Loc.F("BulkDoneWithErrors", done, failed) : Loc.F("BulkDone", done);
    }

    private void SetBusy(bool busy)
    {
        Progress.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        Progress.Value = 0;
        CancelButton.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        DownloadButton.IsEnabled = !busy;
        UpdateAllButton.IsEnabled = !busy;
        foreach (var b in new[] { SelectRegistered, SelectMissing, SelectUpdates, SelectAll, SelectNone }) b.IsEnabled = !busy;
        foreach (var r in rows) r.Check.IsEnabled = !busy;
    }

    private void OnCancel(object sender, RoutedEventArgs e) => cts?.Cancel();

    private void OnClose(object sender, RoutedEventArgs e) => Close();

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        if (cts == null) return;
        if (MessageBox.Show(this, Loc.T("BulkConfirmClose"), "JPEmu Renamer", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            e.Cancel = true;
            return;
        }
        cts.Cancel();
    }
}
