# 次回セッション開始時のチェックリスト

## 📋 セッション開始時の確認事項

### 1. GitHub Actions 状態確認

```powershell
# ブラウザでActions ページを開く
Start-Process "https://github.com/majiros/MusicTheory/actions"
```

**確認項目:**

- [ ] CI ワークフロー（ci.yml）が成功しているか
- [ ] .NET CI ワークフロー（dotnet.yml）が成功しているか
- [ ] Coverage Pages ワークフロー（coverage-pages.yml）が成功しているか
- [ ] すべてのワークフローが75%ゲートをPASSしているか

### 2. 公開カバレッジ確認

```powershell
# 公開カバレッジページを開く
Start-Process "https://majiros.github.io/MusicTheory/coverage/index.html"

# 公開カバレッジ数値を取得
.\Scripts\GetPublicCoverage.ps1
```

**期待値:**

- Line Coverage: **80.5%以上** (現在ローカル: 80.5%)
- Branch Coverage: 69.2%前後
- Method Coverage: 73.3%前後
- Generated On: 最新日時

**注意:** 公開カバレッジが73.5%のままの場合、ワークフローが未実行またはゲート未達の可能性があります。

### 3. ローカル環境確認

```powershell
# Git状態確認
git status
git log --oneline -5

# 最新をプル
git pull origin main

# ローカルカバレッジ確認
.\Scripts\GetLocalCoverage.ps1
```

**期待される状態:**

- Working tree clean
- 最新コミット: `9b88d04` または それ以降
- ローカルカバレッジ: 80.5%

### 4. ビルド・テスト状態確認

```powershell
# クリーンビルド
dotnet clean
dotnet build -c Release

# テスト実行
dotnet test -c Release --nologo
```

**期待される結果:**

- ビルド: 成功（0警告、0エラー）
- テスト: 277 passed, 1 skipped, 0 failed

---

## 🎯 次のフェーズのタスク候補

### Phase 1: 品質向上（優先度: 高）

#### カバレッジ85%達成

```powershell
# 未カバー領域の特定
dotnet test -c Release --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage-report -reporttypes:Html

# レポートを開く
Start-Process "Tests/MusicTheory.Tests/TestResults/coverage-report/index.html"
```

**アクション:**

1. レポートで赤色（未カバー）領域を特定
2. カバレッジ価値の高い順に優先順位付け
3. テストケース追加計画を策定

#### 未カバー領域の分析

**主要な候補（推測）:**

- MidiConductorHelper (0% coverage)
- QuantizePresets (0% coverage)
- TimeSignature (0% coverage)
- 一部の Pitch/Accidental クラス（低カバレッジ）

**目標:**

- [ ] 各クラスのカバレッジを50%以上に
- [ ] プロジェクト全体で85%達成

### Phase 2: ドキュメント改善（優先度: 中）

#### API リファレンス生成

```powershell
# DocFX インストール（初回のみ）
dotnet tool install -g docfx

# ドキュメント生成
docfx init -q
docfx docfx.json
```

**アクション:**

- [ ] DocFX セットアップ
- [ ] XML ドキュメントコメント追加
- [ ] API リファレンス生成
- [ ] GitHub Pages で公開

#### チュートリアル追加

**候補トピック:**

- [ ] 「初めての和声解析」（段階的ガイド）
- [ ] 「CLI ツールの活用」（実践例）
- [ ] 「カスタムHarmonyOptionsの設定」
- [ ] 「モジュレーション解析の読み方」

### Phase 3: 機能拡張（優先度: 中）

#### 声部進行規則の詳細実装

**候補実装:**

- [ ] 平行5度/8度の検出強化
- [ ] 隠伏5度/8度の検出
- [ ] 声域違反の詳細チェック
- [ ] 声部交差の検出

**実装手順:**

1. `Theory/Harmony/VoiceLeadingRules.cs` を拡張
2. テストケース追加（`Tests/MusicTheory.Tests/VoiceLeading*.cs`）
3. CLI オプション追加（`--strictVoiceLeading`）
4. JSON 出力に警告追加

#### モジュレーション推定の精緻化

**候補改善:**

- [ ] 調性確信度のスコアリング
- [ ] 転調タイプの分類（直接/準備/突然）
- [ ] ピボット和音の特定
- [ ] 実サンプル収集と閾値調整

---

## 🔧 開発環境セットアップ（新しいマシンの場合）

