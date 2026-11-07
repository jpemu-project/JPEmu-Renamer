// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using JPEmuRenamer.Emulation;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;
using JPEmuRenamer.Ui;
using JPEmuRenamer.ViewModels;

namespace JPEmuRenamer.Views;

public partial class MainWindow : Window
{
    private readonly MainViewModel vm;

    public MainWindow(MainViewModel vm)
    {
        InitializeComponent();
        this.vm = vm;
        DataContext = vm;

        vm.Confirm = msg => MessageBox.Show(this, msg, "JPEmu Renamer", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        vm.Info = msg => MessageBox.Show(this, msg, "JPEmu Renamer", MessageBoxButton.OK, MessageBoxImage.Information);
        vm.EditSystemDialog = config => WithoutPreview(() => EditSystem(config));
        vm.OpenAppSettingsDialog = () => WithoutPreview(() =>
        {
            new AppSettingsWindow(vm) { Owner = this }.ShowDialog();
            return true;
        });
        vm.OpenPlayWindow = OpenPlayWindow;
        vm.OpenPlaySettingsDialog = (ctx, scope) => WithoutPreview(() => new PlaySettingsWindow(ctx, scope) { Owner = this }.ShowDialog());
        vm.OpenSnapshotBatch = (sys, items) => WithoutPreview(() =>
            new SnapshotBatchWindow(vm.Settings, sys, items, vm.Save) { Owner = this }.ShowDialog());

        vm.SaveListDialog = name =>
        {
            var dlg = new Microsoft.Win32.SaveFileDialog { FileName = name, Filter = "Text (*.txt)|*.txt|All files (*.*)|*.*" };
            return dlg.ShowDialog(this) == true ? dlg.FileName : null;
        };

        preview = new PreviewController(PreviewImage, PreviewText, PreviewSaveButton, () => vm.Settings, vm.Save);
        vm.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(MainViewModel.SelectedItem) or nameof(MainViewModel.SelectedSystem) or nameof(MainViewModel.IsManageMode))
                preview.Select(vm.SelectedSystem?.Config, vm.SelectedItem, !vm.IsManageMode);
            if (e.PropertyName == nameof(MainViewModel.IsManageMode)) UpdateDetailWidth();
        };
        UpdateDetailWidth();
        Activated += (_, _) => preview.SetActive(true);
        Deactivated += (_, _) => preview.SetActive(false);

