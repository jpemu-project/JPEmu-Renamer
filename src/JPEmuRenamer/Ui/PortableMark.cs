// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using JPEmuRenamer.Services;

namespace JPEmuRenamer.Ui;

/// <summary>パス欄の下に出す「フォルダごと持ち運べるか」の印（欄が空なら出さない）</summary>
public sealed class PortableMark : TextBlock
{
    public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
        nameof(Path), typeof(string), typeof(PortableMark),
        new PropertyMetadata("", (d, _) => ((PortableMark)d).Update()));

    public string? Path
    {
        get => (string?)GetValue(PathProperty);
        set => SetValue(PathProperty, value);
    }

    /// <summary>「;」で区切って複数のフォルダを書ける欄か</summary>
    public bool IsList { get; set; }

    public PortableMark()
    {
        FontSize = 12;
        TextWrapping = TextWrapping.Wrap;
        Margin = new Thickness(0, 4, 0, 0);
        Visibility = Visibility.Collapsed;
        Loaded += (_, _) =>
        {
            Loc.Instance.PropertyChanged -= OnLanguageChanged;
            Loc.Instance.PropertyChanged += OnLanguageChanged;
            Update();
        };
        Unloaded += (_, _) => Loc.Instance.PropertyChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(object? sender, PropertyChangedEventArgs e) => Update();

    private void Update()
    {
        var text = Path ?? "";
        var paths = (IsList ? text.Split(';') : new[] { text })
            .Select(p => p.Trim().Trim('"')).Where(p => p.Length > 0).ToList();
        if (paths.Count == 0)
        {
            Visibility = Visibility.Collapsed;
            return;
        }
        int outside = paths.Count(p => !Storage.IsPortable(p));
        Text = outside == 0 ? Loc.T("PortableOk")
             : paths.Count == 1 ? Loc.T("PortableNg")
             : Loc.F("PortableSome", paths.Count, outside);
        SetResourceReference(ForegroundProperty, outside == 0 ? "OkFgBrush" : "WarnFgBrush");
        Visibility = Visibility.Visible;
    }
}
