// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later
namespace JPEmuRenamer.Services;

/// <summary>
/// ヘルプ（マニュアル）の本文。簡単な書式で書き、HelpWindow で表示する。
/// # 見出し、## 小見出し、- 箇条書き、| 表 |（1行目は見出し）、&gt; 補足、**太字**、`キー`
/// </summary>
public static class HelpContent
{
    // 日本語・英語以外のマニュアルは Services\Lang の Help.*.cs にある
    public static string Play(string lang) => lang switch
    {
        "ja" => PlayJa, "zh" => HelpZh.Play, "ko" => HelpKo.Play, "fr" => HelpFr.Play, "it" => HelpIt.Play,
        "es" => HelpEs.Play, "pt" => HelpPt.Play, "ru" => HelpRu.Play, "zh-Hant" => HelpZhHant.Play,
        "de" => HelpDe.Play, "pl" => HelpPl.Play, "tr" => HelpTr.Play, "id" => HelpId.Play, "vi" => HelpVi.Play, "th" => HelpTh.Play, _ => PlayEn
    };

    public static string Rename(string lang) => lang switch
    {
        "ja" => RenameJa, "zh" => HelpZh.Rename, "ko" => HelpKo.Rename, "fr" => HelpFr.Rename, "it" => HelpIt.Rename,
        "es" => HelpEs.Rename, "pt" => HelpPt.Rename, "ru" => HelpRu.Rename, "zh-Hant" => HelpZhHant.Rename,
        "de" => HelpDe.Rename, "pl" => HelpPl.Rename, "tr" => HelpTr.Rename, "id" => HelpId.Rename, "vi" => HelpVi.Rename, "th" => HelpTh.Rename, _ => RenameEn
    };