        RestoreWindowPlacement(vm.Settings);
    }

    /// <summary>
    /// 右の詳細欄の幅：プレイ画面ではテレビ風のプレビューを大きく見せるため広く、
    /// リネーム画面では一覧のファイル名が読めるよう従来の幅にする
    /// </summary>
    private void UpdateDetailWidth() =>
        DetailColumn.Width = new GridLength(vm.IsManageMode ? 360 : 456);

    private PlayWindow? playWindow;
    private HelpWindow? helpWindow;
    private readonly PreviewController preview;

    /// <summary>
    /// 自動プレビューを止めてから処理する（同時に動かせるコアは1つで、コアの更新中はファイルも使えないため）
    /// </summary>
    private T WithoutPreview<T>(Func<T> action)
    {
        preview.Suspend();
        try { return action(); }
        finally { preview.Resume(); }
    }

    private void WithoutPreview(Action action) => WithoutPreview(() => { action(); return true; });

    private void OnPreviewSave(object sender, RoutedEventArgs e)
    {
        try
        {
            if (preview.SaveCurrentFrame()) vm.StatusText = Loc.T("PreviewSaved");
        }
        catch (Exception ex)
        {
            vm.Info?.Invoke(ex.Message);
        }
    }

    /// <summary>DATに無いファイルの移動先を選ぶ</summary>
    private void OnBrowseUnknown(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFolderDialog();
        if (System.IO.Directory.Exists(vm.UnknownFolder)) dlg.InitialDirectory = vm.UnknownFolder;
        if (dlg.ShowDialog(this) == true) vm.UnknownFolder = dlg.FolderName;
    }

    /// <summary>「ツール」：ボタンの下にメニューを開く</summary>
    private void OnTools(object sender, RoutedEventArgs e)
    {
        var menu = ToolsButton.ContextMenu;
        menu.PlacementTarget = ToolsButton;
        menu.DataContext = vm;
        menu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
        menu.IsOpen = true;
    }

    /// <summary>「並び替え」：ボタンの下に並び順の一覧を開く（いまの並び順にチェック）</summary>
    private void OnSortClick(object sender, RoutedEventArgs e)
    {
        var button = (System.Windows.Controls.Button)sender;
        var menu = new System.Windows.Controls.ContextMenu
        {
            PlacementTarget = button,
            Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom
        };
        foreach (var (key, label) in MainViewModel.SortOptions)
        {
            var item = new System.Windows.Controls.MenuItem { Header = Loc.T(label), IsCheckable = true, IsChecked = vm.Sort == key };
            item.Click += (_, _) => vm.Sort = key;
            menu.Items.Add(item);
        }
        menu.IsOpen = true;
    }

    /// <summary>テレビの画面をクリック：自分のスクショと自動プレビューを切り替える</summary>
    private void OnPreviewClick(object sender, MouseButtonEventArgs e) => preview.ToggleOwnShot();

    /// <summary>マニュアルを開く。いま開いているタブ（プレイ／リネーム）に合わせた内容を表示する</summary>
    private void OnHelp(object sender, RoutedEventArgs e) => ShowHelp();

    private void ShowHelp()
    {
        var tab = vm.IsManageMode ? "Rename" : "Play";
        if (helpWindow != null)
        {
            helpWindow.ShowTab(tab);
            helpWindow.Activate();
            return;
        }
        helpWindow = new HelpWindow(tab) { Owner = this };
        helpWindow.Closed += (_, _) => helpWindow = null;
        helpWindow.Show();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        // F1でマニュアル（プレイ中のF1はプレイ画面のメニュー）
        if (e.Key == Key.F1)
        {
            ShowHelp();
            e.Handled = true;
            return;
        }
        base.OnPreviewKeyDown(e);
    }

    /// <summary>内蔵コアのプレイ画面を開く。同時に動かせるコアは1つなので、開いていれば前に出すだけ</summary>
    private bool OpenPlayWindow(PlayContext context, SessionRequest request)
    {
        if (playWindow != null)
        {
            playWindow.Activate();
            return false;
        }
        // プレビューのコアを止めてから遊ぶ（同時に動かせるコアは1つ）
        preview.Suspend();
        playWindow = new PlayWindow(context, request);
        playWindow.Closed += (_, _) =>
        {
            playWindow = null;
            preview.Resume();
            Activate();
        };
        playWindow.Show();
        return true;
    }

    public void ShowWelcomeIfNeeded()
    {
        if (vm.Settings.FirstRunDone) return;
        vm.Settings.FirstRunDone = true;
        vm.Save();
        // 機種は最初は空なので、最初の1つを好きな名前で作ってもらう
        if (vm.HasNoSystems && vm.Confirm?.Invoke(Loc.T("Welcome")) == true)
            vm.AddSystemCommand.Execute(null);
    }

    private (SystemConfig? config, bool deleted) EditSystem(SystemConfig? config)
    {
        var dlg = new SystemEditWindow(config) { Owner = this };
        var ok = dlg.ShowDialog() == true;
        if (dlg.Deleted) return (null, true);
        return ok ? (dlg.Result, false) : (null, false);
    }

    private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0) return;
        vm.ChangeScale(e.Delta);
        e.Handled = true;
    }

    // ---------- 機種一覧のドラッグ＆ドロップ ----------

    private Point systemDragStart;
    private SystemItem? systemDragItem;

    private void OnSystemMouseDown(object sender, MouseButtonEventArgs e)
    {
        systemDragStart = e.GetPosition(SystemList);
        systemDragItem = SystemAt(e.OriginalSource);
    }

    private void OnSystemMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || systemDragItem == null) return;
        var d = e.GetPosition(SystemList) - systemDragStart;
        if (Math.Abs(d.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(d.Y) < SystemParameters.MinimumVerticalDragDistance) return;
        var item = systemDragItem;
        systemDragItem = null;
        DragDrop.DoDragDrop(SystemList, item, DragDropEffects.Move);
        // 一覧の外で離しても、動かした並びのまま保存する
        vm.SaveSystemOrder();
    }

    /// <summary>ドラッグ中は、重ねた機種の位置へその場で動かして見せる</summary>
    private void OnSystemDragOver(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;
        if (e.Data.GetData(typeof(SystemItem)) is not SystemItem dragged) return;
        e.Effects = DragDropEffects.Move;
        e.Handled = true;
        var target = SystemAt(e.OriginalSource);
        if (target == null || target == dragged) return;
        var selected = vm.SelectedSystem;
        vm.MoveSystem(dragged, vm.Systems.IndexOf(target));
        if (vm.SelectedSystem != selected) vm.SelectedSystem = selected;
    }

    private void OnSystemDrop(object sender, DragEventArgs e) => e.Handled = true;

    private static SystemItem? SystemAt(object source)
    {
        for (var o = source as DependencyObject; o != null;
             o = o is System.Windows.Media.Visual ? System.Windows.Media.VisualTreeHelper.GetParent(o) : LogicalTreeHelper.GetParent(o))
            if (o is System.Windows.Controls.ListBoxItem li) return li.DataContext as SystemItem;
        return null;
    }

    private void OnGameDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (vm.SelectedItem != null) vm.LaunchCommand.Execute(null);
    }

    private void OnGameKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && vm.SelectedItem != null)
        {
            vm.LaunchCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void RestoreWindowPlacement(AppSettings s)
    {
        if (s.WindowWidth is > 300) Width = s.WindowWidth.Value;
        if (s.WindowHeight is > 200) Height = s.WindowHeight.Value;
        if (s.WindowLeft.HasValue && s.WindowTop.HasValue)
        {
            var left = s.WindowLeft.Value;
            var top = s.WindowTop.Value;
            // 画面外に出ていたら位置は復元しない
            if (left >= SystemParameters.VirtualScreenLeft - 50 && top >= SystemParameters.VirtualScreenTop - 50 &&
                left < SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - 100 &&
                top < SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - 100)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                Left = left;
                Top = top;
            }
        }
        else
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
        if (s.WindowMaximized) WindowState = WindowState.Maximized;
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        preview.Shutdown();
        playWindow?.Close();
        var s = vm.Settings;
        var bounds = WindowState == WindowState.Normal ? new Rect(Left, Top, Width, Height) : RestoreBounds;
        if (!bounds.IsEmpty && double.IsFinite(bounds.Left) && double.IsFinite(bounds.Width))
        {
            s.WindowLeft = bounds.Left;
            s.WindowTop = bounds.Top;
            s.WindowWidth = bounds.Width;
            s.WindowHeight = bounds.Height;
        }
        s.WindowMaximized = WindowState == WindowState.Maximized;
        vm.Save();
    }
}
