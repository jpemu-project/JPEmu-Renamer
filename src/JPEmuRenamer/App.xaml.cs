// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Threading;
using JPEmuRenamer.Services;
using JPEmuRenamer.ViewModels;
using JPEmuRenamer.Views;

namespace JPEmuRenamer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnUnhandled;

        var settings = Storage.LoadSettings();
        var vm = new MainViewModel(settings);
        var window = new MainWindow(vm);
        MainWindow = window;
        window.Show();
        vm.Start();
        window.ShowWelcomeIfNeeded();
    }

    private void OnUnhandled(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, "JPEmu Renamer", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
