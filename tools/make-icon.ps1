# JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
# アプリのアイコン（app.ico）を作る。白い角丸の台に、携帯ゲーム機（画面・十字キー・赤いボタン2つ）。
# exe・タイトルバー・タスクバーのすべてでこのアイコンを使う。
# 使い方: powershell -File tools/make-icon.ps1 [-Out <app.ico>] [-Preview <プレビューPNG>]
param(
    [string] $Out = (Join-Path $PSScriptRoot '..\src\JPEmuRenamer\Assets\app.ico'),
    [string] $Preview = ''
)
$ErrorActionPreference = 'Stop'
Add-Type -ReferencedAssemblies System.Drawing -TypeDefinition @"
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

public static class JpemuIcon
{
    static GraphicsPath Round(float x, float y, float w, float h, float r)
    {
        var p = new GraphicsPath();
        p.AddArc(x, y, 2 * r, 2 * r, 180, 90);
        p.AddArc(x + w - 2 * r, y, 2 * r, 2 * r, 270, 90);
        p.AddArc(x + w - 2 * r, y + h - 2 * r, 2 * r, 2 * r, 0, 90);
        p.AddArc(x, y + h - 2 * r, 2 * r, 2 * r, 90, 90);
        p.CloseFigure();
        return p;
    }

    // 100x100 の座標で描く。小さいサイズでは線を太めにして、つぶれないようにする
    public static Bitmap Render(int px)
    {
        int s = 8, W = px * s;
        var big = new Bitmap(W, W, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(big))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.ScaleTransform(W / 100f, W / 100f);
            float edge = px <= 16 ? 5f : px <= 32 ? 3.2f : 1.6f;
            var ink = Color.FromArgb(26, 28, 34);
            var red = Color.FromArgb(229, 72, 77);
            using (var tile = Round(1 + edge / 2, 1 + edge / 2, 98 - edge, 98 - edge, 22))
            {
                g.FillPath(Brushes.White, tile);
                using (var pen = new Pen(Color.FromArgb(px <= 32 ? 150 : 205, px <= 32 ? 154 : 209, px <= 32 ? 162 : 216), edge)) g.DrawPath(pen, tile);
            }
            // 画面
            float sw = px <= 16 ? 5f : px <= 32 ? 3.5f : 2f;
            using (var scr = Round(18, 14, 64, 50, 9))
            {
                using (var b = new SolidBrush(Color.FromArgb(236, 238, 242))) g.FillPath(b, scr);
                using (var pen = new Pen(Color.FromArgb(px <= 32 ? 120 : 190, px <= 32 ? 124 : 194, px <= 32 ? 134 : 202), sw)) g.DrawPath(pen, scr);
            }
            // 十字キー
            using (var b = new SolidBrush(ink))
            {
                float t = px <= 16 ? 8f : 6f, len = px <= 16 ? 22f : 19f, cx = 30, cy = 78;
                using (var h = Round(cx - len / 2, cy - t / 2, len, t, 2)) g.FillPath(b, h);
                using (var v = Round(cx - t / 2, cy - len / 2, t, len, 2)) g.FillPath(b, v);
            }
            // 赤いボタン2つ
            using (var b = new SolidBrush(red))
            {
                float r = px <= 16 ? 7f : 5.5f;
                g.FillEllipse(b, 66 - r, 82 - r, 2 * r, 2 * r);
                g.FillEllipse(b, 78 - r, 72 - r, 2 * r, 2 * r);
            }
        }
        var bmp = new Bitmap(px, px, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(big, new Rectangle(0, 0, px, px));
        }
        big.Dispose();
        return bmp;
    }

    // PNG を入れた ICO を書く（Windows Vista 以降で使える形式）
    public static void WriteIco(string path, int[] sizes)
    {
        var pngs = new byte[sizes.Length][];
        for (int i = 0; i < sizes.Length; i++)
            using (var bmp = Render(sizes[i])) using (var ms = new MemoryStream()) { bmp.Save(ms, ImageFormat.Png); pngs[i] = ms.ToArray(); }
        using (var w = new BinaryWriter(File.Create(path)))
        {
            w.Write((short)0); w.Write((short)1); w.Write((short)sizes.Length);
            int offset = 6 + 16 * sizes.Length;
            for (int i = 0; i < sizes.Length; i++)
            {
                int sz = sizes[i];
                w.Write((byte)(sz >= 256 ? 0 : sz)); w.Write((byte)(sz >= 256 ? 0 : sz));
                w.Write((byte)0); w.Write((byte)0); w.Write((short)1); w.Write((short)32);
                w.Write(pngs[i].Length); w.Write(offset);
                offset += pngs[i].Length;
            }
            foreach (var p in pngs) w.Write(p);
        }
    }

    public static void WritePreview(string path, int[] sizes)
    {
        int width = 0; foreach (var s in sizes) width += s + 16;
        using (var bmp = new Bitmap(width + 16, 256 + 32))
        using (var g = Graphics.FromImage(bmp))
        {
            g.Clear(Color.FromArgb(243, 244, 246));
            int x = 16;
            foreach (var s in sizes) { using (var b = Render(s)) g.DrawImage(b, x, 16 + 256 - s); x += s + 16; }
            bmp.Save(path, ImageFormat.Png);
        }
    }
}
"@
$sizes = 16, 20, 24, 32, 40, 48, 64, 96, 128, 256
$full = [IO.Path]::GetFullPath($Out)
[JpemuIcon]::WriteIco($full, [int[]]$sizes)
if ($Preview) { [JpemuIcon]::WritePreview([IO.Path]::GetFullPath($Preview), [int[]](256, 128, 64, 48, 32, 24, 16)) }
"icon -> $full"