    private const string PlayJa = """
# はじめに
JPEmu Renamer は、自分で吸い出したROMを管理し、そのまま遊べるランチャーです。遊ぶときは、アプリに内蔵したエミュレーター（内蔵コア）か、自分で用意したエミュレーター（外部exe）を使います。

## 遊ぶまでの流れ
- 最初は機種がありません。左下の「＋ 機種を追加」で、機種を好きな名前で作ります（例：ファミコン、Famicom）
- 作った機種を選び、「機種の設定」でROMフォルダを指定します
- 上の「プレイ」タブで、遊びたいタイトルを選びます
- 「▶ 起動」を押します。初めて使うコアは、その場でダウンロードするか確認が出ます
- 遊び終わったら `Esc` でランチャーに戻ります

> ROMは、自分が持っているソフトから吸い出したものを使ってください。

# 機種の設定
左の一覧で機種を選び、右上の「機種の設定」を開きます。新しい機種は「＋ 機種を追加」で増やせます。「候補から選ぶ」で機種を選ぶと、拡張子・アイコン・色・内蔵コアが自動で入ります。

| 項目 | 内容 |
| ハード名 | 一覧に表示する名前 |
| アイコン・色 | 機種一覧のアイコン。自作の画像（PNG/ICO/JPG）も使えます |
| DATファイル | リネームに使うタイトル一覧（OfflineList形式）。遊ぶだけなら空でもかまいません |
| ROMフォルダ | ROMを置いたフォルダ。複数あるときは `;` で区切ります |
| 対象拡張子 | 読み込むファイルの拡張子（例：`.nes;.zip`） |
| 既定の起動方法 | 内蔵コアか外部exeか |
| 内蔵コア | この機種で使うコア。「ダウンロード」「更新」で取得します |
| 外部exe・引数 | 外部のエミュレーターで遊ぶときに指定します |

## BIOSが必要な機種
ディスクシステム、メガCD、PCエンジン CD-ROM²、セガサターン、ネオジオなどは、BIOSファイルが必要です。必要なファイル名は「機種の設定」の内蔵コアの下に表示されます。BIOSは「プレイ設定」→「詳細」→「BIOSフォルダ」に置いてください（既定は設定フォルダの `system`）。

# ゲームを起動する
## 起動のしかた
- タイトルを選んで「▶ 起動」を押す
- タイトルをダブルクリックする
- タイトルを選んで `Enter` を押す

## 起動方法を切り替える
タイトルを右クリックすると「内蔵コアで起動」「外部exeで起動」を選べます。既定の起動方法は「機種の設定」で変えられます。外部exeを指定したときに「既定の起動方法を外部exeにしますか？」と聞かれ、「いいえ」なら内蔵コアのままです。

## 外部exeの引数
| 書き方 | 置き換わる内容 |
| `{rom}` | ROMのフルパス |
| `{romdir}` | ROMがあるフォルダ |
| `{romname}` | ROMのファイル名（拡張子なし） |

# プレイ中の操作
## キーボードの標準の配置
A/B などの名前は、スーパーファミコンのボタン配置（右がA、下がB）です。

| 操作 | キー |
| 十字キー | `↑` `↓` `←` `→` |
| A / B | `X` / `Z` |
| X / Y | `S` / `A` |
| L / R | `Q` / `W` |
| L2 / R2 | `E` / `R` |
| セレクト / スタート | `右Shift` / `Enter` |

## ゲームパッド
接続したパッドは自動で認識し、標準の配置を割り当てます。Xbox系（XInput）、汎用USBパッド（DirectInput）、Switch Proコントローラー、DualSenseなどに対応しています。プレイヤー1には1番目に接続したパッド、プレイヤー2には2番目のパッドが対応します。左スティックでも十字キーの操作ができます。振動に対応したゲームではパッドが振動します。

## ホットキー
| 操作 | キー | パッド |
| メニュー | `F1` | ガイドボタン |
| 早送り（押している間） | `Tab` | |
| ステートセーブ | `F2` | |
| ステートロード | `F4` | |
| スロット − / ＋ | `F6` / `F7` | |
| スクリーンショット | `F12` | |
| 全画面の切り替え | `F11` または `Alt+Enter` | |
| 一時停止 | `P` | |
| リセット | `F10` | |
| 終了 | `Esc` | |

> キーやパッドの割り当ては「プレイ設定」で自由に変えられます。全画面で `Esc` を押すと、まずウィンドウに戻ります。

# プレイ中のメニュー
`F1`（またはパッドのガイドボタン）でメニューを開くと、ゲームが一時停止します。`↑` `↓` で選び、`Enter`（パッドのA）で決定、`Esc`（パッドのB）で閉じます。

| 項目 | 内容 |
| ゲームに戻る | メニューを閉じます |
| ステートセーブ / ロード | いまのスロットに保存・読み込みします |
| スロットを変える | 保存先のスロットを順に切り替えます |
| スクリーンショット | ゲーム画面をPNGで保存します |
| 全画面 / ウィンドウ | 表示を切り替えます |
| プレイ設定… | 遊びながら設定を変えられます。変えた内容はその場で反映されます |
| リセット | ゲームをリセットします |
| 終了してランチャーに戻る | ゲームを終了します |

# セーブとステートセーブ
## セーブ（バッテリーバックアップ）
ゲーム内でセーブしたデータは自動で保存されます（約10秒ごとと、終了したとき）。次に起動すると自動で読み込みます。

## ステートセーブ
ゲームのその瞬間の状態をまるごと保存します。`F2` で保存、`F4` で読み込み、`F6` / `F7` でスロット（既定は0〜9の10個）を切り替えます。

## 保存先
| 種類 | 既定の保存先 |
| セーブ | 設定フォルダの `saves\機種名` |
| ステートセーブ | 設定フォルダの `states\機種名` |
| スクリーンショット | 設定フォルダの `screenshots\機種名` |

設定フォルダは `%APPDATA%\JPEmuRenamer` です（exeと同じフォルダに `portable.txt` を置くと、そのフォルダの `data`）。保存先は「プレイ設定」で変えられます。

# プレイ設定
「プレイ」タブの「プレイ設定」ボタン、タイトルの右クリック、プレイ中のメニュー、アプリの「設定」から開けます。

## 全体・機種・ゲーム
設定は「全体 → 機種 → ゲーム」の順に重なります。画面上部で編集する層を選びます。下の層で決めた値を、上の層で上書きできます。上書きした項目には ● が付き、↺ で上書きをやめられます。たとえば「全体はウィンドウ表示、このゲームだけ全画面」のような使い分けができます。

## 基本タブ
| 分類 | 内容 |
| コントローラー・キーボード | プレイヤー1の割り当て。変えたい項目をクリックして、パッドのボタンかキーを押します（`Esc` で取り消し、`Delete` で削除） |
| 保存先 | セーブ・ステートセーブ・スクリーンショットのフォルダ |
| 画面 | ウィンドウ/全画面、表示サイズ（等倍・2倍・3倍・画面に合わせる）、縦横比 |
| 音声 | 音量、ミュート、出力デバイス |

## 詳細タブ
| 分類 | 内容 |
| コントローラー・キーボード | プレイヤー1〜4の割り当て、連射、連射の速さ、スティックのデッドゾーン、ホットキー。キーボードは同時押し（例：`Alt+Enter`）も割り当てられます |
| 保存先 | フォルダの分け方（分けない/機種ごと/ゲームごと）、セーブをROMと同じフォルダに保存、スロット数、BIOSフォルダ |
| 画面 | 全画面時の解像度とリフレッシュレート、整数倍拡大、補間（くっきり/なめらか）、シェーダー（走査線/CRT風）、垂直同期 |
| 音声 | サンプリングレート、出力方式（WASAPI共有/排他）、遅延、音ずれ補正 |
| コア固有 | コアが持つ設定（例：NESのパレット、画面の縦横比、オーバークロック）。一度そのコアで起動すると表示されます |

各タブの「このタブを初期設定に戻す」で、いま選んでいる層のそのタブの設定を元に戻せます。

# 自動プレビュー
「プレイ」タブでタイトルを選んで少し止まると、右上でそのゲームの映像が自動で流れます。内蔵コアを音・入力なしで動かし、タイトル画面のあとに始まるデモを見せます。プレビューには、その機種の内蔵コアがダウンロードされている必要があります。

| 項目 | 既定値 |
| 再生を始めるまで | 選んで0.3秒止まったら（カーソルを連打している間は始めません） |
| 再生速度 | 4倍速 |
| 冒頭を飛ばす長さ | 起動から10秒（メーカーロゴなどを飛ばします） |
| 冒頭から繰り返すまで | 60秒 |
| 音・入力 | なし（セーブデータにも触れません） |

これらはアプリの「設定」→「自動プレビュー」で変えられます。オフにすることもできます。

## スナップショット
- プレビュー中に右下の「この画面を保存」を押すと、その瞬間をスナップショットとして保存します
- タイトルの右クリック →「この機種のスクショを一括作成…」で、機種内の全ROMを裏で順に起動し、指定の時点（既定は30秒）の画面をまとめて保存します。スクショがあるタイトルは飛ばせます
- スナップショットは、プレビューが始まるまでの間と、画面がしばらく動かないゲーム（デモが流れないゲーム）で表示されます
- 保存先は設定フォルダの `snapshots\機種名\タイトル名.png` です

> プレビューやスクショの一括作成の最中にゲームを起動すると、先にプレビューを止めてから起動します。

# 内蔵コアの一括ダウンロード・更新
アプリの「設定」→「内蔵コアの一括ダウンロード・更新…」で開きます。

- コアの一覧に、使う機種、大きさ、状態（未ダウンロード/最新/更新あり）が表示されます
- 「登録済みの機種のコア」「未ダウンロードのコア」「更新があるコア」などでまとめて選び、「ダウンロード」を押します
- 「更新をすべて適用」で、更新があるコアをまとめて最新版にします

> コアは libretro 公式の配布サイトからダウンロードします。遊んでいる最中のコアは更新できません。

# 困ったとき
| 症状 | 確認すること |
| 起動できない・「ゲームを読み込めませんでした」 | BIOSが必要な機種なら、BIOSフォルダにファイルがあるか確認してください。別のコアに切り替えると動くこともあります |
| 「OpenGLを使えませんでした」 | N64などOpenGLで描画するコアです。グラフィックドライバーを更新するか、別のコアを選んでください |
| 音が途切れる・遅れる | 「プレイ設定」→「詳細」→「音声」の遅延を大きくするか、音ずれ補正をオンにしてください |
| 動きがカクカクする | 垂直同期をオフにするか、ほかのアプリを閉じてください。N64などの重いコアは性能の高いPCが必要です |
| パッドが反応しない | 「プレイ設定」の上に、認識したパッドが表示されます。表示されないときは接続し直してください。1台目のパッドがプレイヤー1です |
| 全画面の解像度が戻らない | ゲームを終了するか、`Esc` でウィンドウに戻すと元の解像度に戻ります |
| ゲームの最初のボタン入力が効かない | 起動直後の数秒間は入力を受け付けないゲームがあります。少し待ってから押してください |
| 自動プレビューが流れない | その機種の内蔵コアがダウンロードされているか、「設定」で自動プレビューがオンになっているか確認してください。アプリが裏に回っている間は止まります |
""";

