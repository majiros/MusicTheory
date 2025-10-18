# MusicTheory プロジェクト ロードマップ

## 📍 現在の状態: v1.0.0 (Released 2025-10-18)

### ✅ 完了済み機能（v1.0.0）

#### コア機能
- ✅ Pitch / Interval / Scale / Chord モデル
- ✅ Roman Numeral 解析と Tonal Function（I/ii/IV/V → T/S/D）
- ✅ Triad 転回形（6/64）と Seventh 転回形（7/65/43/42）
- ✅ 二次機能（V/x, vii°/x, vii°7/x）の完全サポート
- ✅ 増六和音（It6, Fr43, Ger65）
- ✅ Neapolitan（bII, bII6）with enforceN6 オプション
- ✅ 借用和音（bVI, bVII, bIII, bII）+ 7th アドバイザリ警告（bVI7, bII7, iv7）
- ✅ 九の和音（V9 vs V7(9) 表記切替）
- ✅ Six-four 分類（Cadential, Passing, Pedal）+ hide64 オプション
- ✅ カデンツ解析（PAC/IAC/Half/Deceptive/Plagal）
- ✅ モジュレーション推定（stable/permissive プリセット）

#### CLI & JSON
- ✅ CLI ツール with 決定的JSON出力
- ✅ JSON スキーマ（main + util:roman/dur/pcs）
- ✅ 警告/エラーシステム（warnings/errors フィールド）
- ✅ Roman入力パーサー（転回/7th/二次/増六対応）
- ✅ 20+ CLI オプション（--help で確認可能）

#### 品質保証（v1.0.0認証済み）
- ✅ **カバレッジ: 84.8%** Line, 74.8% Branch, 92.4% Method
- ✅ **テスト: 915 passed**, 2 skipped (917 total)
- ✅ **カバレッジゲート: ≥75%** (PASS by +9.8 points ✅)
- ✅ CI/CD: 4 GitHub Actions workflows (test.yml NEW)
- ✅ GitHub Pages: 公開カバレッジレポート
- ✅ **ドキュメント: ~4,000行**
  - QUICKSTART.md: 5分間開発者ガイド
  - COVERAGE_ACHIEVEMENT.md: Phase 1-18 達成レポート
  - LESSONS_LEARNED.md: テスト戦略知見
  - PROJECT_STATUS.md: 本番認証レポート
  - SESSION_SUMMARY.md: セッション成果
  - RELEASE_NOTES.md: v1.0.0 リリースノート

#### インフラ
- ✅ VS Code タスク（ビルド/テスト/カバレッジ/デモ）40+
- ✅ カバレッジスクリプト（ローカル/公開取得）
- ✅ ベンチマークサンプル（BenchmarkDotNet）
- ✅ WPF サンプル（NoteValueZoom）
- ✅ GitHub Release: v1.0.0 公開済み

### 📊 プロジェクト統計（v1.0.0）

- **C# ファイル**: 142個
- **総コード行数**: ~14,000行
- **テストカバレッジ**: 84.8% (Line), 74.8% (Branch), 92.4% (Method)
- **ユニットテスト**: 917件（915合格, 2スキップ）
- **カバレッジゲート**: ≥75% (PASS ✅ +9.8 points)
- **ドキュメント**: ~4,000行
- **リリースステータス**: 🟢 PRODUCTION READY

---

## 🎯 v1.1.0 - Quality & Developer Experience Enhancement

**Target Release**: Q1 2026 (3-4 months)  
**Focus**: Integration testing, architecture documentation, performance optimization

### 統合テスト基盤（Integration Testing Foundation）

**Goal**: 85%+ coverage through integration tests

#### Scope
- [ ] **HarmonyAnalyzer Integration Tests**
  - Full progression analysis (I-IV-V-I, ii-V-I, etc.)
  - Multi-chord sequences with modulation
  - Real-world chord progressions from popular music
  
- [ ] **ProgressionAnalyzer Scenario Tests**
  - Common progressions: C→Am→F→G, I-vi-IV-V
  - Jazz progressions: ii-V-I, iii-vi-ii-V-I
  - Modal interchange scenarios
  
- [ ] **End-to-End CLI Tests**
  - JSON output validation for complex inputs
  - Schema compliance verification
  - Error handling for invalid inputs

#### Expected Impact
- **Coverage gain**: +0.5-1.0% (target: 85.0-85.5%)
- **Test count**: +30-50 integration tests (~945-965 total)
- **Confidence**: Higher confidence in multi-component interactions

