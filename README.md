# MusicTheory

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet) ![C#](https://img.shields.io/badge/C%23-Library-blue?logo=c-sharp) [![CI](https://github.com/majiros/MusicTheory/actions/workflows/ci.yml/badge.svg)](https://github.com/majiros/MusicTheory/actions/workflows/ci.yml) [![.NET CI](https://github.com/majiros/MusicTheory/actions/workflows/dotnet.yml/badge.svg)](https://github.com/majiros/MusicTheory/actions/workflows/dotnet.yml) [![Test & Coverage](https://github.com/majiros/MusicTheory/actions/workflows/test.yml/badge.svg)](https://github.com/majiros/MusicTheory/actions/workflows/test.yml) [![coverage-pages](https://github.com/majiros/MusicTheory/actions/workflows/coverage-pages.yml/badge.svg)](https://github.com/majiros/MusicTheory/actions/workflows/coverage-pages.yml) [![docs-lint](https://github.com/majiros/MusicTheory/actions/workflows/docs-lint.yml/badge.svg)](https://github.com/majiros/MusicTheory/actions/workflows/docs-lint.yml) [![coverage: combined](https://majiros.github.io/MusicTheory/badge_combined.svg)](https://majiros.github.io/MusicTheory/index.html) [![coverage: line](https://majiros.github.io/MusicTheory/badge_linecoverage.svg)](https://majiros.github.io/MusicTheory/index.html) [![coverage: branch](https://majiros.github.io/MusicTheory/badge_branchcoverage.svg)](https://majiros.github.io/MusicTheory/index.html) [![coverage: method](https://majiros.github.io/MusicTheory/badge_methodcoverage.svg)](https://majiros.github.io/MusicTheory/index.html)

<!-- 🎯 Coverage Achievement: 84.8% (915 tests) | Phase 18 milestone completed 2025-10-18 -->
<!-- カバレッジバッジは GitHub Pages に公開されたバッジ SVG を参照しています (75%+ gate enforced)。
詳細: COVERAGE_ACHIEVEMENT.md | 学習記録: LESSONS_LEARNED.md -->

コード/スケール/インターバル基礎と和音解析 (Chord Analyzer) を含む .NET 8 ライブラリです。

参考: 初めての方向けの簡易ドキュメントは Docs/UserGuide.md をご覧ください。

- CLI 詳細は「4.1 CLI オプション詳細」
- JSON スキーマと診断は「5.4 JSON スキーマ（詳細）」「5.5 警告/エラー」
- 推定器の出力は「5.6 Estimator/Segments/Keys 出力」
- 具体例は「5.7 代表例と比較スナップショット」
- 音価虫眼鏡ルールは「5.3 音価虫眼鏡ルール（編集UIの指針）」

## 目次

- [主な機能](#主な機能)
- [クイックスタート](#クイックスタート)
- [HarmonyOptions（判定のタイブレーク/好みの調整）](#harmonyoptions判定のタイブレーク好みの調整)
- [6-4分類（Passing / Pedal）](#6-4分類passing--pedal)
- [Augmented Sixth (It6/Fr43/Ger65)](#augmented-sixth-it6fr43ger65)
- [Strict PAC オプション（厳格なPAC判定）](#strict-pac-オプション厳格なpac判定)
- [詳細カデンツ API（PAC/IAC とカデンツ6-4）](#詳細カデンツ-apipaciac-とカデンツ6-4)
- [ビルド・テスト](#ビルドテスト)
- [初回ウォームアップと安定実行の注意](#初回ウォームアップと安定実行の注意)
- [ベンチマーク（サンプル）](#ベンチマークサンプル)
- [CLI サンプルの実行](#cli-サンプルの実行)
- [CLI JSON出力](#cli-json出力)
- [Mixture-7th 警告(JSON)](#mixture-7th-警告json)
- [CLI ユーティリティ](#cli-ユーティリティ)
- [サンプルWPFの起動](#サンプルwpfの起動)
- [トラブルシューティング](#トラブルシューティング)
- [CI（GitHub Actions）](#cigithub-actions)
- [和声法ロードマップ（v1の目安）](#和声法ロードマップv1の目安)
- [カバレッジ（公開/ローカル）](#カバレッジ公開ローカル)


## 主な機能

- Pitch / Interval / Scale / Chord モデル
- `FunctionalInterval` による軽量インターバル表現
- 拡張スケール (Ionian / Dorian / Lydian / Lydian Dominant / Altered / Whole Tone / Bebop / Diminished 他)
- コードフォーミュラ (maj7, 7, m7, m7b5, dim7, 9, 13, 7alt, maj13 等)

- Roman Numeral と Tonal Function（I/ii/IV/V… → T/S/D）
- Triad の転回形表記（6 / 64）と Seventh の転回形（7 / 65 / 43 / 42）
  - 二次機能の三和音（V/x・vii°/x）でも、voicing が与えられた場合は 6/64 を付与します（例: V6/ii, V64/ii, vii°6/V, vii°64/V）。
  - メジャーセブンス（例: IVmaj7）の転回表記はオプションで「maj」を含められます（既定は含めない）。例: 既定=IV65/IV43/IV42、オプション有効=IVmaj65/IVmaj43/IVmaj42。
  - 七和音の図形はベースと根音の相対から決定します（root=7, 1st=65, 2nd=43, 3rd=42）。°/ø は必要なときに一度だけ付与し、重複しません。二次導音（vii°7/x・viiø7/x）でも voicing があれば図形を付けます（例: vii°65/V）。
- 短調（和声的短音階）対応：V / V7、vii°（導音基準）
- 借用和音（modal mixture, major での i / iv / bIII / bVI / bVII の最小対応）
- 借用7th（iv7 / bVII7 / bII7 / bVI7）の検出とラベリング（和声的短音階ベース）
  - Roman 入力でも `bVI7`/`bII7`/`bVII7`/`N7` を 4 音として解釈（例: C で `bVI7` → Ab–C–Eb–Gb）。
  - 付与した 7/65/43/42 はボイシングヒント（BassPcHint）に反映されます。
- Seventh の品質サフィックス（maj7 / 7 / ø7 / °7）
- 減三和音の度数記号“°”（例: vii° / vii°6）
- 四声ボイシング検証：並び順、S-A/A-T 間隔、重なり、完全協和音程の並行
- 簡易カデンツ検出（Authentic/Plagal/Half/Deceptive）と進行アナライザ
- キー推定（簡易）とモジュレーション区間の抽出（`ProgressionAnalyzer.AnalyzeWithKeyEstimate`）
- Dominant Ninth (V9) の認識（5度省略可、4–5音、誤検出防止の優先ショートサーキット）
- Augmented Sixth（It6/Fr43/Ger65）の認識（bass=b6 必須、bVI7 と決定的に棲み分け。曖昧時の選好はオプションで反転可）
- 二次機能（V/x、vii°/x、vii°7/ x、viiø7/ x）と /V 優先のタイブレーク

補足（検出の安定化・優先順位）

- 4音（重複なし）なら Seventh を優先して認識します（IV vs iv7 等の誤判定を低減）。
- Triad/Seventh ともに「厳密一致（含有かつ音数一致）」で照合し、7th 和音の部分集合を Triad と誤認しないようにしています。
- Triad の転回サフィックスは「distinct なピッチクラスがちょうど3音」のときのみ付与します（4音以上では triad へ降格せず、7th/テンション系を優先）。
- V9 は判定最優先で、処理の最後にも安全再チェックを走らせて triad への誤ラベルを抑止します。
- 借用7th も 4音以上かつ FourPartVoicing が与えられている場合に、最後の安全チェックで転回表記を再確認します。

転回形サフィックス早見（Roman → 付与記号）

| 種別 | Root | 1st | 2nd | 3rd |
|------|------|-----|-----|-----|
| Triad | (無) | 6 | 64 | - |
| Seventh | 7 | 65 | 43 | 42 |

七和音の図形決定と記号付与（要点）:

- 根拠: ベース音から見た根音までの相対（第三/五度/第七）に基づき、7→65→43→42 を決定。
- 記号: 減七/半減七には °/ø を一度だけ付与（重複防止）。メジャーセブンスの転回に「maj」を含めるかは `HarmonyOptions.IncludeMajInSeventhInversions` で制御。
- 二次導音: `vii°7/x` と `viiø7/x` は、voicing が与えられた場合に図形（65/43/42）を付与します。早期優先の `/V` パターン（例: `vii°7/V`）でも図形は同様に反映されます。

例（Cメジャー、vii°7/V の転回）:

```csharp
var key = new Key(60, true); // C major
int Pc(int m) => ((m % 12) + 12) % 12;
var pcs = new[]{ Pc(6), Pc(9), Pc(0), Pc(3) }; // F# A C Eb (= vii°7/V)
var v65 = new FourPartVoicing(86, 81, 69, 78); // Bass=A → 65
var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v65, null);
// r.RomanText == "vii°65/V"
```

使用例（Cメジャーで C 和音 triad を分析）:

```csharp
using MusicTheory.Theory.Harmony;

var key = new Key(60, true); // Cメジャー基準
var pcs = new[] { 0, 4, 7 };          // C, E, G のピッチクラス
var prev = new FourPartVoicing(76, 69, 60, 48);
var curr = new FourPartVoicing(79, 72, 67, 55);
var result = HarmonyAnalyzer.AnalyzeTriad(pcs, key, curr, prev);
// result.Success=true, result.Roman=I, result.Function=Tonic
```

Seventh と転回形の例（G7 in C major → V7, V65 など）:

```csharp
var key = new Key(60, true); // C major
var pcsV7 = new[] { 7, 11, 2, 5 };   // G B D F
var r1 = HarmonyAnalyzer.AnalyzeTriad(pcsV7, key);
// r1.RomanText == "V7"（四声ボイシングを与えると 7/65/43/42 を付与）

var pcsV65 = new[] { 11, 2, 5, 7 };  // 転回（B を最上にしない配列でも可）
var r2 = HarmonyAnalyzer.AnalyzeTriad(pcsV65, key, new FourPartVoicing(86, 81, 74, 71));
// r2.RomanText == "V65"
```

ドミナント9th（V9）の例（G9 in C major、5度有無どちらも許容）:

```csharp
var key = new Key(60, true); // C major
int Pc(int m) => ((m % 12) + 12) % 12;

// フル G9: G B D F A
var v9_full = new[] { Pc(7), Pc(11), Pc(2), Pc(5), Pc(9) };
var r_v9_full = HarmonyAnalyzer.AnalyzeTriad(v9_full, key);
// r_v9_full.RomanText == "V9"

// 5度省略 G9: G B F A（D 省略）
var v9_no5 = new[] { Pc(7), Pc(11), Pc(5), Pc(9) };
var r_v9_no5 = HarmonyAnalyzer.AnalyzeTriad(v9_no5, key);
// r_v9_no5.RomanText == "V9"
```

### V9 表記ポリシー（V9 vs V7(9)）

- 既定の表示は `V9`（機能は Dominant）。
- オプション `PreferV7Paren9OverV9` を有効にするか、CLI の `--v7n9` を指定すると表示を `V7(9)` に切り替えます（機能/解析順序は不変、表示のみ変更）。
- プリセット `HarmonyOptions.NotationV7Paren9` でも同等の指定が可能です。
- JSON 出力の `options.v9` には `"V9"` または `"V7(9)"` が入ります。

簡易比較:

| 設定 | 表示 | 備考 |
|------|------|------|
| 既定 | `V9` | 解析/機能は Dominant |
| `PreferV7Paren9OverV9=true` または CLI `--v7n9` | `V7(9)` | 表示のみ変更 |

切替例:

```csharp
var key = new Key(60, true);
var pcsV9 = new[] { 7, 11, 2, 5, 9 }; // G B D F A
var r1 = HarmonyAnalyzer.AnalyzeTriad(pcsV9, key);
// r1.RomanText == "V9"

var opts = HarmonyOptions.NotationV7Paren9;
var r2 = HarmonyAnalyzer.AnalyzeTriad(pcsV9, key, opts);
// r2.RomanText == "V7(9)"
```

短調の導音と減三和音（A minor の vii°）:

```csharp
var aMinor = new Key(57, false); // A minor
var pcsDim = new[] { 11, 2, 5 }; // G# B D（導音基準の vii°）
var r3 = HarmonyAnalyzer.AnalyzeTriad(pcsDim, aMinor);
// r3.RomanText == "vii°"（転回なら "vii°6" など）
```

カデンツ検出と進行解析:

```csharp
var cad = CadenceAnalyzer.Detect(RomanNumeral.V, RomanNumeral.I, isMajor: true); // Authentic

var progression = new[] { new[] { 0, 4, 7 }, new[] { 5, 9, 0 }, new[] { 7, 11, 2 }, new[] { 0, 4, 7 } };
var p = ProgressionAnalyzer.Analyze(progression, new Key(60, true));
// p.Cadences や各和音の RomanText を参照
```

今後は V9、借用和音、モジュレーション、進行ルールの精緻化等を段階的に拡張予定です。
→ V9 / bII7 / bVI7 は実装済み。以降は借用和音のさらなる拡張（変化和音の体系化）、進行ルール強化、モジュレーション検出の精緻化を継続します。

## カバレッジ（公開/ローカル）

- 公開（GitHub Pages）
  - レポート: <https://majiros.github.io/MusicTheory/index.html>
  - バッジ: Line <https://majiros.github.io/MusicTheory/badge_linecoverage.svg> / Branch <https://majiros.github.io/MusicTheory/badge_branchcoverage.svg> / Method <https://majiros.github.io/MusicTheory/badge_methodcoverage.svg>
  - ワークフロー: <https://github.com/majiros/MusicTheory/actions/workflows/coverage-pages.yml>
    - main への push と毎日深夜（UTC）に自動実行されます。
    - 公開前ゲート: `site/coverage/Summary.xml` に対し `Scripts/CheckCoverage.ps1 -Threshold 75` を適用し、基準未満なら公開を中止します。
    - トップページではバッジに加えて Line/Branch/Method のカバレッジ率を自動表示します（`coverage/Summary.xml` を読み取り）。
    - 初回公開直後や直近の更新直後はキャッシュの都合で表示が遅れる／404 が出ることがあります。数十秒〜数分待って再読み込みしてください（ハードリロード推奨）。
    - VS Code タスク（ローカル）:
      - `coverage: open (public)` … 公開トップを既定ブラウザで開く
      - `coverage: fetch public Summary.xml` … 公開 `coverage/Summary.xml` を取得し Line/Branch/Method/Generated on (UTC) を表示（リトライつき）。

- ローカル生成（VS Code タスク）
  - 推奨: `coverage: full warm stable (simple)` → HTML/XmlSummary/Badges を生成
  - 生成物: `Tests/MusicTheory.Tests/TestResults/coverage-report/index.html`
  - バッジ（ローカル）: Line `Tests/MusicTheory.Tests/TestResults/coverage-report/badge_linecoverage.svg` / Branch `Tests/MusicTheory.Tests/TestResults/coverage-report/badge_branchcoverage.svg` / Method `Tests/MusicTheory.Tests/TestResults/coverage-report/badge_methodcoverage.svg`

## Strict PAC オプション（厳格なPAC判定）

Authentic を Perfect Authentic Cadence (PAC) とみなす条件を厳格化するためのオプションを提供します。既定ではオフ（従来互換）で、教育/採点用途などで厳しめの PAC を要求したい場合のみ有効化してください。

主なオプション（`HarmonyOptions`）

- StrictPacPlainTriadsOnly: PAC は素朴な三和音のみ（テンション/第7音を含む場合は PAC にしない）
- StrictPacDisallowDominantExtensions: ドミナント側の拡張（V7/V9/付加音）を含む場合は PAC にしない
- StrictPacRequireSopranoTonic: 最終 I のソプラノが主音（tonic）であることを要求（ボイシング必須）
- StrictPacRequireSopranoLeadingToneResolution: 直前の V でソプラノが導音なら、上行で主音に解決することを要求（ボイシング必須）

加えて、ボイシングが与えられている場合、ドミナントは原位（root position）であることを PAC の必要条件とします（反転ドミナントは Authentic 可だが PAC ではない）。

注意（抑止・誤検出防止のガード）

- I64→V の Half は抑止され、後続の V→I Authentic に集約します（Cadential 6-4）。
- Augmented Sixth（It6/Fr43/Ger65）→ V の Half は抑止（終止の重複を避けるため）。
- 二次導音（vii…/V ファミリ）→ 直接 I の遷移は Authentic としません（Ger65 の異名同音などでの誤認を防止）。
- テキストフォールバックの Authentic は厳密に V→I のヘッド一致に限定します（vii°7/V などを誤って V とみなさない）。

最小コード例（Cメジャー、厳格 PAC）

```csharp
var key = new Key(60, true);
var opts = new HarmonyOptions {
  StrictPacPlainTriadsOnly = true,
  StrictPacDisallowDominantExtensions = true,
  StrictPacRequireSopranoTonic = true,
  StrictPacRequireSopranoLeadingToneResolution = true,
};
var seq = new[]{ new[]{0,4,7}, new[]{7,11,2}, new[]{0,4,7} }; // I → V → I
var v = new FourPartVoicing?[] {
  new FourPartVoicing(79,72,67,55), // I root
  new FourPartVoicing(83,79,74,67), // V root
  new FourPartVoicing(84,76,72,60), // I root（Soprano=C）
};
var (chords, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, opts, v);
// cadences[1].Type == Authentic
// cadences[1].IsPerfectAuthentic == true （Strict 条件を満たす場合）
```

CLI 例（Strict PAC の効果はボイシングが必要なため、JSON では PAC フラグの有無で確認するのが確実です）:

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "I; V; I" --segments --trace --json
```

最小 JSON 確認（例）:

```json
{
  "cadences": [
    { "indexFrom": 0, "type": "Half", "pac": false, "cad64": false, "sixFour": "None" },
    { "indexFrom": 1, "type": "Authentic", "pac": false, "cad64": false, "sixFour": "None" }
  ]
}
```

ヒント:

- 反転やドミナント拡張（V7/V9）がある場合は Authentic でも PAC にはならない設計です。
- 最終 I のソプラノ=主音を要求するため、`FourPartVoicing` を渡してください。
- Ger65（= enharmonic vii°7/V）絡みの誤検出を避けるため、直接 I に進む遷移は Authentic に分類しません。通常は Ger65→V→I のみが終止として評価されます。

PowerShell のクォートについて（Windows）

- 複合値（例: --roman や --pcs の「;」区切り）は二重引用符で囲ってください。
- 例: `--pcs "0,4,7; 7,11,2; 0,4,7"` / `--roman "I; V7; I"`

関連ドキュメント: [PAC/導音解決と抑止規則（設計ノート）](Docs/PAC-and-LeadingTone.md)

### 最小 JSON スニペット（各 Strict PAC オプション）

以下は、各フラグを有効化したときに Authentic が pac=false になる最小確認用の JSON 抜粋です。対応する VS Code タスク名も併記します。

- StrictPacRequireSopranoTonic（タスク: `cli: json (Strict PAC: soprano tonic)`）

  ```json
  {
    "chords": [ { "roman": "I" }, { "roman": "V" }, { "roman": "I" } ],
    "cadences": [
      { "indexFrom": 1, "type": "Authentic", "pac": false, "cad64": false, "sixFour": "None" }
    ]
  }
  ```

- StrictPacDisallowDominantExtensions（タスク: `cli: json (Strict PAC: no dominant ext)`）

  ```json
  {
    "chords": [ { "roman": "I" }, { "roman": "V7" }, { "roman": "I" } ],
    "cadences": [
      { "indexFrom": 1, "type": "Authentic", "pac": false, "cad64": false, "sixFour": "None" }
    ]
  }
  ```

- StrictPacPlainTriadsOnly（タスク: `cli: json (Strict PAC: triads only)`）

  ```json
  {
    "chords": [ { "roman": "I" }, { "roman": "V" }, { "roman": "I7" } ],
    "cadences": [
      { "indexFrom": 1, "type": "Authentic", "pac": false, "cad64": false, "sixFour": "None" }
    ]
  }
  ```

- StrictPacRequireSopranoLeadingToneResolution（タスク: `cli: json (Strict PAC: LT resolve)`）

  ```json
  {
    "chords": [ { "roman": "I" }, { "roman": "V7" }, { "roman": "I" } ],
    "cadences": [
      { "indexFrom": 1, "type": "Authentic", "pac": false, "cad64": false, "sixFour": "None" }
    ]
  }
  ```

備考:

- 上記は最小抜粋です。実行時の `options.*` や `warnings` などは省略しています。
- Strict PAC の一部（ソプラノ主音・導音解決）はボイシングが必要です。CLI デモでは pac=false で厳格条件を満たしていないことを確認するのが確実です。

関連: IAC の最小 JSON 抜粋は README 末尾の「最小例（IAC の JSON 抜粋）」セクションを参照。

#### IAC vs Strict PAC（短い対比）

- IAC（Imperfect Authentic）は「Authentic（V→I 系）」だが PAC 条件を満たさないため pac=false になるケース。代表例は V6→I と V→I6。最小 JSON は README 末尾の該当セクションを参照。
- Strict PAC フラグは PAC をさらに厳格にするためのオプション群（素朴 triad 限定／ドミナント拡張禁止／ソプラノ=主音／導音の上行解決）。通常 pac=true になる並びでも、これらの条件で pac=false になり得ます。
- 使い分けの目安:
  - 「Authentic だが PAC ではない」ことを素直に確認したい → IAC 最小例（V6→I / V→I6）
  - 教材/採点で PAC の要件を厳しくしたい → Strict PAC の各タスク（soprano tonic / no dominant ext / triads only / LT resolve）


## クイックスタート

PowerShell でコピペ実行できます。

```powershell
# ビルド & テスト（Release）
dotnet build -c Release
dotnet test -c Release --nologo --no-build

# CLI（ローマ数字の最小デモ）
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "I; V; I" --segments --trace

# モジュレーション推定（プリセット）
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release `
  --pcs "0,4,7; 7,11,2; 0,4,7; 2,6,9,0; 7,11,2; 4,7,11; 9,0,4; 2,6,9; 7,11,2" `
  --segments --trace --preset stable

# Aug6 と bVI7 の曖昧時に bVI7 を優先（--preferMixture7）
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release `
  --roman "Ger65; bVI7; V; I" --preferMixture7 --trace

# カバレッジ（Cobertura 収集 → HTML 出力）
dotnet test -c Release --nologo --no-build --results-directory Tests/MusicTheory.Tests/TestResults `
  --collect 'XPlat Code Coverage' -- `
  DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura
reportgenerator -reports:Tests/MusicTheory.Tests/TestResults/**/coverage.cobertura.xml `
  -targetdir:Tests/MusicTheory.Tests/TestResults/coverage-report -reporttypes:Html
Start-Process -FilePath 'Tests/MusicTheory.Tests/TestResults/coverage-report/index.html'

# サンプル WPF の起動
dotnet run --project Samples/NoteValueZoom.Wpf/NoteValueZoom.Wpf.csproj -c Release
```

VS Code タスク（推奨）:

- `dotnet: build`, `dotnet: test`, `dotnet: test (no build)`
- `cli: demo (roman)`, `cli: demo (modulation C->G preset stable|permissive)`
- `cli: json (roman demo|6-4 passing|maj7Inv minimal|modulation preset ...)`
- `cli: json (IAC: V6->I|IAC: V->I6)`
- `cli: json (Strict PAC: soprano tonic|Strict PAC: no dominant ext|Strict PAC: triads only|Strict PAC: LT resolve)`
- `coverage: full stable (simple)`, `coverage: open`
- `coverage: full stable (simple)`, `coverage: full+check+open stable`, `coverage: open`
- `wpf: run`

再現マッピング（代表例 → タスク名）:

- V9 表示トグル（V9 ⇄ V7(9)）→ `cli: json (V9)` / `cli: json (V7(9))`
- Cadential 6-4 を V64-53 表示 → `cli: demo (cadential 6-4 as dominant)`
- Cadential 6-4（JSON）→ `cli: json (cadential 6-4)` / `cli: json (cadential 6-4 as dominant)`
- Neapolitan を常に bII6 → `cli: demo (Neapolitan enforceN6)`
- 一般 6-4: Passing → `cli: demo (6-4 passing)` / Pedal → `cli: demo (6-4 pedal)`
- Mixture-7th 警告（bVI7/bII7 など）→ `cli: demo (mixture 7th)`
- bVI7 転回（7/65/43/42）→ `cli: demo (mixture 7th inversions)`
- 二次導音の転回（vii°7/V 系）→ `cli: demo (secondary LT inversions)`
- IAC（Authentic だが PAC=false）→ `cli: json (IAC: V6->I)` / `cli: json (IAC: V->I6)`
- PAC 最小例（Authentic かつ pac=true）→ `cli: json (roman demo)`
- Strict PAC（ソプラノ=トニックを要求）→ `cli: json (Strict PAC: soprano tonic)`
- Strict PAC（ドミナント拡張を禁止）→ `cli: json (Strict PAC: no dominant ext)`
- Strict PAC（終止 I は triad 限定）→ `cli: json (Strict PAC: triads only)`
- Strict PAC（導音→主音の解決を要求）→ `cli: json (Strict PAC: LT resolve)`
- モジュレーション（プリセット）→ `cli: demo (modulation C->G preset stable|permissive)` / JSON 版 → `cli: json (modulation preset stable|permissive)`
- 音価ユーティリティ（Roman/PCS）→ `cli: util (romanJson demo)` / `cli: util (pcsJson demo)`

### 二次Triadの転回表記（V/x・vii°/x の 6/64）

二次機能の三和音でも、voicing が与えられベース音が確定できる場合、転回サフィックス（6 / 64）を付与します。

- 対象: `V/x` と `vii°/x` の triad（distinct なピッチクラスが3音のとき）
- 条件: `FourPartVoicing` など、現在和音のベース音が判定できる voicing を渡すこと
- 付与規則: ベースが第3音なら `6`、第5音なら `64`、根音ならサフィックスなし

例（Cメジャー、ターゲット ii と V の二次機能）:

```csharp
var key = new Key(60, true); // C major
int Pc(int m) => ((m % 12) + 12) % 12;

// V/ii = A C# E（pc: 9, 1, 4）
var v_over_ii = new[]{ Pc(9), Pc(1), Pc(4) };

// 1転回（C#がベース）→ V6/ii
var v1 = new FourPartVoicing(85, 73, 69, 61); // Bass=61 (C#)
var r_v6ii = HarmonyAnalyzer.AnalyzeTriad(v_over_ii, key, v1, null);
// r_v6ii.RomanText == "V6/ii"

// 2転回（Eがベース）→ V64/ii
var v2 = new FourPartVoicing(88, 76, 72, 64); // Bass=64 (E)
var r_v64ii = HarmonyAnalyzer.AnalyzeTriad(v_over_ii, key, v2, null);
// r_v64ii.RomanText == "V64/ii"

// vii°/V = F# A C（pc: 6, 9, 0）
var viio_over_V = new[]{ Pc(6), Pc(9), Pc(0) };

// 1転回（Aがベース）→ vii°6/V
var v3 = new FourPartVoicing(81, 76, 69, 69); // Bass=69 (A)
var r_viio6V = HarmonyAnalyzer.AnalyzeTriad(viio_over_V, key, v3, null);
// r_viio6V.RomanText == "vii°6/V"

// 2転回（Cがベース）→ vii°64/V
var v4 = new FourPartVoicing(84, 72, 72, 60); // Bass=60 (C)
var r_viio64V = HarmonyAnalyzer.AnalyzeTriad(viio_over_V, key, v4, null);
// r_viio64V.RomanText == "vii°64/V"
```

注意:

- Triad への 6/64 付与は distinct PC が3音のときのみです。4音以上では seventh/tension を優先します。
- 二次機能の seventh（V7/x・vii°7/x・viiø7/x）では従来通り 7/65/43/42 を用います。
- 既定のラベリングで `V/ii` や `vii°/V` が出た場合でも、voicing が与えられベースが第3/第5音であることが明確なら、分析器側で自動的に `V6/ii`/`V64/ii`、`vii°6/V`/`vii°64/V` に補強されます（三和音のとき）。

補足（オプション指定）

- 解析規則の曖昧部をインスタンスオプションで切替可能になりました（`HarmonyOptions`）。
- `HarmonyAnalyzer.AnalyzeTriad` と `ProgressionAnalyzer` にオプション付きオーバーロードを用意しています。

### Neapolitan (bII) の例

```csharp
var key = new Key(60, true); // C major
var bII = new[] { 1, 5, 8 }; // Db F Ab
var r_bII = HarmonyAnalyzer.AnalyzeTriad(bII, key);
// r_bII.RomanText == "bII"
```

#### Neapolitan（bII）の診断（警告）

Neapolitan を検出した際、慣用に基づく非破壊の警告を付与します。

- すべての bII 形（triad）に「Neapolitan: typical resolution to V」（V への解決ヒント）
- ルート位置（bII）および第二転回（bII64）の場合に「Neapolitan: prefer bII6 (first inversion)」（第3音ベース推奨）

取得例（Warnings は解析成功を妨げません）:

```csharp
var key = new Key(60, true); // C major
int Pc(int m) => ((m % 12) + 12) % 12;

// ルート位置の bII（三和音）
var bII_root = new[] { Pc(61), Pc(65), Pc(68) }; // Db F Ab
var r1 = HarmonyAnalyzer.AnalyzeTriad(bII_root, key);
// r1.RomanText == "bII"
// r1.Warnings には "prefer bII6" と "typical resolution to V" が含まれる

// 第二転回（bII64）— ベース=第5音（Ab）になるよう voicing を指定
var v64 = new FourPartVoicing(81, 77, 69, 68); // S/A/T 任意, Bass=68(Ab)
var r2 = HarmonyAnalyzer.AnalyzeTriad(bII_root, key, v64, null);
// r2.RomanText == "bII64"
// r2.Warnings にも上記のヒントが含まれる
```

#### Neapolitan を bII6 へ強制（--enforceN6）

CLI の `--enforceN6` を使うと、Neapolitan（三和音の bII）を常に第一転回（bII6）として正規化します（七和音 bII7 には非適用）。

- roman 入力の例: `"bII; V; I"` に `--enforceN6` を付けると、最初の bII が bII6 として出力されます。
- pcs 入力の例: `--key C --pcs "1,5,8" --enforceN6` でも bII6 に正規化されます（Db–F–Ab）。

備考:

- 既定では強制されません（互換重視）。教育的プリセット `HarmonyOptions.Pedagogical` では強制が有効です。
- voicing を与えた場合でも、Root/64 は 6 に正規化され、既に 6 の場合はそのまま維持されます。
- minor 調でも同様に bII6 へ統一されます。

#### bII6 → V（または V7）の声部進行ヒント

典型的な解決は V（または V7）です。Cメジャーの例（bII6 = Db–F–Ab, ベース=F）を念頭に、次の傾向音を意識すると滑らかです。

- b6（Ab）→ 5（G）: 半音下行。
- b2（Db）→ 2（D）: 半音上行。I6 へ進む場合は 1（C）への下行も慣用的です。
- ベースの 4（F）:
  - V7 に進む場合は共通音として保持（V7 の第7音）。
  - V（三和音）の場合は 3（E）または 1（G）へ移動し、並行完全を回避。

注意:

- 進行先が V7 だと F を共通音にできるため、平滑な連結になりやすいです。
- 5度や8度の並行に注意し、上記の半音解決（Db→D / Ab→G）を優先すると衝突が起きにくくなります。

##### 最小 CLI デモ

```powershell
# roman 入力（--enforceN6 で bII を bII6 に統一）
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "bII; V; I" --enforceN6 --trace

# pcs 入力（C: Db–F–Ab を bII6 として解釈）
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --key C --pcs "1,5,8; 7,11,2; 0,4,7" --enforceN6 --trace
```

### 検出順序（サマリ）

HarmonyAnalyzer は以下の順序で確定的に評価します（早期リターン + 最終安全チェック）。

1. 前処理: 入力PCを正規化し、distinct セット/順序を得る。
2. 4音以上なら seventh/tension 優先（triad 分岐はスキップ）。
3. V9 早期判定: {1,3,b7,9}(+任意5度) のみ・4–5音・許容外音が無い場合に "V9" で確定。

4. Augmented Sixth 早期判定: voicing 指定あり、bass=b6、音集合一致（It6/Fr43/Ger65）。

- ソプラノ=b6 の場合はオプションで抑制可能（慣用的な bVI7 配置を尊重）。
- 成立時はダイアトニック7thや bVI7 より優先して確定（衝突回避）。

1. 借用7th（iv7 / bVII7 / bII7 / bVI7）+ ボイシング有: ベース音で 7/65/43/42 を付与し確定。

1. ダイアトニック7th:（必要に応じて転回を付与）確定。
1. 借用7th（ボイシング無）: ルート表記で確定。
1. Triad 分岐: distinct PC がちょうど3音のときのみ triad を試行（ダイアトニック → ミクスチャ）。減三和音は “°”。転回 6/64 はこの分岐でのみ付与。
1. 最終安全: どの経路でも最後に V9 を再チェック（triad への誤ラベル抑止）。ボイシング有かつ4音以上では借用7th 転回の再確認も実施。
1. 付随: Voice leading の警告/エラー（音域/並び/重なり/並行完全協和）はローマ数字確定後に付与。

要点:

- Triad の転回は「distinct 3音」のときのみ。4音以上は seventh/tension を優先。

#### 判定フローチャート（簡易）

```text
入力PC → distinct化 → 音数チェック
  ├─ 4–5音? ── Yes ─→ V9 早期判定（{1,3,b7,9}[+5] 以外が含まれていないか）
  │                      ├─ 一致 → "V9" 決定
  │                      └─ 不一致 → 借用7th(ボイシング有) → ダイアトニック7th → 借用7th → 最終 V9 再チェック
  └─ 3音? ──── Yes ─→ Triad（ダイアトニック → ミクスチャ）
                         ↳ 減三和音には "°"、転回は 6/64。

      （どの経路でも最終段で V9 再チェック。ボイシング有かつ4音以上は借用7th転回も再確認）
```

#### 既知の制限 / 注意点（Harmony）

- V9 は {1,3,b7,9}(+任意5度) のみで判定します（distinct 4–5音）。それ以外の構成音を含むと V9 にはなりません。
- Triad の転回サフィックスは distinct PC が3音のときのみ付与します。4音以上では triad としてはラベルされません。
- 借用7th は iv7 / bVII7 / bII7 / bVI7 に対応（メジャー基準）。その他の借用七和音は未対応です。
- 借用 triad は i / iv / bIII / bVI / bVII（+ Neapolitan bII）を認識。七和音のミクスチャは上記に限定。
- Seventh 転回の付与には FourPartVoicing を渡す必要があります（未指定時はルート表記）。
- 入力はピッチクラス（0..11）で評価され、オクターブ差は無視されます（重複は自動で除外）。
- 4音以上では Seventh/Tension を優先するため、Triad ラベルが欲しい場合はテンション/第7音を含めないでください。

補足（混合和音の転回付与の判定順）

- bVII/bVI/bIII/bII や iv/i など、接頭辞が重なり得るパターンは「より長い接頭辞を先に」評価します。
  - 例: bVII → bVI → bIII → bII、iv → i の順。
  - これにより bVII が bVI に、bIII が bII に誤って一致することを防ぎ、ベース音に応じた 6/64 図形が正しく付与されます。

補足（Augmented Sixth の棲み分け）

- Augmented Sixth（It6/Fr43/Ger65）は voicing が必要で、ベースが必ず b6 であることを要求します。
- bVI7 と構成音が同一になるケース（例: C major の Ab–C–Eb–Gb）は、以下の決定規則で衝突回避します。
  - voicing ありかつ Aug6 条件を満たす → Aug6 を優先（例: "Ger65"）。
  - さらにソプラノまで b6 の場合は、慣用的に bVI7 とみなす配置を尊重し、Aug6 ラベルを抑制します。
  - voicing なし → Aug6 は判定しないため、bVI7（ルート表記）となります。

補足（二次導音和音の優先）

- vii°7/x（完全減七）で多義的な一致がある場合は /V を強優先します（例: vii°7/V を vii°7/iii より優先）。
- 短調の iiø7 はダイアトニックとして二次解釈より優先されます。
- `/IV` をターゲットにする二次導音（例: `viiø/iv`, `viiø7/iv`）は、半減七の綴りにおいて B♮ を採用します。Cメジャーでは E–G–B–D（pc: 4, 7, 11, 2）を返します（B♭ではなくBナチュラル）。

#### FAQ / Tips（Harmony）

- 借用 iv7 を期待したのに iv6 と出る
  - distinct PC が3音のときは triad 扱いになり、ベースが第3音なら「6」が付きます。iv7 を得たい場合は第7音を加えて4音（重複除外後）にし、可能であれば FourPartVoicing を渡してください（転回 7/65/43/42 が付与されます）。
- V9 が誤検出される/されない気がする
  - V9 は {1,3,b7,9}(+任意5度) のみ・4–5音・許容外音なしで成立します。余計なテンション（例: 11, 13 など）が混在すると V9 にはなりません。音集合の distinct 化は内部で行われます。
- Triad の転回が出ない
  - 4音以上では Seventh/Tension 優先のため triad の 6/64 は付与しません。triad 転回が必要な場合は distinct 3音にしてください。

### 借用7th の転回表記（概要）

メジャーでの iv7（F–Ab–C–Eb）/ bVII7（Bb–D–F–Ab）/ bII7（Db–F–Ab–Cb(B)）/ bVI7（Ab–C–Eb–Gb）を認識し、ベース音に応じて以下を付与します。

- 根音: 7（例: iv7, bVII7）
- 第3音: 65（例: iv65, bVII65）
- 第5音: 43（例: iv43, bVII43）
- 第7音: 42（例: iv42, bVII42）

四和音が検出された場合は Seventh を優先し、triad と取り違えないよう厳密な音数一致で判定します。

例（Cメジャー、iv65 のラベリング）:

```csharp
var key = new Key(60, true); // C major
int Pc(int m) => ((m % 12) + 12) % 12;
var iv7 = new[]{ Pc(65), Pc(68), Pc(72), Pc(75) }; // F Ab C Eb
var v = new FourPartVoicing(86, 81, 74, 68);      // Ab がベース → 65
var r = HarmonyAnalyzer.AnalyzeTriad(iv7, key, v, null);
// r.RomanText == "iv65"
```

例（Cメジャー、bVII43 のラベリング）:

```csharp
var key = new Key(60, true); // C major
int Pc(int m) => ((m % 12) + 12) % 12;
var bVII7 = new[]{ Pc(70), Pc(74), Pc(77), Pc(80) }; // Bb D F Ab
var v2 = new FourPartVoicing(89, 82, 77, 65);        // F がベース → 43
var r_bVII = HarmonyAnalyzer.AnalyzeTriad(bVII7, key, v2, null);
// r_bVII.RomanText == "bVII43"
```

例（Cメジャー、bII7 と転回）:

```csharp
var key = new Key(60, true); // C major
int Pc(int m) => ((m % 12) + 12) % 12;
var bII7 = new[]{ Pc(61), Pc(65), Pc(68), Pc(71) }; // Db F Ab B

// ルート位置
var vRoot = new FourPartVoicing(85, 77, 69, 61);
var r_bII7 = HarmonyAnalyzer.AnalyzeTriad(bII7, key, vRoot, null);
// r_bII7.RomanText == "bII7"

// 1転回（F がベース）→ bII65
var v65 = new FourPartVoicing(86, 81, 73, 65);
var r_bII65 = HarmonyAnalyzer.AnalyzeTriad(bII7, key, v65, null);
// r_bII65.RomanText == "bII65"
```

例（Cメジャー、bVI7 と転回）:

```csharp
var key = new Key(60, true); // C major
int Pc(int m) => ((m % 12) + 12) % 12;
var bVI7 = new[]{ Pc(68), Pc(72), Pc(75), Pc(66) }; // Ab C Eb Gb

// ルート位置
var vRoot_bVI = new FourPartVoicing(92, 84, 75, 68);
var r_bVI7 = HarmonyAnalyzer.AnalyzeTriad(bVI7, key, vRoot_bVI, null);
// r_bVI7.RomanText == "bVI7"

// 1転回（C がベース）→ bVI65
var v_bVI65 = new FourPartVoicing(91, 84, 79, 72);
var r_bVI65 = HarmonyAnalyzer.AnalyzeTriad(bVI7, key, v_bVI65, null);
// r_bVI65.RomanText == "bVI65"
```

さらに転回の例（bVI43 / bVI42）:

```csharp
// 2転回（Eb がベース）→ bVI43
var v_bVI43 = new FourPartVoicing(91, 84, 79, 75);
var r_bVI43 = HarmonyAnalyzer.AnalyzeTriad(bVI7, key, v_bVI43, null);
// r_bVI43.RomanText == "bVI43"

// 3転回（Gb がベース）→ bVI42
var v_bVI42 = new FourPartVoicing(90, 81, 78, 66);
var r_bVI42 = HarmonyAnalyzer.AnalyzeTriad(bVI7, key, v_bVI42, null);
// r_bVI42.RomanText == "bVI42"
```

## Augmented Sixth (It6/Fr43/Ger65)

Augmented Sixth は次の音集合で検出します（トニックを 1 として）。

- It6: { b6, 1, #4 }
- Fr43: { b6, 1, 2, #4 }
- Ger65: { b6, 1, b3, #4 }

判定ルール:

- voicing が必須で、ベースは必ず b6（例: C major なら Ab）。
- bVI7 と構成音が一致する場合は、上記の「棲み分け」規則に従い決定的に選好します。

例（C major, German 六の和音）:

```csharp
var key = new Key(60, true); // C major
int Pc(int m) => ((m % 12) + 12) % 12;
var ger = new[]{ Pc(68), Pc(60), Pc(63), Pc(66) }; // Ab C Eb Gb
var v = new FourPartVoicing(86, 81, 75, 68);      // bass Ab=b6
var r = HarmonyAnalyzer.AnalyzeTriad(ger, key, v, null);
// r.RomanText == "Ger65"
```

### Aug6 と bVI7 の比較（要点）

- 構成音の一致: Cメジャーの Ab–C–Eb–Gb は Ger65 と bVI7 で同一の PC セットになります。
- 判定に必要な情報:
  - Aug6: voicing が必須、ベース=b6 を要求（It6/Fr43/Ger65）。
  - bVI7: voicing なしでも 4音なら Seventh として検出（転回は voicing があれば 7/65/43/42 を付与）。
- 既定の優先:
  - bass=b6 かつ Aug6 条件合致 → Aug6 を優先（例: Ger65）。
  - ただし「ソプラノ=b6」のときは慣用配置を尊重し、Aug6 表示を抑制して bVI7 系にします（`DisallowAugmentedSixthWhenSopranoFlat6=true`）。
- オプションによる反転:
  - `PreferMixtureSeventhOverAugmentedSixthWhenAmbiguous=true` または CLI `--preferMixture7` で曖昧時に bVI7 を先に評価。
  - 表示のみの切替ではなく、ラベリングの決定順序を入れ替えます（抑制設定とも両立）。
- クイックデモ（VS Code タスク）:
  - 既定: `cli: demo (Aug6 vs bVI7)`
  - bVI7 優先: `cli: demo (Aug6 vs bVI7 prefer mixture)`


## HarmonyOptions（判定のタイブレーク/好みの調整）

Augmented Sixth と bVI7 の棲み分けや、二次導七の /V 優先など、分析上の“好み”を `HarmonyOptions` で切替できます。既定はテストで検証済みの安定挙動です。

主な項目:

- PreferAugmentedSixthOverMixtureWhenBassFlat6: bass=b6 で Aug6 条件合致時は bVI7 より Aug6 を優先（既定: true）
- DisallowAugmentedSixthWhenSopranoFlat6: ソプラノも b6 のときは Aug6 表示を抑制（慣用的な Ab7 配置を bVI7 とする）（既定: true）
- PreferSecondaryLeadingToneTargetV: 完全減七の曖昧一致時は vii°7/V を強優先（既定: true）
- PreferDiatonicIiHalfDimInMinor: 短調では二次解釈より iiø7 を優先（既定: true）
- EnforceNeapolitanFirstInversion: Neapolitan（bII）の三和音を常に bII6 に正規化（既定: false）
- ShowNonCadentialSixFour: Passing/Pedal の 6-4 を詳細カデンツ結果に表示（既定: true）。false にすると「非カデンツ項目（Type==None）の 6-4 表示のみ抑制」し、Authentic 等のカデンツ項目に付随する 6-4 情報は表示対象のままです。
- PreferCadentialSixFourAsDominant: カデンツ 6-4（I64→V→I）を記譜上 V64-53（属機能）として扱う（既定: false）。詳細カデンツ解析で I64 を V64-53 にリラベルします（機能は Dominant）。
- PreferV7Paren9OverV9: ドミナント9th の表記を "V9" ではなく "V7(9)" に切り替えます（既定: false）。機能判定は V のまま、表示テキストのみ変更します。
  - プリセット: `HarmonyOptions.NotationV7Paren9` で簡単に切替できます。
- IncludeMajInSeventhInversions: ダイアトニックのメジャーセブンス和音（Imaj7/IVmaj7/VImaj7 等）の転回表記に「maj」を含めます（既定: false）。
  - 例（Cメジャーの IVmaj7=F–A–C–E）: 既定では 1転回=IV65, 2転回=IV43, 3転回=IV42。オプション有効時は IVmaj65/IVmaj43/IVmaj42。
  - ルート位置は常に "IVmaj7" のまま（オプションに依らず）。短調の IIImaj7/VImaj7 にも適用されます。
- PreferMixtureSeventhOverAugmentedSixthWhenAmbiguous: Aug6（It6/Fr43/Ger65）と bVI7 が同一の PC セットで曖昧な場合、Mixture 7th を先に試みます（既定: false）。
  - 例: Cメジャーの Ab–C–Eb–Gb（=Ger65 と同音集合）。voicing があり bass=b6 でも、このオプション有効時は bVI7 系を優先的に評価します（ソプラノ=b6 の慣用抑制とも両立）。

補足（ポリシーの要点）

- 終止エントリ（CadenceInfo.Type ≠ None）には一般 6-4（Passing/Pedal）は付与されません。カデンツ 6-4 は Cadential のみ付随します。

使い方（個別呼び出し）:

```csharp
var opts = new HarmonyOptions { DisallowAugmentedSixthWhenSopranoFlat6 = false };
var res = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, voicing);
```

Neapolitan を常に第一転回（bII6）で統一したい場合:

```csharp
var opts = new HarmonyOptions { EnforceNeapolitanFirstInversion = true };
var key = new Key(60, true); // C major
int Pc(int m) => ((m % 12) + 12) % 12;
var pcs = new[]{ Pc(61), Pc(65), Pc(68) }; // Db F Ab
var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts);
// r.RomanText == "bII6"
```

具体例（Aug6 と bVI7 のトグル）:

```csharp
// C major で Ab–C–Eb–Gb（Ger65 と同音集合）。
// ベース=Ab(b6)、ソプラノ=Ab の配置（慣用的に Ab7 と読むことが多い）。
int Pc(int m) => ((m % 12) + 12) % 12;
var key = new Key(60, true);
var pcs = new[] { Pc(68), Pc(60), Pc(63), Pc(66) };      // Ab C Eb Gb
var v   = new FourPartVoicing(80, 75, 63, 68);           // S=E♭, A=C, T=E♭, B=Ab（S も Ab にすれば b6/b6 配置）

// 既定: ソプラノが b6 の場合は Aug6 ラベルを抑制 → bVI7（または転回表記）
var r1 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, HarmonyOptions.Default, v);
// 例: r1.RomanText == "bVI7"（または bVI65/43/42）

// トグル: 抑制を無効化すると、同じ配置でも Aug6 を優先 → Ger65
var opts = new HarmonyOptions { DisallowAugmentedSixthWhenSopranoFlat6 = false };
var r2 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v);
// r2.RomanText == "Ger65"
```

進行解析で一括適用:

```csharp
var opts = new HarmonyOptions { PreferAugmentedSixthOverMixtureWhenBassFlat6 = true };
var prog = ProgressionAnalyzer.Analyze(seq, key, opts, voicings);
var (prog2, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, opts, voicings);

// 非カデンツ 6-4 を隠す例（Passing/Pedal の単独項目を出さない）
var hide = new HarmonyOptions { ShowNonCadentialSixFour = false };
var (_, cadencesNo64) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, hide, voicings);

// 出力例（概念図）
// ShowNonCadentialSixFour = true のとき
//   [
//     CadenceInfo { IndexFrom = 0, Type = None, SixFour = Passing },
//     CadenceInfo { IndexFrom = 1, Type = Authentic, HasCadentialSixFour = True, SixFour = Cadential }
//   ]
// ShowNonCadentialSixFour = false のとき
//   [
//     // 非カデンツ項目(Type==None)に付随する Passing/Pedal は抑制
//     CadenceInfo { IndexFrom = 1, Type = Authentic, HasCadentialSixFour = True, SixFour = Cadential }
//   ]

// V9 表記の切替例
var optsV9 = new HarmonyOptions { PreferV7Paren9OverV9 = true };
var v9_full = new[]{ Pc(67), Pc(71), Pc(74), Pc(65), Pc(69) }; // G B D F A (Cメジャーの V9)
var res = HarmonyAnalyzer.AnalyzeTriad(v9_full, new Key(60, true), optsV9);
// res.RomanText == "V7(9)"  // デフォルト設定では "V9"

// プリセット利用
var res2 = HarmonyAnalyzer.AnalyzeTriad(v9_full, new Key(60, true), HarmonyOptions.NotationV7Paren9);
// res2.RomanText == "V7(9)"
// cadencesNo64 内には Type==None かつ SixFour==Passing/Pedal の項目は含まれません

#### クイックリファレンス（設定抜粋）

```csharp
// 非カデンツ 6-4（Passing/Pedal）を一覧から隠す
var optsHide64 = new HarmonyOptions { ShowNonCadentialSixFour = false };

// V9 を V7(9) 表記にする（表示のみ変更）
var optsV7n9 = HarmonyOptions.NotationV7Paren9;

// 教育的プリセット（例: Neapolitan は常に bII6）
var optsPed = HarmonyOptions.Pedagogical;
```

プリセット:

```csharp
// 教育的な出力重視（Neapolitanは常に bII6）
var opts = HarmonyOptions.Pedagogical;
```

既存の `HarmonyRules` 静的プロパティは既定オプションのプロキシです（後方互換）。

キー推定（Options）と併用:

```csharp
var keyOpts = new KeyEstimator.Options { Window = 1, CollectTrace = true };
var harmonyOpts = new HarmonyOptions { PreferSecondaryLeadingToneTargetV = true };

// トレース付き
var (res, segments, keys) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, new Key(60, true), keyOpts, harmonyOpts, out var trace, voicings);

// 信頼度付き（withConfidence）
var (res2, segsWithConf, keys2) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, new Key(60, true), keyOpts, harmonyOpts, out var trace2, voicings, withConfidence: true);
```

## 詳細カデンツ API（PAC/IAC とカデンツ6-4）

詳細情報（PAC/IAC 近似、カデンツ 6-4 フラグ付き）でカデンツを取得できます。

補足: CadenceInfo.IndexFrom は、直前の和音（prev）から現在の和音（curr）への遷移を表すときの「prev 側（開始）」のインデックスです。

```csharp
var key = new Key(60, true);
int Pc(int m) => ((m % 12) + 12) % 12;
var I = new[]{ Pc(60), Pc(64), Pc(67) };
var V = new[]{ Pc(67), Pc(71), Pc(62) };
var seq = new[] { I, V, I };
var voicings = new FourPartVoicing?[]
{
  new FourPartVoicing(81, 76, 72, 67), // I64 (G がベース)
  new FourPartVoicing(83, 79, 74, 67), // V root
  new FourPartVoicing(84, 76, 72, 60), // I root
};
var (chords, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
// cadences[0].Type == Authentic
// cadences[0].HasCadentialSixFour == true
// cadences[0].IsPerfectAuthentic == true
// 追加: cadences[i].SixFour は Cadential/Passing/Pedal の何れか（該当なしは None）
```

Half を抑制するロジック:

- 直前が I64 で現時点が V の場合（I64→V）、Half を抑制して次の Authentic（V→I）に集約します。

### 6-4分類（Passing / Pedal）

要点: 中間が「64」で前後が同一和音なら Passing / Pedal 判定対象。終止項目には付与せず、カデンツ 6-4 は別扱い（`Cadential`）。

最新ポリシー（voicing-first）：

1. Voicing（三つの連続和音のベース）が利用可能で、前後の和音が同じ Roman ヘッド（例: IV / IV）で中間が 64 のとき、まず実際のベース音の動きで判定します。

- ベースが「(根音) → (5度) → (3度)」または「(3度) → (5度) → (根音)」の線形移動になっている場合は Passing 6-4（上行・下行の両方向を許容）。
- ベースが最初と最後で同一、かつ中間だけ 5度（もしくは他声が保持）になって“踏み留まる”場合は Pedal 6-4。

1. Voicing が欠ける / 判定が不十分な場合はローマ数字サフィックスのヒューリスティックにフォールバック：

- 3つ並びの中央が 64 で、直後の和音が 6 を伴う（例: IV → IV64 → IV6）なら Passing。
- それ以外（例: IV → IV64 → IV）を Pedal。

1. 外側 2 和音の Roman ヘッドが異なる場合は非判定（SixFour=None）。

この優先順により従来 Pedal と誤って分類されやすかった降行パターン（IV6 → IV64 → IV）が Passing と正しく認識されます。

判定は `ProgressionAnalyzer.AnalyzeWithDetailedCadences` の内部で行い、結果は `CadenceInfo.SixFour` に格納されます。

詳細カデンツAPIでは、Cadential（I64→V→I）以外にも Passing / Pedal 6-4 を自動識別します。

重要:

- 終止エントリ（Authentic/Plagal/Half/Deceptive）には一般 6-4（Passing/Pedal）は付与されません。カデンツ 6-4 は Cadential のみ付随します。

- Passing: I → V64 → I6 のような進行で V64 を Passing 6-4 と分類
- Pedal: IV → IV64 → IV のような進行で IV64 を Pedal 6-4 と分類

分類は `ProgressionAnalyzer.AnalyzeWithDetailedCadences` の `CadenceInfo.SixFour` フィールドで取得できます。ベースボイスを渡さない場合はフォールバック規則になるため、上行・下行 Passing の精度を高めるには voicing を提供してください。

#### 検出例（一般6-4は「非カデンツの遷移」にのみ付与）

```csharp
var key = new Key(60, true); // C major
int Pc(int m) => ((m % 12) + 12) % 12;
// 非カデンツの例: IV → IV64 → IV6（同一和音内の 6-4）
var IV = new[]{ Pc(65), Pc(69), Pc(72) }; // F A C
var seq = new[] { IV, IV, IV };
var voicings = new FourPartVoicing?[]
{
  new FourPartVoicing(81, 77, 72, 65), // IV root (F)
  new FourPartVoicing(81, 77, 72, 72), // IV64 (C)
  new FourPartVoicing(81, 77, 72, 69), // IV6 (A)
};
var (chords, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
// 非カデンツ遷移に対して Passing が付与される
// cadences[0].SixFour == SixFourType.Passing
```

Pedal 6-4 の例:

```csharp
var IV = new[]{ Pc(65), Pc(69), Pc(72) }; // F A C
var seq2 = new[] { IV, IV, IV };
var voicings2 = new FourPartVoicing?[]
{
  new FourPartVoicing(81, 77, 72, 65), // IV root (F)
  new FourPartVoicing(81, 77, 72, 72), // IV64 (C)
  new FourPartVoicing(81, 77, 72, 65), // IV root (F)
};
var (chords2, cadences2) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq2, key, voicings2);
// cadences2[0].SixFour == SixFourType.Pedal
```

降行 Passing 6-4 の例（IV6 → IV64 → IV）:

```csharp
var key = new Key(60, true); // C major
int Pc(int m) => ((m % 12) + 12) % 12;
var IV = new[] { Pc(65), Pc(69), Pc(72) }; // F A C
var seq3 = new[] { IV, IV, IV };
var voicings3 = new FourPartVoicing?[]
{
  new FourPartVoicing(81, 77, 72, 69), // IV6 (A がベース)
  new FourPartVoicing(81, 77, 72, 72), // IV64 (C がベース)
  new FourPartVoicing(81, 77, 72, 65), // IV root (F がベース)
};
var (_, cadences3) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq3, key, voicings3);
// cadences3[0].SixFour == SixFourType.Passing （voicing-first により降行でも Passing）
```

CLI デモ（VS Code タスク `cli: demo (6-4 passing)`）でも同様に `SixFour=Passing` を確認できます。

分類ロジック:

- 中間和音（prevText）が "64" で、前々と次の和音のローマ数字ヘッドが一致する場合、
  - 次が "6" なら Passing
  - それ以外は Pedal

#### 表示ポリシー（抑制オプション）

- HarmonyOptions.ShowNonCadentialSixFour = true（既定）
  - 非カデンツの 6-4（Type==None の Passing/Pedal）も CadenceInfo として列挙します。
- HarmonyOptions.ShowNonCadentialSixFour = false
  - 非カデンツの 6-4 は列挙しません（Type==None かつ SixFour==Passing/Pedal を抑制）。
  - Cadential 6-4（I64→V→I）は Authentic 等のカデンツ項目に付随情報として残ります。

最小例（IV→IV64→IV の Passing 6-4 を抑制）:

```csharp
var opts = new HarmonyOptions { ShowNonCadentialSixFour = false };
var (_, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, opts, voicings);
// cadences には Type==None かつ SixFour==Passing/Pedal の項目は含まれません

注意:
- カデンツ項目（Authentic/Plagal/Half/Deceptive）には一般6-4（Passing/Pedal）は付与されません。
- カデンツ 6-4（I64→V→I）は、該当するカデンツ項目に Cadential としてのみ付随します。
```

## ビルド・テスト

<a id="ビルド--テスト"></a>

プロジェクト全体のビルド/テスト（Release）:

```powershell
# ビルド
dotnet build -c Release

# テスト（すべて: ユニットテスト + 統合テスト）
dotnet test -c Release --nologo

# 統合テストのみ実行
dotnet test -c Release --nologo --filter "FullyQualifiedName~IntegrationTests"
```

**統合テスト**: エンドツーエンドの和声解析フローを検証する統合テストは `Tests/MusicTheory.IntegrationTests` に配置されています。詳細は [INTEGRATION_TESTING.md](INTEGRATION_TESTING.md) を参照してください。

VS Code のタスク（.vscode/tasks.json がある環境）:

- Build: dotnet: build
- Test: dotnet: test
- Test (no build): dotnet: test (no build) — 直前のビルドを流用して高速実行
- Watch Test: dotnet: watch test — 変更監視で継続実行（バックグラウンド）
- Test (TRX): dotnet: test (trx) — TRX ログ出力（CI/レポート向け）
- Coverage (full): coverage: full — build → coverage（collector）→ HTML まで一括実行
- Coverage (full stable): coverage: full stable — build → coverage（安定設定）→ HTML まで一括実行（HTML 生成は simple 実行を使用）
- Coverage (full stable, simple): coverage: full stable (simple) — ReportGenerator がパスにある前提の簡素版
- Coverage (full warm stable): coverage: full warm stable — build → test(no build) → coverage（安定設定）→ HTML（プリウォームで初回の揺れ低減）
- Coverage (full warm stable, simple): coverage: full warm stable (simple) — 上記の簡素版（`reportgenerator` がパスに必要）
- Coverage (open): coverage: open — 生成済み HTML レポートを既定ブラウザで開く
- Test (coverage): dotnet: test (coverage) — Cobertura 形式を生成（`Tests/MusicTheory.Tests/TestResults/**/coverage.cobertura.xml`）
- Test (coverage stable): dotnet: test (coverage stable) — 初回JITの揺れを抑えるための環境変数（`COMPlus_TieredCompilation=0`, `COMPlus_ReadyToRun=0`）を有効にして収集
- カバレッジ（Coverlet/collector + Cobertura）

  - VS Code タスク `dotnet: test (coverage)` を実行すると、`coverlet.collector` により `Tests/MusicTheory.Tests/TestResults/<GUID>/coverage.cobertura.xml` が生成されます（GUID は毎回変わります）。
  - コマンド実行例（PowerShell）:

    ```powershell
    # 事前に Release ビルドを済ませると安定します
    dotnet build -c Release

    # XPlat Code Coverage (Cobertura) を収集し、出力フォルダを指定
    dotnet test -c Release --nologo --no-build `
      --results-directory Tests/MusicTheory.Tests/TestResults `
      --collect 'XPlat Code Coverage' -- `
      DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura
    ```

  - HTML レポート（ローカル）を生成するには ReportGenerator が必要です。
  - VS Code タスク `coverage: html (simple)` はグローバルツール `reportgenerator` を直接呼び出します。未導入の場合は `dotnet tool install -g dotnet-reportgenerator-globaltool` を実行してください。
    - インストール（グローバルツール）:

      ```powershell
      dotnet tool install -g dotnet-reportgenerator-globaltool
      # 既にインストール済みの場合はアップデート
      dotnet tool update -g dotnet-reportgenerator-globaltool
      ```

    - VS Code タスク `coverage: html (simple)` を実行すると、
      `Tests/MusicTheory.Tests/TestResults/coverage-report` に HTML が生成されます（内部で `Tests/MusicTheory.Tests/TestResults/**/coverage.cobertura.xml` を自動検出）。

  - Windows PowerShell ではスクリプトブロックのクオート問題を避けるため、連結タスクは simple 版（`coverage: full stable (simple)` など）を推奨します。単独でも以下のように生成できます。

      ```powershell
      reportgenerator -reports:Tests/MusicTheory.Tests/TestResults/**/coverage.cobertura.xml `
        -targetdir:Tests/MusicTheory.Tests/TestResults/coverage-report -reporttypes:Html
      ```

- TRX ログ（テスト結果ファイル）

  - VS Code タスク `dotnet: test (trx)` で TRX を生成します。出力は `Tests/MusicTheory.Tests/TestResults/` 配下に保存されます。
  - コマンド実行例（PowerShell。セミコロンを含むためクオート必須）:

    ```powershell
    dotnet build -c Release
    dotnet test -c Release --nologo --no-build --logger "trx;LogFileName=test_results.trx"
    ```


小さな安定化のヒント:

- xUnit の並列実行を無効化して断続的な不一致を防ぐ設定を含みます（`Tests/MusicTheory.Tests/xunit.runner.json` と `AssemblyInfo.cs` の `DisableTestParallelization`）。
- C# の `using` はファイル先頭（namespace 直下）に配置してください。`using` の前に宣言や属性があると CS1529 が発生します。
- カバレッジ収集は、可能なら VS Code タスクの「dotnet: test (coverage stable)」または連結タスク「coverage: full stable」の利用を推奨します（初回のJITの揺れを低減）。
  - PowerShell のスクリプトブロックでクオート問題が出る場合は「coverage: html (simple)」「coverage: full stable (simple)」を利用してください（`reportgenerator` がパスに必要）。

## 初回ウォームアップと安定実行の注意

- ライブラリ/テスト双方でモジュール初期化（`ModuleInitializer`）により軽量ウォームアップを実行し、初回JITによる判定ブレを抑えています。
- CI/ローカル実行ともに「`dotnet build -c Release` → `dotnet test -c Release --no-build`」の順にすると、より安定して再現性ある結果が得られます。
- 稀に発生し得る初回の差異（例: `bIII`/`viiø/V`）に対しては自己検証で再パースを一度だけ行い、以降は確定的に動作します。
- 降順 Passing 6-4 (IV6→IV64→IV) は初期 JIT 状態でごく稀に Pedal と誤ラベルされる揺らぎがあったため、`AssemblyWarmUp` に同進行を追加し、さらに安定性テスト（同一進行を2回解析し結果が不変）を追加しました。no-build 直後に失敗した場合は一度 `dotnet build -c Release` を挟んで再実行してください。
- no-build を常用する場合: 軽量化目的でも、ロジックを編集した直後は必ず一度 build を行い最新 DLL を生成してから `--no-build` テストに切り替えることで揺らぎを回避できます。

補足（カバレッジ安定実行）:

- VS Code では「dotnet: test (coverage stable)」タスクが `COMPlus_TieredCompilation=0` / `COMPlus_ReadyToRun=0` を設定して収集します。
- 一括実行は「coverage: full stable」（build → coverage stable → html）を使うと便利です。
- 初回JITのブレをさらに抑えたいときは「coverage: full warm stable」を利用してください（build → test(no build) でウォーム後に coverage）。

PowerShell での手動実行（簡易例）:

```powershell
dotnet build -c Release
dotnet test -c Release --nologo --no-build
$env:COMPlus_TieredCompilation=0; $env:COMPlus_ReadyToRun=0
dotnet test -c Release --nologo --no-build --results-directory Tests/MusicTheory.Tests/TestResults --collect 'XPlat Code Coverage' -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura
reportgenerator -reports:Tests/MusicTheory.Tests/TestResults/**/coverage.cobertura.xml -targetdir:Tests/MusicTheory.Tests/TestResults/coverage-report -reporttypes:Html
```

補足（Duration の連符表記）:

- `DurationNotation.ToNotation(d, extendedTuplets: false)` のときは `(a:b)`、`true` のときは `*a:b` で出力します。
- 例: 付点8分三連は `E(3:2)`（extendedTuplets=false）/ `E*3:2`（extendedTuplets=true）。

## ベンチマーク（サンプル）

<a id="ベンチマーク-サンプル"></a>

BenchmarkDotNet を使った軽量スモーク:

```powershell
dotnet run --project Benchmarks/MusicTheory.Benchmarks.csproj -c Release
```

レポートは `BenchmarkDotNet.Artifacts/results` に出力されます（html / csv / md）。

## CLI サンプルの実行

`Samples/MusicTheory.Cli` は I64→V→I の最小進行を解析し、和音とカデンツ概要を表示します。

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release
```

### デモ出力例（Aug6 vs bVI7）

VS Code タスク `cli: demo (Aug6 vs bVI7)` を実行すると、次のような出力になります（Cメジャー）。

```text
Key: Major C
Options: hide64=off, cad64Dominant=off, V9=V9, maj7Inv=off, preferMixture7=off, enforceN6=off, tuplets=paren

Chords:
[0] Ger65  (Subdominant)
[1] bVI  (Tonic)
[2] V  (Dominant)
[3] I  (Tonic)

Cadences:
@1: Half  PAC=No  Cad64=No  SixFour=None
@2: Authentic  PAC=Yes  Cad64=No  SixFour=None
```

### デモ出力例（二次導音の転回: vii°7/V 7/65/43/42）

VS Code タスク `cli: demo (secondary LT inversions)` を実行すると、転回図形が期待どおりに表示されます。

```text
Key: Major C
Options: hide64=off, cad64Dominant=off, V9=V9, maj7Inv=off, preferMixture7=off, enforceN6=off, tuplets=paren

Chords:
[0] vii°7/V  (Dominant)
[1] vii°65/V  (Dominant)
[2] vii°43/V  (Dominant)
[3] vii°42/V  (Dominant)

Cadences:
(none)
```

補足: `RomanInputParser` は 5度の種別（減/完全/増 = 6/7/8）を実際の PC から選択し、`43` のケースでも減五度（例: C）がベースになったときに正しく `43` を維持します。

出力イメージ:

```text
Chords:
[0] I64  (Tonic)
[1] V    (Dominant)
[2] I    (Tonic)

Cadences:
@0: Authentic  PAC=Yes  Cad64=Yes  SixFour=Cadential

Done.
```

VS Code:

- デバッグ構成「Run CLI (Release)」で実行
- タスク「cli: run」でも実行可能
- デモ: タスク「cli: demo (roman)」「cli: demo (Aug6+N)」「cli: demo (sevenths inversions)」でクイックに検証
  - デモ: タスク「cli: demo (Aug6 vs bVI7)」で Aug6 と bVI7 の棲み分けを確認
    - デモ: タスク「cli: demo (Aug6 vs bVI7 prefer mixture)」で曖昧時に bVI7 優先の挙動を確認（`--preferMixture7`）
  - デモ: タスク「cli: demo (secondary LT inversions)」で二次導音 vii°7/V の各転回を確認
  - デモ: タスク「cli: demo (mixture 7th)」で借用7th（bVI7, bII7）の検出を確認
  - デモ: タスク「cli: demo (mixture 7th inversions)」で bVI7 の転回（7/65/43/42）を確認
  - デモ: タスク「cli: demo (cadential 6-4)」で I64→V→I の Cadential 6-4 を確認（Cad64=Yes）
    - デモ: タスク「cli: demo (cadential 6-4 as dominant)」で I64 を V64-53 として表示（`--cad64Dominant`）
  - デモ: タスク「cli: demo (6-4 passing)」で 非カデンツ Passing 6-4（IV→IV64→IV6）の分類を確認
    - デモ: タスク「cli: demo (6-4 passing hide64)」で `--hide64` により非カデンツ 6-4 表示が抑制されることを確認
  - デモ: タスク「cli: demo (6-4 pedal)」で 非カデンツ Pedal 6-4（IV→IV64→IV）の分類を確認
    - デモ: タスク「cli: demo (6-4 pedal hide64)」で `--hide64` により非カデンツ 6-4 表示が抑制されることを確認
  - デモ: タスク「cli: demo (Neapolitan)」で bII → V → I の基本例を確認
    - デモ: タスク「cli: demo (Neapolitan enforceN6)」で `--enforceN6` により bII が bII6 に正規化されることを確認
  - デモ: タスク「cli: demo (modulation C->G)」で簡易キーセグメントの抽出（`--segments --trace`、Estimator パラメータ例付き）を確認（注: 旧しきい値デモ。現在は下記のプリセット版を推奨）
    - デモ: タスク「cli: demo (modulation C->G preset permissive)」で `--preset permissive` による緩めの閾値の例を確認
    - デモ: タスク「cli: demo (modulation C->G preset stable)」で `--preset stable` による安定寄りの閾値の例を確認
    - アプリ: タスク「wpf: run」でサンプルWPF（NoteValueZoom.Wpf）を起動

## CLI オプション早見表

- 入力系: `--roman "I; V; I"` / `--pcs "0,4,7; 7,11,2; 0,4,7"` / `--key C`
- 出力形式: `--json`（機械可読）/ テキスト（既定） / スキーマは `--schema`（下記参照）
- 推定器: `--segments`（キー区間）/ `--trace`（トレース）/ `--preset stable|permissive`
  - 明示指定で微調整: `--window` `--minSwitch` `--prevBias` `--switchMargin` `--minSegLen` `--minSegConf`（プリセットより優先）
- 和声・表示: `--v7n9`（V7(9)表記）/ `--maj7Inv`（maj付き転回）/ `--hide64`（非カデンツ6-4隠し）/ `--cad64Dominant`（カデンツ6-4を属へ）/ `--enforceN6`（NeapolitanをbII6へ）/ `--preferMixture7`（Aug6曖昧時はbVI7優先）
- ユーティリティ: `--romanJson` / `--pcsJson`
- スキーマ: `--schema`（main）/ `--schema util:roman|util:dur|util:pcs`

  ### デモ出力例（モジュレーション C→G）

  VS Code タスク `cli: demo (modulation C->G)` の抜粋（非推奨・互換用。プリセットは下記をご利用ください）:

  ```text
  Key: Major C
  Options: hide64=off, cad64Dominant=off, V9=V9, maj7Inv=off, preferMixture7=off, enforceN6=off, tuplets=paren

  Chords:
  [0] I  (Tonic)
  [1] V  (Dominant)
  [2] I  (Tonic)
  [3] V7/V  (Dominant)
  [4] V  (Dominant)
  ...

  Estimator: window=2, minSwitch=0, prevBias=1, initBias=0, switchMargin=1, outPenalty=0

  Key Segments:
  [0..2] C Major (conf=0.14)
  ```
  注: 最小長/信頼度のフィルタにより短い/低信頼の区間は除外され、全除外時は先頭1件にフォールバックします。細かい分割を確認したい場合は閾値を緩めてください（例: `--minSegLen 1 --minSegConf 0`）。

  例（許容的設定）:

  ```powershell
  dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --pcs "0,4,7; 7,11,2; 0,4,7; 2,6,9,0; 7,11,2; 4,7,11; 9,0,4; 2,6,9; 7,11,2" --segments --trace --preset permissive --json
  ```
  出力イメージ（許容的設定時の区間）:

  ```text
  Key Segments:
  [0..2] C Major (conf=0.14)
  [3..8] G Major (conf=0.18)
  ```

  ### 推定器プリセット（--preset）

  簡便に Estimator のしきい値を切替えるために `--preset` を用意しています（明示フラグがあればそちらが優先）。

  - stable: `window=1, minSwitch=2, prevBias=2, switchMargin=2, minSegLen=2, minSegConf=0.2`
  - permissive: `window=2, minSwitch=0, prevBias=0, switchMargin=0, minSegLen=1, minSegConf=0`

  例（permissive）:

  ```powershell
  dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --pcs "0,4,7; 7,11,2; 0,4,7; 2,6,9,0; 7,11,2; 4,7,11; 9,0,4; 2,6,9; 7,11,2" --segments --trace --preset permissive
  ```

  例（stable）:

  ```powershell
  dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --pcs "0,4,7; 7,11,2; 0,4,7; 2,6,9,0; 7,11,2; 4,7,11; 9,0,4; 2,6,9; 7,11,2" --segments --trace --preset stable
  ```

### デモ出力例（Cadential 6-4）

VS Code タスク `cli: demo (cadential 6-4)` の抜粋:

```text
Key: Major C
Options: hide64=off, cad64Dominant=off, V9=V9, maj7Inv=off, preferMixture7=off, enforceN6=off, tuplets=paren

Chords:
[0] I64  (Tonic)
[1] V    (Dominant)
[2] I    (Tonic)

Cadences:
@0: Authentic  PAC=Yes  Cad64=Yes  SixFour=Cadential
```
  
### デモ出力例（Pedal 6-4）

VS Code タスク `cli: demo (6-4 pedal)` の抜粋:

```text
Key: Major C
Options: hide64=off, cad64Dominant=off, V9=V9, maj7Inv=off, preferMixture7=off, enforceN6=off, tuplets=paren

Chords:
[0] IV  (Subdominant)
[1] IV64  (Subdominant)
[2] IV  (Subdominant)

Cadences:
@1: None  PAC=No  Cad64=No  SixFour=Pedal
```

## CLI JSON出力

CLIは `--json` で機械可読のJSONを出力します（スキーマは `--schema` で取得可能）。デモ用タスクも用意しています。

- デモ: タスク「cli: json (roman demo)」— ローマ数字の最小例
- デモ: タスク「cli: json (6-4 passing)」「cli: json (6-4 pedal)」— 6-4分類のJSON
- デモ: タスク「cli: json (cadential 6-4)」「cli: json (cadential 6-4 as dominant)」— カデンツ6-4のJSON
- デモ: タスク「cli: json (maj7Inv minimal)」— メジャーセブンス転回の表示切替
- デモ: タスク「cli: json (modulation preset permissive|stable)」— 推定器プリセット付きモジュレーション

V9 表示トグルのJSON例:

```powershell
# 既定（V9）
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --pcs 7,11,2,5,9 --json
# → options.v9 は "V9"、最初の和音 roman は "V9"

# V7(9) 表示
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --pcs 7,11,2,5,9 --v7n9 --json
# → options.v9 は "V7(9)"、最初の和音 roman は "V7(9)"
```

最小例（6-4 の JSON 抜粋）:

```json
{
  "chords": [
    { "roman": "I64" },
    { "roman": "V"   },
    { "roman": "I"   }
  ],
  "cadences": [
    { "indexFrom": 1, "type": "Authentic", "pac": true, "cad64": true, "sixFour": "Cadential" }
  ]
}
```

カデンツ 6-4（V64-53 として表示; タスク: cli: json (cadential 6-4 as dominant)）:

```json
{
  "chords": [
    { "roman": "V64-53" },
    { "roman": "V"      },
    { "roman": "I"      }
  ],
  "cadences": [
    { "indexFrom": 1, "type": "Authentic", "pac": true, "cad64": true, "sixFour": "Cadential" }
  ]
}
```

Passing 6-4（IV → IV64 → IV6; タスク: cli: json (6-4 passing)）:

```json
{
  "chords": [
    { "roman": "IV"   },
    { "roman": "IV64" },
    { "roman": "IV6"  }
  ],
  "cadences": [
    { "indexFrom": 1, "type": "None", "pac": false, "cad64": false, "sixFour": "Passing" }
  ]
}
```

Pedal 6-4（IV → IV64 → IV; タスク: cli: json (6-4 pedal)）:

```json
{
  "chords": [
    { "roman": "IV"   },
    { "roman": "IV64" },
    { "roman": "IV"   }
  ],
  "cadences": [
    { "indexFrom": 1, "type": "None", "pac": false, "cad64": false, "sixFour": "Pedal" }
  ]
}
```

<a id="iac-json"></a>

### 最小例（IAC の JSON 抜粋） {#iac-json}


- IAC: V6→I（タスク: cli: json (IAC: V6->I)）

```json
{
  "chords": [
    { "roman": "I" },
    { "roman": "V6" },
    { "roman": "I" }
  ],
  "cadences": [
    { "indexFrom": 1, "type": "Authentic", "pac": false, "cad64": false, "sixFour": "None" }
  ]
}
```

- IAC: V→I6（タスク: cli: json (IAC: V->I6)）

```json
{
  "chords": [
    { "roman": "I" },
    { "roman": "V" },
    { "roman": "I6" }
  ],
  "cadences": [
    { "indexFrom": 1, "type": "Authentic", "pac": false, "cad64": false, "sixFour": "None" }
  ]
}
```

例（モジュレーションJSON・permissive）:

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --pcs "0,4,7; 7,11,2; 0,4,7; 2,6,9,0; 7,11,2; 4,7,11; 9,0,4; 2,6,9; 7,11,2" --segments --trace --preset permissive --json
```

スキーマの出力:

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --schema
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --schema util:roman
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --schema util:dur
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --schema util:pcs
```

## CLI ユーティリティ

入力をJSONに整形するユーティリティを同梱しています。

- タスク: 「cli: util (romanJson demo)」— キー指定+ローマ数字をJSON化
- タスク: 「cli: util (pcsJson demo)」— ピッチクラス列をJSON化

例:

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --key C --romanJson "I; V6; Ger65; N6"
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --pcsJson "0,4,7; 7,11,2; 0,4,7"
```

## Mixture-7th 警告(JSON)

CLI の JSON 出力には、各和音ごとに `warnings`/`errors` 配列が含まれます。借用7th（iv7 / bVII7 / bII7 / bVI7）検出時に慣用的な解決ヒントをアドバイザリとして追加します。

注意（iv7 の入力法）:

- Roman パーサはメジャーで `iv7` を `IVmaj7` に正規化します。iv7 の検証には `--key` と `--pcs` を用いて明示PCを渡してください（例: Cメジャーで F–Ab–C–Eb → `--key C --pcs 5,8,0,3`）。

最小例（bVI7 の警告を確認）:

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --key C --roman "bVI7" --json
# → 出力例（抜粋）
#   {
#     "roman": "bVI7",
#     "warnings": [
#       "Mixture: bVI7 typically resolves to V"
#     ]
#   }
```

最小例（iv7 の警告を確認: pcs で指定）:

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --key C --pcs 5,8,0,3 --json
# → 出力例（抜粋）
#   {
#     "roman": "iv7",
#     "warnings": [
#       "Mixture: iv7 typically resolves to V"
#     ]
#   }
```

VS Code タスク（JSONデモ）:

- `cli: json (mixture7: bVI7)`
- `cli: json (mixture7: bVII7)`
- `cli: json (mixture7: iv7)` — `--key C --pcs 5,8,0,3` を使用

### 実出力例（bVI7; bII7; V; I）

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "bVI7; bII7; V; I" --json
```

抜粋（chords だけ）：

```json
{
  "chords": [
    {
      "index": 0,
      "roman": "bVI7",
      "function": "Tonic",
      "warnings": [
        "Mixture: bVI7 typically resolves to V"
      ],
      "errors": []
    },
    {
      "index": 1,
      "roman": "bII7",
      "function": "Subdominant",
      "warnings": [
        "Mixture: bII7 (Neapolitan 7) often resolves to V or I6"
      ],
      "errors": []
    },
    {
      "index": 2,
      "roman": "V",
      "function": "Dominant",
      "warnings": [
        "Parallel perfects detected"
      ],
      "errors": [
        "Voice overlap"
      ]
    },
    {
      "index": 3,
      "roman": "I",
      "function": "Tonic",
      "warnings": [
        "Parallel perfects detected"
      ],
      "errors": [
        "Voice overlap"
      ]
    }
  ]
}
```

### 実出力例（iv7 を pcs で指定）

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --key C --pcs 5,8,0,3 --json
```

抜粋（chords だけ）：

```json
{
  "chords": [
    {
      "index": 0,
      "roman": "iv7",
      "function": "Subdominant",
      "warnings": [
        "Mixture: iv7 typically resolves to V"
      ],
      "errors": []
    }
  ]
}
```

## CI（GitHub Actions）

本リポジトリのローカル実行では、以下のコマンド/タスクで再現可能なビルドとテストが行えます。CIワークフローの導入時は、これらのコマンドをベースにジョブを構成してください。

```powershell
dotnet build -c Release
dotnet test -c Release --nologo --no-build
```

- タスク: 「dotnet: build」「dotnet: test」「dotnet: test (coverage|coverage stable)」
- カバレッジHTML生成: 「coverage: html (simple)」「coverage: full stable」
  - CIのWindowsジョブでは HTML/XmlSummary に加えて Badges(SVG) も生成し、同じアーティファクトに含めます。

### カバレッジゲート（>= 75%）

- ローカル（VS Code タスク）
  - `coverage: full+check stable` を実行すると、Release ビルド → 安定設定でのカバレッジ収集 → HTML 生成 → しきい値チェック（75%）までを一括実行します。
  - 単体チェックのみは `coverage: check (75%)`（直近の `Tests/MusicTheory.Tests/TestResults/**/coverage.cobertura.xml` から算出）。

- ローカル（PowerShell 手動）

```powershell
dotnet build -c Release
dotnet test -c Release --nologo --no-build --results-directory Tests/MusicTheory.Tests/TestResults `
  --collect 'XPlat Code Coverage' -- `
  DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura
reportgenerator -reports:Tests/MusicTheory.Tests/TestResults/**/coverage.cobertura.xml `
  -targetdir:Tests/MusicTheory.Tests/TestResults/coverage-report -reporttypes:Html
powershell -NoProfile -ExecutionPolicy Bypass -File Scripts/CheckCoverage.ps1 -Threshold 75
```

- CI（GitHub Actions）
  - Windows ジョブでカバレッジ（Cobertura）→ HTML / XmlSummary 生成 → `Scripts/CheckCoverage.ps1 -Threshold 75` によりゲートを適用します。
  - 収集安定化のため `COMPlus_TieredCompilation=0` / `COMPlus_ReadyToRun=0` を設定。
  - アーティファクトとして TRX / Cobertura XML / HTML / XmlSummary をアップロードします。

補足:

- README 先頭のワークフローバッジはプレースホルダーです。GitHub 上の実リポジトリに合わせて `OWNER/REPO` を置換してください。

### カバレッジバッジ（任意）

- 生成（ローカル）:

```powershell
reportgenerator -reports:Tests/MusicTheory.Tests/TestResults/**/coverage.cobertura.xml `
  -targetdir:Tests/MusicTheory.Tests/TestResults/coverage-report -reporttypes:Badges
```

- 出力例（SVG）: `Tests/MusicTheory.Tests/TestResults/coverage-report/badge_linecoverage.svg` ほか
- README への埋め込み例（ローカル相対パス; CIで公開する場合はURLに置換）:

```markdown
![coverage](Tests/MusicTheory.Tests/TestResults/coverage-report/badge_linecoverage.svg)
```

- CIでの扱い: アーティファクトに含めるか、GitHub Pages 等で公開してURL参照してください。
  - 手動公開ワークフロー: `.github/workflows/coverage-pages.yml` を手動実行（workflow_dispatch）すると、`Tests/MusicTheory.Tests/TestResults/coverage-report` が Pages に公開されます。
  - 公開後のURL例: `https://<OWNER>.github.io/<REPO>/badge_linecoverage.svg`（所有者/リポ名に置換）。README の画像参照先をURLに切替可能です。
  
補足: README 先頭のカバレッジバッジはローカル生成後に表示されます。CI/Pages で公開する場合は画像パスを公開URLに置換してください。

## 和声法ロードマップ（v1の目安）

- 転回形の強化（triad 6/64 の安定付与、seventh 図形の徹底）
- V7/V9 の表示・判定整理（V7(9) 表示切替の徹底）
- 借用和音の拡張（bVI/bVII/bIII/bII + 7th、Neapolitan正規化）
- 進行ルール強化（6-4分類、カデンツ詳細の安定化）
- モジュレーション検出の精緻化（プリセット運用、segments/traceの強化）

## サンプルWPFの起動

サンプルの WPF アプリ（NoteValueZoom.Wpf）を起動します。

```powershell
dotnet run --project Samples/NoteValueZoom.Wpf/NoteValueZoom.Wpf.csproj -c Release
```

## トラブルシューティング

- テストが一時的に失敗する/挙動が不一致に見える
  - まず Release ビルド→ノービルドでテストを実行してください。
    ```powershell
    dotnet build -c Release
    dotnet test -c Release --nologo --no-build
    ```
  - 生成物の世代差による不整合を防げます。

- PowerShell の引数クオート（セミコロン/クオート含む）
  - `--roman` などセミコロン区切りは全体をダブルクオートで囲みます。
    ```powershell
    dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "I; V; I" --trace
    ```
  - `--logger` の TRX 例（セミコロン含む値はダブルクオート）:
    ```powershell
    dotnet test -c Release --nologo --no-build --logger "trx;LogFileName=test_results.trx"
    ```

- ReportGenerator が見つからない（coverage HTML 生成時）
  - グローバルツールをインストール/更新してください。
    ```powershell
    dotnet tool install -g dotnet-reportgenerator-globaltool
    dotnet tool update -g dotnet-reportgenerator-globaltool
    ```
  - その後、`coverage: html (simple)` あるいは以下を実行:
    ```powershell
    reportgenerator -reports:Tests/MusicTheory.Tests/TestResults/**/coverage.cobertura.xml `
      -targetdir:Tests/MusicTheory.Tests/TestResults/coverage-report -reporttypes:Html
    Start-Process -FilePath 'Tests/MusicTheory.Tests/TestResults/coverage-report/index.html'
    ```

- カバレッジの安定化（初回JITのブレ低減）
  - タスク「coverage: full stable」または「coverage: full stable (simple)」を利用。
  - 手動時は安定化用のタスク定義（環境変数）を参照してください。