    private const string RenameJa = """
# リネームのしくみ
ROMの中身からCRC32（ファイルの指紋のような値）を計算し、DATファイル（タイトルとCRC32の一覧）と照らし合わせます。一致したファイルだけを、DATに書かれた正しい名前に変更します。ファイル名がどんなにバラバラでも、中身が同じなら正しく判定できます。

> リネームの前には必ず一覧で確認でき、実行したあとも「元に戻す」で1回分を丸ごと戻せます。

# 準備
## DATファイルを用意する
OfflineList形式のDATファイル（XML）を使います。機種ごとに用意してください。古いShift-JISのDATにも対応しています。

## 機種を設定する
左の機種一覧で機種を選び、「機種の設定」で次を指定します。

| 項目 | 内容 |
| DATファイル | 用意したDATファイル |
| ROMフォルダ | ROMを置いたフォルダ（複数は `;` で区切る） |
| 対象拡張子 | 読み込む拡張子（例：`.nes;.zip`） |
| ヘッダーの扱い | iNES・FDSなどのヘッダーをCRC32の計算に含めるか（ふつうは「自動」） |

設定すると、ROMの読み込みと照合が自動で始まります。2回目からは変わっていないファイルの計算を省くので、すぐ終わります。

# リネームタブの見方
上の「リネーム」タブに切り替えると、管理表が表示されます。

| 列 | 内容 |
| ✓ | リネームするかどうか（名前違いのものだけ選べます） |
| 状態 | 照合の結果（下の表） |
| 現在のファイル名 | いまのファイル名 |
| → DAT上の名前 | リネーム後のファイル名 |
| CRC32・サイズ・形式 | ROMの情報 |

## 状態の意味
| 状態 | 意味 | リネーム |
| 一致 | CRC32が一致し、名前もDATと同じ | 不要 |
| 名前違い | CRC32が一致し、名前がDATと違う | 対象 |
| 重複 | 同じタイトルのファイルがすでにある | しない |
| DATに無し | DATにCRC32が載っていない | しない |
| 未所有 | DATにあるが、手元にファイルが無い | ― |

# リネームの手順
- 「リネーム」タブで、「名前違い」の行を確認します（最初はすべてにチェックが入っています）
- リネームしたくない行はチェックを外します。見出しのチェックで、まとめて切り替えられます
- 右側に選んだ行の「変更前」と「変更後」が表示されるので、内容を確かめます
- 下の「リネームを実行」を押し、確認で「はい」を選びます
- 終わると件数が表示され、一覧が自動で更新されます

> CRC32がDATと一致したファイルだけを変更します。同じ名前のファイルがすでにある場合はスキップします。

## ZIPの中のROM名
「設定」→「ZIP内のROM名もリネームする」がオンのときは、ZIPファイルの名前と一緒に、中のROMの名前もDATの名前に変えます。

## Windowsで使えない文字
`\` `/` `:` `*` `?` `"` `<` `>` `|` は、同じ形の全角文字に置き換えます。

# 元に戻す
「リネーム」タブの下の「元に戻す」で、最後に実行したリネームを1回分まるごと元に戻せます。もう一度押すと、その前の回を戻せます（直近50回分まで）。

> 「元に戻す」は機種に関係なく、最後に実行した回が対象です。確認画面に日時と件数が表示されます。

# 便利な機能
| 機能 | 使い方 |
| 所有のみ / すべて / 未所有のみ | 一覧の上で、表示するタイトルを絞り込みます。未所有のみで、まだ持っていないタイトルが分かります |
| 検索 | 上の検索欄に入力すると、タイトルとファイル名で絞り込みます |
| 再照合 | ROMを追加・削除したあとに押すと、読み込み直します |
| 拡張子を表示しない | 「設定」→「タイトル一覧でファイル名の拡張子を表示しない」 |
| サブフォルダも読み込む | 「設定」→「サブフォルダの中のROMも読み込む」 |
| 表示名だけDATの名前にする | アーケード系のように、ファイル名を変えるとエミュレーターで動かなくなる機種向けです。「機種の設定」でオンにすると、ファイル名は変えずに一覧の表示だけDATの名前にします |

# 困ったとき
| 症状 | 確認すること |
| 照合が0件になる | DATファイルの機種が合っているか確認してください。「ヘッダーの扱い」を「ヘッダー込み」や「ヘッダー除外」に変えると一致することがあります |
| 「DATファイルを読み込めませんでした」 | OfflineList形式（XML）のDATか確認してください |
| 「重複」になる | 同じタイトルのROMが2つ以上あります。片方を別のフォルダに移してください |
| 「（既存あり）」でスキップされた | リネーム後の名前のファイルがすでにあります。どちらを残すか決めてから、手で整理してください |
| ZIPの中の名前が変わらない | ZIPファイルの名前がすでに正しい場合、中の名前は変わりません |
""";