### アーキテクチャドキュメント（Architecture Documentation）

**Goal**: Visual documentation for system understanding

#### Deliverables
- [ ] **System Architecture Diagram**
  - Component relationships (Theory, Harmony, Analysis, CLI)
  - Data flow visualization
  - Module dependencies
  
- [ ] **Harmony Analysis Pipeline Diagram**
  - Chord identification → Degree assignment → Roman numeral generation
  - Key estimation → Segmentation → Modulation detection
  
- [ ] **Class Hierarchy Diagrams**
  - Chord types hierarchy
  - Analyzer architecture
  - Parser relationships

#### Tools
- Mermaid diagrams in markdown
- PlantUML for detailed class diagrams
- Architecture Decision Records (ADRs)

### パフォーマンス最適化（Performance Optimization）

**Goal**: Reduce analysis time by 20-30%

#### Target Areas
- [ ] **Chord Analysis Caching**
  - Cache frequently analyzed chord patterns
  - Memoization for expensive computations
  
- [ ] **Key Estimation Optimization**
  - Reduce redundant pitch class set comparisons
  - Optimize sliding window algorithm
  
- [ ] **Memory Allocation Reduction**
  - Pool commonly used objects
  - Reduce LINQ allocations in hot paths

#### Benchmarking
- [ ] Expand BenchmarkDotNet suite
- [ ] Add memory allocation benchmarks
- [ ] Compare before/after optimization results

### サンプルプロジェクト（Example Projects）

**Goal**: Demonstrate library usage in real-world scenarios

#### Projects
- [ ] **Console Music Analyzer**
  - Interactive CLI for chord progression analysis
  - File input support (JSON, MIDI)
  - Export analysis results
  
- [ ] **ASP.NET Core Web API**
  - RESTful API for harmony analysis
  - Swagger/OpenAPI documentation
  - Rate limiting and caching
  
- [ ] **Blazor WebAssembly App**
  - Browser-based music theory tool
  - Interactive chord progression builder
  - Real-time analysis visualization

#### Documentation
- [ ] Step-by-step setup guides
- [ ] API integration examples
- [ ] Best practices documentation

---

## 🚀 v1.2.0 - Feature Expansion

**Target Release**: Q2 2026 (2-3 months after v1.1.0)  
**Focus**: Extended harmony features, voice leading, CLI enhancements

### 高度な和声機能（Advanced Harmony Features）

#### Voice Leading Analysis
- [ ] **Part-writing validation**
  - Parallel 5ths/8ves detection
  - Spacing rules (soprano-alto, alto-tenor, tenor-bass)
  - Range validation for SATB
  
- [ ] **Voice crossing detection**
- [ ] **Doubling analysis** (root, third, fifth preferences)

#### Extended Chord Support
- [ ] **Ninth chords**: maj9, min9, dom9 (beyond V9)
- [ ] **Eleventh chords**: 11th, sus11
- [ ] **Thirteenth chords**: 13th variations
- [ ] **Add chords**: add9, add11, add13

#### Modal Analysis
- [ ] **Mode detection**: Ionian, Dorian, Phrygian, Lydian, Mixolydian, Aeolian, Locrian
- [ ] **Modal interchange tracking**
- [ ] **Modal cadences**

### CLI拡張（CLI Enhancements）

#### Interactive Mode
- [ ] REPL for chord-by-chord analysis
- [ ] History navigation
- [ ] Auto-completion

#### File Format Support
- [ ] **Input**: MusicXML, MIDI, ABC notation
- [ ] **Output**: PDF reports, HTML visualization

#### Batch Processing
- [ ] Analyze multiple files in parallel
- [ ] Progress reporting
- [ ] Error aggregation

---

## 🌟 v2.0.0 - Advanced Features & ML Integration

**Target Release**: Q4 2026 (Long-term)  
**Focus**: Machine learning, real-time MIDI, ecosystem expansion

### 機械学習統合（Machine Learning Integration）

#### Chord Prediction
- [ ] **Next chord prediction** using trained models
- [ ] **Progression generation** based on style
- [ ] **Harmonic similarity search**

#### Style Classification
- [ ] Classify progressions by genre (Classical, Jazz, Pop, Rock)
- [ ] Era detection (Baroque, Classical, Romantic, Contemporary)
- [ ] Composer style fingerprinting

### リアルタイムMIDI解析（Real-time MIDI Analysis）

#### MIDI Input Processing
- [ ] Real-time chord recognition from MIDI keyboard
- [ ] Live key estimation
- [ ] On-the-fly Roman numeral display

