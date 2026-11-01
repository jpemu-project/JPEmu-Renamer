# JPEmu Renamer

自分で吸い出したROMを **管理・リネーム・起動** まで1本で扱う、Windows用のエミュレーターランチャーです。
A Windows emulator launcher that manages, renames (by CRC32 against OfflineList DATs) and launches your own ROM dumps.

## 現在のバージョン（0.1.0）でできること

- **ROM管理**：機種ごとにROMフォルダとDATを登録し、CRC32を計算して照合（ZIP内のROM、iNES/FDSヘッダー込み・除外の両方に対応）
- **未所有の表示**：DATにあって手元に無いタイトルも一覧に表示（所有のみ／すべて／未所有のみ）
- **リネーム**：CRC32が一致したファイルだけをDATの名前に変更。ZIP内のROM名も変更可。実行前に一覧で確認、「元に戻す」で1回分を丸ごと戻せる
- **起動**：機種ごとに外部exe（エミュレーター）と引数（`{rom}` `{romdir}` `{romname}`）を指定して起動
- **表示**：日本語／英語、表示倍率75〜200%（Ctrl＋マウスホイール）、文字サイズ変更、ウィンドウ位置の記憶
- **機種アイコン**：オリジナル線画9種＋色の変更、自作アイコン画像（PNG/ICO/JPG）

次のバージョン以降の予定：内蔵コア（libretro）での起動、自動プレビュー、スクショ自動作成、一括解凍・圧縮、機種追加ウィザードとチュートリアル。

## 使い方

1. [Releases](../../releases) または Actions の成果物から `JPEmuRenamer-win-x64.zip` をダウンロードして展開
2. `JPEmuRenamer.exe` を起動（インストール不要）
3. 「機種の設定」でDATファイル（OfflineList形式）とROMフォルダを指定
4. 「整理（管理表）」で照合結果を確認し、「リネームを実行」

設定は `%APPDATA%\JPEmuRenamer` に保存されます。exeと同じフォルダに空の `portable.txt` を置くと、そのフォルダの `data\` に保存します（ポータブル運用）。

## ビルド

.NET 8 SDK（Windows）で次を実行します。

```
dotnet publish src/JPEmuRenamer/JPEmuRenamer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

`main` へのpushで GitHub Actions が自動ビルドし、`v*` のタグを付けると Releases にzipを添付します。

## ライセンス

GPL-3.0-or-later（[LICENSE](LICENSE)）。

利用しているライブラリ：CommunityToolkit.Mvvm（MIT）、System.IO.Hashing（MIT）。
アプリアイコンとロゴの画像は、Dancing Script（SIL Open Font License 1.1）で描いた文字を画像化したものです（`tools/make_icon.py`）。
