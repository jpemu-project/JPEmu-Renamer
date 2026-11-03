// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Threading;
using JPEmuRenamer.Services;
using JPEmuRenamer.ViewModels;
using JPEmuRenamer.Views;

namespace JPEmuRenamer;

public partial class App : Application
{
    // 二重起動を防ぐ印（アプリが終わるまで持ち続ける）
    private static Mutex? singleInstance;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += OnUnhandled;

        // すでに起動しているときは、新しく開かずにそのウィンドウを前に出す
        singleInstance = new Mutex(true, @"Local\JPEmuRenamer.SingleInstance", out bool isFirst);
        if (!isFirst)
        {
            BringExistingToFront();
            Shutdown();
            return;
        }

        var settings = Storage.LoadSettings();
        Ui.ThemeManager.Apply(settings.Theme);
        var vm = new MainViewModel(settings);
        var window = new MainWindow(vm);
        MainWindow = window;
        window.Show();
        vm.Start();
        window.ShowWelcomeIfNeeded();
    }

    private static void BringExistingToFront()
    {
        var me = System.Diagnostics.Process.GetCurrentProcess();
        foreach (var p in System.Diagnostics.Process.GetProcessesByName(me.ProcessName))
        {
            if (p.Id == me.Id || p.MainWindowHandle == IntPtr.Zero) continue;
            if (IsIconic(p.MainWindowHandle)) ShowWindow(p.MainWindowHandle, SwRestore);
            SetForegroundWindow(p.MainWindowHandle);
            return;
        }
    }

    private const int SwRestore = 9;
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern bool IsIconic(IntPtr hWnd);

    private void OnUnhandled(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(e.Exception.Message, "JPEmu Renamer", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }
}