#### MIDI Output Generation
- [ ] Generate MIDI from Roman numeral input
- [ ] Voicing suggestions based on rules
- [ ] Playback preview

### 高度な解析（Advanced Analysis）

#### Multi-key Orchestral Works
- [ ] Simultaneous key tracking for multiple instruments
- [ ] Polytonality detection
- [ ] Cross-key relationships

#### Form Analysis
- [ ] Section detection (verse, chorus, bridge)
- [ ] Harmonic rhythm analysis
- [ ] Cadence-based structure recognition

---

## 📦 Distribution & Ecosystem

### NuGet Package Publication

**Target**: v1.1.0 or v1.2.0

#### Packages
- [ ] **MusicTheory.Core**: Core analysis library
- [ ] **MusicTheory.Cli**: Command-line tool
- [ ] **MusicTheory.AspNetCore**: ASP.NET Core integration (v2.0+)

#### Requirements
- [ ] Complete API documentation (XML comments)
- [ ] Package icon and README
- [ ] Semantic versioning compliance
- [ ] License selection (MIT, Apache 2.0, etc.)

### VS Code Extension

**Target**: v2.0+

#### Features
- [ ] Inline chord analysis in music notation files
- [ ] Hover tooltips for Roman numerals
- [ ] Quick actions for chord transformations
- [ ] Integration with MIDI input

### Plugin Architecture

**Target**: v2.0+

#### Extensibility Points
- [ ] Custom chord type definitions
- [ ] User-defined analysis rules
- [ ] Third-party notation format support
- [ ] Custom output formatters

---

## 🎯 次期開発目標（旧Phase 1-3を統合）

#### カバレッジ向上
- [ ] 目標: **85%以上**
- [ ] 未カバー領域の分析と優先順位付け
- [ ] 境界条件テストの追加
- [ ] エッジケースの網羅

#### ドキュメント強化
- [ ] API リファレンスの自動生成（DocFX検討）
- [ ] チュートリアル追加（段階的学習パス）
- [ ] コード例の拡充
- [ ] FAQ セクション追加

#### パフォーマンス最適化
- [ ] ベンチマーク基準値の設定
- [ ] ホットパスの最適化
- [ ] メモリアロケーション削減
- [ ] キャッシング戦略の実装

### Phase 2: Harmony機能拡張（中期）

#### 和声進行ルール強化
- [ ] 声部進行規則の詳細チェック
  - [ ] 平行5度/8度の検出
  - [ ] 隠伏5度/8度の検出
  - [ ] 声域違反のチェック
  - [ ] 声部交差の検出
- [ ] 進行妥当性スコアリング
- [ ] 推奨代替進行の提案

#### モジュレーション推定の精緻化
- [ ] 調性確信度の可視化
- [ ] 転調タイプの分類（直接/準備/突然）
- [ ] ピボット和音の特定
- [ ] 実サンプル収集と閾値調整

#### カデンツ判定の洗練
- [ ] PAC/IAC 近似判定の閾値再検討
- [ ] Half cadence 抑制ルールの最適化
- [ ] Plagal cadence の分類強化
- [ ] カデンツ強度スコアの導入

### Non-Functional Harmony（和声外音）

**Target**: v1.3.0+

#### 和声外音の分析
- [ ] 経過音（Passing tone）の分析
- [ ] 倚音（Appoggiatura）の検出
- [ ] 掛留音（Suspension）のサポート
- [ ] ペダルポイント（Pedal point）の識別

#### 高度な処理
- [ ] 装飾音の分類
- [ ] 予備・解決パターンの検証
- [ ] メロディック分析との統合

---

## 📊 Success Metrics

### v1.1.0 Goals
- ✅ Coverage: 85%+ (target: 85.0-85.5%)
- ✅ Performance: 20-30% faster chord analysis
- ✅ Documentation: Architecture diagrams complete
- ✅ Examples: 3 sample projects published

### v1.2.0 Goals
- ✅ Extended chords: 9th, 11th, 13th support
- ✅ Voice leading: Basic validation implemented
- ✅ CLI: Interactive mode functional
- ✅ Formats: MusicXML/MIDI input support

### v2.0.0 Goals
- ✅ ML Integration: Chord prediction model trained
- ✅ Real-time MIDI: Working prototype
- ✅ NuGet: Published and documented
- ✅ Ecosystem: VS Code extension released

---

## 🗓️ Timeline Overview

