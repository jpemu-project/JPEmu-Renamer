// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using CommunityToolkit.Mvvm.ComponentModel;
using JPEmuRenamer.Services;

namespace JPEmuRenamer.Models;

public enum MatchStatus
{
    /// <summary>CRC一致・名前も同じ</summary>
    Match,
    /// <summary>CRC一致・名前が違う（リネーム候補）</summary>
    NameDiff,
    /// <summary>同じタイトルのファイルが既にある</summary>
    Duplicate,
    /// <summary>DATに無い</summary>
    NotInDat,
    /// <summary>DATにあるが手元に無い</summary>
    Missing
}

/// <summary>一覧の1行</summary>
public partial class GameItem : ObservableObject
{
    /// <summary>DATに無いファイル（ファイル名をそのまま表示する行）で、拡張子を表示しない</summary>
    public static bool HideExtensions { get; set; }

    private readonly string title;

    public GameItem(MatchStatus status, string title, DatEntry? dat, RomFile? rom, string targetFileName)
    {
        Status = status;
        this.title = title;
        Dat = dat;
        Rom = rom;
        TargetFileName = targetFileName;
        isChecked = status == MatchStatus.NameDiff;
    }

    public MatchStatus Status { get; }
    /// <summary>表示名（照合済みはDATの名前、未照合はファイル名）</summary>
    public string Title => HideExtensions && Status == MatchStatus.NotInDat ? System.IO.Path.GetFileNameWithoutExtension(title) : title;
    public DatEntry? Dat { get; }
    public RomFile? Rom { get; }
    /// <summary>リネーム後のファイル名</summary>
    public string TargetFileName { get; }

    [ObservableProperty]
    private bool isChecked;

    public bool IsOwned => Rom != null;
    public bool CanRename => Status == MatchStatus.NameDiff;

    public string Publisher => Dat?.Publisher ?? "";
    public string CurrentFileName => Rom != null ? System.IO.Path.GetFileName(Rom.Path) : "";
    public string CrcText => Rom != null ? Rom.Crc.ToString("X8") : (Dat?.Crc?.ToString("X8") ?? "");
    public string SizeText => Rom != null ? FormatSize(Rom.RomSize > 0 ? Rom.RomSize : Rom.Size) : "";
    public string FormatText => Rom != null ? System.IO.Path.GetExtension(Rom.Path).TrimStart('.').ToUpperInvariant() : "";
    public string TargetText => Status switch
    {
        MatchStatus.NameDiff => TargetFileName,
        MatchStatus.Match => Loc.T("NoChange"),
        MatchStatus.Duplicate => TargetFileName + " " + Loc.T("AlreadyExists"),
        _ => "―"
    };

    public string StatusText => Status switch
    {
        MatchStatus.Match => Loc.T("StMatch"),
        MatchStatus.NameDiff => Loc.T("StNameDiff"),
        MatchStatus.Duplicate => Loc.T("StDuplicate"),
        MatchStatus.NotInDat => Loc.T("StNotInDat"),
        _ => Loc.T("StMissing")
    };

    /// <summary>状態バッジの種類（XAMLのトリガーで色分け）</summary>
    public string StatusKey => Status.ToString();

    private static string FormatSize(long bytes)
    {
        if (bytes >= 1024L * 1024 * 1024) return (bytes / 1024d / 1024 / 1024).ToString("0.0") + "GB";
        if (bytes >= 1024L * 1024) return (bytes / 1024d / 1024).ToString("0.0") + "MB";
        return Math.Max(1, bytes / 1024) + "KB";
    }
}
