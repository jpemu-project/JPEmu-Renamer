// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Text.Encodings.Web;
using System.Text.Json;
using JPEmuRenamer.Models;

namespace JPEmuRenamer.Services;

/// <summary>
/// 設定・走査キャッシュ・リネーム履歴の保存先と読み書き。
/// exe と同じフォルダの data\ に保存する（フォルダごとUSBメモリなどに入れて持ち運べる）。
/// exe のフォルダに書き込めないとき（Program Files など）だけ %APPDATA%\JPEmuRenamer を使う。
/// </summary>
public static class Storage
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>exe のあるフォルダ（末尾の区切りなし）</summary>
    public static string AppDir { get; } = System.IO.Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory);

    private static string LegacyDataDir =>
        System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JPEmuRenamer");

    public static string DataDir { get; } = ResolveDataDir();

    private static string ResolveDataDir()
    {
        var portable = System.IO.Path.Combine(AppDir, "data");
        if (CanWrite(portable))
        {
            // 1.1.0 までの %APPDATA% の設定を、初めて起動したときに一度だけ引き継ぐ（元は消さない）
            if (!File.Exists(System.IO.Path.Combine(portable, "settings.json"))
                && File.Exists(System.IO.Path.Combine(LegacyDataDir, "settings.json")))
            {
                try { CopyDir(LegacyDataDir, portable); } catch { }
            }
            return portable;
        }
        Directory.CreateDirectory(LegacyDataDir);
        return LegacyDataDir;
    }

    private static bool CanWrite(string dir)
    {
        try
        {
            Directory.CreateDirectory(dir);
            var probe = System.IO.Path.Combine(dir, ".write-test");
            File.WriteAllText(probe, "");
            File.Delete(probe);
            return true;
        }
        catch { return false; }
    }

    private static void CopyDir(string from, string to)
    {
        Directory.CreateDirectory(to);
        foreach (var f in Directory.GetFiles(from))
        {
            var dest = System.IO.Path.Combine(to, System.IO.Path.GetFileName(f));
            if (!File.Exists(dest)) File.Copy(f, dest);
        }
        foreach (var d in Directory.GetDirectories(from))
            CopyDir(d, System.IO.Path.Combine(to, System.IO.Path.GetFileName(d)));
    }

    // ---------- 持ち運び用のパス変換 ----------
    // 保存するとき：exe のフォルダの中のパスは相対パス（"roms\nes" など）にする。
    // 読み込むとき：相対パスは exe のフォルダから組み立てる。見つからない絶対パスは、
    //   ドライブ文字だけを exe のドライブに替えてあれば、そちらを使う（USBメモリのドライブ文字が変わったとき）。

    /// <summary>保存用の形にする</summary>
    public static string ToStoredPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !System.IO.Path.IsPathFullyQualified(path)) return path;
        try
        {
            var rel = System.IO.Path.GetRelativePath(AppDir, path);
            if (rel == ".") return ".";
            if (!System.IO.Path.IsPathRooted(rel) && !rel.StartsWith("..", StringComparison.Ordinal)) return rel;
        }
        catch { }
        return path;
    }

    /// <summary>保存された形から、いまの場所での絶対パスにする</summary>
    public static string ResolvePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return path;
        try
        {
            if (!System.IO.Path.IsPathRooted(path))
                return System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDir, path));
            if (System.IO.Path.IsPathFullyQualified(path) && path.Length >= 2 && path[1] == ':'
                && AppDir.Length >= 2 && AppDir[1] == ':'
                && char.ToUpperInvariant(path[0]) != char.ToUpperInvariant(AppDir[0])
                && !File.Exists(path) && !Directory.Exists(path))
            {
                var moved = AppDir[0] + path[1..];
                if (File.Exists(moved) || Directory.Exists(moved)) return moved;
            }
        }
        catch { }
        return path;
    }

    private static string? Map(string? p, Func<string, string> f) => p == null ? null : f(p);

    private static void MapPaths(PlaySettings p, Func<string, string> f)
    {
        p.SaveDir = Map(p.SaveDir, f);
        p.StateDir = Map(p.StateDir, f);
        p.ScreenshotDir = Map(p.ScreenshotDir, f);
    }

    private static void MapPaths(AppSettings s, Func<string, string> f)
    {
        s.BiosDir = f(s.BiosDir);
        s.Preview.OwnShotFolder = f(s.Preview.OwnShotFolder);
        MapPaths(s.Play, f);
        foreach (var c in s.Systems)
        {
            c.CustomIconPath = f(c.CustomIconPath);
            c.RomFolders = c.RomFolders.Select(f).ToList();
            c.DatPath = f(c.DatPath);
            c.UnknownFolder = f(c.UnknownFolder);
            c.OwnShotFolder = f(c.OwnShotFolder);
            c.EmulatorPath = f(c.EmulatorPath);
            c.EmulatorWorkDir = f(c.EmulatorWorkDir);
            MapPaths(c.Play, f);
        }
    }

    /// <summary>JSONを経由した複製に、保存用のパス変換をかけて書き出す（使用中の設定は変えない）</summary>
    private static string SerializeStored<T>(T value, Action<T, Func<string, string>> map)
    {
        var copy = JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, Json), Json)!;
        map(copy, ToStoredPath);
        return JsonSerializer.Serialize(copy, Json);
    }

    private static string SettingsPath => System.IO.Path.Combine(DataDir, "settings.json");
    private static string HistoryPath => System.IO.Path.Combine(DataDir, "history.json");
    private static string CacheDir => System.IO.Path.Combine(DataDir, "cache");
    public static string IconDir => System.IO.Path.Combine(DataDir, "icons");
    public static string CoresDir => System.IO.Path.Combine(DataDir, "cores");
    public static string DefaultBiosDir => System.IO.Path.Combine(DataDir, "system");
    public static string DefaultSaveDir => System.IO.Path.Combine(DataDir, "saves");
    public static string DefaultStateDir => System.IO.Path.Combine(DataDir, "states");
    public static string DefaultScreenshotDir => System.IO.Path.Combine(DataDir, "screenshots");
    private static string GamesDir => System.IO.Path.Combine(DataDir, "games");

    /// <summary>ゲーム単位の上書き設定（キーはROMのCRC32。リネームしても変わらない）</summary>
    public static GameOverrides LoadGame(string gameKey)
    {
        try
        {
            var p = System.IO.Path.Combine(GamesDir, gameKey + ".json");
            if (File.Exists(p))
            {
                var g = JsonSerializer.Deserialize<GameOverrides>(File.ReadAllText(p), Json) ?? new();
                MapPaths(g.Play, ResolvePath);
                return g;
            }
        }
        catch { }
        return new();
    }

    public static void SaveGame(string gameKey, GameOverrides game)
    {
        var p = System.IO.Path.Combine(GamesDir, gameKey + ".json");
        if (game.Play.IsEmpty)
        {
            try { File.Delete(p); } catch { }
            return;
        }
        Directory.CreateDirectory(GamesDir);
        WriteAtomic(p, SerializeStored(game, (g, f) => MapPaths(g.Play, f)));
    }

    /// <summary>コアが宣言した設定項目の控え（一度起動したコアの設定を、起動していなくても編集できるように）</summary>
    public static List<Emulation.CoreOption> LoadCoreOptions(string coreName)
    {
        try
        {
            var p = System.IO.Path.Combine(CoresDir, coreName + ".options.json");
            if (File.Exists(p))
                return JsonSerializer.Deserialize<List<Emulation.CoreOption>>(File.ReadAllText(p), Json) ?? new();
        }
        catch { }
        return new();
    }

    public static void SaveCoreOptions(string coreName, IEnumerable<Emulation.CoreOption> options)
    {
        try
        {
            Directory.CreateDirectory(CoresDir);
            WriteAtomic(System.IO.Path.Combine(CoresDir, coreName + ".options.json"), JsonSerializer.Serialize(options.ToList(), Json));
        }
        catch { }
    }

    public static AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var s = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsPath), Json);
                if (s != null)
                {
                    MapPaths(s, ResolvePath);
                    return s;
                }
            }
        }
        catch
        {
            // 壊れた設定はバックアップして初期化
            try { File.Copy(SettingsPath, SettingsPath + ".broken", true); } catch { }
        }
        // 機種は最初は空。利用者が好きな名前で作る
        return new AppSettings();
    }

    public static void SaveSettings(AppSettings settings) => WriteAtomic(SettingsPath, SerializeStored(settings, MapPaths));

    public static Dictionary<string, RomFile> LoadCache(string systemId)
    {
        try
        {
            var p = System.IO.Path.Combine(CacheDir, systemId + ".json");
            if (File.Exists(p))
            {
                var list = JsonSerializer.Deserialize<List<RomFile>>(File.ReadAllText(p), Json) ?? new();
                return list.GroupBy(r => r.Path, StringComparer.OrdinalIgnoreCase)
                           .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            }
        }
        catch { }
        return new Dictionary<string, RomFile>(StringComparer.OrdinalIgnoreCase);
    }

    public static void SaveCache(string systemId, IEnumerable<RomFile> roms)
    {
        Directory.CreateDirectory(CacheDir);
        WriteAtomic(System.IO.Path.Combine(CacheDir, systemId + ".json"), JsonSerializer.Serialize(roms.ToList(), Json));
    }

    public static void DeleteCache(string systemId)
    {
        try { File.Delete(System.IO.Path.Combine(CacheDir, systemId + ".json")); } catch { }
    }

    public static List<RenameRun> LoadHistory()
    {
        try
        {
            if (File.Exists(HistoryPath))
                return JsonSerializer.Deserialize<List<RenameRun>>(File.ReadAllText(HistoryPath), Json) ?? new();
        }
        catch { }
        return new();
    }

    public static void SaveHistory(List<RenameRun> runs)
    {
        // 直近50回分だけ残す
        var keep = runs.Count > 50 ? runs.Skip(runs.Count - 50).ToList() : runs;
        WriteAtomic(HistoryPath, JsonSerializer.Serialize(keep, Json));
    }

    private static void WriteAtomic(string path, string content)
    {
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, content);
        File.Move(tmp, path, true);
    }
}