```
2025-10-18  v1.0.0 Released (PRODUCTION READY)
    |
    ├── Q4 2025: Integration tests foundation work
    ├── Q1 2026: v1.1.0 development (integration tests, architecture docs)
    |
2026-Q1     v1.1.0 Release (85% coverage, optimized, documented)
    |
    ├── Q1-Q2 2026: Feature expansion work
    |
2026-Q2     v1.2.0 Release (extended chords, voice leading, CLI enhancements)
    |
    ├── Q2-Q4 2026: ML integration, MIDI support
    |
2026-Q4     v2.0.0 Release (ML, real-time MIDI, NuGet, ecosystem)
```

---

## 🤝 Contributing

### How to Contribute

v1.1.0以降のリリースでは、コミュニティからの貢献を歓迎します。

#### Priority Areas for Contributions
1. **Integration Tests** (v1.1.0)
   - Real-world chord progression test cases
   - Edge case scenario coverage
   
2. **Example Projects** (v1.1.0)
   - Web API integration samples
   - UI/UX demonstration apps
   
3. **Performance Optimization** (v1.1.0)
   - Profiling and bottleneck identification
   - Algorithm improvements
   
4. **Documentation** (All versions)
   - Tutorial creation
   - API documentation improvements
   - Translation (internationalization)

#### Contribution Guidelines
- Follow existing code style and conventions
- Include unit tests for new features
- Update documentation for API changes
- Maintain or improve coverage percentage

---

## 📝 Notes

### Flexibility
このロードマップは現時点での計画であり、コミュニティのフィードバックや技術的発見により変更される可能性があります。

### Prioritization
各バージョンの機能は優先順位付けされており、必要に応じて次バージョンへ延期される場合があります。

### Feedback
ロードマップへの提案や要望は、GitHub Discussions または Issues で受け付けています。

---

**Last Updated**: 2025-10-18  
**Current Version**: v1.0.0  
**Next Milestone**: v1.1.0 (Q1 2026)

🔗 **v1.0.0 Release**: https://github.com/majiros/MusicTheory/releases/tag/v1.0.0

---

🎵 **Building the future of music theory analysis together!** ✨

#### スケール・モード拡張
- [ ] エキゾチックスケールの追加
- [ ] モードミクスチャーの解析
- [ ] スケール推定の精度向上

---

## 🚀 将来の方向性

### アプリケーション開発

#### 楽譜制作ソフト（別リポジトリ推奨）
- **アーキテクチャ**: MusicTheory を基盤ライブラリとして参照
- **統合方法**: NuGetパッケージ / Git submodule / プロジェクト参照
- **開発戦略**: 
  1. Samples フォルダでプロトタイプ作成
  2. 成熟度に応じて専用リポジトリへ分離
  3. MusicTheory の公開API安定化

#### 想定されるアプリケーション例
- [ ] 楽譜エディタ（GUI）
- [ ] 和声解析ツール（ビジュアライゼーション）
- [ ] 音楽理論学習アプリ
- [ ] MIDI/MusicXML コンバータ
- [ ] リアルタイム和音認識

### エコシステム拡張
- [ ] NuGet パッケージ公開
- [ ] VS Code Extension 開発
- [ ] Web API サービス
- [ ] クラウドベース解析サービス

---

## 📅 マイルストーン

### v0.9（品質向上フェーズ）
- カバレッジ 85%達成
- ドキュメント完成度 90%
- パフォーマンスベンチマーク確立
- **目標期日**: TBD

### v1.0（安定版リリース）
- 全コア機能安定化
- 公開API凍結
- 完全なドキュメント
- NuGetパッケージ公開
- **目標期日**: TBD

### v1.x（機能拡張）
- Non-functional harmony サポート
- 高度な和声外音処理
- 拡張和音の完全サポート
- **目標期日**: TBD

### v2.0（次世代）
- ジャズ和声の完全対応
- リアルタイム解析
- Web API 公開
- エコシステム拡充
- **目標期日**: TBD

---

## 🤝 コントリビューション

現在は個人プロジェクトとして開発中です。将来的に以下の領域でのコントリビューションを歓迎する可能性があります：

- テストケースの追加
- ドキュメント改善
- バグ報告・修正
- 新機能の提案
- パフォーマンス最適化

---

## 📚 参考資料

- README.md - プロジェクト概要と使用方法
- Docs/UserGuide.md - ユーザーガイド
- Docs/PAC-and-LeadingTone.md - PAC設計の詳細
- CHANGELOG.md - 変更履歴

---

**Last Updated**: 2025年10月4日  
**Project Status**: Active Development  
**License**: TBD
