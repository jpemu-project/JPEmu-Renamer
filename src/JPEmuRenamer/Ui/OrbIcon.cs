// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Media;
using JPEmuRenamer.Models;
using JPEmuRenamer.Services;

namespace JPEmuRenamer.Ui;

/// <summary>
/// 機種のアイコン：機種の色の水玉の中に、実機の絵（MachineIcons）か線画（IconFactory）を入れる。
/// 48×48 の絵で、どの大きさでもぼやけない。
/// </summary>
public static class OrbIcon
{
    private const double BallRadius = 23.4;
    /// <summary>絵が収まる半径（水玉のふちに少し余白を残す）</summary>
    private const double ArtRadius = 20.8;
    private static readonly Point Center = new(24, 24);

    private static readonly Dictionary<(string art, Color color), ImageSource> Cache = new();

    public static ImageSource For(SystemConfig c) => Get(c.Name, c.IconKind, c.AutoIcon, c.Color);

    /// <param name="auto">機種名に合う実機の絵があれば、それを使う</param>
    public static ImageSource Get(string name, string? kind, bool auto, string? color)
    {
        var machine = auto ? SystemPresets.JapaneseNameOf(name) : null;
        var parts = MachineIcons.Get(machine);
        var ball = BrushUtil.FromHex(color).Color;
        ball.A = 255;
        var key = (parts != null ? "m:" + machine : "k:" + kind, ball);
        if (Cache.TryGetValue(key, out var cached)) return cached;

        var image = new DrawingImage(Draw(ball, parts, kind));
        image.Freeze();
        Cache[key] = image;
        return image;
    }

    private static Color Mix(Color a, Color b, double t) =>
        Color.FromRgb((byte)(a.R + (b.R - a.R) * t), (byte)(a.G + (b.G - a.G) * t), (byte)(a.B + (b.B - a.B) * t));

    private static Color Alpha(Color c, double a) => Color.FromArgb((byte)(a * 255), c.R, c.G, c.B);
    private static Color White(double a) => Alpha(Colors.White, a);

    /// <summary>色相を保ったまま濃くする（黒を混ぜると黄色がオリーブ色に濁るため）</summary>
    private static Color Deep(Color c)
    {
        static byte F(byte v) => (byte)(255 * Math.Pow(v / 255.0, 1.8) * .92);
        return Color.FromRgb(F(c.R), F(c.G), F(c.B));
    }

    private static LinearGradientBrush Vertical(params (double at, Color color)[] stops)
    {
        var b = new LinearGradientBrush { StartPoint = new Point(.5, 0), EndPoint = new Point(.5, 1) };
        foreach (var (at, color) in stops) b.GradientStops.Add(new GradientStop(color, at));
        return b;
    }

    private static Pen Line(Color c, double width) => new(new SolidColorBrush(c), width)
    {
        LineJoin = PenLineJoin.Round, StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round
    };

    private static Drawing Draw(Color color, IconPart[]? parts, string? kind)
    {
        var all = new DrawingGroup();
        using (var dc = all.Open())
        {
            dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, 48, 48)); // 絵の大きさを 48×48 にそろえる

            // 水玉：上が濃く、下に光が集まる
            var fill = new RadialGradientBrush { Center = new Point(.5, .8), GradientOrigin = new Point(.5, .92), RadiusX = .8, RadiusY = .88 };
            fill.GradientStops.Add(new GradientStop(Mix(color, Colors.White, .8), 0));
            fill.GradientStops.Add(new GradientStop(Mix(color, Colors.White, .18), .38));
            fill.GradientStops.Add(new GradientStop(color, .62));
            fill.GradientStops.Add(new GradientStop(Deep(color), 1));
            dc.DrawEllipse(fill, Line(Alpha(Deep(Deep(color)), .75), .8), Center, BallRadius, BallRadius);

            // 奥の照り返し
            dc.DrawEllipse(Vertical((0, White(.75)), (.5, White(.22)), (1, White(0))), null, new Point(24, 11.5), 16.5, 10.2);
            dc.DrawEllipse(new RadialGradientBrush(White(.7), White(0)), null, new Point(24, 42.5), 13, 4.4);

            dc.PushClip(new EllipseGeometry(Center, BallRadius, BallRadius));
            if (parts != null) DrawMachine(dc, parts);
            else DrawLineArt(dc, color, kind);
            dc.Pop();