    private const string PlayEn = """
# Getting started
JPEmu Renamer manages ROMs you dumped yourself and lets you play them right away, using either the built-in emulator (built-in core) or an emulator you provide (emulator exe).

## From setup to playing
- There are no systems at first. Use "+ Add system" at the bottom left to create one with any name (e.g. Famicom, NES)
- Select it and set its ROM folder in "System settings"
- On the "Play" tab, choose a title
- Press "▶ Launch". If the core is not installed yet, you are asked whether to download it
- Press `Esc` to return to the launcher

> Use ROMs dumped from software you own.

# System settings
Choose a system on the left and open "System settings" at the top right. Add systems with "+ Add system". Choosing a preset fills in extensions, icon, color and built-in core.

| Item | Description |
| System name | Name shown in the list |
| Icon / color | Icon in the system list. Custom images (PNG/ICO/JPG) are supported |
| DAT file | Title list used for renaming (OfflineList format). Optional if you only play |
| ROM folders | Folders containing ROMs. Separate several with `;` |
| Extensions | Extensions to read (e.g. `.nes;.zip`) |
| Default launch method | Built-in core or emulator exe |
| Built-in core | Core used for this system. Get it with "Download" / "Update" |
| Emulator exe / arguments | Used when playing with an external emulator |

## Systems that need a BIOS
Famicom Disk System, Mega-CD, PC Engine CD-ROM², Saturn, Neo Geo and others need BIOS files. The file names are shown under the built-in core in "System settings". Put them in "Play settings" → "Advanced" → "BIOS folder" (default: `system` in the settings folder).

# Launching games
## How to launch
- Select a title and press "▶ Launch"
- Double-click a title
- Select a title and press `Enter`

## Switching the launch method
Right-click a title to choose "Launch with built-in core" or "Launch with emulator exe". The default method is set in "System settings".

## Emulator arguments
| Placeholder | Replaced with |
| `{rom}` | Full path of the ROM |
| `{romdir}` | Folder of the ROM |
| `{romname}` | ROM file name without extension |

# Controls
## Default keyboard layout
Button names follow the Super Famicom layout (A on the right, B at the bottom).

| Control | Key |
| D-pad | `↑` `↓` `←` `→` |
| A / B | `X` / `Z` |
| X / Y | `S` / `A` |
| L / R | `Q` / `W` |
| L2 / R2 | `E` / `R` |
| Select / Start | `Right Shift` / `Enter` |

## Gamepads
Connected pads are detected automatically and get a standard layout: Xbox (XInput), generic USB (DirectInput), Switch Pro Controller, DualSense and more. Player 1 uses the first connected pad, player 2 the second. The left stick also works as the D-pad. Rumble works in games that support it.

## Hotkeys
| Action | Key | Pad |
| Menu | `F1` | Guide button |
| Fast forward (hold) | `Tab` | |
| Save state | `F2` | |
| Load state | `F4` | |
| Slot − / + | `F6` / `F7` | |
| Screenshot | `F12` | |
| Toggle full screen | `F11` or `Alt+Enter` | |
| Pause | `P` | |
| Reset | `F10` | |
| Quit | `Esc` | |

> All keys and buttons can be changed in "Play settings". In full screen, `Esc` first returns to a window.

# In-game menu
`F1` (or the pad's Guide button) opens the menu and pauses the game. Use `↑` `↓` to select, `Enter` (pad A) to confirm and `Esc` (pad B) to close.

| Item | Description |
| Resume | Close the menu |
| Save / load state | Save to or load from the current slot |
| Change slot | Cycle through slots |
| Screenshot | Save the game screen as PNG |
| Full screen / window | Switch the display |
| Play settings… | Change settings while playing; changes apply immediately |
| Reset | Reset the game |
| Quit to launcher | End the game |

# Saves and save states
## Save data (battery backup)
In-game saves are stored automatically (about every 10 seconds and when you quit) and loaded the next time.

## Save states
Save the exact moment of the game. `F2` saves, `F4` loads, `F6` / `F7` change the slot (10 slots, 0–9, by default).

## Locations
| Type | Default location |
| Save data | `saves\<system>` in the settings folder |
| Save states | `states\<system>` in the settings folder |
| Screenshots | `screenshots\<system>` in the settings folder |

The settings folder is `%APPDATA%\JPEmuRenamer` (or `data` next to the exe when `portable.txt` is there). Locations can be changed in "Play settings".

# Play settings
Open them from the "Play settings" button on the Play tab, a title's right-click menu, the in-game menu or the app's "Settings".

## All, system and game
Settings stack in the order "All → System → Game". Choose the level to edit at the top. A higher level can override the value from the level below. Overridden items show ●, and ↺ stops overriding. For example: window mode for everything, full screen for just one game.

## Basic tab
| Category | Description |
| Controller & keyboard | Player 1 mappings. Click an item, then press a pad button or key (`Esc` cancels, `Delete` clears) |
| Save locations | Folders for save data, save states and screenshots |
| Video | Window/full screen, window size (1x/2x/3x/fit), aspect ratio |
| Audio | Volume, mute, output device |

## Advanced tab
| Category | Description |
| Controller & keyboard | Players 1–4, turbo and turbo speed, stick dead zone, hotkeys. Keyboard combinations (e.g. `Alt+Enter`) are supported |
| Save locations | Subfolders (none/per system/per game), save data next to the ROM, number of slots, BIOS folder |
| Video | Full-screen resolution and refresh rate, integer scaling, filtering, shaders (scanlines/CRT), v-sync |
| Audio | Sample rate, output mode (WASAPI shared/exclusive), latency, audio sync correction |
| Core options | Options of the core (e.g. NES palette, aspect ratio, overclocking). They appear after launching a game with the core once |

"Reset this tab" restores that tab for the selected level.

# Auto preview
On the "Play" tab, select a title and pause for a moment: footage of the game plays at the top right. The built-in core runs without sound or input and shows the demo that starts after the title screen. The system's built-in core must be installed.

| Item | Default |
| Start | After the selection rests for 0.3 seconds (not while scrolling quickly) |
| Speed | 4x |
| Skip at the start | The first 10 seconds (logos and so on) |
| Restart after | 60 seconds |
| Sound / input | None (save data is not touched) |

Change these, or turn the preview off, in "Settings" → "Auto preview".

## Snapshots
- During a preview, "Save this screen" at the bottom right saves that moment as a snapshot
- Right-click a title → "Create snapshots for this system…" starts every ROM of the system in the background and saves the screen at a set time (30 seconds by default). Titles that already have a snapshot can be skipped
- Snapshots are shown until the preview starts and for games whose screen does not move
- They are saved as `snapshots\<system>\<title>.png` in the settings folder

> Launching a game during a preview or bulk snapshot stops the preview first.

# Downloading and updating cores
Open "Settings" → "Download / update built-in cores…".

- The list shows each core's systems, size and status (not installed / up to date / update available)
- Select cores with "Cores for my systems", "Not installed", "With updates" and so on, then press "Download"
- "Apply all updates" updates every core that has an update

> Cores are downloaded from the official libretro site. A core in use by a running game cannot be updated.

# Troubleshooting
| Problem | What to check |
| Cannot start / "could not load the game" | If the system needs a BIOS, check the BIOS folder. Another core may work |
| "OpenGL is not available" | The core renders with OpenGL (e.g. N64). Update your graphics driver or choose another core |
| Audio crackles or lags | Increase latency or turn on audio sync correction in "Play settings" → "Advanced" → "Audio" |
| Choppy motion | Turn off v-sync or close other apps. Demanding cores such as N64 need a fast PC |
| Pad does not respond | Detected pads are listed at the top of "Play settings". Reconnect if missing. The first pad is player 1 |
| Resolution stays changed | Quit the game or press `Esc` to return to a window |
| The first button press is ignored | Some games ignore input for a few seconds after starting. Wait a moment |
| No auto preview | Check that the system's built-in core is installed and auto preview is on in "Settings". It pauses while the app is in the background |
""";

