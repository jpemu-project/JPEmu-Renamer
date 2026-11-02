# JPEmu Renamer

自分で吸い出したROMを管理・リネーム・起動するWindows用エミュレーターランチャー。C# / WPF / .NET 8（MVVMはCommunityToolkit.Mvvm）。機能と今後の予定は [README.md](README.md) を参照。

## 資料

- 仕様書: https://claude.ai/artifact/A4aDEdPFG8s1gg2Kt6JbfE
- UIイメージ: https://claude.ai/artifact/1suURn3hYcjR2uTRk2v7WE

## ビルド

```
dotnet publish src/JPEmuRenamer/JPEmuRenamer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

- テストプロジェクトはない。
- ローカルに.NET 8 SDKがなければ、GitHub Actions（`.github/workflows/build.yml`）のビルド結果で確認する。pull requestと`main`へのpushでビルドされ、成果物は`JPEmuRenamer-win-x64.zip`。
- `v*`タグを付けるとReleasesにzipが添付される。

## 構成（`src/JPEmuRenamer/`）

- `Models/`: データと設定（`GameItem`、`Settings`）
- `Services/`: DAT解析、ROM走査、CRC32照合、リネーム、保存、表示文言（`Loc`）
- `ViewModels/`: `MainViewModel`、`SystemItem`
- `Views/`、`Ui/`: 画面、テーマ、アイコン、コンバーター

## 規約

- 各ソースの先頭に `// JPEmu Renamer - SPDX-License-Identifier: GPL-3.0-or-later` を置く。ライセンスはGPL-3.0-or-later。
- コメントとコミットメッセージは日本語。
- 画面の文言は`Services/Loc.cs`で管理する。日本語と英語の両方に対応している。
- `System.IO`は`GlobalUsings.cs`でグローバルusingしている。`System.Windows.Shapes.Path`と紛らわしいので、コードでは`System.IO.Path`と明記する。

## データの扱い

- リネームはCRC32が一致したファイルだけを対象にし、実行前に一覧で確認でき、「元に戻す」で1回分を丸ごと戻せる。この挙動を崩さない。利用者のROMを壊す変更は避ける。
- 設定は`%APPDATA%\JPEmuRenamer`に保存する。exeと同じフォルダに`portable.txt`があれば、そのフォルダの`data\`に保存する。
