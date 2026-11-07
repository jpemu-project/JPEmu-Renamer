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
    private static bool imeFailed;
    private static readonly Dictionary<string, string> cache = new(StringComparer.Ordinal);

    /// <summary>並べ替え用のキー（ひらがな・全角半角をそろえたもの）。結果は覚えておく</summary>
    public static string Key(string title)
    {
        if (string.IsNullOrEmpty(title)) return "";
        lock (cache)
        {
            if (cache.TryGetValue(title, out var k)) return k;
            k = Normalize(NeedsReading(title) ? Phonetic(title) ?? title : title);
            cache[title] = k;
            return k;
        }
    }

    private static bool NeedsReading(string s) => s.Any(c => c >= 0x4E00 && c <= 0x9FFF || c == '々');

    private static string? Phonetic(string s)
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
