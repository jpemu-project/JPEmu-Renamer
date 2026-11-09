// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace JPEmuRenamer.Services;

/// <summary>
/// あいうえお順に並べるための読み（ひらがな）。漢字は Windows の日本語入力（MS-IME）で読みを求める。
/// IME が使えない環境では、文字そのもの（カタカナはひらがなにそろえる）で並べる。
/// </summary>
public static class JapaneseReading
{
    [ComImport, Guid("019F7152-E6DB-11d0-83C3-00C04FDDB82E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFELanguage
    {
        int Open();
        int Close();
        int GetJMorphResult(uint dwRequest, uint dwCMode, int cwchInput, [MarshalAs(UnmanagedType.LPWStr)] string pwchInput, IntPtr pfCInfo, out IntPtr ppResult);
        int GetConversionModeCaps(ref uint pdwCaps);
        int GetPhonetic([MarshalAs(UnmanagedType.BStr)] string str, int start, int length, [MarshalAs(UnmanagedType.BStr)] out string result);
        int GetConversion([MarshalAs(UnmanagedType.BStr)] string str, int start, int length, [MarshalAs(UnmanagedType.BStr)] out string result);
    }

    private static IFELanguage? ime;
    private static volatile bool imeFailed;
    private static readonly Dictionary<string, string> cache = new(StringComparer.Ordinal);
    /// <summary>IME で求めた読み（data\readings.json に保存し、次に起動したときも使う）</summary>
    private static readonly Dictionary<string, string> readings = new(StringComparer.Ordinal);
    private static bool loaded, dirty;

    private static string ReadingsPath => System.IO.Path.Combine(Storage.DataDir, "readings.json");

    /// <summary>並べ替え用のキー（ひらがな・全角半角をそろえたもの）。結果は覚えておく</summary>
    public static string Key(string title)
    {
        if (string.IsNullOrEmpty(title)) return "";
        bool needs = NeedsReading(title);
        lock (cache)
        {
            if (cache.TryGetValue(title, out var k)) return k;
            EnsureLoaded();
            if (needs && readings.TryGetValue(title, out var saved))
            {
                k = Normalize(saved);
                cache[title] = k;
                return k;
            }
        }
        // IME への問い合わせは時間がかかるので、ロックの外で行う
        string? phonetic = needs ? Phonetic(title) : null;
        var key = Normalize(phonetic ?? title);
        lock (cache)
        {
            cache[title] = key;
            if (phonetic != null)
            {
                readings[title] = phonetic;
                dirty = true;
            }
        }
        return key;
    }

    /// <summary>
    /// 一覧を出す前に、裏のスレッドで読みを求めておく（初めて開く機種でも、画面が止まらないように）。
    /// 新しく求めた読みは保存する。
    /// </summary>
    public static void Prefetch(IEnumerable<string> titles)
    {
        foreach (var t in titles) Key(t);
        Save();
    }

    /// <summary>新しく求めた読みがあれば data\readings.json に保存する</summary>
    public static void Save()
    {
        string json;
        lock (cache)
        {
            if (!dirty) return;
            json = System.Text.Json.JsonSerializer.Serialize(readings, new System.Text.Json.JsonSerializerOptions
            {
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            dirty = false;
        }
        try
        {
            var tmp = ReadingsPath + "." + Environment.CurrentManagedThreadId + ".tmp";
            File.WriteAllText(tmp, json);
            File.Move(tmp, ReadingsPath, true);
        }
        catch { }
    }

    private static void EnsureLoaded()
    {
        if (loaded) return;
        loaded = true;
        try
        {
            if (File.Exists(ReadingsPath))
            {
                var d = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(ReadingsPath));
                if (d != null) foreach (var kv in d) readings[kv.Key] = kv.Value;
            }
        }
        catch { }
    }

    private static bool NeedsReading(string s) => s.Any(c => c >= 0x4E00 && c <= 0x9FFF || c == '々');

    // MS-IME の COM オブジェクトは、作ったスレッド（STA）からだけ使う。
    // どのスレッドから頼まれても、専用のスレッドで順番に問い合わせる。
    private static readonly System.Collections.Concurrent.BlockingCollection<(string text, TaskCompletionSource<string?> done)> imeQueue = new();
    private static Thread? imeThread;
    private static readonly object imeThreadLock = new();

    private static string? Phonetic(string s)
    {
        if (imeFailed) return null;
        lock (imeThreadLock)
        {
            if (imeThread == null)
            {
                imeThread = new Thread(ImeLoop) { IsBackground = true, Name = "MS-IME reading" };
                imeThread.SetApartmentState(ApartmentState.STA);
                imeThread.Start();
            }
        }
        var done = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
        imeQueue.Add((s, done));
        return done.Task.GetAwaiter().GetResult();
    }

    private static void ImeLoop()
    {
        foreach (var (text, done) in imeQueue.GetConsumingEnumerable())
            done.SetResult(PhoneticCore(text));
    }

    private static string? PhoneticCore(string s)
    {
        if (imeFailed) return null;
        try
        {
            if (ime == null)
            {
                var type = Type.GetTypeFromProgID("MSIME.Japan");
                if (type == null) { imeFailed = true; return null; }
                ime = (IFELanguage)Activator.CreateInstance(type)!;
                ime.Open();
            }
            return ime.GetPhonetic(s, 1, -1, out var r) == 0 ? r : null;
        }
        catch
        {
            imeFailed = true;
            return null;
        }
    }

    /// <summary>全角英数を半角に、カタカナをひらがなに、英字を小文字にそろえる</summary>
    private static string Normalize(string s)
    {
        s = s.Normalize(NormalizationForm.FormKC).ToLowerInvariant();
        var sb = new StringBuilder(s.Length);
        foreach (var c in s) sb.Append(c >= 'ァ' && c <= 'ヶ' ? (char)(c - 0x60) : c);
        return sb.ToString();
    }

    /// <summary>日本語の照合順（ひらがな・カタカナ、全角・半角の違いは無視）</summary>
    public static readonly CompareInfo Compare = CultureInfo.GetCultureInfo("ja-JP").CompareInfo;
    public const CompareOptions Options = CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase;
}
