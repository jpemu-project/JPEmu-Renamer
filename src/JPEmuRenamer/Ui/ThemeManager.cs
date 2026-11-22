// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Media;

namespace JPEmuRenamer.Ui;

/// <summary>テーマ（ライト／ダーク／アクア／メタル／ピンク／シック）の色を切り替える。色は DynamicResource で参照しているので、開いている画面もその場で変わる</summary>
public static class ThemeManager
{
    public const string Light = "Light";
    public const string Dark = "Dark";
    public const string Aqua = "Aqua";
    public const string Metal = "Metal";
    public const string Pink = "Pink";
    public const string Chic = "Chic";

    /// <summary>テーマが変わった（DynamicResource で追えない、描いた絵を作り直すため）</summary>
    public static event Action? Changed;

    /// <summary>いまのテーマの色（選んだボタンのグラデーションの、始まりと終わり）</summary>
    public static (Color from, Color to) AccentColors
    {
        get
        {
            if (Application.Current?.TryFindResource("AccentBrush") is GradientBrush { GradientStops.Count: > 0 } b)
                return (b.GradientStops[0].Color, b.GradientStops[b.GradientStops.Count - 1].Color);
            return (Color.FromRgb(0x22, 0xB8, 0xCF), Color.FromRgb(0x3D, 0xD6, 0xA0));
        }
    }

    public static void Apply(string theme)
    {
        var name = theme is Dark or Aqua or Metal or Pink or Chic ? theme : Light;
        var colors = new ResourceDictionary { Source = new Uri($"pack://application:,,,/Ui/Colors.{name}.xaml") };
        // App.xaml の MergedDictionaries の先頭が色の辞書
        Application.Current.Resources.MergedDictionaries[0] = colors;
        Changed?.Invoke();
    }
}
