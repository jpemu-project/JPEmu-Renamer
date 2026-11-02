// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Text.Encodings.Web;
using System.Text.Json;
using JPEmuRenamer.Models;

namespace JPEmuRenamer.Services;

/// <summary>
/// 設定・走査キャッシュ・リネーム履歴の保存先と読み書き。
/// exe と同じフォルダに portable.txt があれば、そのフォルダの data\ に保存する（ポータブル運用）。
/// </summary>
public static class Storage
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static string DataDir { get; } = ResolveDataDir();

    private static string ResolveDataDir()
    {
        var exeDir = AppContext.BaseDirectory;
        string dir = File.Exists(System.IO.Path.Combine(exeDir, "portable.txt"))
            ? System.IO.Path.Combine(exeDir, "data")
            : System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "JPEmuRenamer");
        Directory.CreateDirectory(dir);
        return dir;
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
                return JsonSerializer.Deserialize<GameOverrides>(File.ReadAllText(p), Json) ?? new();
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
        WriteAtomic(p, JsonSerializer.Serialize(game, Json));
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
                    if (s.Systems.Count == 0) s.Systems.Add(SystemPresets.DefaultFamicom());
                    return s;
                }
            }
        }
        catch
        {
            // 壊れた設定はバックアップして初期化
            try { File.Copy(SettingsPath, SettingsPath + ".broken", true); } catch { }
        }
        var fresh = new AppSettings();
        fresh.Systems.Add(SystemPresets.DefaultFamicom());
        return fresh;
    }

    public static void SaveSettings(AppSettings settings) => WriteAtomic(SettingsPath, JsonSerializer.Serialize(settings, Json));

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
