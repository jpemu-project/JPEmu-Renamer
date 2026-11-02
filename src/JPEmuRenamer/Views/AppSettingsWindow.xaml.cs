// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Controls;
using JPEmuRenamer.Models;
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
        HideExtCheck.IsChecked = vm.Settings.HideExtensions;
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
        bool extChanged = vm.Settings.HideExtensions != (HideExtCheck.IsChecked == true);
        vm.Settings.RenameInsideZip = InsideZipCheck.IsChecked == true;
        vm.Settings.ScanSubfolders = SubfolderCheck.IsChecked == true;
        vm.Settings.HideExtensions = HideExtCheck.IsChecked == true;
        GameItem.HideExtensions = vm.Settings.HideExtensions;
        vm.Save();
        // 一覧の行を作り直して表示名を反映する（読み込み済みのCRCは再利用されるのですぐ終わる）
        if (subChanged || extChanged) _ = vm.ReloadAsync();
    }

    private void OnPlaySettings(object sender, RoutedEventArgs e) => vm.OpenPlaySettingsCommand.Execute("Global");

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
