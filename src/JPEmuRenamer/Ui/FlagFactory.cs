// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Media;

namespace JPEmuRenamer.Ui;

/// <summary>
/// OfflineList の DAT の &lt;location&gt;（発売地域の番号）から、タイトルの前に出す小さな国旗を作る。
/// 国旗は 18×12 の図形で描き、複数地域の発売（欧州＋米国など）は横に並べる。
/// </summary>
public static class FlagFactory
{
    // OfflineList の地域番号 → 国旗（国・地域コード。複数地域は並べる）
    private static readonly Dictionary<int, string[]> Locations = new()
    {
        [0] = new[] { "EU" }, [1] = new[] { "US" }, [2] = new[] { "DE" }, [3] = new[] { "CN" },
        [4] = new[] { "ES" }, [5] = new[] { "FR" }, [6] = new[] { "IT" }, [7] = new[] { "JP" },
        [8] = new[] { "NL" }, [9] = new[] { "SE" }, [10] = new[] { "DK" }, [11] = new[] { "FI" },
        [12] = new[] { "NO" }, [13] = new[] { "PL" }, [14] = new[] { "PT" }, [15] = new[] { "GB" },
        [16] = new[] { "EU", "US" }, [17] = new[] { "EU", "US", "JP" }, [18] = new[] { "US", "JP" },
        [19] = new[] { "AU" }, [20] = new[] { "KP" }, [21] = new[] { "BR" }, [22] = new[] { "KR" },
        [23] = new[] { "BR", "EU" }, [24] = new[] { "BR", "EU", "US" }, [25] = new[] { "BR", "US" },
    };

    private const double W = 18, H = 12, Gap = 3;
    private static readonly Dictionary<int, ImageSource?> Cache = new();

    public static ImageSource? Get(int? location)
    {
        if (location is not int loc) return null;
        if (Cache.TryGetValue(loc, out var img)) return img;
        img = Locations.TryGetValue(loc, out var codes) ? Build(codes) : null;
        Cache[loc] = img;
        return img;
    }

    private static ImageSource Build(string[] codes)
    {
        var group = new DrawingGroup();
        for (int i = 0; i < codes.Length; i++)
        {
            var flag = new DrawingGroup { Transform = new TranslateTransform(i * (W + Gap), 0) };
            Draw(flag, codes[i]);
            // 白っぽい国旗が背景に溶けないよう、薄い枠を付ける
            flag.Children.Add(new GeometryDrawing(null, new Pen(new SolidColorBrush(Color.FromArgb(0x55, 0, 0, 0)), 0.6),
                new RectangleGeometry(new Rect(0.3, 0.3, W - 0.6, H - 0.6))));
            group.Children.Add(flag);
        }
        var image = new DrawingImage(group);
        image.Freeze();
        return image;
    }

    // ---------- 国旗の描画（小さく表示するので細部は省いた形） ----------

