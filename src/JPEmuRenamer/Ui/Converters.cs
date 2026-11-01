// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JPEmuRenamer.Ui;

/// <summary>false/null → Visible、true → Collapsed</summary>
public sealed class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? Visibility.Collapsed : Visibility.Visible;
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is Visibility v && v != Visibility.Visible;
}

/// <summary>値と ConverterParameter が等しいかどうか（RadioButton の切り替えに使う）</summary>
public sealed class EqualsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        Equals(value?.ToString(), parameter?.ToString());
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is true ? parameter?.ToString() ?? "" : Binding.DoNothing;
}

public static class BrushUtil
{
    public static SolidColorBrush FromHex(string? hex, string fallback = "#9AA3B2")
    {
        try
        {
            var c = (Color)ColorConverter.ConvertFromString(string.IsNullOrWhiteSpace(hex) ? fallback : hex);
            var b = new SolidColorBrush(c);
            b.Freeze();
            return b;
        }
        catch (FormatException)
        {
            var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(fallback));
            b.Freeze();
            return b;
        }
    }

    /// <summary>自作アイコン画像を読み込む（ファイルをロックしないようメモリに読み込む）</summary>
    public static ImageSource? LoadImage(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path)) return null;
        try
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.DecodePixelWidth = 128;
            bmp.UriSource = new Uri(path, UriKind.Absolute);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch
        {
            return null;
        }
    }
}
