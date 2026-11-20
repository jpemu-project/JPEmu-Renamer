// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JPEmuRenamer.Ui;

/// <summary>
/// へこんだ面（ソフトUI）。地の色と枠に加えて、内側に影を描く：左上のふちに暗い影、右下のふちに明るい影。
/// WPF には内側の影が無いので、ふちから内側へ少しずつ薄くなる線を重ねて、ぼかした影のように見せる。
/// 影は子要素（一覧など）の下に描くので、文字にはかからない。
/// </summary>
public class SoftInsetBorder : Border
{
    public static readonly DependencyProperty ShadowColorProperty = DependencyProperty.Register(
        nameof(ShadowColor), typeof(Color), typeof(SoftInsetBorder),
        new FrameworkPropertyMetadata(Colors.Gray, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty HighlightColorProperty = DependencyProperty.Register(
        nameof(HighlightColor), typeof(Color), typeof(SoftInsetBorder),
        new FrameworkPropertyMetadata(Colors.White, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>影の幅（ふちから内側へ何ピクセルまで薄くなるか）</summary>
    public static readonly DependencyProperty ShadowSizeProperty = DependencyProperty.Register(
        nameof(ShadowSize), typeof(double), typeof(SoftInsetBorder),
        new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>影の濃さ（0〜1）</summary>
    public static readonly DependencyProperty ShadowStrengthProperty = DependencyProperty.Register(
        nameof(ShadowStrength), typeof(double), typeof(SoftInsetBorder),
        new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public Color ShadowColor { get => (Color)GetValue(ShadowColorProperty); set => SetValue(ShadowColorProperty, value); }
    public Color HighlightColor { get => (Color)GetValue(HighlightColorProperty); set => SetValue(HighlightColorProperty, value); }
    public double ShadowSize { get => (double)GetValue(ShadowSizeProperty); set => SetValue(ShadowSizeProperty, value); }
    public double ShadowStrength { get => (double)GetValue(ShadowStrengthProperty); set => SetValue(ShadowStrengthProperty, value); }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        double w = ActualWidth, h = ActualHeight;
        if (w < 4 || h < 4) return;
        double r = CornerRadius.TopLeft;
        double s = Math.Min(ShadowSize, Math.Min(w, h) / 2);
        var dark = Darker(ShadowColor);
        double da = ShadowStrength * 0.75, la = ShadowStrength;

        // 角の丸みに合わせて切り抜き、上と左のふちに暗い帯、下と右のふちに明るい帯を描く（CSS の inset の影と同じ考え方）。
        // 帯はふちで濃く、内側へ向かってなめらかに透明になる
        dc.PushClip(new RectangleGeometry(new Rect(0, 0, w, h), r, r));
        dc.DrawRectangle(Band(dark, da, new Point(0, 0), new Point(0, 1)), null, new Rect(0, 0, w, s));
        dc.DrawRectangle(Band(dark, da, new Point(0, 0), new Point(1, 0)), null, new Rect(0, 0, s, h));
        dc.DrawRectangle(Band(HighlightColor, la, new Point(0, 1), new Point(0, 0)), null, new Rect(0, h - s, w, s));
        dc.DrawRectangle(Band(HighlightColor, la, new Point(1, 0), new Point(0, 0)), null, new Rect(w - s, 0, s, h));
        dc.Pop();
    }

    /// <summary>ふち（start）で濃く、内側（end）で透明になる帯。ぼかしたように見えるよう途中の濃さを落とす</summary>
    private static Brush Band(Color c, double alpha, Point start, Point end)
    {
        var b = new LinearGradientBrush { StartPoint = start, EndPoint = end };
        b.GradientStops.Add(new GradientStop(WithAlpha(c, alpha), 0));
        b.GradientStops.Add(new GradientStop(WithAlpha(c, alpha * 0.35), 0.4));
        b.GradientStops.Add(new GradientStop(WithAlpha(c, 0), 1));
        b.Freeze();
        return b;
    }

    /// <summary>影の色を少しだけ暗くする（地の色に近い影だと、へこみが平らに見えるため）</summary>
    private static Color Darker(Color c) =>
        Color.FromRgb((byte)(c.R * 0.82), (byte)(c.G * 0.82), (byte)(c.B * 0.82));

    private static Color WithAlpha(Color c, double a) =>
        Color.FromArgb((byte)Math.Clamp(a * 255, 0, 255), c.R, c.G, c.B);
}