    private static void Draw(DrawingGroup g, string code)
    {
        switch (code)
        {
            case "JP": Rect(g, "#FFFFFF"); Circle(g, "#BC002D", 9, 6, 3.6); break;
            case "US":
                for (int s = 0; s < 7; s++) Rect(g, s % 2 == 0 ? "#B22234" : "#FFFFFF", 0, s * H / 7, W, H / 7 + 0.05);
                Rect(g, "#3C3B6E", 0, 0, 8, H * 4 / 7);
                break;
            case "EU":
                Rect(g, "#003399");
                for (int s = 0; s < 12; s++)
                {
                    double a = s * Math.PI / 6;
                    Circle(g, "#FFCC00", 9 + Math.Sin(a) * 3.8, 6 - Math.Cos(a) * 3.8, 0.6);
                }
                break;
            case "DE": Bands(g, false, "#000000", "#DD0000", "#FFCE00"); break;
            case "CN": Rect(g, "#DE2910"); Star(g, "#FFDE00", 4, 3.6, 2.4); break;
            case "ES": Rect(g, "#AA151B"); Rect(g, "#F1BF00", 0, 3, W, 6); break;
            case "FR": Bands(g, true, "#0055A4", "#FFFFFF", "#EF4135"); break;
            case "IT": Bands(g, true, "#009246", "#FFFFFF", "#CE2B37"); break;
            case "NL": Bands(g, false, "#AE1C28", "#FFFFFF", "#21468B"); break;
            case "SE": Rect(g, "#006AA7"); Cross(g, "#FECC00", 5, 2.2); break;
            case "DK": Rect(g, "#C8102E"); Cross(g, "#FFFFFF", 5, 2.2); break;
            case "FI": Rect(g, "#FFFFFF"); Cross(g, "#002F6C", 5, 2.6); break;
            case "NO": Rect(g, "#BA0C2F"); Cross(g, "#FFFFFF", 5, 3.2); Cross(g, "#00205B", 5.6, 1.4); break;
            case "PL": Rect(g, "#FFFFFF"); Rect(g, "#DC143C", 0, 6, W, 6); break;
            case "PT": Rect(g, "#FF0000"); Rect(g, "#006600", 0, 0, 7, H); Circle(g, "#FFCC00", 7, 6, 2.2); break;
            case "GB":
                Rect(g, "#012169");
                Line(g, "#FFFFFF", 0, 0, W, H, 2.4); Line(g, "#FFFFFF", W, 0, 0, H, 2.4);
                Line(g, "#C8102E", 0, 0, W, H, 0.8); Line(g, "#C8102E", W, 0, 0, H, 0.8);
                Rect(g, "#FFFFFF", 7, 0, 4, H); Rect(g, "#FFFFFF", 0, 4, W, 4);
                Rect(g, "#C8102E", 7.8, 0, 2.4, H); Rect(g, "#C8102E", 0, 4.8, W, 2.4);
                break;
            case "AU":
                Rect(g, "#012169");
                Rect(g, "#FFFFFF", 0, 2.2, 8, 1.6); Rect(g, "#FFFFFF", 3.2, 0, 1.6, 6);
                Rect(g, "#C8102E", 0, 2.6, 8, 0.8); Rect(g, "#C8102E", 3.6, 0, 0.8, 6);
                Star(g, "#FFFFFF", 4, 9, 1.6); Star(g, "#FFFFFF", 13.5, 3, 1); Star(g, "#FFFFFF", 15.5, 6.5, 1); Star(g, "#FFFFFF", 13, 9.5, 1);
                break;
            case "KP":
                Rect(g, "#024FA2"); Rect(g, "#FFFFFF", 0, 2, W, 8); Rect(g, "#ED1C27", 0, 2.6, W, 6.8);
                Circle(g, "#FFFFFF", 6, 6, 2.4); Star(g, "#ED1C27", 6, 6, 2);
                break;
            case "BR":
                Rect(g, "#009C3B");
                g.Children.Add(new GeometryDrawing(Brush("#FFDF00"), null, Geometry.Parse("M9,1.2 L16.6,6 L9,10.8 L1.4,6 Z")));
                Circle(g, "#002776", 9, 6, 2.8);
                break;
            case "KR":
                Rect(g, "#FFFFFF");
                g.Children.Add(new GeometryDrawing(Brush("#CD2E3A"), null, Geometry.Parse("M6,6 A3,3 0 0 1 12,6 Z")));
                g.Children.Add(new GeometryDrawing(Brush("#0047A0"), null, Geometry.Parse("M6,6 A3,3 0 0 0 12,6 Z")));
                break;
        }
    }

    private static SolidColorBrush Brush(string hex)
    {
        var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        b.Freeze();
        return b;
    }

    private static void Rect(DrawingGroup g, string hex, double x = 0, double y = 0, double w = W, double h = H) =>
        g.Children.Add(new GeometryDrawing(Brush(hex), null, new RectangleGeometry(new Rect(x, y, w, h))));

    private static void Circle(DrawingGroup g, string hex, double cx, double cy, double r) =>
        g.Children.Add(new GeometryDrawing(Brush(hex), null, new EllipseGeometry(new Point(cx, cy), r, r)));

    private static void Line(DrawingGroup g, string hex, double x1, double y1, double x2, double y2, double thick) =>
        g.Children.Add(new GeometryDrawing(null, new Pen(Brush(hex), thick), new LineGeometry(new Point(x1, y1), new Point(x2, y2))));

    /// <summary>3色の帯（vertical=true で縦縞）</summary>
    private static void Bands(DrawingGroup g, bool vertical, params string[] colors)
    {
        for (int i = 0; i < colors.Length; i++)
        {
            if (vertical) Rect(g, colors[i], i * W / colors.Length, 0, W / colors.Length + 0.05, H);
            else Rect(g, colors[i], 0, i * H / colors.Length, W, H / colors.Length + 0.05);
        }
    }

    /// <summary>北欧の十字（縦の線は左寄り）</summary>
    private static void Cross(DrawingGroup g, string hex, double x, double thick)
    {
        Rect(g, hex, x + 1 - thick / 2, 0, thick, H);
        Rect(g, hex, 0, H / 2 - thick / 2, W, thick);
    }

    private static void Star(DrawingGroup g, string hex, double cx, double cy, double r)
    {
        var fig = new PathFigure { IsClosed = true, IsFilled = true };
        for (int i = 0; i < 10; i++)
        {
            double rr = i % 2 == 0 ? r : r * 0.42;
            double a = -Math.PI / 2 + i * Math.PI / 5;
            var p = new Point(cx + Math.Cos(a) * rr, cy + Math.Sin(a) * rr);
            if (i == 0) fig.StartPoint = p; else fig.Segments.Add(new LineSegment(p, true));
        }
        g.Children.Add(new GeometryDrawing(Brush(hex), null, new PathGeometry(new[] { fig })));
    }
}
