// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Media;

namespace JPEmuRenamer.Ui;

/// <summary>
/// オリジナルの機種アイコン（線画9種）。48×48 の座標で描き、表示側で Viewbox を使って拡大縮小する。
/// </summary>
public static class IconFactory
{
    private static readonly Dictionary<string, Geometry> Cache = new();

    public static Geometry Get(string? kind)
    {
        kind ??= "pad";
        if (Cache.TryGetValue(kind, out var g)) return g;
        g = Build(kind);
        g.Freeze();
        Cache[kind] = g;
        return g;
    }

    private static RectangleGeometry R(double x, double y, double w, double h, double r) => new(new Rect(x, y, w, h), r, r);
    private static EllipseGeometry C(double cx, double cy, double r) => new(new Point(cx, cy), r, r);
    private static Geometry P(string d) => Geometry.Parse(d);

    private static Geometry Build(string kind)
    {
        var g = new GeometryGroup { FillRule = FillRule.Nonzero };
        void Add(params Geometry[] parts) { foreach (var p in parts) g.Children.Add(p); }

        switch (kind)
        {
            case "cart": // 据置カートリッジ機
                Add(R(15, 6, 18, 14, 2), P("M19,11 L29,11"), R(5, 20, 38, 20, 5),
                    C(13, 30, 1.2), C(20, 30, 1.2), P("M28,28 L37,28 M28,32 L37,32"));
                break;
            case "diskcard": // ディスクカード機
                Add(P("M13,5 L31,5 L36,10 L36,41 Q36,43 34,43 L13,43 Q11,43 11,41 L11,7 Q11,5 13,5 Z"),
                    R(18, 5, 11, 11, 1), P("M17,25 L31,25 M17,30 L31,30 M17,35 L26,35"));
                break;
            case "disc": // ディスク機
                Add(C(24, 13, 9), C(24, 13, 2.5), R(5, 26, 38, 16, 5), P("M11,34 L23,34"), C(35, 34, 1.2));
                break;
            case "handV": // 縦型携帯機
                Add(R(12, 4, 24, 40, 5), R(16, 8, 16, 13, 1.5), P("M17,31 L24,31 M20.5,27.5 L20.5,34.5"),
                    C(29, 32, 1), C(32.5, 28.5, 1), P("M20,39 L23,39 M26,39 L29,39"));
                break;
            case "handH": // 横型携帯機
                Add(R(3, 13, 42, 22, 9), R(15, 17, 18, 14, 1.5), P("M6.5,24 L12.5,24 M9.5,21 L9.5,27"),
                    C(38, 22, 1), C(41, 26, 1));
                break;
            case "dual": // 2画面携帯機
                Add(R(9, 4, 30, 18, 3), R(15, 8, 18, 10, 1), P("M14,24 L34,24"), R(9, 26, 30, 18, 3),
                    R(17, 29, 14, 10, 1), P("M11.5,34 L15,34"), C(35, 34, 0.8));
                break;
            case "arcade": // アーケード
                Add(P("M13,44 L13,11 L17,4 L31,4 L35,11 L35,44 Z"), R(17, 12, 14, 11, 1), P("M13,28 L35,28 M19,32 L19,29.5"),
                    C(19, 28.5, 1), C(26, 31, 0.8), C(30, 31, 0.8), P("M19,39 L29,39"));
                break;
            case "pc": // パソコン
                Add(R(3, 14, 42, 22, 3),
                    P("M8,20 L11,20 M14,20 L17,20 M20,20 L23,20 M26,20 L29,20 M32,20 L35,20 M38,20 L40,20"),
                    P("M10,25 L13,25 M16,25 L19,25 M22,25 L25,25 M28,25 L31,25 M34,25 L37,25"),
                    P("M14,30 L34,30"));
                break;
            default: // その他（ゲームパッド）
                Add(P("M11,15 L37,15 A7,7 0 0 1 44,22 L44,29 A6,6 0 0 1 33.5,33 L30,29 L18,29 L14.5,33 A6,6 0 0 1 4,29 L4,22 A7,7 0 0 1 11,15 Z"),
                    P("M11,23 L18,23 M14.5,19.5 L14.5,26.5"), C(31, 24, 1), C(35, 20.5, 1));
                break;
        }
        return g;
    }
}
