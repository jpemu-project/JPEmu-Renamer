// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;

namespace JPEmuRenamer.Ui;

/// <summary>ライト／ダーク／アクアの色を切り替える。色は DynamicResource で参照しているので、開いている画面もその場で変わる</summary>
public static class ThemeManager
{
    public const string Light = "Light";
    public const string Dark = "Dark";
    public const string Aqua = "Aqua";

    public static void Apply(string theme)
    {
        var name = theme is Dark or Aqua ? theme : Light;
        var colors = new ResourceDictionary { Source = new Uri($"pack://application:,,,/Ui/Colors.{name}.xaml") };
        // App.xaml の MergedDictionaries の先頭が色の辞書
        Application.Current.Resources.MergedDictionaries[0] = colors;
    }
}
