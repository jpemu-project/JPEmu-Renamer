// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
using System.ComponentModel;
using System.Globalization;

namespace JPEmuRenamer.Services;

/// <summary>
/// 日本語・英語の表示文言。XAMLからは {Binding [Key], Source={x:Static s:Loc.Instance}} で参照する。
/// 言語を切り替えると、バインドしている文言がその場で変わる。
/// </summary>
public sealed class Loc : INotifyPropertyChanged
{
    public static Loc Instance { get; } = new();

    private string language = "ja";

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Language
    {
        get => language;
        set
        {
            var v = value is "ja" or "en" ? value : DetectOsLanguage();
            if (v == language) return;
            language = v;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Language)));
        }
    }

    public string this[string key]
    {
        get
        {
            var dict = language == "en" ? En : Ja;
            return dict.TryGetValue(key, out var v) ? v : (Ja.TryGetValue(key, out var j) ? j : key);
        }
    }

    public static string T(string key) => Instance[key];
    public static string F(string key, params object[] args) => string.Format(Instance[key], args);

    public static string DetectOsLanguage() =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ja" ? "ja" : "en";

    private static readonly Dictionary<string, string> Ja = new()
    {
        ["Play"] = "遊ぶ（リスト）",
        ["Manage"] = "整理（管理表）",
        ["Search"] = "タイトル・ファイル名で検索",
        ["AddSystem"] = "＋ 機種を追加",
        ["Systems"] = "機種",
        ["Settings"] = "設定",
        ["OwnedMissing"] = "所有 {0:N0}本 ／ 未所有 {1:N0}本",
        ["FilterOwned"] = "所有のみ",
        ["FilterAll"] = "すべて",
        ["FilterMissing"] = "未所有のみ",
        ["ColTitle"] = "タイトル",
        ["ColPublisher"] = "メーカー",
        ["ColStatus"] = "状態",
        ["ColCurrent"] = "現在のファイル名",
        ["ColTarget"] = "→ DAT上の名前",
        ["ColCrc"] = "CRC32",
        ["ColSize"] = "サイズ",
        ["ColFormat"] = "形式",
        ["StMatch"] = "一致",
        ["StNameDiff"] = "名前違い",
        ["StDuplicate"] = "重複",
        ["StNotInDat"] = "DATに無し",
        ["StMissing"] = "未所有",
        ["NoChange"] = "（変更なし）",
        ["AlreadyExists"] = "（既存あり）",
        ["LaunchCore"] = "▶ 起動",
        ["LaunchExternal"] = "▶ 外部exeで起動",
        ["OpenFolder"] = "フォルダを開く",
        ["PreviewSoon"] = "自動プレビュー\n（次のバージョンで対応予定）",
        ["Rescan"] = "再照合",
        ["Undo"] = "元に戻す",
        ["RunRename"] = "リネームを実行",
        ["SelectedCount"] = "{0}件を選択中",
        ["EditSystem"] = "機種の設定",
        ["Before"] = "変更前",
        ["After"] = "変更後",
        ["RenameNote"] = "CRC32がDATと一致したファイルだけを変更します。実行ごとに履歴を保存し、「元に戻す」で1回分を丸ごと戻せます。",
        ["NoSystemSetup"] = "この機種はまだDATファイルとROMフォルダが設定されていません。\n「機種の設定」から指定してください。",
        ["Scanning"] = "走査中… {0}/{1}",
        ["Ready"] = "準備完了",
        ["Loaded"] = "{0}：照合 {1}件 ／ 名前違い {2}件 ／ DATに無し {3}件",
        ["DatError"] = "DATファイルを読み込めませんでした：{0}",
        ["ConfirmRename"] = "{0}件のファイルをリネームします。よろしいですか？\n（あとから「元に戻す」で戻せます）",
        ["RenameDone"] = "{0}件をリネームしました。スキップ {1}件。",
        ["NothingToUndo"] = "元に戻せる履歴がありません。",
        ["ConfirmUndo"] = "{0} に行った {1}件のリネームを元に戻しますか？",
        ["UndoDone"] = "{0}件を元に戻しました。",
        ["CoreNotYet"] = "内蔵コアでの起動は次のバージョンで対応予定です。\n「機種の設定」で外部exe（エミュレーター）を指定してください。",
        ["NoEmulator"] = "外部exeが設定されていません。「機種の設定」で指定してください。",
        ["LaunchFailed"] = "起動できませんでした：{0}",
        ["MissingCannotLaunch"] = "未所有のタイトルは起動できません。",
        ["SetDefaultExe"] = "この機種の既定の起動方法を外部exeにしますか？",
        ["Name"] = "ハード名",
        ["Preset"] = "候補から選ぶ",
        ["Icon"] = "アイコン",
        ["IconColor"] = "アイコンの色",
        ["CustomIcon"] = "自作アイコン（任意）",
        ["DatFile"] = "DATファイル（OfflineList形式）",
        ["RomFolder"] = "ROMフォルダ（複数は ; 区切り）",
        ["Extensions"] = "対象拡張子（; 区切り）",
        ["HeaderMode"] = "ヘッダーの扱い（CRC計算）",
        ["HeaderAuto"] = "自動（DATに合わせる）",
        ["HeaderInclude"] = "ヘッダー込み",
        ["HeaderExclude"] = "ヘッダー除外",
        ["DisplayRenameOnly"] = "ファイル名は変えず、表示名だけDATの名前にする（アーケード系向け）",
        ["LaunchMode"] = "既定の起動方法",
        ["ModeCore"] = "内蔵コア（次バージョン）",
        ["ModeExternal"] = "外部exe",
        ["EmulatorPath"] = "外部exe（エミュレーター）",
        ["EmulatorArgs"] = "引数（{rom}=ROMのフルパス、{romdir}、{romname}）",
        ["Browse"] = "参照…",
        ["Ok"] = "OK",
        ["Cancel"] = "キャンセル",
        ["Delete"] = "この機種を削除",
        ["ConfirmDelete"] = "「{0}」を一覧から削除しますか？（ROMファイルは削除しません）",
        ["NameRequired"] = "ハード名を入力してください。",
        ["AppSettings"] = "アプリの表示",
        ["LanguageLabel"] = "言語 / Language",
        ["LangAuto"] = "自動（OSに合わせる）",
        ["UiScale"] = "表示倍率",
        ["FontSizeLabel"] = "文字サイズ",
        ["RenameInsideZip"] = "ZIP内のROM名もリネームする",
        ["ScanSubfolders"] = "サブフォルダも走査する",
        ["Close"] = "閉じる",
        ["ScaleHint"] = "Ctrl＋マウスホイールでも変更できます",
        ["Welcome"] = "JPEmu Renamer へようこそ。\n最初に、ファミコンのDATファイルとROMフォルダを設定しましょう。今すぐ設定を開きますか？",
        ["Footer"] = "Enter：起動　↑↓：選択　Ctrl＋ホイール：表示倍率",
    };

    private static readonly Dictionary<string, string> En = new()
    {
        ["Play"] = "Play (List)",
        ["Manage"] = "Organize (Table)",
        ["Search"] = "Search titles or file names",
        ["AddSystem"] = "+ Add system",
        ["Systems"] = "Systems",
        ["Settings"] = "Settings",
        ["OwnedMissing"] = "Owned {0:N0} / Missing {1:N0}",
        ["FilterOwned"] = "Owned",
        ["FilterAll"] = "All",
        ["FilterMissing"] = "Missing",
        ["ColTitle"] = "Title",
        ["ColPublisher"] = "Publisher",
        ["ColStatus"] = "Status",
        ["ColCurrent"] = "Current file name",
        ["ColTarget"] = "→ Name in DAT",
        ["ColCrc"] = "CRC32",
        ["ColSize"] = "Size",
        ["ColFormat"] = "Format",
        ["StMatch"] = "Match",
        ["StNameDiff"] = "Rename",
        ["StDuplicate"] = "Duplicate",
        ["StNotInDat"] = "Not in DAT",
        ["StMissing"] = "Missing",
        ["NoChange"] = "(no change)",
        ["AlreadyExists"] = "(exists)",
        ["LaunchCore"] = "▶ Launch",
        ["LaunchExternal"] = "▶ Launch with emulator",
        ["OpenFolder"] = "Open folder",
        ["PreviewSoon"] = "Auto preview\n(coming in the next version)",
        ["Rescan"] = "Rescan",
        ["Undo"] = "Undo",
        ["RunRename"] = "Rename",
        ["SelectedCount"] = "{0} selected",
        ["EditSystem"] = "System settings",
        ["Before"] = "Before",
        ["After"] = "After",
        ["RenameNote"] = "Only files whose CRC32 matches the DAT are renamed. Every run is logged, and Undo reverts the whole run.",
        ["NoSystemSetup"] = "No DAT file or ROM folder is set for this system yet.\nOpen \"System settings\" to set them.",
        ["Scanning"] = "Scanning… {0}/{1}",
        ["Ready"] = "Ready",
        ["Loaded"] = "{0}: {1} matched / {2} to rename / {3} not in DAT",
        ["DatError"] = "Could not read the DAT file: {0}",
        ["ConfirmRename"] = "Rename {0} files?\n(You can revert with Undo.)",
        ["RenameDone"] = "Renamed {0} files. Skipped {1}.",
        ["NothingToUndo"] = "There is nothing to undo.",
        ["ConfirmUndo"] = "Undo the {1} renames made at {0}?",
        ["UndoDone"] = "Reverted {0} files.",
        ["CoreNotYet"] = "Launching with the built-in core is coming in the next version.\nSet an emulator exe in \"System settings\".",
        ["NoEmulator"] = "No emulator exe is set. Set one in \"System settings\".",
        ["LaunchFailed"] = "Could not launch: {0}",
        ["MissingCannotLaunch"] = "Missing titles cannot be launched.",
        ["SetDefaultExe"] = "Make the emulator exe the default way to launch this system?",
        ["Name"] = "System name",
        ["Preset"] = "Choose a preset",
        ["Icon"] = "Icon",
        ["IconColor"] = "Icon color",
        ["CustomIcon"] = "Custom icon (optional)",
        ["DatFile"] = "DAT file (OfflineList format)",
        ["RomFolder"] = "ROM folders (separate with ;)",
        ["Extensions"] = "Extensions (separate with ;)",
        ["HeaderMode"] = "Header handling (CRC)",
        ["HeaderAuto"] = "Auto (follow the DAT)",
        ["HeaderInclude"] = "Include header",
        ["HeaderExclude"] = "Exclude header",
        ["DisplayRenameOnly"] = "Keep file names; show DAT names only (for arcade sets)",
        ["LaunchMode"] = "Default launch method",
        ["ModeCore"] = "Built-in core (next version)",
        ["ModeExternal"] = "Emulator exe",
        ["EmulatorPath"] = "Emulator exe",
        ["EmulatorArgs"] = "Arguments ({rom}=full ROM path, {romdir}, {romname})",
        ["Browse"] = "Browse…",
        ["Ok"] = "OK",
        ["Cancel"] = "Cancel",
        ["Delete"] = "Delete this system",
        ["ConfirmDelete"] = "Remove \"{0}\" from the list? (ROM files are not deleted.)",
        ["NameRequired"] = "Enter a system name.",
        ["AppSettings"] = "Display",
        ["LanguageLabel"] = "言語 / Language",
        ["LangAuto"] = "Auto (follow Windows)",
        ["UiScale"] = "UI scale",
        ["FontSizeLabel"] = "Text size",
        ["RenameInsideZip"] = "Also rename the ROM inside ZIP files",
        ["ScanSubfolders"] = "Scan subfolders",
        ["Close"] = "Close",
        ["ScaleHint"] = "You can also use Ctrl + mouse wheel",
        ["Welcome"] = "Welcome to JPEmu Renamer.\nFirst, set the Famicom DAT file and ROM folder. Open the settings now?",
        ["Footer"] = "Enter: launch   ↑↓: select   Ctrl+wheel: UI scale",
    };
}
