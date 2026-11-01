// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Controls;
using JPEmuRenamer.Services;
using JPEmuRenamer.ViewModels;

namespace JPEmuRenamer.Views;

/// <summary>アプリの表示（言語・表示倍率・文字サイズなど）。変更はその場で反映される。</summary>
public partial class AppSettingsWindow : Window
{
    private readonly MainViewModel vm;
    private readonly bool loading;

    public AppSettingsWindow(MainViewModel vm)
    {
        InitializeComponent();
        this.vm = vm;
        DataContext = vm;

        loading = true;
        LanguageBox.Items.Add(new ComboBoxItem { Content = Loc.T("LangAuto"), Tag = "" });
        LanguageBox.Items.Add(new ComboBoxItem { Content = "日本語", Tag = "ja" });
        LanguageBox.Items.Add(new ComboBoxItem { Content = "English", Tag = "en" });
        foreach (ComboBoxItem item in LanguageBox.Items)
            if ((string)item.Tag == vm.Settings.Language) LanguageBox.SelectedItem = item;
        if (LanguageBox.SelectedItem == null) LanguageBox.SelectedIndex = 0;

        InsideZipCheck.IsChecked = vm.Settings.RenameInsideZip;
        SubfolderCheck.IsChecked = vm.Settings.ScanSubfolders;
        loading = false;
    }

    private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
    {
        if (loading) return;
        if (LanguageBox.SelectedItem is ComboBoxItem item)
            vm.ApplyLanguage((string)item.Tag);
    }

    private void OnOptionChanged(object sender, RoutedEventArgs e)
    {
        if (loading) return;
        bool subChanged = vm.Settings.ScanSubfolders != (SubfolderCheck.IsChecked == true);
        vm.Settings.RenameInsideZip = InsideZipCheck.IsChecked == true;
        vm.Settings.ScanSubfolders = SubfolderCheck.IsChecked == true;
        vm.Save();
        if (subChanged) _ = vm.ReloadAsync();
    }

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