### 必須ツール

```powershell
# .NET 8 SDK インストール確認
dotnet --version  # 8.0.x 期待

# ReportGenerator インストール
dotnet tool install -g dotnet-reportgenerator-globaltool

# VS Code 拡張機能
# - C# Dev Kit
# - C# Extensions
# - Markdown All in One
# - markdownlint
```

### プロジェクトセットアップ

```powershell
# リポジトリクローン
git clone https://github.com/majiros/MusicTheory.git
cd MusicTheory

# 依存関係復元
dotnet restore

# ビルド
dotnet build -c Release

# テスト実行
dotnet test -c Release

# カバレッジ生成
# VS Code タスク: "coverage: full warm stable (simple)" を実行
```

---

## 📚 参考資料

### プロジェクトドキュメント

- `README.md` - メインドキュメント（82KB、包括的）
- `ROADMAP.md` - プロジェクトロードマップ
- `QUICK_REFERENCE.md` - 開発者向けクイックガイド
- `CHANGELOG.md` - 変更履歴
- `Docs/UserGuide.md` - ユーザーガイド
- `Docs/PAC-and-LeadingTone.md` - PAC設計詳細

### 重要なリンク

- リポジトリ: <https://github.com/majiros/MusicTheory>
- Actions: <https://github.com/majiros/MusicTheory/actions>
- カバレッジ: <https://majiros.github.io/MusicTheory/coverage/>

### VS Code タスク（よく使うもの）

- `dotnet: build` - Release ビルド
- `dotnet: test` - テスト実行
- `coverage: full warm stable (simple)` - カバレッジ生成
- `coverage: open` - ローカルレポート表示
- `cli: demo (roman)` - CLI デモ実行

---

## ⚠️ 既知の注意点

### カバレッジ収集の注意

- **初回実行の揺らぎ**: 初回実行でプリセット比較テストが稀に失敗することがあります
- **推奨手順**: `dotnet: build` → `dotnet: test (no build)` の順で実行
- **安定版タスク**: `coverage: full warm stable (simple)` を使用

### ファイルロックエラー

- **原因**: テスト実行中のDLLロック
- **対処**: VS Code 再起動または `dotnet clean` 実行

### 公開カバレッジの遅延

- **原因**: GitHub Actions ワークフローの実行時間（5-10分）
- **対処**: `.\Scripts\GetPublicCoverage.ps1` で定期的に確認

---

## 🎯 今回のセッションで達成したこと（振り返り）

### 実装完了項目

- ✅ 75%カバレッジゲート実装（全ワークフロー）
- ✅ Scripts/GetLocalCoverage.ps1 作成
- ✅ ROADMAP.md 作成（プロジェクト計画）
- ✅ QUICK_REFERENCE.md 作成（開発者ガイド）
- ✅ CHANGELOG.md 更新（75%ゲート履歴）
- ✅ copilot-instructions.md 更新（進捗追跡）

### Git コミット

```text
9b88d04 Add QUICK_REFERENCE.md for developer convenience
5ba4912 Add ROADMAP.md and update project tracking
ecc819a Update CHANGELOG and copilot-instructions: 75% coverage gate complete
a62b4e5 Upgrade coverage gate to 75% across all workflows and docs
```

### 品質メトリクス（達成）

- Line Coverage: **80.5%** ✅ (ゲート: ≥75%, +5.5%)
- Tests: 277 passed, 1 skipped ✅
- Build: PASS ✅
- Documentation: 包括的 ✅

---

## 📝 次回セッションの目標候補

### Option A: 品質向上に注力

**目標**: カバレッジ85%達成

1. 未カバー領域の分析
2. 優先順位付け
3. テストケース追加（10-15個）
4. カバレッジ再測定

**期待期間**: 1-2セッション

### Option B: ドキュメント改善

**目標**: ドキュメント完成度90%

1. API リファレンス生成
2. チュートリアル追加（2-3本）
3. FAQ セクション追加
4. コード例の拡充

**期待期間**: 2-3セッション

### Option C: 機能拡張

**目標**: 声部進行規則の詳細実装

1. 平行5度/8度検出
2. 隠伏5度/8度検出
3. CLI オプション追加
4. テスト追加

**期待期間**: 2-4セッション

---

**Last Updated**: 2025年10月4日 19:10 JST  
**Next Session**: TBD  
**Current State**: ✅ All systems ready, awaiting GitHub Actions completion
