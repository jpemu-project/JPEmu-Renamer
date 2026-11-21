# JPEmu Renamer

自分で吸い出したROMを **管理・リネーム・起動** まで1本で扱う、Windows用の無料・ポータブルなエミュレーターランチャーです。
A free, portable Windows emulator launcher that manages, renames (by CRC32 against No-Intro / OfflineList DATs) and plays your own ROM dumps with built-in libretro cores.

- 公式サイト / Website：**https://jpemu-project.github.io/**
- ダウンロード / Download：[最新版 / Latest release](../../releases/latest)（`JPEmuRenamer-win-x64.zip`、インストール不要）
- 日本語DAT / Japanese DATs：[公式サイトで配布](https://jpemu-project.github.io/#download)

## 最新版（1.5）でできること

- **105機種**に対応（ファミコンからプレイステーション2・ゲームキューブ・Wii・ドリームキャスト・PSP、アタリ・3DO・PC-FX、MSX・PC-8801・PC-9801・X68000・FM TOWNS などのパソコンまで）。表示は**16言語**、テーマは6種類
- **ROM管理**：機種ごとにROMフォルダとDAT（OfflineList形式、または No-Intro などの clrmamepro 形式。ZIPのままでも可）を登録し、CRC32を計算して照合（ZIP内のROM、iNES/FDSヘッダー込み・除外の両方に対応）
- **未所有の表示**：DATにあって手元に無いタイトルも一覧に表示（すべて／所有のみ／未所有のみ）。左の機種一覧に所有／未所有の本数を表示
- **並び替え**：あいうえお順・発売日順・メーカー順・ジャンル順・価格順・状態順。発売地域の国旗も表示
- **リネーム**：CRC32が一致したファイルだけをDATの名前に変更。ZIP内のROM名も変更可。実行前に一覧で確認、「元に戻す」で1回分を丸ごと戻せる。DATに無いファイルや重複ファイルは、指定したフォルダへ移動できる
- **自動プレビュー**：タイトルを選ぶと、テレビ風の画面でゲームの冒頭を早送りで流す。自分で撮ったスクリーンショットも表示できる
- **ツール**：ゲームリストの保存（全部／所有／未所有）、全ROMの一括解凍・圧縮、内蔵コアの一括ダウンロード・更新
- **内蔵コアで起動**：libretroコアをアプリ内で動かして遊べる（外部エミュレーター不要）
  - コアは機種ごとに候補から選び、libretro公式の配布サイトから利用者の操作でダウンロード（アプリには同梱しない）
  - ウィンドウ／全画面、表示サイズ（等倍〜3倍・画面に合わせる）、縦横比、整数倍拡大、補間、走査線・CRT風シェーダー、垂直同期
  - キーボードとゲームパッド（SDL2：XInput・DirectInput・Switch Proコントローラー・DualSenseなど）、プレイヤー1〜4、連射、振動、ホットキー
  - バッテリーバックアップの自動保存、ステートセーブ（スロット切り替え）、スクリーンショット、早送り
  - プレイ中のメニュー（F1）から設定を変えるとその場で反映。設定は「全体→機種→ゲーム」の順に上書きでき、コア固有の設定も変更可
  - OpenGLで描画するコア（N64など）にも対応
- **外部exeで起動**：機種ごとに外部exe（エミュレーター）と引数（`{rom}` `{romdir}` `{romname}`）を指定して起動。右クリックで起動方法を切り替え可
- **表示**：16言語（日本語・英語ほか）、6種類のテーマ、表示倍率75〜200%（Ctrl＋マウスホイール）、文字サイズ変更、ウィンドウ位置の記憶
- **機種アイコン**：オリジナル線画9種＋色の変更、自作アイコン画像（PNG/ICO/JPG）
- **パソコン系の機種**：キーボードをそのままゲームに送る（メニューは F12）。Amiga・X68000・PC-9801 などでは、ゲーム画面をクリックするとマウスも使える。ディスクが2枚以上のゲームは、メニューから入れ替えられる
- **ポータブル**：設定やセーブはexeの横の `data\` に保存。フォルダごとUSBメモリに入れて持ち運べる
- **ヘルプ**：アプリ内に使い方の説明（F1）

日本で発売されたゲームのタイトルを日本語で収めたDATを[公式サイト](https://jpemu-project.github.io/)で配布しています。ダウンロードして、「機種の設定」でDATとして指定してください。

## 使い方

1. [Releases](../../releases) または Actions の成果物から `JPEmuRenamer-win-x64.zip` をダウンロードして展開
2. `JPEmuRenamer.exe` を起動（インストール不要）
3. 「機種の設定」でDATファイル（OfflineList形式、または No-Intro などの clrmamepro 形式）とROMフォルダを指定
4. 「リネーム」タブで照合結果を確認し、「リネームを実行」
5. 「プレイ」タブでタイトルを選んで「起動」。初めて使うコアは、その場でダウンロードするか確認します

BIOSが必要な機種（ディスクシステム、メガCD、PCエンジンCD-ROM²、ネオジオなど）は、プレイ設定の「BIOSフォルダ」（既定は設定フォルダの `system`）にBIOSファイルを置いてください。

設定やコア、セーブデータは、exeと同じフォルダの `data\` に保存されます。フォルダごとUSBメモリなどに入れて持ち運べます（ポータブル版）。exeのフォルダの中にあるROMやDATなどの場所は相対パスで覚えるので、USBメモリのドライブ文字が変わってもそのまま使えます。1.1.0 までの `%APPDATA%\JPEmuRenamer` の設定は、初めて起動したときに自動で引き継ぎます（元のフォルダは残ります）。exeのフォルダに書き込めないとき（Program Files など）は、これまでどおり `%APPDATA%\JPEmuRenamer` を使います。

## ビルド

.NET 8 SDK（Windows）で次を実行します。

```
dotnet publish src/JPEmuRenamer/JPEmuRenamer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

`main` へのpushで GitHub Actions が自動ビルドし、`v*` のタグを付けると Releases にzipを添付します。

## ライセンス

GPL-3.0-or-later（[LICENSE](LICENSE)）。

利用しているライブラリ：CommunityToolkit.Mvvm（MIT）、System.IO.Hashing（MIT）、NAudio（MIT）、Silk.NET（MIT）、SDL2（zlib）。
内蔵コア（libretroコア）はアプリに含まれず、利用者がダウンロードしたものを使います。各コアのライセンスはそれぞれのコアに従います。
アプリアイコンとロゴの画像は、Dancing Script（SIL Open Font License 1.1）で描いた文字を画像化したものです（`tools/make_icon.py`）。
