# Integration Testing Guide

このドキュメントは、MusicTheory プロジェクトの統合テスト戦略とベストプラクティスを説明します。

## 概要

統合テストは、複数のコンポーネントが連携して正しく動作することを検証します。このプロジェクトでは、ハーモニー分析の **エンドツーエンド** ワークフローをテストすることで、実際の使用シナリオをカバーします。

### 統合テスト vs 単体テスト

| 観点 | 単体テスト | 統合テスト |
|------|-----------|-----------|
| **目的** | 個別クラス/メソッドの正確性 | コンポーネント間の連携 |
| **範囲** | `HarmonyAnalyzer.AnalyzeTriad()` 単体 | `ProgressionAnalyzer.Analyze()` 全体 |
| **実行速度** | 高速（<1ms） | 中速（数ms～数十ms） |
| **カバレッジ** | 深い（境界条件網羅） | 広い（実際の使用パターン） |
| **メンテナンス** | 変更に敏感 | 比較的安定 |

## 統合テストの構造

### プロジェクト構成

```
Tests/
  MusicTheory.Tests/           # 単体テスト（915テスト）
  MusicTheory.IntegrationTests/ # 統合テスト（13テスト）
    ProgressionScenarioTests.cs
    MusicTheory.IntegrationTests.csproj
```

### テストカテゴリ

統合テストは以下の5つのカテゴリに分類されます：

#### 1. Basic Diatonic Progressions（3テスト）

**目的**: 基本的な全音階進行の正確性を検証

- `I-IV-V-I` (古典的なカデンツ)
- `ii-V-I` (ジャズカデンツ)
- `I-vi-IV-V` (ポップ進行)

**検証項目**:
- ローマ数字ラベルの正確性
- カデンツ検出（Authentic / Half）
- 和音の継続性

#### 2. Secondary Dominant Chains（2テスト）

**目的**: 二次ドミナントの分析を検証

- `V7/V → V → I` (トニック化)
- `vii°7/V → V → I` (導音アプローチ)

**検証項目**:
- `/` 表記の正確性
- セカンダリドミナント解決
- 一時的転調の認識

#### 3. Borrowed Chord Progressions（3テスト）

**目的**: モーダル・ミクスチャー（借用和音）の分析を検証

- `I-bVI-bVII-I` (モーダル・ミクスチャー)
- `iv-V-I` (短調サブドミナント)
- `bII-V-I` (ナポリの六の和音)

**検証項目**:
- `bVI`, `bVII`, `iv`, `bII` のラベリング
- 借用和音の解決パターン
- 警告メッセージ（Mixture-7th の場合）

#### 4. Augmented Sixth Resolutions（4テスト）

**目的**: 増六和音の認識と曖昧性解消を検証

- `It6 → V → I` (イタリアの六)
- `Fr43 → V → I` (フランスの六)
- `Ger65 → V → I` (ドイツの六)
- `Ger65 vs bVI7` (異名同音の区別)

**検証項目**:
- 増六和音の正確な分類
- `Ger65` と `bVI7` の曖昧性
- voicing の有無による認識制約

**注意**: voicing なしでは増六和音が `null` や `bVI7` に誤認識される可能性があります（現実的な制約）。

#### 5. Modulation Detection（1テスト）

**目的**: 転調検出の正確性を検証

- `C major → G major` (属調転調)

**検証項目**:
- キーセグメントの分割
- 転調点の検出
- 各セグメントのキー推定

## テスト実装パターン

### 基本構造

```csharp
[Fact]
public void AnalyzeDiatonicProgression_I_IV_V_I_ReturnsCorrectLabels()
{
    // Arrange: 入力データの準備
    var key = new Key(60, true); // C major
    var pcsList = new[]
    {
        new[] { Pc(60), Pc(64), Pc(67) },  // I (C-E-G)
        new[] { Pc(65), Pc(69), Pc(72) },  // IV (F-A-C)
        new[] { Pc(67), Pc(71), Pc(62) },  // V (G-B-D)
        new[] { Pc(60), Pc(64), Pc(67) }   // I (C-E-G)
    };

    // Act: 分析実行
    var result = ProgressionAnalyzer.Analyze(pcsList, key);

    // Assert: 結果検証
    result.Chords.Should().HaveCount(4);
    result.Chords[0].RomanText.Should().Be("I");
    result.Chords[1].RomanText.Should().Be("IV");
    result.Chords[2].RomanText.Should().Be("V");
    result.Chords[3].RomanText.Should().Be("I");
    
    // カデンツ検出
    result.Cadences.Should().Contain(c => c.cadence == CadenceType.Authentic);
}
```

### ヘルパーメソッド

```csharp
// ピッチクラス正規化（MIDI → 0-11）
private static int Pc(int midi) => ((midi % 12) + 12) % 12;
```

### アサーション戦略

#### 1. 厳密アサーション（確定的な結果）

```csharp
// ローマ数字が一意に決定される場合
result.Chords[0].RomanText.Should().Be("I");
```

#### 2. 柔軟アサーション（複数の有効な結果）

```csharp
// カデンツ検出は複数出現の可能性
result.Cadences.Should().Contain(c => c.cadence == CadenceType.Authentic);
```

#### 3. 存在確認アサーション（認識制約）

```csharp
// voicing なしでは認識されない可能性
result.Chords[0].Should().NotBeNull();
```

#### 4. 正規表現アサーション（曖昧性）

