# Changelog

All notable changes to this project will be documented in this file.

## [1.0.0] - 2025-10-18

### 🎉 Production Ready Release

v1.0.0 は本番リリース版です。18フェーズにわたる段階的な品質向上を経て、**84.8% カバレッジ**（業界標準70-80%を超える）、**915テスト**、**完全なCI/CD自動化**を達成しました。

#### Highlights
- **Coverage**: Line 84.8%, Branch 74.8%, Method 92.4%
- **Tests**: 915 passing, 2 skipped (917 total)
- **Documentation**: ~4,000 lines (QUICKSTART, COVERAGE_ACHIEVEMENT, LESSONS_LEARNED, PROJECT_STATUS, SESSION_SUMMARY)
- **CI/CD**: 4 GitHub Actions workflows (test.yml NEW, coverage-pages.yml, ci.yml, dotnet.yml)
- **Quality Gate**: 75% minimum enforced automatically

#### Core Features Completed
- Diatonic chords (triads & sevenths with inversions)
- Secondary functions (V/x, vii°/x, vii°7/x)
- Borrowed chords (bVI, bVII, bIII, bII with 7th warnings)
- Augmented sixth chords (It6, Fr43, Ger65)
- Neapolitan sixth (bII6 with enforceN6 option)
- Ninth chords (V9 vs V7(9) toggle)
- 6-4 classification (Cadential, Passing, Pedal)
- Key estimation with modulation detection
- Cadence analysis (PAC, IAC, HC, DC, PC)
- CLI with JSON output & schema support

詳細は [RELEASE_NOTES.md](RELEASE_NOTES.md) を参照してください。

## Unreleased

### Changed

- **カバレッジゲートを75%に引き上げ**: 全ワークフロー（CI・Pages）とドキュメントで統一（旧: 70%）
  - `Scripts/CheckCoverage.ps1` の既定閾値を 75.0 に更新
  - README のカバレッジゲートセクションを 75% 標準に更新
  - GitHub Pages ワークフローで `site/coverage/Summary.xml` に対する 75% プレ公開ゲートを適用
  - VS Code タスク: 75%/80% チェックを提供、70% チェックは `[legacy]` にマーク

### Added

- `Scripts/GetLocalCoverage.ps1`: ローカル Summary.xml の検査（Line/Branch/Method/GeneratedOn）
- `Scripts/CheckCoverage.ps1`: 新旧 ReportGenerator フォーマット対応の XmlSummary パース強化
- `Scripts/GetPublicCoverage.ps1`: リトライとキャッシュバスティング機能付き公開カバレッジ取得
- README: Pages プレ公開ゲートとローカル/公開タスクのガイダンスを明記

- HarmonyOptions.IncludeMajInSeventhInversions: ダイアトニックのメジャーセブンス和音における転回表記へ「maj」を含めるオプションを追加（既定: false）。
  - 例: Cメジャー IVmaj7 の転回 → 既定=IV65/IV43/IV42、オプション有効=IVmaj65/IVmaj43/IVmaj42。ルート表記は常に IVmaj7。
  - CLI: `--maj7Inv` で切り替え可能。

- CLI: `--schema`（JSONスキーマ出力）、キー推定オプション `--window`, `--minSwitch`, `--prevBias`, `--initBias`, `--switchMargin`, `--outPenalty` を追加。
- CLI: `--roman` 入力の強化（転回/七の和音/二次/増六のトークン対応は既存、オプションと併用可能）。
- CLI: `--segments`/`--trace`（または `--json`）使用時に推定器オプションの実効値を表示。
- JSON Schema: `Samples/MusicTheory.Cli/schema/music-theory.cli.v1.schema.json` を追加し、ビルド出力にコピー。
- README: CLI JSON出力セクション（スキーマと例）、推定器オプションの例を追記。
- Harmony: カデンツ 6-4（I64→V→I）を V64-53（属機能）として表記するオプションを追加（`HarmonyOptions.PreferCadentialSixFourAsDominant` / CLI `--cad64Dominant`）。
- Harmony: 二次 triad（`V/x`, `vii°/x`）で voicing が与えられた場合に 6/64 の転回サフィックスを付与（例: `V6/ii`, `V64/ii`, `vii°6/V`, `vii°64/V`）。
- README: 上記の二次 triad 転回表記の説明とサンプルコードを追記。
- CLI: `--schema` に `util:dur|roman|pcs` を受け付ける拡張を追加し、ユーティリティ用スキーマの出力にも対応。
- Harmony: 二次属三和音の早期判定（Early disambiguation）。メジャー三和音に厳密一致するセットに対し、根音から一意にターゲット度数を逆算して `V/x` を確定するロジックを追加（曖昧なケースでの誤ラベルを防止）。
- Warm-up: `V/vi` および `V/vii`（各 root/6/64）を含むウォームアップを追加（`LibraryWarmUp` / `RomanInputParser` / `Tests` のアセンブリ初期化）。
- README: V9 表記ポリシー（`PreferV7Paren9OverV9`）の説明と例を追記。
- VS Code Tasks: カバレッジ安定実行用 `dotnet: test (coverage stable)` と連結 `coverage: full stable` を追加（初回のゆらぎ低減）。
- VS Code Tasks: Aug6 と bVI7 の棲み分けを確認する `cli: demo (Aug6 vs bVI7)` を追加。
- VS Code Tasks: 二次導音の図形（`vii°7/V` の 7/65/43/42）を確認する `cli: demo (secondary LT inversions)` を追加。

