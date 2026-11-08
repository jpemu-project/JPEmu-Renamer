# JPEmu Renamer

自分で吸い出したROMを **管理・リネーム・起動** まで1本で扱う、Windows用のエミュレーターランチャーです。
A Windows emulator launcher that manages, renames (by CRC32 against OfflineList DATs) and launches your own ROM dumps.

## 現在のバージョン（0.1.0）でできること

- **ROM管理**：機種ごとにROMフォルダとDATを登録し、CRC32を計算して照合（ZIP内のROM、iNES/FDSヘッダー込み・除外の両方に対応）
- **未所有の表示**：DATにあって手元に無いタイトルも一覧に表示（所有のみ／すべて／未所有のみ）
- **リネーム**：CRC32が一致したファイルだけをDATの名前に変更。ZIP内のROM名も変更可。実行前に一覧で確認、「元に戻す」で1回分を丸ごと戻せる
- **内蔵コアで起動**：libretroコアをアプリ内で動かして遊べる（外部エミュレーター不要）
  - コアは機種ごとに候補から選び、libretro公式の配布サイトから利用者の操作でダウンロード（アプリには同梱しない）
  - ウィンドウ／全画面、表示サイズ（等倍〜3倍・画面に合わせる）、縦横比、整数倍拡大、補間、走査線・CRT風シェーダー、垂直同期
  - キーボードとゲームパッド（SDL2：XInput・DirectInput・Switch Proコントローラー・DualSenseなど）、プレイヤー1〜4、連射、振動、ホットキー
  - バッテリーバックアップの自動保存、ステートセーブ（スロット切り替え）、スクリーンショット、早送り
  - プレイ中のメニュー（F1）から設定を変えるとその場で反映。設定は「全体→機種→ゲーム」の順に上書きでき、コア固有の設定も変更可
  - OpenGLで描画するコア（N64など）にも対応
- **外部exeで起動**：機種ごとに外部exe（エミュレーター）と引数（`{rom}` `{romdir}` `{romname}`）を指定して起動。右クリックで起動方法を切り替え可
- **表示**：日本語／英語、表示倍率75〜200%（Ctrl＋マウスホイール）、文字サイズ変更、ウィンドウ位置の記憶
- **機種アイコン**：オリジナル線画9種＋色の変更、自作アイコン画像（PNG/ICO/JPG）

次のバージョン以降の予定：自動プレビュー、スクショ自動作成、一括解凍・圧縮、機種追加ウィザードとチュートリアル。

## 使い方

1. [Releases](../../releases) または Actions の成果物から `JPEmuRenamer-win-x64.zip` をダウンロードして展開
2. `JPEmuRenamer.exe` を起動（インストール不要）
3. 「機種の設定」でDATファイル（OfflineList形式）とROMフォルダを指定
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