    private const string RenameEn = """
# How renaming works
The app computes the CRC32 (a fingerprint of the contents) of each ROM and compares it with the DAT file (a list of titles and CRC32 values). Only matching files are renamed to the correct name from the DAT, no matter how they are named now.

> You always review the list before renaming, and "Undo" reverts a whole run afterwards.

# Preparation
## Get a DAT file
Use an OfflineList-format DAT (XML) for each system. Old Shift-JIS DATs are supported.

## Set up the system
Choose the system on the left and set the following in "System settings".

| Item | Description |
| DAT file | The DAT file |
| ROM folders | Folders containing ROMs (separate with `;`) |
| Extensions | Extensions to read (e.g. `.nes;.zip`) |
| Header handling | Whether iNES/FDS headers are included in the CRC32 (usually "Auto") |

Reading and matching start automatically. Unchanged files are skipped from the second time on, so it is quick.

# The Rename tab
Switch to the "Rename" tab at the top to see the table.

| Column | Description |
| ✓ | Whether to rename (only "Rename" rows can be selected) |
| Status | Result of matching (see below) |
| Current file name | The current name |
| → Name in DAT | The name after renaming |
| CRC32 / size / format | ROM information |

## Status
| Status | Meaning | Renamed |
| Match | CRC32 matches and the name is already correct | No need |
| Rename | CRC32 matches but the name differs | Yes |
| Duplicate | A file for the same title already exists | No |
| Not in DAT | The CRC32 is not in the DAT | No |
| Missing | In the DAT, but you do not have the file | ― |

# Renaming step by step
- On the "Rename" tab, review the "Rename" rows (all are checked at first)
- Uncheck rows you do not want to rename. The header checkbox toggles all
- The right side shows "Before" and "After" for the selected row
- Press "Rename" at the bottom and confirm with "Yes"
- The count is shown and the list refreshes automatically

> Only files whose CRC32 matches the DAT are changed. Files are skipped if a file with the new name already exists.

## ROM names inside ZIP files
With "Settings" → "Also rename the ROM inside ZIP files" on, the ROM inside each ZIP is renamed along with the ZIP.

## Characters Windows does not allow
`\` `/` `:` `*` `?` `"` `<` `>` `|` are replaced with full-width look-alikes.

# Undo
"Undo" at the bottom of the "Rename" tab reverts the last run as a whole. Press it again to revert the run before (up to the last 50 runs).

> "Undo" reverts the most recent run regardless of system. The confirmation shows its date and count.

# Handy features
| Feature | How to use |
| Owned / All / Missing | Filters the list. "Missing" shows titles you do not have yet |
| Search | Type in the search box to filter by title or file name |
| Rescan | Press after adding or removing ROMs |
| Hide extensions | "Settings" → "Hide file extensions in the title list" |
| Scan subfolders | "Settings" → "Scan subfolders" |
| Show DAT names only | For arcade sets, where renaming files breaks emulators. Turn it on in "System settings" to keep file names and show DAT names in the list |

# Troubleshooting
| Problem | What to check |
| Nothing matches | Check that the DAT is for the right system. Changing "Header handling" to "Include" or "Exclude" may help |
| "Could not read the DAT file" | Make sure it is an OfflineList (XML) DAT |
| "Duplicate" | You have two or more ROMs for the same title. Move one to another folder |
| Skipped with "(exists)" | A file with the new name already exists. Decide which to keep and sort them by hand |
| The name inside the ZIP did not change | If the ZIP name is already correct, the inner name is left as is |
""";
}
