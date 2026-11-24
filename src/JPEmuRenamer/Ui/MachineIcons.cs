// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows;
using System.Windows.Media;

namespace JPEmuRenamer.Ui;

/// <summary>絵の部品。Body は本体（光沢を付ける）、Screen は画面（映り込みを付ける）</summary>
public sealed record IconPart(Geometry Shape, Color Color, bool Body = false, bool Screen = false);

/// <summary>
/// 機種ごとの実機の絵（48×48 の座標。すべて描き起こしで、メーカーのロゴは入れない）。
/// キーは候補（SystemPresets）の日本語名。無い機種は IconFactory の線画で表示する。
/// </summary>
public static partial class MachineIcons
{
    private static readonly Lazy<Dictionary<string, IconPart[]>> All = new(Build);

    public static IReadOnlyCollection<string> Names => All.Value.Keys;

    public static IconPart[]? Get(string? japaneseName) =>
        japaneseName != null && All.Value.TryGetValue(japaneseName, out var parts) ? parts : null;

    private static Color Hex(string c) => (Color)ColorConverter.ConvertFromString(c);

    private static Geometry Freeze(Geometry g)
    {
        g.Freeze();
        return g;
    }

    private static Geometry Rect(double x, double y, double w, double h, double r) => Freeze(new RectangleGeometry(new Rect(x, y, w, h), r, r));

    private static IconPart B(double x, double y, double w, double h, double r, string c) => new(Rect(x, y, w, h, r), Hex(c), Body: true);
    private static IconPart Bp(string d, string c) => new(Freeze(Geometry.Parse(d)), Hex(c), Body: true);
    private static IconPart R(double x, double y, double w, double h, double r, string c) => new(Rect(x, y, w, h, r), Hex(c));
    private static IconPart S(double x, double y, double w, double h, double r, string c) => new(Rect(x, y, w, h, r), Hex(c), Screen: true);
    private static IconPart C(double x, double y, double r, string c) => new(Freeze(new EllipseGeometry(new Point(x, y), r, r)), Hex(c));
    private static IconPart P(string d, string c) => new(Freeze(Geometry.Parse(d)), Hex(c));

    /// <summary>十字キー</summary>
    private static IconPart X(double x, double y, double len, double t, string c) => new(
        Freeze(Geometry.Combine(Rect(x - len / 2, y - t / 2, len, t, .4), Rect(x - t / 2, y - len / 2, t, len, .4), GeometryCombineMode.Union, null)),
        Hex(c));

    /// <summary>等間隔に並んだ四角（キー・スリット）を1つの部品にする</summary>
    private static IconPart G(double x, double y, int cols, int rows, double w, double h, double gap, string c)
    {
        var g = new StreamGeometry();
        using (var d = g.Open())
            for (int r = 0; r < rows; r++)
                for (int k = 0; k < cols; k++)
                {
                    double left = x + k * (w + gap), top = y + r * (h + gap);
                    d.BeginFigure(new Point(left, top), true, true);
                    d.PolyLineTo(new[] { new Point(left + w, top), new Point(left + w, top + h), new Point(left, top + h) }, true, false);
                }
        return new IconPart(Freeze(g), Hex(c));
    }

    private static Dictionary<string, IconPart[]> Build() =>
        Classics().Concat(Consoles()).Concat(Handhelds()).Concat(Computers()).Concat(Arcade()).ToDictionary(p => p.Key, p => p.Value);

