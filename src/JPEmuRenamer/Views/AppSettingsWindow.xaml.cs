// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Controls;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;
using JPEmuRenamer.Ui;
using JPEmuRenamer.ViewModels;

namespace JPEmuRenamer.Views;

/// <summary>アプリの表示（言語・表示倍率・文字サイズなど）。変更はその場で反映される。</summary>
public partial class AppSettingsWindow : Window
{
    private readonly MainViewModel vm;
    private readonly bool loading;
    private readonly Dictionary<RadioButton, string> themeRadios;

    public AppSettingsWindow(MainViewModel vm)
    {
        InitializeComponent();
        this.vm = vm;
        DataContext = vm;

        loading = true;
        LanguageBox.Items.Add(new ComboBoxItem { Content = Loc.T("LangAuto"), Tag = "" });
        foreach (var (code, name) in Loc.Languages)
            LanguageBox.Items.Add(new ComboBoxItem { Content = name, Tag = code });
        foreach (ComboBoxItem item in LanguageBox.Items)
            if ((string)item.Tag == vm.Settings.Language) LanguageBox.SelectedItem = item;
        if (LanguageBox.SelectedItem == null) LanguageBox.SelectedIndex = 0;
        themeRadios = new()
        {
            [ThemeLightRadio] = ThemeManager.Light, [ThemeDarkRadio] = ThemeManager.Dark, [ThemeAquaRadio] = ThemeManager.Aqua,
            [ThemeMetalRadio] = ThemeManager.Metal, [ThemePinkRadio] = ThemeManager.Pink, [ThemeChicRadio] = ThemeManager.Chic
        };
        (themeRadios.FirstOrDefault(r => r.Value == vm.Settings.Theme).Key ?? ThemeLightRadio).IsChecked = true;

        InsideZipCheck.IsChecked = vm.Settings.RenameInsideZip;
        SubfolderCheck.IsChecked = vm.Settings.ScanSubfolders;
        HideExtCheck.IsChecked = vm.Settings.HideExtensions;
        ShowFlagsCheck.IsChecked = vm.Settings.ShowFlags;
        PreviewCheck.IsChecked = vm.Settings.Preview.Enabled;
        var p = vm.Settings.Preview;
        OwnShotFolderBox.Text = p.OwnShotFolder;
        PreferOwnShotCheck.IsChecked = p.PreferOwnShot;
        AddSlider(Loc.T("PreviewSpeed"), 1, 8, p.Speed, "{0}x", v => p.Speed = v);
        AddSlider(Loc.T("PreviewSkip"), 0, 30, p.SkipSeconds, Loc.T("SecondsFmt"), v => p.SkipSeconds = v);
        AddSlider(Loc.T("PreviewLoop"), 10, 180, p.LoopSeconds, Loc.T("SecondsFmt"), v => p.LoopSeconds = v, 10);
        AddSlider(Loc.T("PreviewSnapshotTime"), 5, 120, p.SnapshotSeconds, Loc.T("SecondsFmt"), v => p.SnapshotSeconds = v, 5);
        loading = false;
    }

    /// <summary>自動プレビューの数値の設定（つまみ1本）</summary>
    private void AddSlider(string label, int min, int max, int value, string format, Action<int> set, int step = 1)
    {
        var grid = new Grid { Margin = new Thickness(0, 4, 0, 0) };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(190) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(56) });
        var name = new TextBlock { Text = label, FontSize = 13, VerticalAlignment = VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
        var text = new TextBlock { FontSize = 13, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) };
        var slider = new Slider
        {
            Minimum = min, Maximum = max, TickFrequency = step, IsSnapToTickEnabled = true,
            Value = Math.Clamp(value, min, max), VerticalAlignment = VerticalAlignment.Center
        };
        text.Text = string.Format(format, (int)slider.Value);
        slider.ValueChanged += (_, _) =>
        {
            text.Text = string.Format(format, (int)slider.Value);
            set((int)slider.Value);
            vm.Save();
        };
        Grid.SetColumn(slider, 1);
        Grid.SetColumn(text, 2);
        grid.Children.Add(name);
        grid.Children.Add(slider);
        grid.Children.Add(text);
        PreviewOptionsPanel.Children.Add(grid);
    }

    private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
    {
        if (loading) return;
        if (LanguageBox.SelectedItem is ComboBoxItem item)
            vm.ApplyLanguage((string)item.Tag);
    }

    private void OnResetScale(object sender, RoutedEventArgs e) => vm.UiScale = new AppSettings().UiScale;

    private void OnResetFontSize(object sender, RoutedEventArgs e) => vm.FontSize = new AppSettings().FontSize;

    private void OnThemeChanged(object sender, RoutedEventArgs e)
    {
        if (loading) return;
        vm.Settings.Theme = themeRadios.FirstOrDefault(r => r.Key.IsChecked == true).Value ?? ThemeManager.Light;
        ThemeManager.Apply(vm.Settings.Theme);
        vm.Save();
    }

    private void OnOptionChanged(object sender, RoutedEventArgs e)
    {
        if (loading) return;
        bool subChanged = vm.Settings.ScanSubfolders != (SubfolderCheck.IsChecked == true);
        bool extChanged = vm.Settings.HideExtensions != (HideExtCheck.IsChecked == true)
                          || vm.Settings.ShowFlags != (ShowFlagsCheck.IsChecked == true);
        vm.Settings.RenameInsideZip = InsideZipCheck.IsChecked == true;
        vm.Settings.ScanSubfolders = SubfolderCheck.IsChecked == true;
        vm.Settings.HideExtensions = HideExtCheck.IsChecked == true;
        vm.Settings.ShowFlags = ShowFlagsCheck.IsChecked == true;
        vm.Settings.Preview.Enabled = PreviewCheck.IsChecked == true;
        vm.Settings.Preview.PreferOwnShot = PreferOwnShotCheck.IsChecked == true;
        GameItem.HideExtensions = vm.Settings.HideExtensions;
        GameItem.ShowFlags = vm.Settings.ShowFlags;
        vm.Save();
        // 一覧の行を作り直して表示名を反映する（読み込み済みのCRCは再利用されるのですぐ終わる）
        if (subChanged || extChanged) _ = vm.ReloadAsync();
    }

    private void OnBrowseOwnShotFolder(object sender, RoutedEventArgs e)
    {
        var dlg = new Microsoft.Win32.OpenFolderDialog();
        if (Directory.Exists(vm.Settings.Preview.OwnShotFolder)) dlg.InitialDirectory = vm.Settings.Preview.OwnShotFolder;
        if (dlg.ShowDialog(this) != true) return;
        vm.Settings.Preview.OwnShotFolder = dlg.FolderName;
        OwnShotFolderBox.Text = dlg.FolderName;
        vm.Save();
    }

    private void OnClearOwnShotFolder(object sender, RoutedEventArgs e)
    {
        vm.Settings.Preview.OwnShotFolder = "";
        OwnShotFolderBox.Text = "";
        vm.Save();
    }

    private void OnPlaySettings(object sender, RoutedEventArgs e) => vm.OpenPlaySettingsCommand.Execute("Global");

    private void OnBulkCores(object sender, RoutedEventArgs e) =>
        new CoreDownloadWindow(vm.Settings.Systems) { Owner = this }.ShowDialog();

    private void OnClose(object sender, RoutedEventArgs e) => Close();
}
