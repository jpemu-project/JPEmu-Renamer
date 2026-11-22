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
    //   読み替えは推測なので、利用者が変えないかぎり、保存するときは元の書き方のまま残す（外付けを外していただけのことがある）。
    //   exe が Windows のドライブにあるときは、USBメモリではないので読み替えない。

    /// <summary>exe のあるドライブに読み替えてよいか</summary>
    private static readonly bool CanRemapDrive = AppDir.Length >= 2 && AppDir[1] == ':'
        && char.ToUpperInvariant(AppDir[0]) != char.ToUpperInvariant(Environment.SystemDirectory[0]);

    /// <summary>保存用の形にする</summary>
    public static string ToStoredPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !System.IO.Path.IsPathFullyQualified(path)) return path;
        try
        {
            var rel = System.IO.Path.GetRelativePath(AppDir, path);
            if (rel == ".") return ".";
            if (!System.IO.Path.IsPathRooted(rel) && rel != ".." && !rel.StartsWith(@"..\", StringComparison.Ordinal)) return rel;
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
            var full = FromInput(path);
            if (OnAppDrive(full) is string moved && !File.Exists(full) && !Directory.Exists(full)
                && (File.Exists(moved) || Directory.Exists(moved)))
                return moved;
            return full;
        }
        catch { }
        return path;
    }

    /// <summary>入力されたパスを絶対パスにする（相対パスは exe のフォルダから。ドライブの読み替えはしない）</summary>
    public static string FromInput(string path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && !System.IO.Path.IsPathRooted(path))
                return System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDir, path));
        }
        catch { }
        return path;
    }

    /// <summary>ドライブ文字を exe のドライブに替えたパス（替える必要が無い・替えてはいけないなら null）</summary>
    private static string? OnAppDrive(string path) =>
        CanRemapDrive && path.Length >= 3 && path[1] == ':' && (path[2] == '\\' || path[2] == '/')
        && char.ToUpperInvariant(path[0]) != char.ToUpperInvariant(AppDir[0])
            ? AppDir[0] + path[1..] : null;

    /// <summary>フォルダごと持ち運んでも、そのまま使えるパスか（exe のフォルダの中にある）</summary>
    public static bool IsPortable(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        if (System.IO.Path.IsPathRooted(ToStoredPath(DataDir))) return false; // 設定が %APPDATA% にあるときは持ち運べない
        try { return !System.IO.Path.IsPathRooted(ToStoredPath(System.IO.Path.GetFullPath(FromInput(path)))); }
        catch { return false; }
    }

    /// <summary>項目の名前と、その項目のパス（1つ以上）を受け取り、変換したものを返す</summary>
    private delegate List<string> PathMapper(string key, List<string> paths);

    /// <summary>読み込んだときのパス（項目の名前 → 読み替えた後と、保存されていた書き方）</summary>
    private static readonly Dictionary<string, List<(string resolved, string stored)>> LoadedPaths = new();

    private static List<string> MapLoaded(string key, List<string> stored)
    {
        var plain = stored.Select(FromInput).ToList();
        var resolved = stored.Select((p, i) =>
        {
            var r = ResolvePath(p);
            // 読み替え先も同じ一覧に登録されているなら、別々のフォルダなので読み替えない
            return r != plain[i] && plain.Contains(r, StringComparer.OrdinalIgnoreCase) ? plain[i] : r;
        }).ToList();
        lock (LoadedPaths) LoadedPaths[key] = resolved.Select((r, i) => (r, stored[i])).ToList();
        return resolved;
    }

    private static List<string> MapStored(string key, List<string> paths)
    {
        List<(string resolved, string stored)>? loaded;
        lock (LoadedPaths) LoadedPaths.TryGetValue(key, out loaded);
        return paths.Select(p =>
        {
            // 読み込んだときから変わっていなければ、読み替える前の書き方で残す
            var original = loaded?.FirstOrDefault(x => x.resolved == p).stored;
            return original != null && System.IO.Path.IsPathRooted(original) ? ToStoredPath(original) : ToStoredPath(p);
        }).ToList();
    }

    private static string MapOne(PathMapper f, string key, string path) => f(key, new List<string> { path })[0];

    private static void MapPaths(PlaySettings p, string scope, PathMapper f)
    {
        if (p.SaveDir != null) p.SaveDir = MapOne(f, scope + "SaveDir", p.SaveDir);
        if (p.StateDir != null) p.StateDir = MapOne(f, scope + "StateDir", p.StateDir);
        if (p.ScreenshotDir != null) p.ScreenshotDir = MapOne(f, scope + "ScreenshotDir", p.ScreenshotDir);
    }

    private static void MapPaths(AppSettings s, PathMapper f)
    {
        s.BiosDir = MapOne(f, "app|BiosDir", s.BiosDir);
        s.Preview.OwnShotFolder = MapOne(f, "app|OwnShotFolder", s.Preview.OwnShotFolder);
        MapPaths(s.Play, "app|", f);
        foreach (var c in s.Systems)
        {
            var scope = "sys:" + c.Id + "|";
            c.CustomIconPath = MapOne(f, scope + "CustomIconPath", c.CustomIconPath);
            c.RomFolders = f(scope + "RomFolders", c.RomFolders);
            c.DatPath = MapOne(f, scope + "DatPath", c.DatPath);
            c.UnknownFolder = MapOne(f, scope + "UnknownFolder", c.UnknownFolder);
            c.OwnShotFolder = MapOne(f, scope + "OwnShotFolder", c.OwnShotFolder);
            c.EmulatorPath = MapOne(f, scope + "EmulatorPath", c.EmulatorPath);
            c.EmulatorWorkDir = MapOne(f, scope + "EmulatorWorkDir", c.EmulatorWorkDir);
            MapPaths(c.Play, scope, f);
        }
    }

    /// <summary>JSONを経由した複製に、保存用のパス変換をかけて書き出す（使用中の設定は変えない）</summary>
    private static string SerializeStored<T>(T value, Action<T> map)
    {
        var copy = JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value, Json), Json)!;
        map(copy);
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
                MapPaths(g.Play, "game:" + gameKey + "|", MapLoaded);
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
        WriteAtomic(p, SerializeStored(game, g => MapPaths(g.Play, "game:" + gameKey + "|", MapStored)));
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
                    MapPaths(s, MapLoaded);
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

    public static void SaveSettings(AppSettings settings) => WriteAtomic(SettingsPath, SerializeStored(settings, s => MapPaths(s, MapStored)));

    public static Dictionary<string, RomFile> LoadCache(string systemId)
    {
        var result = new Dictionary<string, RomFile>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var p = System.IO.Path.Combine(CacheDir, systemId + ".json");
            if (!File.Exists(p)) return result;
            var list = JsonSerializer.Deserialize<List<RomFile>>(File.ReadAllText(p), Json) ?? new();
            foreach (var r in list)
            {
                r.Path = FromInput(r.Path);
                result.TryAdd(r.Path, r);
            }
            // ドライブ文字が変わっていても使えるようにする。使う前に大きさと日時を比べるので、ここではファイルを見ない
            foreach (var r in list)
                if (OnAppDrive(r.Path) is string moved)
                    result.TryAdd(moved, r.WithPath(moved));
        }
        catch { }
        return result;
    }

    public static void SaveCache(string systemId, IEnumerable<RomFile> roms)
    {
        Directory.CreateDirectory(CacheDir);
        var stored = roms.Select(r => r.WithPath(ToStoredPath(r.Path))).ToList();
        WriteAtomic(System.IO.Path.Combine(CacheDir, systemId + ".json"), JsonSerializer.Serialize(stored, Json));
    }

    public static void DeleteCache(string systemId)
    {
        try { File.Delete(System.IO.Path.Combine(CacheDir, systemId + ".json")); } catch { }
    }

    /// <summary>リネームの記録。パスは保存されたままの形で返す（使う直前に ResolveRecord で今の場所にする）</summary>
    public static List<RenameRun> LoadHistory()
    {
        try
        {
            if (File.Exists(HistoryPath))
                return JsonSerializer.Deserialize<List<RenameRun>>(File.ReadAllText(HistoryPath), Json) ?? new();
        }
        catch
        {
            // 読めない記録は、次の保存で上書きされる前に控えを取る
            try { File.Copy(HistoryPath, HistoryPath + ".broken", true); } catch { }
        }
        return new();
    }

    public static void SaveHistory(List<RenameRun> runs)
    {
        // 直近50回分だけ残す
        var keep = runs.Count > 50 ? runs.Skip(runs.Count - 50).ToList() : runs;
        WriteAtomic(HistoryPath, SerializeStored(keep, list =>
        {
            foreach (var r in list.SelectMany(run => run.Records))
            {
                r.From = ToStoredPath(r.From);
                r.To = ToStoredPath(r.To);
            }
        }));
    }

    /// <summary>記録された「元の場所」と「いまの場所」を、現在の絶対パスにする</summary>
    public static (string from, string to) ResolveRecord(RenameRecord r)
    {
        var plainTo = FromInput(r.To);
        var to = ResolvePath(r.To);
        var from = FromInput(r.From);
        // 元の名前のファイルはもう無いので存在では確かめられない。いまの場所と同じドライブだったなら、同じように読み替える
        if (to != plainTo && from.Length >= 3 && from[1] == ':'
            && char.ToUpperInvariant(from[0]) == char.ToUpperInvariant(plainTo[0]))
            from = to[0] + from[1..];
        return (from, to);
    }

    private static void WriteAtomic(string path, string content)
    {
        var tmp = path + ".tmp";
        File.WriteAllText(tmp, content);
        File.Move(tmp, path, true);
    }
}
