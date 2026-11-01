// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Controls;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;
using JPEmuRenamer.Ui;
using Microsoft.Win32;

namespace JPEmuRenamer.Views;

/// <summary>機種の追加・編集</summary>
public partial class SystemEditWindow : Window
{
    private readonly SystemConfig config;
    private readonly bool isNew;
    private bool loading = true;

    public SystemConfig? Result { get; private set; }
    public bool Deleted { get; private set; }

    private static readonly (string kind, string ja, string en)[] IconNames =
    {
        ("cart", "据置カートリッジ機", "Home console (cartridge)"),
        ("diskcard", "ディスクカード機", "Disk card"),
        ("disc", "ディスク機", "Disc console"),
        ("handV", "縦型携帯機", "Handheld (vertical)"),
        ("handH", "横型携帯機", "Handheld (horizontal)"),
        ("dual", "2画面携帯機", "Dual-screen handheld"),
        ("arcade", "アーケード", "Arcade"),
        ("pc", "パソコン", "Computer"),
        ("pad", "その他", "Other"),
    };

    public SystemEditWindow(SystemConfig? existing)
    {
        InitializeComponent();
        isNew = existing == null;
        config = existing?.Clone() ?? new SystemConfig { Name = "", Extensions = new List<string> { ".zip" } };
        DeleteButton.Visibility = isNew ? Visibility.Collapsed : Visibility.Visible;

        PresetBox.ItemsSource = SystemPresets.All;
        bool en = Loc.Instance.Language == "en";
        foreach (var (kind, ja, enName) in IconNames)
            IconBox.Items.Add(new ComboBoxItem { Content = en ? enName : ja, Tag = kind });
        AddItem(HeaderBox, "Auto", Loc.T("HeaderAuto"));
        AddItem(HeaderBox, "Include", Loc.T("HeaderInclude"));
        AddItem(HeaderBox, "Exclude", Loc.T("HeaderExclude"));
        AddItem(LaunchModeBox, "Core", Loc.T("ModeCore"));
        AddItem(LaunchModeBox, "External", Loc.T("ModeExternal"));

        Fill(config);
        loading = false;
        UpdateIconPreview();
    }

    private static void AddItem(ComboBox box, string value, string label) =>
        box.Items.Add(new ComboBoxItem { Content = label, Tag = value });

    private static void Select(ComboBox box, string value)
    {
        foreach (ComboBoxItem item in box.Items)
        {
            if ((string)item.Tag == value) { box.SelectedItem = item; return; }
        }
        if (box.Items.Count > 0) box.SelectedIndex = 0;
    }

    private static string Selected(ComboBox box, string fallback) =>
        (box.SelectedItem as ComboBoxItem)?.Tag as string ?? fallback;

    private void Fill(SystemConfig c)
    {
        NameBox.Text = c.Name;
        Select(IconBox, c.IconKind);
        ColorBox.Text = c.Color;
        CustomIconBox.Text = c.CustomIconPath;
        DatBox.Text = c.DatPath;
        RomBox.Text = string.Join(";", c.RomFolders);
        ExtBox.Text = string.Join(";", c.Extensions);
        Select(HeaderBox, c.HeaderMode);
        DisplayOnlyCheck.IsChecked = c.DisplayRenameOnly;
        Select(LaunchModeBox, c.LaunchMode);
        ExeBox.Text = c.EmulatorPath;
        ArgsBox.Text = c.EmulatorArgs;
    }

    private void OnPresetChanged(object sender, SelectionChangedEventArgs e)
    {
        if (loading || PresetBox.SelectedItem is not SystemPreset p) return;
        NameBox.Text = p.Name;
        ExtBox.Text = p.Extensions;
        Select(IconBox, p.Icon);
        ColorBox.Text = p.Color;
        DisplayOnlyCheck.IsChecked = p.DisplayRenameOnly;
    }

    private void OnIconChanged(object sender, SelectionChangedEventArgs e) => UpdateIconPreview();
    private void OnColorChanged(object sender, TextChangedEventArgs e) => UpdateIconPreview();

    private void UpdateIconPreview()
    {
        if (IconPreview == null || IconBox == null || ColorBox == null) return;
        IconPreview.Data = IconFactory.Get(Selected(IconBox, "pad"));
        IconPreview.Stroke = BrushUtil.FromHex(ColorBox.Text);
    }

    private void OnBrowseIcon(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "Image|*.png;*.ico;*.jpg;*.jpeg;*.bmp|All|*.*" };
        if (dlg.ShowDialog(this) == true) CustomIconBox.Text = dlg.FileName;
    }

    private void OnBrowseDat(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "DAT|*.xml;*.dat|All|*.*" };
        if (dlg.ShowDialog(this) == true) DatBox.Text = dlg.FileName;
    }

    private void OnBrowseRom(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog { Multiselect = true };
        if (dlg.ShowDialog(this) != true) return;
        var current = RomBox.Text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
        foreach (var f in dlg.FolderNames)
            if (!current.Contains(f, StringComparer.OrdinalIgnoreCase)) current.Add(f);
        RomBox.Text = string.Join(";", current);
    }

    private void OnBrowseExe(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog { Filter = "exe|*.exe|All|*.*" };
        if (dlg.ShowDialog(this) != true) return;
        ExeBox.Text = dlg.FileName;
        if (Selected(LaunchModeBox, "Core") != "External" &&
            MessageBox.Show(this, Loc.T("SetDefaultExe"), "JPEmu Renamer", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            Select(LaunchModeBox, "External");
        }
    }

    private void OnDelete(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(this, Loc.F("ConfirmDelete", config.Name), "JPEmu Renamer", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;
        Deleted = true;
        DialogResult = false;
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        var name = NameBox.Text.Trim();
        if (name.Length == 0)
        {
            MessageBox.Show(this, Loc.T("NameRequired"), "JPEmu Renamer", MessageBoxButton.OK, MessageBoxImage.Information);
            NameBox.Focus();
            return;
        }

        config.Name = name;
        config.IconKind = Selected(IconBox, "pad");
        config.Color = ColorBox.Text.Trim();
        config.CustomIconPath = CopyCustomIcon(CustomIconBox.Text.Trim());
        config.DatPath = DatBox.Text.Trim().Trim('"');
        config.RomFolders = RomBox.Text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(f => f.Trim('"')).ToList();
        config.Extensions = SystemPresets.ParseExtensions(ExtBox.Text);
        config.HeaderMode = Selected(HeaderBox, "Auto");
        config.DisplayRenameOnly = DisplayOnlyCheck.IsChecked == true;
        config.LaunchMode = Selected(LaunchModeBox, "Core");
        config.EmulatorPath = ExeBox.Text.Trim().Trim('"');
        config.EmulatorArgs = string.IsNullOrWhiteSpace(ArgsBox.Text) ? "\"{rom}\"" : ArgsBox.Text.Trim();

        Result = config;
        DialogResult = true;
    }

    /// <summary>自作アイコンはアプリのデータフォルダにコピーして使う（元画像を消しても表示が残る）</summary>
    private string CopyCustomIcon(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "";
        try
        {
            path = path.Trim('"');
            if (!File.Exists(path)) return "";
            Directory.CreateDirectory(Storage.IconDir);
            if (System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(path))!
                .Equals(System.IO.Path.GetFullPath(Storage.IconDir), StringComparison.OrdinalIgnoreCase))
                return path;
            var dest = System.IO.Path.Combine(Storage.IconDir, config.Id + System.IO.Path.GetExtension(path).ToLowerInvariant());
            File.Copy(path, dest, true);
            return dest;
        }
        catch
        {
            return path;
        }
    }
}