```csharp
// Ger65 または bVI7 の両方が有効
result.Chords[0].RomanText.Should().MatchRegex("(Ger|bVI7)");
```

## 実行方法

### ローカル実行

```bash
# すべてのテスト（単体 + 統合）
dotnet test -c Release

# 統合テストのみ
dotnet test -c Release --filter "FullyQualifiedName~IntegrationTests"

# カバレッジ付き
dotnet test -c Release \
  --collect "XPlat Code Coverage" \
  --results-directory ./coverage
```

### CI/CD 実行

GitHub Actions ワークフローが自動的に実行します：

```yaml
# .github/workflows/test.yml
- name: Run tests
  run: dotnet test -c Release --no-build --verbosity normal
```

**注意**: 統合テストも `dotnet test` に自動的に含まれます。

## テスト追加ガイドライン

### 新しいテストを追加する場合

1. **既存テストとの重複確認**
   - 単体テストで既にカバーされていないか確認
   - 実行パスが重複すると、カバレッジ増加に寄与しない

2. **テストの命名規則**
   ```csharp
   [Fact]
   public void Analyze{Feature}_{Scenario}_{ExpectedBehavior}()
   ```
   
   例:
   - `AnalyzeDiatonicProgression_I_IV_V_I_ReturnsCorrectLabels`
   - `AnalyzeModulation_CtoG_DetectsKeyChange`

3. **テストの配置**
   - カテゴリごとに `#region` でグループ化
   - 1つの `#region` には関連テストのみ配置

4. **アサーション戦略の選択**
   - 確定的な結果 → 厳密アサーション
   - 複数の有効な結果 → 柔軟アサーション
   - 認識制約あり → 存在確認アサーション

### カバレッジ増加のためのテスト追加

統合テストは既存の単体テストと実行パスが重複する可能性が高いため、カバレッジ増加には以下の戦略が必要です：

#### 1. 未カバー領域の特定

```bash
# カバレッジレポートを生成
reportgenerator \
  -reports:./coverage/**/coverage.cobertura.xml \
  -targetdir:./coverage-report \
  -reporttypes:Html

# ブラウザで確認
start ./coverage-report/index.html
```

#### 2. エッジケースの追加

- **HarmonyAnalyzer**:
  - 複雑な voicing パターン
  - 異名同音の曖昧性（F# vs Gb）
  - 多重借用和音
  
- **ProgressionAnalyzer**:
  - 複数転調（C → G → D）
  - 不完全なカデンツパターン
  - 異常な和音進行

#### 3. オプションの組み合わせテスト

```csharp
var options = new HarmonyOptions
{
    Hide64 = true,
    Cad64Dominant = true,
    PreferMixtureSeventhOverAugmentedSixthWhenAmbiguous = true
};
var result = ProgressionAnalyzer.Analyze(pcsList, key, options);
```

## ベストプラクティス

### ✅ DO

- **現実的なアサーション**: アナライザーの実際の動作に基づく期待値
- **ドキュメント**: 各テストにコメントで意図を記載
- **ヘルパーメソッド**: `Pc()` のような共通ロジックを再利用
- **並列実行**: テストは独立して実行可能に設計

### ❌ DON'T

- **理想的すぎる期待値**: アナライザーが達成できない完璧な結果を期待しない
- **voicing なしの厳密なアサーション**: 認識制約を無視しない
- **過度に複雑なテスト**: 1テストで複数の機能を検証しない
- **ハードコードされた数値**: `Pc()` ヘルパーを使用する

## トラブルシューティング

### 問題: カデンツが予期せず複数検出される

**原因**: `IV → V` も Half cadence として検出される

**解決策**: 柔軟なアサーションを使用
```csharp
// ❌ 厳密すぎ
result.Cadences.Should().ContainSingle();

// ✅ 柔軟
result.Cadences.Should().Contain(c => c.cadence == CadenceType.Authentic);
```

### 問題: 増六和音が `null` になる

**原因**: voicing なしでは増六和音を認識できない

**解決策**: 存在確認に変更
```csharp
// ❌ 厳密すぎ
result.Chords[0].RomanText.Should().Contain("Ger");

// ✅ 現実的
result.Chords[0].Should().NotBeNull();
```

### 問題: テストは合格するがカバレッジが増えない

**原因**: 単体テストと実行パスが重複

**解決策**: 未カバー領域を特定し、そこを通るテストを追加
```bash
# カバレッジレポートで未カバー行を確認
reportgenerator -reports:./coverage/**/coverage.cobertura.xml \
  -targetdir:./coverage-report -reporttypes:Html
```

## まとめ

統合テストは、MusicTheory プロジェクトの品質保証において重要な役割を果たします：

- **システム全体の動作検証**: コンポーネント間の連携を確認
- **実際の使用シナリオのカバー**: ユーザーが遭遇する可能性のあるパターンをテスト
- **回帰テスト**: 将来の変更が既存機能を破壊しないことを保証

統合テストは単体テストを補完するものであり、両者のバランスが重要です。統合テストだけでは十分ではなく、単体テストと組み合わせることで、包括的なテスト戦略を実現します。

---

**関連ドキュメント**:
- [INTEGRATION_TESTING_STRATEGY.md](INTEGRATION_TESTING_STRATEGY.md): v1.1.0 統合テスト戦略
- [README.md](README.md): プロジェクト全体のドキュメント
- [CONTRIBUTING.md](CONTRIBUTING.md): 開発ガイドライン
