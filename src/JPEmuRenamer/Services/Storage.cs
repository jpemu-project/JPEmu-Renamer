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
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
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
