// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
namespace JPEmuRenamer.Models;

/// <summary>アプリ全体の設定（settings.json に保存）</summary>
public class AppSettings
{
    /// <summary>"" = OSの言語に合わせる / "ja" / "en"</summary>
    public string Language { get; set; } = "";
    /// <summary>"Light" / "Dark"</summary>
    public string Theme { get; set; } = "Light";
    /// <summary>UI全体の表示倍率（0.75〜2.0）</summary>
    public double UiScale { get; set; } = 1.0;
    /// <summary>文字サイズ（px）</summary>
    public double FontSize { get; set; } = 14;
    /// <summary>All / Owned / Missing</summary>
    public string ListFilter { get; set; } = "All";
    /// <summary>ZIP内のROM名もリネームする</summary>
    public bool RenameInsideZip { get; set; } = true;
    /// <summary>サブフォルダも走査する</summary>
    public bool ScanSubfolders { get; set; } = true;
    /// <summary>タイトル一覧でファイル名の拡張子を表示しない</summary>
    public bool HideExtensions { get; set; }

    public double? WindowWidth { get; set; }
    public double? WindowHeight { get; set; }
    public double? WindowLeft { get; set; }
    public double? WindowTop { get; set; }
    public bool WindowMaximized { get; set; }

    public string? LastSystemId { get; set; }
    public bool FirstRunDone { get; set; }

    /// <summary>BIOSなどを置くフォルダ（空ならデータフォルダの system）</summary>
    public string BiosDir { get; set; } = "";
    /// <summary>内蔵コアのプレイ設定（全体）</summary>
    public PlaySettings Play { get; set; } = new();
    /// <summary>自動プレビュー</summary>
    public PreviewSettings Preview { get; set; } = new();

    public List<SystemConfig> Systems { get; set; } = new();
}

/// <summary>自動プレビューの設定</summary>
public class PreviewSettings
{
    public bool Enabled { get; set; } = true;
    /// <summary>選んでから再生を始めるまでの待ち（ミリ秒）</summary>
    public int DelayMs { get; set; } = 300;
    /// <summary>再生速度（倍）</summary>
    public int Speed { get; set; } = 4;
    /// <summary>起動から表示せずに進める秒数（メーカーロゴなどを飛ばす）</summary>
    public int SkipSeconds { get; set; } = 10;
    /// <summary>冒頭から繰り返すまでの秒数</summary>
    public int LoopSeconds { get; set; } = 60;
    /// <summary>スクショの一括作成で、何秒の時点の画面を保存するか</summary>
    public int SnapshotSeconds { get; set; } = 30;
}

/// <summary>機種ごとの設定</summary>
public class SystemConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    /// <summary>cart / diskcard / disc / handV / handH / dual / arcade / pc / pad</summary>
    public string IconKind { get; set; } = "cart";
    /// <summary>アイコンの色（#RRGGBB）</summary>
    public string Color { get; set; } = "#F06B70";
    /// <summary>自作アイコン画像のパス（空なら内蔵アイコン）</summary>
    public string CustomIconPath { get; set; } = "";
    public List<string> RomFolders { get; set; } = new();
    /// <summary>対象拡張子（".nes" など、小文字）</summary>
    public List<string> Extensions { get; set; } = new();
    public string DatPath { get; set; } = "";
    /// <summary>Auto / Include / Exclude（iNESヘッダーなどの扱い）</summary>
    public string HeaderMode { get; set; } = "Auto";
    /// <summary>ファイル名を変えない「表示リネーム」（アーケード系向け）</summary>
    public bool DisplayRenameOnly { get; set; }
    /// <summary>Core / External</summary>
    public string LaunchMode { get; set; } = "Core";
    public string EmulatorPath { get; set; } = "";
    public string EmulatorArgs { get; set; } = "\"{rom}\"";
    public string EmulatorWorkDir { get; set; } = "";
    /// <summary>内蔵コアの名前（"mesen" など。空なら候補の既定コア）</summary>
    public string CoreName { get; set; } = "";
    /// <summary>内蔵コアのプレイ設定（この機種で上書きする分）</summary>
    public PlaySettings Play { get; set; } = new();

    public SystemConfig Clone()
    {
        var c = (SystemConfig)MemberwiseClone();
        c.RomFolders = new List<string>(RomFolders);
        c.Extensions = new List<string>(Extensions);
        c.Play = Play.Clone();
        return c;
    }
}

/// <summary>DATの1エントリ（1タイトル×1CRC）</summary>
public class DatEntry
{
    public string Title { get; set; } = "";
    public uint? Crc { get; set; }
    public string Extension { get; set; } = "";
    public string Publisher { get; set; } = "";
    public string ReleaseNumber { get; set; } = "";
    public string Genre { get; set; } = "";
    public string ReleaseDate { get; set; } = "";
    public string Price { get; set; } = "";
    public string Comment { get; set; } = "";
}

/// <summary>走査したROMファイル（キャッシュにも使う）</summary>
public class RomFile
{
    public string Path { get; set; } = "";
    public long Size { get; set; }
    public long LastWriteTicks { get; set; }
    /// <summary>ファイル全体（ZIPなら中のROM）のCRC32</summary>
    public uint Crc { get; set; }
    /// <summary>ヘッダー（iNESなど）を除いたCRC32。ヘッダーが無ければ null</summary>
    public uint? CrcNoHeader { get; set; }
    /// <summary>ZIP内のROMのエントリ名（ZIPでなければ null）</summary>
    public string? InnerName { get; set; }
    public long RomSize { get; set; }
}

/// <summary>リネーム1回分の履歴</summary>
public class RenameRun
{
    public DateTime At { get; set; } = DateTime.Now;
    public string SystemId { get; set; } = "";
    public List<RenameRecord> Records { get; set; } = new();
}

public class RenameRecord
{
    public string From { get; set; } = "";
    public string To { get; set; } = "";
    /// <summary>ZIP内エントリ名の変更（変更しなかった場合は null）</summary>
    public string? InnerFrom { get; set; }
    public string? InnerTo { get; set; }
}