    /// <summary>主要な21機種</summary>
    private static Dictionary<string, IconPart[]> Classics() => new()
    {
        ["ファミコン"] = new[]
        {
            B(5, 7, 38, 34, 2.5, "#F3EEE4"),
            // 左右に収納したコントローラー
            R(5.8, 9.5, 5.6, 26, 1.2, "#A8232F"), R(36.6, 9.5, 5.6, 26, 1.2, "#A8232F"),
            R(6.8, 12, 3.6, 21, .7, "#D8B56A"), R(37.6, 12, 3.6, 21, .7, "#D8B56A"),
            X(8.6, 16.5, 3, 1, "#25252B"), C(8.6, 26.5, 1, "#A8232F"), C(8.6, 29.5, 1, "#A8232F"),
            X(39.4, 16.5, 3, 1, "#25252B"), C(39.4, 26.5, 1, "#A8232F"), C(39.4, 29.5, 1, "#A8232F"),
            R(13.5, 10, 21, 14, 1.2, "#A8232F"), R(16.5, 13.5, 15, 3.4, .9, "#3D0F14"),
            R(13.5, 28, 6, 5.5, 1, "#A8232F"), R(28.5, 28, 6, 5.5, 1, "#A8232F"), R(21.8, 28.5, 4.4, 8, 1, "#B9B0A3"),
        },
        ["ディスクシステム"] = new[]
        {
            B(5, 6, 38, 36, 3.5, "#B3222F"),
            R(8.5, 9.5, 31, 22, 1.5, "#2A2528"), R(11, 12, 26, 3.6, 1, "#0E0C0E"),
            // 黄色のディスクカード
            R(12.5, 17.5, 23, 12, 1, "#F5C21B"), R(18.5, 17.5, 11, 5.5, .6, "#4B4036"), R(25.5, 18.5, 2.4, 3.5, .4, "#B9B2A6"),
            R(15.5, 25, 17, 2.8, .5, "#FFF1B0"),
            R(8.5, 34, 31, 4.8, 1, "#161215"), R(33, 35.2, 4, 2.4, .5, "#F5C21B"), C(12, 36.4, 1.2, "#E33A3A"),
        },
        ["バーチャルボーイ"] = new[]
        {
            P("M22.2,27 L25.8,27 L25.8,32 L36,42 L32,42 L24,35 L16,42 L12,42 L22.2,32 Z", "#3B3F48"),
            Bp("M9,7 L39,7 Q45,7 45,13 L44,24 Q43,28 38,28 L10,28 Q5,28 4,24 L3,13 Q3,7 9,7 Z", "#D3212C"),
            R(7.5, 12, 33, 12.5, 5, "#1B1A1F"),
            S(11.5, 15, 10, 6.5, 2.8, "#E33A3A"), S(26.5, 15, 10, 6.5, 2.8, "#E33A3A"),
            R(21, 8, 6, 2, .8, "#8F141D"),
        },
        ["スーパーファミコン"] = new[]
        {
            B(5, 7, 38, 34, 6.5, "#CFCFD4"),
            R(10, 10, 28, 15.5, 3.5, "#9D9DA6"), R(13.5, 14, 21, 4.4, 1.6, "#3C3C46"), R(20.5, 20.5, 7, 2.6, .8, "#74747D"),
            R(10.5, 27.5, 6.5, 5.5, 1.2, "#66666F"), R(31, 27.5, 6.5, 5.5, 1.2, "#66666F"),
            C(19.7, 35.8, 1.9, "#2D5BD7"), C(24, 32.6, 1.9, "#2AA65A"), C(24, 38.6, 1.9, "#E3B92B"), C(28.3, 35.8, 1.9, "#D8343C"),
        },
        ["NINTENDO64"] = new[]
        {
            Bp("M8,10 Q24,5 40,10 Q45,21 45,33 Q45,38.5 39,38.5 Q24,34.5 9,38.5 Q3,38.5 3,33 Q3,21 8,10 Z", "#3C3D43"),
            R(11.5, 10.5, 25, 11, 4.5, "#5B5D66"), R(14.5, 14, 19, 3.8, 1.3, "#121316"),
            C(9.5, 30.5, 2.5, "#AEB0B7"), C(16.3, 29.3, 2.5, "#AEB0B7"), C(31.7, 29.3, 2.5, "#AEB0B7"), C(38.5, 30.5, 2.5, "#AEB0B7"),
            R(20.8, 26.3, 3.2, 3.2, .6, "#2AA65A"), R(24, 26.3, 3.2, 3.2, .6, "#E3B92B"),
            R(20.8, 29.5, 3.2, 3.2, .6, "#2D5BD7"), R(24, 29.5, 3.2, 3.2, .6, "#D8343C"),
        },
        ["64DD"] = new[]
        {
            B(3, 23, 42, 19, 5, "#303136"),
            Bp("M9,6 Q24,3 39,6 Q43,14 43,22 Q43,25 39,25 Q24,22 9,25 Q5,25 5,22 Q5,14 9,6 Z", "#43444B"),
            R(14.5, 9.5, 19, 3.8, 1.2, "#121316"), C(11.5, 19, 2, "#AEB0B7"), C(36.5, 19, 2, "#AEB0B7"),
            R(21.2, 15.6, 2.8, 2.8, .5, "#2AA65A"), R(24, 15.6, 2.8, 2.8, .5, "#E3B92B"),
            R(21.2, 18.4, 2.8, 2.8, .5, "#2D5BD7"), R(24, 18.4, 2.8, 2.8, .5, "#D8343C"),
            // ディスクの差し込み口
            R(8.5, 29, 31, 5.5, 1.2, "#0C0D10"), R(12, 28.6, 19, 3, .5, "#8D9099"), C(38, 37.8, 1.6, "#F0A028"),
        },
        ["ゲームボーイ"] = new[]
        {
            Bp("M12,2 L36,2 Q38,2 38,4 L38,39 Q38,46 31,46 L12,46 Q10,46 10,44 L10,4 Q10,2 12,2 Z", "#CBC9C1"),
            R(12.8, 5.8, 22.4, 19.4, 2, "#62626E"), S(16.4, 8.5, 15.2, 14, .5, "#9BBC3B"), C(14.4, 14, .7, "#E33A3A"),
            X(17.5, 32, 8.8, 3.1, "#25252B"), C(28.2, 34.2, 2.6, "#A8285E"), C(34, 31, 2.6, "#A8285E"),
            R(18, 39.6, 4.6, 1.7, .8, "#77767E"), R(24.2, 39.6, 4.6, 1.7, .8, "#77767E"),
            P("M30,42.8 L31,42.8 L34.2,38.8 L33.2,38.8 Z M32.8,42.8 L33.8,42.8 L36.4,39.5 L35.4,39.5 Z", "#8F8D86"),
        },
        ["ゲームボーイカラー"] = new[]
        {
            Bp("M13,2 L35,2 Q38,2 38,5 L38,39 Q38,46 24,46 Q10,46 10,39 L10,5 Q10,2 13,2 Z", "#7651B5"),
            P("M13,5 L35,5 L35,24 Q24,28.8 13,24 Z", "#25222E"), S(16.3, 7.3, 15.4, 14, .5, "#A9D5DA"),
            C(19.5, 23.6, 1, "#E33A3A"), C(22.5, 24, 1, "#E3B92B"), C(25.5, 24, 1, "#2AA65A"), C(28.5, 23.6, 1, "#2D8FE0"),
            X(17.5, 33, 8.4, 2.9, "#25222E"), C(28.4, 35.2, 2.4, "#25222E"), C(33.6, 32.2, 2.4, "#25222E"),
            R(19, 40, 4, 1.5, .7, "#3B2862"), R(25, 40, 4, 1.5, .7, "#3B2862"),
        },
        ["ゲームボーイアドバンス"] = new[]
        {
            Bp("M10,11 Q24,8 38,11 Q47,14 47,24 Q47,36 39,38 Q24,41 9,38 Q1,36 1,24 Q1,14 10,11 Z", "#5A50B8"),
            R(12.8, 13.6, 22.4, 20.4, 2.6, "#22222B"), S(15.8, 16.2, 16.4, 13.2, .5, "#9CCBD4"),
            X(7.2, 22, 7.8, 2.8, "#E2E2EA"), C(38.8, 25, 2.2, "#E2E2EA"), C(43, 21, 2.2, "#E2E2EA"),
            C(9.8, 30, 1, "#B9B6D8"), C(9.8, 33, 1, "#B9B6D8"),
        },
        ["ニンテンドーDS"] = new[]
        {
            R(12, 21, 24, 6, 1, "#1C2648"),
            B(7, 2, 34, 21, 3, "#2E57C8"), B(7, 25, 34, 21, 3, "#2E57C8"),
            R(14.8, 5.6, 18.4, 14, .7, "#20242E"), S(16.3, 7, 15.4, 11.2, .3, "#9DC6E8"),
            R(14.8, 28.4, 18.4, 14, .7, "#20242E"), S(16.3, 29.8, 15.4, 11.2, .3, "#9DC6E8"),
            X(11, 34, 5.6, 2, "#D6DAE4"), C(37, 31.5, 1.4, "#D6DAE4"), C(37, 36.5, 1.4, "#D6DAE4"),
            C(10.8, 10, .8, "#1C2648"), C(10.8, 13, .8, "#1C2648"), C(37.2, 10, .8, "#1C2648"), C(37.2, 13, .8, "#1C2648"),
        },
        ["SG-1000"] = new[]
        {
            B(2, 10, 44, 28, 2.5, "#EEEDE8"),
            R(10.5, 13, 27, 9.5, 1, "#A9ACB2"), R(13.5, 16, 21, 3.4, .7, "#26282E"),
            R(2.5, 25, 43, 5, 0, "#1E57B5"), R(2.5, 30.2, 43, 1.8, 0, "#D8343C"),
            R(37, 33.4, 5.4, 2.8, .5, "#D8343C"), R(6, 33.4, 12, 2.4, .5, "#A9ACB2"),
        },
        ["マスターシステム"] = new[]
        {
            B(2, 9, 44, 30, 2.5, "#232428"),
            P("M24,9.5 L45.5,9.5 L45.5,25.5 L30.5,25.5 Z", "#D3212C"), R(6, 14, 18, 4.4, .8, "#08090A"),
            R(34, 14, 8, 2.4, .5, "#6D1219"), R(34, 18.5, 8, 1.4, .5, "#FF7A80"),
            R(2.5, 28, 43, 1.6, 0, "#5A5C63"), R(6.5, 31.8, 9, 2.8, .5, "#D3212C"), C(39, 33, 1.8, "#D3212C"),
        },
        ["スーパー32X"] = new[]
        {
            B(2, 16, 44, 25, 4, "#26272B"),
            Bp("M14,5 L34,5 Q41,5 41,12 L41,20 Q41,28 32,28 L16,28 Q7,28 7,20 L7,12 Q7,5 14,5 Z", "#3A3B41"),
            R(13.5, 10.5, 21, 4.4, 1.5, "#08090A"),
            P("M16.5,20 L31.5,20 L29.3,24.8 L18.7,24.8 Z", "#E3B92B"), P("M18.6,21.2 L23,21.2 L21.5,23.6 L19.8,23.6 Z", "#D3212C"),
            R(8, 33, 9, 3.2, 1, "#6A6C73"), C(38.5, 34.6, 1.8, "#D3212C"),
        },
        ["メガドライブ"] = new[]
        {
            B(2, 7, 44, 34, 3, "#222328"),
            C(27, 20.5, 12.4, "#5A5D66"), C(27, 20.5, 10.8, "#141519"), R(17.5, 18.3, 19, 4.4, 1, "#70737C"),
            R(5.2, 12, 4.4, 18, 1, "#4C4E56"), R(5.9, 22, 3, 5, .6, "#C4C7CF"),
            R(19.5, 35, 14, 3, .5, "#D8B04A"), C(40.3, 36.2, 1.7, "#E33A3A"),
        },
        ["ゲームギア"] = new[]
        {
            Bp("M8,10 Q24,7 40,10 Q47,12 47,24 Q47,37 40,39 Q24,42 8,39 Q1,37 1,24 Q1,12 8,10 Z", "#24252A"),
            R(12.6, 12.6, 22.8, 21.8, 2.2, "#50525A"), S(15.6, 15.4, 16.8, 13.6, .5, "#7FCBC8"),
            C(7.4, 23.5, 4.4, "#0D0E10"), X(7.4, 23.5, 6.8, 2, "#8A8D96"),
            C(38.8, 26.8, 2.2, "#B6B8C0"), C(43, 22.8, 2.2, "#B6B8C0"), R(38.5, 14.5, 4, 1.6, .8, "#8A8D96"),
            R(20.6, 31, 2.2, 1.8, .3, "#D3212C"), R(23, 31, 2.2, 1.8, .3, "#2D8FE0"), R(25.4, 31, 2.2, 1.8, .3, "#2AA65A"),
        },
        ["PCエンジン"] = new[]
        {
            B(6, 6, 36, 36, 2.5, "#F1F0EC"),
            R(10, 9.5, 28, 15, 1, "#CFCEC8"), P("M18,14 L31,14 L29.3,18.4 L16.3,18.4 Z", "#E8622C"),
            // HuCARD の差し込み口
            R(11.5, 27.5, 25, 3.2, .7, "#26282E"),
            R(9.5, 33.4, 8, 4.6, .8, "#2AA65A"), R(30.5, 34.2, 7, 3.2, .8, "#9EA0A6"),
        },
        ["PCエンジン スーパーグラフィックス"] = new[]
        {
            Bp("M14,5 L34,5 L34,10 L45,10 L45,38 L34,38 L34,43 L14,43 L14,38 L3,38 L3,10 L14,10 Z", "#50545B"),
            R(5.8, 13.5, 7.6, 21, 1, "#30333A"), R(34.6, 13.5, 7.6, 21, 1, "#30333A"),
            R(7, 16, 5.2, 1.6, .4, "#787C84"), R(7, 19.5, 5.2, 1.6, .4, "#787C84"),
            R(35.8, 16, 5.2, 1.6, .4, "#787C84"), R(35.8, 19.5, 5.2, 1.6, .4, "#787C84"),
            R(19.4, 10.5, 9.2, 20.5, 1.6, "#22B8C4"), R(21.5, 13, 5, 15.5, 1, "#86E6EC"),
            R(16.5, 34.6, 15, 3, .5, "#101215"), C(9.6, 30.5, 1.5, "#22B8C4"),
        },
        ["ワンダースワン"] = Swan("#4E5158", "#E8E9EC", "#8C9A90"),
        ["ワンダースワンカラー"] = Swan("#1F8EE0", "#EAF4FB", "#4B4470"),
        ["ネオジオポケット"] = Pocket("#33383B", "#15171A", "#A9B59E", "#8A9095", null),
        ["ネオジオポケットカラー"] = Pocket("#86A9D6", "#1D2129", "#A8D8E8", "#1D2129", "#E3B92B"),
    };

