# MusicTheory - Quick Reference

## 📁 プロジェクト構造

```
MusicTheory/
├── .github/              # GitHub Actions ワークフローとプロジェクト追跡
│   ├── workflows/        # CI/CD 設定 (ci.yml, coverage-pages.yml, dotnet.yml)
│   └── copilot-instructions.md
├── .vscode/              # VS Code タスクと設定
│   └── tasks.json        # ビルド/テスト/カバレッジ/CLI デモタスク
├── Benchmarks/           # パフォーマンスベンチマーク
├── Docs/                 # ドキュメント
│   ├── UserGuide.md
│   └── PAC-and-LeadingTone.md
├── NoteValue/            # 音価関連クラス
├── Samples/              # サンプルアプリケーション
│   ├── MusicTheory.Cli/  # CLI ツール (JSON出力、スキーマ、ユーティリティ)
│   └── NoteValueZoom.Wpf/# WPF デモアプリ
├── Scale/                # スケール関連
├── Scripts/              # PowerShell スクリプト
│   ├── CheckCoverage.ps1       # カバレッジ閾値チェック (デフォルト75%)
│   ├── GetLocalCoverage.ps1    # ローカルカバレッジ取得
│   └── GetPublicCoverage.ps1   # 公開カバレッジ取得
├── Tests/                # ユニットテスト
│   └── MusicTheory.Tests/
├── Theory/               # コアライブラリ
│   ├── Analysis/         # 和音解析
│   ├── Chord/            # コード定義
│   ├── Harmony/          # 和声解析（Roman Numeral, カデンツ, モジュレーション）
│   ├── Interval/         # インターバル
│   ├── Pitch/            # ピッチクラス
│   ├── Scale/            # スケール
│   └── Time/             # 音価、Duration
├── CHANGELOG.md          # 変更履歴
├── README.md             # メインドキュメント
├── ROADMAP.md            # プロジェクトロードマップ
└── MusicTheory.sln       # ソリューションファイル
```

## 🚀 よく使うコマンド

### ビルドとテスト

```powershell
# ビルド
dotnet build -c Release

# テスト実行
dotnet test -c Release --nologo

# カバレッジ付きテスト
dotnet test -c Release --collect:"XPlat Code Coverage" --results-directory Tests/MusicTheory.Tests/TestResults
```

### カバレッジ確認

```powershell
# ローカルカバレッジ確認
.\Scripts\GetLocalCoverage.ps1

# 公開カバレッジ確認
.\Scripts\GetPublicCoverage.ps1

# カバレッジゲートチェック (75%)
.\Scripts\CheckCoverage.ps1 -Threshold 75
```

### CLI サンプル

```powershell
# 基本的な Roman 入力
dotnet run --project Samples/MusicTheory.Cli -c Release -- --roman "I; V; I"

# JSON 出力
dotnet run --project Samples/MusicTheory.Cli -c Release -- --roman "I; V; I" --json

# スキーマ出力
dotnet run --project Samples/MusicTheory.Cli -c Release -- --schema

# ピッチクラス入力
dotnet run --project Samples/MusicTheory.Cli -c Release -- --pcs "0,4,7; 7,11,2; 0,4,7"

# モジュレーション解析
dotnet run --project Samples/MusicTheory.Cli -c Release -- --pcs "0,4,7; 7,11,2; 0,4,7; 2,6,9,0; 7,11,2" --segments --preset stable
```

## 🎯 VS Code タスク（主要）

### ビルド・テスト

- `dotnet: build` - Release ビルド
- `dotnet: test` - テスト実行
- `dotnet: test (no build)` - ビルドなしテスト実行

### カバレッジ

- `coverage: full warm stable (simple)` - 安定版カバレッジ生成（build → test → coverage → html → summary → badges）
- `coverage: open` - ローカルカバレッジレポートをブラウザで開く
- `coverage: check (75%)` - 75%ゲートチェック

### CLI デモ

- `cli: demo (roman)` - 基本的なRoman解析
- `cli: demo (mixture 7th)` - 借用7th和音デモ
- `cli: demo (cadential 6-4)` - カデンツ6-4デモ
- `cli: demo (V9)` / `cli: demo (V7(9))` - V9表記切替デモ
- `cli: demo (modulation C->G preset stable)` - モジュレーション解析

### JSON出力

- `cli: json (roman demo)` - Roman入力のJSON出力
- `cli: json (modulation preset stable)` - モジュレーション解析JSON

### ユーティリティ

