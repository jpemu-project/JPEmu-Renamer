// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using JPEmuRenamer.Emulation;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;
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
        vm.EditSystemDialog = EditSystem;
        vm.OpenAppSettingsDialog = () => new AppSettingsWindow(vm) { Owner = this }.ShowDialog();
        vm.OpenPlayWindow = OpenPlayWindow;
        vm.OpenPlaySettingsDialog = (ctx, scope) => new PlaySettingsWindow(ctx, scope) { Owner = this }.ShowDialog();

        RestoreWindowPlacement(vm.Settings);
    }

    private PlayWindow? playWindow;

    /// <summary>内蔵コアのプレイ画面を開く。同時に動かせるコアは1つなので、開いていれば前に出すだけ</summary>
    private bool OpenPlayWindow(PlayContext context, SessionRequest request)
    {
        if (playWindow != null)
        {
            playWindow.Activate();
            return false;
        }
        playWindow = new PlayWindow(context, request);
        playWindow.Closed += (_, _) =>
        {
            playWindow = null;
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
        if (vm.NeedsSetup && vm.Confirm?.Invoke(Loc.T("Welcome")) == true)
            vm.EditSystemCommand.Execute(null);
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