    private static IconPart[] Swan(string body, string trim, string screen) => new[]
    {
        Bp("M9,10 L41,10 Q46,10 46,17 L45,33 Q45,39 38,39 L8,39 Q2,39 2,32 L3,17 Q3,10 9,10 Z", body),
        R(15.6, 13, 21.8, 19, 2.6, trim), S(18.6, 15.7, 15.8, 13, .5, screen),
        C(9, 15, 1.6, trim), C(6.3, 17.8, 1.6, trim), C(11.7, 17.8, 1.6, trim), C(9, 20.6, 1.6, trim),
        C(9, 27, 1.6, trim), C(6.3, 29.8, 1.6, trim), C(11.7, 29.8, 1.6, trim), C(9, 32.6, 1.6, trim),
        C(41.2, 24.5, 2.1, trim), C(40.2, 30.5, 2.1, trim), R(22, 34.3, 4.4, 1.9, .9, "#E33A3A"),
    };

    private static IconPart[] Pocket(string body, string bezel, string screen, string button, string? stripe)
    {
        var parts = new List<IconPart>
        {
            B(2, 10, 44, 28, 6.5, body),
            R(14.6, 12.6, 19.8, 22.6, 2, bezel), S(17.2, 15.3, 14.6, 14.4, .5, screen),
            C(8.4, 20, 4.3, bezel), C(8.4, 20, 2, "#6A7078"),
            C(38, 22.6, 2.2, button), C(42.4, 19, 2.2, button), R(37.5, 30, 4.5, 1.6, .8, bezel),
        };
        if (stripe != null) parts.Add(R(20, 31.8, 9, 1.6, .6, stripe));
        return parts.ToArray();
    }
}