- Tests: RomanInputParser 回帰テスト `Parses_Secondary_LeadingTone_Seventh_Inversion_BassHints` を追加（Cメジャー `/V` の `vii°7` 各転回で BassPcHint が根/3/5/7 を正しく指すことを検証）。
- README: Aug6 vs bVI7 と二次導音転回デモの想定出力を追記。Windows PowerShell でのスクリプトブロック問題回避として `coverage: full stable (simple)` と `reportgenerator` 単体コマンドの利用を明記。

- Tests: Harmony 回帰テスト `HarmonySecondaryLeadingToneSeventhInversionTests` を追加し、`vii°7/V` の 7/65/43/42 ラベリングを直接検証。
- Warm-up: `LibraryWarmUp` に `vii°7/V; vii°65/V; vii°43/V; vii°42/V` の解析を追加し、初回実行の図形ゆらぎを更に低減。
- VS Code Tasks: `coverage: full stable` 系の出力生成を `coverage: html (simple)` に切り替え、PowerShell スクリプトブロック依存を排除。
- CI: カバレッジ収集を安定化するため `COMPlus_TieredCompilation=0` と `COMPlus_ReadyToRun=0` をテストジョブに設定し、リトライを追加（Windows/Linux 双方）。

- Tests: マイナー iiø7（B–D–F–A in a minor）の全転回（7/65/43/42）を追加し、半減七の表記回帰を強化。
- Tests: 二次導七（Cメジャーの `vii°7/V` = F#–A–C–Eb）の全転回（7/65/43/42）を追加し、二次導七の図形一貫性を回帰テスト化。
- README: 七和音の図形決定と記号付与（7/65/43/42、°/ø重複防止、maj7転回のオプション、二次導音の図形付与）の要点を追記。
- VS Code Tasks: `cli: demo (sevenths inversions)` を追加（V7 と `vii°7/V` の各転回をデモ）。

### Changed

- 二次三和音（V/x・vii°/x）の転回表記の一貫性を強化。voicing からベースが第3/第5音と判定できる場合、三和音であれば `6`/`64` を自動補強（例: V6/ii, V64/ii, vii°6/V, vii°64/V）。
- README: CLI オプション一覧を最新化（`--schema`/推定器オプション/閾値オプション）。
- CLI: 人間可読出力での推定器オプションの表示を追加（`--segments`/`--trace` 時）。
- Harmony: RomanInputParser の二次導音 `/IV`（例: `viiø/iv`, `viiø7/iv`）で B♮ を採用するよう修正（Cメジャーでは E–G–B–D）。テスト `Parses_Secondary_LeadingTone_To_ii_and_iv` がグリーン。
- Harmony: `TryRomanizeSecondaryDominant` の度数走査順よりも前に、三和音一致から `secRoot→targetPc` を一意決定する早期判定を導入（`V/vi` が稀に `V/iii` と出る問題を根治）。
- Harmony: 二次属の三和音/七の一致判定を `SequenceEqual` に変更し、厳密一致のみを許容（部分一致の可能性を排除して決定性を強化）。
- Harmony: 二次導七（`vii°7/V`）の強優先分岐でも、voicing 提供時は 7/65/43/42 の転回図形を付与するように変更。
- README: CLI サンプルに七和音転回デモのコマンドを追記。
- README: HarmonyOptions の `ShowNonCadentialSixFour` の重複記載を統合して整理。
- Build: `LibraryWarmUp` の `ModuleInitializer` に対する CA2255 をファイル内限定で抑止（挙動は変更なし）。

### Fixed

- マイナーにおける IIImaj7 の転回が一部ケースで検知されないことがあった問題を、後段のダイアトニック7th再チェックにより解消。
- Cメジャーで E–G#–B（三和音）が `V/iii` とラベルされることがある問題を修正（`V/vi` に確定）。
- Harmony: 減七/半減七の転回表記を決定的にし、2転（43）で "7" になることがある問題を修正（ベースと根音差分による図形決定を一元化）。
- Harmony: ルート位の七和音で `°`/`ø` 記号が二重付与されることがある問題（例: `vii°°7`, `iiøø7`）を修正。
- RomanInputParser: 5度の検出を強化（6/7/8 を許容）。減五度/増五度を含む和音で `43` 図形が失われるケースを修正（例: `vii°43/V` が `vii°7/V` になることがある）。
- Coverage Tasks: Windows PowerShell で `coverage: html` が `UnexpectedToken '}'` で失敗する問題の回避（simple 版への切替）。

### Internal

- Tests: 追加テスト（モジュレーション閾値全除外時のフォールバック）を含め安定。Release でビルド/テスト確認。
- Tests: `SixFourNoPassingOnCadenceTests.cs` の不要なコードフェンス混入を修正（断続的 CS1529 の恒久対処）。
- CLI: util スキーマ（dur/roman/pcs）をビルド出力に同梱（CopyToOutputDirectory=PreserveNewest）。
- Tests: 二次三和音（`V/vi`・`V/vii`・`vii°/ii`）の root/6/64 転回サフィックス補強テストを追加。
- Tests: アセンブリ初期化で二次 triad（`V/vi` と `V/vii`）のウォームアップを実行し、初回/カバレッジ実行の安定性を向上。
- Dev Experience: カバレッジ安定タスクを追加し、CI/ローカルの初回動作の一貫性を改善。