            // 手前のつや
            dc.DrawEllipse(Vertical((0, White(.55)), (.55, White(.08)), (1, White(0))), null, new Point(24, 8.2), 13.5, 6.6);
            dc.DrawEllipse(null, Line(White(.42), .8), Center, BallRadius - 1, BallRadius - 1);
        }
        all.ClipGeometry = new RectangleGeometry(new Rect(0, 0, 48, 48));
        return all;
    }

    /// <summary>絵を水玉に収まる最大の大きさにして、中央に置く</summary>
    internal static Matrix Fit(IReadOnlyCollection<IconPart> parts)
    {
        var bounds = Rect.Empty;
        foreach (var p in parts) bounds.Union(p.Shape.Bounds);
        var mid = new Point(bounds.X + bounds.Width / 2, bounds.Y + bounds.Height / 2);

        double far = 1;
        foreach (var p in parts)
            foreach (var figure in p.Shape.GetFlattenedPathGeometry(.1, ToleranceType.Absolute).Figures)
            {
                far = Math.Max(far, (figure.StartPoint - mid).Length);
                foreach (var segment in figure.Segments)
                {
                    if (segment is PolyLineSegment poly)
                        foreach (var q in poly.Points) far = Math.Max(far, (q - mid).Length);
                    else if (segment is LineSegment line)
                        far = Math.Max(far, (line.Point - mid).Length);
                }
            }
        var k = ArtRadius / far;
        return new Matrix(k, 0, 0, k, Center.X - k * mid.X, Center.Y - k * mid.Y);
    }

    private static void DrawMachine(DrawingContext dc, IconPart[] parts)
    {
        dc.PushTransform(new MatrixTransform(Fit(parts)));

        // 水玉の中に落ちる影
        dc.PushTransform(new TranslateTransform(0, 1.2));
        var shadow = new SolidColorBrush(Alpha(Color.FromRgb(8, 24, 48), .22));
        foreach (var p in parts.Where(p => p.Body)) dc.DrawGeometry(shadow, Line(Alpha(Color.FromRgb(8, 24, 48), .12), 1.6), p.Shape);
        dc.Pop();

        foreach (var p in parts)
        {
            var c = p.Color;
            if (p.Body)
            {
                // 黒い本体は青みのある黒にして、つぶれないようにする
                if (.299 * c.R + .587 * c.G + .114 * c.B < 85) c = Mix(c, Color.FromRgb(0x52, 0x68, 0x88), .25);
                var fill = Vertical((0, Mix(c, Colors.White, .4)), (.32, c), (.66, Mix(c, Colors.Black, .14)), (.9, Mix(c, Colors.White, .1)), (1, Mix(c, Colors.White, .4)));
                dc.DrawGeometry(fill, Line(Mix(c, Colors.Black, .45), 1), p.Shape);
                dc.PushClip(p.Shape);
                dc.DrawGeometry(null, Line(White(.5), 2.4), p.Shape);
                dc.Pop();
            }
            else
            {
                var fill = Vertical((0, Mix(c, Colors.White, .22)), (.55, c), (1, Mix(c, Colors.White, .1)));
                dc.DrawGeometry(fill, Line(Alpha(Mix(c, Colors.Black, .5), .5), .35), p.Shape);
            }
        }

        // 本体のつや（部品の色が分かる程度に薄く）
        foreach (var p in parts.Where(p => p.Body))
        {
            var r = p.Shape.Bounds;
            dc.PushClip(p.Shape);
            dc.DrawEllipse(Vertical((0, White(.45)), (.65, White(.12)), (1, White(.02))), null,
                new Point(r.X + r.Width * .45, r.Y - r.Height * .04), r.Width * .78, r.Height * .4);
            dc.Pop();
        }

        // 画面の斜めの映り込み
        foreach (var p in parts.Where(p => p.Screen))
        {
            var r = p.Shape.Bounds;
            var glint = new StreamGeometry();
            using (var g = glint.Open())
            {
                g.BeginFigure(r.TopLeft, true, true);
                g.LineTo(new Point(r.X + r.Width * .7, r.Y), true, false);
                g.LineTo(new Point(r.X, r.Y + r.Height * .8), true, false);
            }
            dc.PushClip(p.Shape);
            dc.DrawGeometry(new SolidColorBrush(White(.3)), null, glint);
            dc.Pop();
        }
        dc.Pop();
    }

    private static void DrawLineArt(DrawingContext dc, Color color, string? kind)
    {
        const double k = .7;
        var g = IconFactory.Get(kind);
        dc.PushTransform(new MatrixTransform(new Matrix(k, 0, 0, k, 24 * (1 - k), 24 * (1 - k))));
        dc.DrawGeometry(null, Line(Alpha(Deep(Deep(color)), .55), 4.8), g);
        dc.DrawGeometry(null, Line(Colors.White, 3), g);
        dc.Pop();
    }
}
