// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JPEmuRenamer.Models;

namespace JPEmuRenamer.Services;

/// <summary>
/// タイトルごとのスナップショット（自動プレビューの代わりに表示する画像）。
/// 保存先は data\snapshots\{機種}\{DAT上の名前}.png
/// </summary>
public static class Snapshots
{
    public static string Dir => System.IO.Path.Combine(Storage.DataDir, "snapshots");

    /// <summary>このタイトルのスナップショットのパス（DATに無いファイルはファイル名から）</summary>
    public static string? PathFor(SystemConfig sys, GameItem item)
    {
        string? name = item.Dat?.Title ?? (item.Rom != null ? System.IO.Path.GetFileNameWithoutExtension(item.Rom.Path) : null);
        if (string.IsNullOrWhiteSpace(name)) return null;
        return System.IO.Path.Combine(Dir, Matcher.SafeFileName(sys.Name), Matcher.SafeFileName(name) + ".png");
    }

    public static bool Exists(SystemConfig sys, GameItem item) => PathFor(sys, item) is string p && File.Exists(p);

    private static readonly string[] OwnShotExtensions = { ".jpg", ".jpeg", ".png" };

    /// <summary>
    /// 自分で撮ったスクリーンショットを探す。ROMのファイル名（拡張子なし）か、DATのタイトルと同じ名前の画像を、
    /// フォルダ直下と「機種名」フォルダの中から探す。見つからなければ null
    /// </summary>
    public static string? OwnShotFor(string folder, SystemConfig sys, GameItem item)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder)) return null;
        var names = new List<string>();
        if (item.Rom != null) names.Add(System.IO.Path.GetFileNameWithoutExtension(item.Rom.Path));
        if (item.Dat?.Title is string title) names.Add(Matcher.SafeFileName(title));
        var dirs = new[] { System.IO.Path.Combine(folder, Matcher.SafeFileName(sys.Name)), folder };
        foreach (var dir in dirs)
            foreach (var name in names.Distinct())
                foreach (var ext in OwnShotExtensions)
                {
                    var p = System.IO.Path.Combine(dir, name + ext);
                    if (File.Exists(p)) return p;
                }
        return null;
    }

    /// <summary>
    /// 画像を読み込む。ファイルをつかんだままにせず、ストリームから読むので同じパスの古い画像も使い回されない
    /// （ストリームから読むときに IgnoreImageCache を付けると例外になるので付けない）
    /// </summary>
    public static ImageSource? Load(string? path)
    {
        if (path == null || !File.Exists(path)) return null;
        try
        {
            using var fs = File.OpenRead(path);
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.StreamSource = fs;
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>BGRA32 の画素を PNG で保存する</summary>
    public static void Save(string path, int[] pixels, int width, int height)
    {
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path)!);
        var bmp = BitmapSource.Create(width, height, 96, 96, PixelFormats.Bgr32, null, pixels, width * 4);
        var enc = new PngBitmapEncoder();
        enc.Frames.Add(BitmapFrame.Create(bmp));
        var tmp = path + ".tmp";
        using (var fs = File.Create(tmp)) enc.Save(fs);
        File.Move(tmp, path, true);
    }
}