- `cli: util (romanJson demo)` - Roman文字列のパースJSON
- `cli: util (pcsJson demo)` - ピッチクラスのパースJSON
- `cli: schema (main)` - メインスキーマ出力
- `cli: schema (util:roman)` - Romanユーティリティスキーマ

## 📊 品質メトリクス（現在）

| 指標 | 値 | 目標 | 状態 |
|------|-----|------|------|
| **Line Coverage** | 80.5% | ≥75% | ✅ PASS |
| **Branch Coverage** | 69.2% | - | 📊 |
| **Method Coverage** | 73.3% | - | 📊 |
| **テスト合格** | 277 | - | ✅ |
| **テストスキップ** | 1 | - | 🔵 |
| **C# ファイル** | 142 | - | 📁 |
| **総行数** | 13,771 | - | 📝 |

## 🔗 重要なURL

- **GitHub リポジトリ**: https://github.com/majiros/MusicTheory
- **GitHub Actions**: https://github.com/majiros/MusicTheory/actions
- **カバレッジページ**: https://majiros.github.io/MusicTheory/coverage/index.html
- **カバレッジランディング**: https://majiros.github.io/MusicTheory/

## 🎓 学習パス

### 初心者向け

1. `Docs/UserGuide.md` を読む
2. `cli: demo (roman)` タスクを実行して基本動作を確認
3. README の「クイックスタート」セクションを試す

### 中級者向け

1. README の各セクション（HarmonyOptions, 6-4分類, Augmented Sixth等）を読む
2. CLI JSON出力でスキーマを理解
3. テストコードを参照して使用例を学ぶ

### 上級者向け

1. `Theory/Harmony/` 以下のソースコードを読む
2. `Docs/PAC-and-LeadingTone.md` で設計思想を理解
3. ROADMAP.md で将来の方向性を把握
4. コントリビューション（テスト追加、ドキュメント改善等）

## 🐛 トラブルシューティング

### テストが失敗する

```powershell
# クリーンビルド
dotnet clean
dotnet build -c Release

# リトライ付きテスト
# VS Code タスク: "dotnet: test (retry)" を使用
```

### カバレッジが取得できない

```powershell
# テスト結果ディレクトリをクリア
Remove-Item -Recurse -Force Tests/MusicTheory.Tests/TestResults

# カバレッジ再生成
# VS Code タスク: "coverage: full warm stable (simple)" を使用
```

### ファイルロックエラー

```powershell
# VS Codeを再起動するか、プロセスを確認
Get-Process | Where-Object { $_.Path -like "*MusicTheory*" }
```

## 📝 コミット規約

```
<type>: <subject>

<body>

<footer>
```

### Type
- `feat`: 新機能
- `fix`: バグ修正
- `docs`: ドキュメントのみ
- `style`: コードの意味に影響しない変更（空白、フォーマット等）
- `refactor`: リファクタリング
- `perf`: パフォーマンス改善
- `test`: テスト追加・修正
- `chore`: ビルドプロセスやツールの変更

### 例

```
feat: add Neapolitan sixth chord support

- Implement bII6 recognition with enforceN6 option
- Add CLI --enforceN6 flag
- Update tests and documentation

Closes #123
```

## 🔧 開発ワークフロー

### 新機能追加

1. ブランチ作成: `git checkout -b feat/feature-name`
2. 実装 + テスト追加
3. ローカルでテスト実行: `dotnet test -c Release`
4. カバレッジ確認: `.\Scripts\GetLocalCoverage.ps1`
5. コミット: `git commit -m "feat: add feature"`
6. プッシュ: `git push origin feat/feature-name`
7. Pull Request 作成
8. CI通過確認
9. マージ

### バグ修正

1. 再現テストを追加
2. 修正実装
3. テスト確認
4. コミット: `git commit -m "fix: resolve issue"`

### ドキュメント更新

1. 修正実施
2. マークダウンリント確認: `markdownlint README.md`
3. コミット: `git commit -m "docs: update README"`

## 🎯 今後の主要タスク（優先順位順）

### Phase 1: 品質向上（短期）

1. カバレッジ85%達成
2. 未カバー領域の特定と優先順位付け
3. ドキュメント完成度90%達成

### Phase 2: 機能拡張（中期）

1. 声部進行規則の詳細チェック実装
2. モジュレーション推定の精緻化
3. Non-functional harmony サポート

### Phase 3: エコシステム（長期）

1. NuGet パッケージ公開
2. 楽譜制作ソフト開発（別リポジトリ）
3. Web API サービス構築

---

**Last Updated**: 2025年10月4日  
**Version**: 開発中  
**Maintainer**: majiros
