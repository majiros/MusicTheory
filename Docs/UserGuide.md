## Strict PAC（CLI）

教育/採点用途向けに PAC 判定を引き締める CLI フラグを提供します。既定は互換重視（オフ）で、必要時のみ有効化してください。

CLI フラグ（要点）

- --strictPacTriads: PAC はプレーン triad のみに限定（Imaj7 などを PAC から除外）
- --strictPacNoExt: ドミナント側の拡張（V7/V9/V7(9) 等）を含む場合は PAC にしない
- --strictPacSoprano: 最終 I のソプラノ=主音（voicing 必須）を PAC の必要条件にする
- --strictPacLtResolve: 直前 V でソプラノが導音なら主音へ解決を要求（--strictPacSoprano を内包）

ヒント

- ローマ入力（--roman）の場合、CLI は簡易ボイシングを自動生成します。最終 I のソプラノが必ずしも主音になるとは限らないため、--strictPacSoprano を付けると pac=false になるケースがあります。
- 厳格 PAC はテキスト出力では PAC=Yes/No、JSON では cadences[].pac で判定できます。

関連: 不完全終止（IAC）の最小 JSON 例は本ドキュメント後半の「IAC 近似（Authentic だが pac=false の最小例）」を参照。

### 最小デモ（Strict PAC）

#### ケースA: ソプラノ=主音の要求（--strictPacSoprano）

PowerShell:

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "I; V; I" --strictPacSoprano --json
```

確認ポイント（JSON 抜粋の cadences 最終項目）:

- type="Authentic"
- pac=false（ソプラノ=主音要件を満たさない場合）
- cad64=false, sixFour="None"

最小抜粋（実行結果の要旨）:

```json
{
  "cadences": [
    { "indexFrom": 0, "type": "Half", "pac": false, "cad64": false, "sixFour": "None" },
    { "indexFrom": 1, "type": "Authentic", "pac": false, "cad64": false, "sixFour": "None" }

### カバレッジの生成と公開（任意）

ローカルでテストカバレッジを収集し、HTML/バッジを生成できます。VS Code のタスクを使うと簡単です。

- 推奨フロー（安定設定）
  - 1) タスク: `dotnet: build`
  - 2) タスク: `dotnet: test (coverage stable)`
  - 3) タスク: `coverage: html (simple)` と `coverage: badges`
  - 生成物: `Tests/MusicTheory.Tests/TestResults/coverage-report/`（`index.html` と `badge_linecoverage.svg` ほか）

README の先頭バッジはこのバッジファイルを参照しています。

GitHub Pages で公開したい場合は、用意済みのワークフローを手動実行します。

- Actions: `coverage-pages`（workflow_dispatch）
  - Release ビルド → 安定設定でのカバレッジ収集 → HTML/Badges 生成 → Pages へ公開
  - 公開後は README のバッジリンクを公開URL（例: `https://<OWNER>.github.io/<REPO>/badge_linecoverage.svg`）に切替できます。

備考（Windows PowerShell）:

- セミコロンや二重引用符を含む引数はダブルクォートで囲ってください。
- ReportGenerator が見つからない場合は、グローバルツールを導入してください（`dotnet tool install -g dotnet-reportgenerator-globaltool`）。

  ]
}
```

VS Code タスク: `cli: json (Strict PAC: soprano tonic)`

#### ケースB: ドミナント拡張の禁止（--strictPacNoExt）

PowerShell:

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "I; V7; I" --strictPacNoExt --json
```

確認ポイント（JSON 抜粋の cadences 最終項目）:

- type="Authentic"
- pac=false（V7 を含むため PAC 不可）
- cad64=false, sixFour="None"

最小抜粋（実行結果の要旨）:

```json
{
  "chords": [ { "roman": "I" }, { "roman": "V7" }, { "roman": "I" } ],
  "cadences": [
    { "indexFrom": 0, "type": "Half", "pac": false, "cad64": false, "sixFour": "None" },
    { "indexFrom": 1, "type": "Authentic", "pac": false, "cad64": false, "sixFour": "None" }
  ]
}
```

VS Code タスク: `cli: json (Strict PAC: no dominant ext)`

#### ケースC: 終止 I は triad のみ（--strictPacTriads）

- 目的: 終止 I に第7音（I7/Imaj7など）が含まれる場合は PAC を否定する
- コマンド例:
  - dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "I; V; I7" --strictPacTriads --json
- 最小JSON確認ポイント（抜粋）:
  - cadences の2件目: type="Authentic", pac=false, cad64=false, sixFour="None"
  - 参考（最小抜粋）:
    ```json
    {
      "cadences": [
        { "indexFrom": 0, "type": "Half", "pac": false, "cad64": false, "sixFour": "None" },
        { "indexFrom": 1, "type": "Authentic", "pac": false, "cad64": false, "sixFour": "None" }
      ]
    }
    ```
- VS Code タスク: `cli: json (Strict PAC: triads only)`

#### ケースD: 導音の解決（--strictPacLtResolve）

- 目的: 直前の V でソプラノが導音なら、上行で主音に解決しない限り PAC を否定する（--strictPacSoprano を含む）
- コマンド例:
  - dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "I; V7; I" --strictPacLtResolve --json
- 最小JSON確認ポイント（抜粋）:
  - cadences の2件目: type="Authentic", pac=false, cad64=false, sixFour="None"
  - 参考（最小抜粋）:
    ```json
    {
      "cadences": [
        { "indexFrom": 0, "type": "Half", "pac": false, "cad64": false, "sixFour": "None" },
        { "indexFrom": 1, "type": "Authentic", "pac": false, "cad64": false, "sixFour": "None" }
      ]
    }
    ```
- VS Code タスク: `cli: json (Strict PAC: LT resolve)`

補足: 通常の PAC 最小例は `cli: json (roman demo)` で pac=true を確認できます（I→V→I）。

# MusicTheory ユーザーガイド（第三者向け）

このドキュメントは、初めて MusicTheory を利用する第三者（開発者/研究者/教育者）向けの、仕様と取扱説明の要約です。ライブラリのコア機能、CLI の使い方、主要オプション、JSON 出力、テストとカバレッジの回し方、既知の制限やトラブルシューティングをコンパクトにまとめています。

- 対象バージョン: .NET 8 ランタイム/SDK
- 対応 OS: Windows（PowerShell 例を記載）。macOS/Linux でも .NET があれば動作します。
- リポジトリ: majiros/MusicTheory（このワークスペース）

---


## 目次

- [1. 概要と目的](#1-概要と目的)
- [2. セットアップ](#2-セットアップ)
- [3. クイックスタート（CLI）](#3-クイックスタートcli)
- [4. CLI 使い方（概要）](#4-cli-使い方概要)
  - [4.1 CLI オプション詳細](#41-cli-オプション詳細)
- [5. JSON 出力（要点）](#5-json-出力要点)
- [5.1 Roman/Function/Cadence の仕様（詳細）](#51-romanfunctioncadence-の仕様詳細)
- [5.2 音価（Duration/Note/Rest/Tuplet）の仕様（詳細）](#52-音価durationnoteresttupletの仕様詳細)
- [5.3 音価虫眼鏡ルール（編集UIの指針）](#53-音価虫眼鏡ルール編集uiの指針)
- [5.4 JSON スキーマ（詳細）](#54-json-スキーマ詳細)
- [5.5 警告/エラー（voice leading・ミクスチャ等）](#55-警告エラーvoice-leadingミクスチャ等)
- [5.6 Estimator/Segments/Keys 出力](#56-estimatorsegmentskeys-出力)
- [5.7 代表例と比較スナップショット](#57-代表例と比較スナップショット)
- [6. 主要ポリシー（分析設計の要旨）](#6-主要ポリシー分析設計の要旨)
- [7. 既知の制限](#7-既知の制限)
- [8. テスト--カバレッジ](#8-テスト--カバレッジ)
- [9. トラブルシューティング](#9-トラブルシューティング)
- [10. FAQ（抜粋）](#10-faq抜粋)
- [11. 用語（簡易）](#11-用語簡易)
- [12. 参考](#12-参考)

---

## 1. 概要と目的

MusicTheory は、音高/音程/スケール/和音モデルと、和声分析（ローマ数字・機能・カデンツ）を行う .NET 8 ライブラリです。コマンドライン（CLI）でのデモ実行・JSON 出力にも対応し、教育・分析・研究用途での再現可能な実験やツール連携を支援します。

主な機能（要点）:

- [Pitch / Interval / Scale のモデル](#pitch--interval--scale-の仕様詳細) / [Chord のモデル](#chord和音モデルの仕様詳細)（→ 下の詳細セクション参照）
- Roman Numeral と Tonal Function の推定（I, ii, IV, V… → T/S/D）
- Triad の転回（6/64）と Seventh の転回（7/65/43/42）
- 借用和音（i/iv/bIII/bVI/bVII + Neapolitan bII）と借用 Seventh（iv7/bVII7/bII7/bVI7）
- V9（5度省略可）の優先検出、Augmented Sixth（It6/Fr43/Ger65）
- キー推定とモジュレーション区間抽出（簡易）
- 簡易カデンツ（Authentic/Plagal/Half/Deceptive）と PAC 近似の厳格化オプション
- CLI JSON 出力（各和音ごとの warnings/errors 付き）、スキーマ表示（→ [Roman/Function/Cadence 詳細](#51-romanfunctioncadence-の仕様詳細)）


### Pitch / Interval / Scale の仕様（詳細）

以下はコアのデータモデルと最小 API の仕様です（C#）。Chord/Analyzer には依存せず単体で利用できます。

#### Pitch（音高）

- PitchClass

  - 型: `record struct PitchClass(int Value)`
  - 仕様:
    - `Pc`: 0..11 に正規化（mod 12）
    - `FromMidi(int midi)`: MIDI→PC（mod 12）
    - 加算: `pc + semitones`（mod 12）
    - 文字列表現: 2 桁ゼロパディング（"00"〜"11"）

- SpelledPitch / Letter / Accidental

  - `enum Letter { C, D, E, F, G, A, B }`
  - `record struct Accidental(int Semitones)`
    - 表示法は `AccidentalNotation` で切替（Unicode/ASCII/ASCII_X）
    - 表示時は -3..+3 にクランプ（トリプルまで）
  - `record struct SpelledPitch(Letter Letter, Accidental Acc)`

- Pitch（絶対音）

  - `record struct Pitch(SpelledPitch Spelling, int Octave)`
  - `PitchUtils.ToMidi(Pitch)`: C-1 = 0 を基準に MIDI 値へ（C4=60）
  - `PitchUtils.FromMidi(int midi, SpelledPitch prefer=default, int? preferOctave=null)`
    - 既定は黒鍵を ♯ 寄りの簡易綴りで復元（例: 61→C♯）
    - Octave は `(midi/12)-1`

- PitchUtils（主な関数）

  - `LetterBasePc(Letter)`: C=0, D=2, E=4, F=5, G=7, A=9, B=11
  - `ToPc(SpelledPitch|Pitch)`: 綴り→PC
  - `Transpose(Pitch|PitchClass, SemitoneInterval)`: 半音数で移調
  - `GetEnharmonicSpellings(PitchClass pc)`: 異名同音候補（各 Letter × ±3 までの臨時記号を探索）

例（Pitch の基本操作）:

```csharp
var pc = new PitchClass(-1);           // Pc==11
var c4 = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 4);
int midi = PitchUtils.ToMidi(c4);      // 60
var cSharp = PitchUtils.FromMidi(61);  // Spelling: C♯, Octave: 4
```

注意:

- `FromMidi` は簡易綴り（♯優先）での復元です。調号やスケール文脈に沿った綴り最適化は未実装です。
- `GetEnharmonicSpellings` は候補列挙で、最適綴りの選好ロジックは含みません。

#### Interval（音程）

- SemitoneInterval（軽量）

  - `record struct SemitoneInterval(int Semitones)`
  - `int` と暗黙変換（双方向）あり。表示は "Nst"（例: 7→"7st"）

- FunctionalInterval（機能付き）

  - 和声側で用いる機能的音程（定義は別ファイル）。
  - `IntervalExtensions.ToSemitone(FunctionalInterval)` で半音数に変換。

- ユーティリティ

  - `IntervalUtils.Between(Pitch a, Pitch b)`: MIDI 差から `FunctionalInterval` を作成
  - `IntervalUtils.DegreeBetween(Pitch a, Pitch b)`: レター基準の度数差（A→B で 2、オクターブ跨ぎ考慮）
  - `IntervalUtils.IsEnharmonic(Pitch a, Pitch b)`: PC 一致で異名同音判定
  - `IntervalUtils.GetIntervalName(FunctionalInterval)`: 「完全/長/短/増/減（重増/重減含む）」の近似名称を返す

例（Interval の基本）:

```csharp
var c4 = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 4);
var g4 = new Pitch(new SpelledPitch(Letter.G, new Accidental(0)), 4);
var ivl = IntervalUtils.Between(c4, g4);              // 機能的音程（MIDI 差 7）
var name = IntervalUtils.GetIntervalName(ivl);        // 近似: 完全5度相当
var deg  = IntervalUtils.DegreeBetween(c4, g4);       // 5
```

注意:

- 名称付けは半音数に基づく近似で、全ケースで伝統的な度数体系と一致を保証するものではありません。

#### Scale（スケール）

- IScale / PcScale

  - `IScale` は `Name` と `GetSemitoneSet()`（PC セット）を定義。
  - `record struct PcScale(string Name, PitchClass[] Degrees)` は PC ベースのスケール実装。
  - 主な API:
    - `Contains(PitchClass pc)` / `GetSemitoneSet()`
    - `Transposed(int rootPc)`: ルート PC をずらした同型スケールを返す
    - `GetSpelledDegreesWithNames(SpelledPitch root)`: 簡易綴り＋日本語度名（主音/上主音/…）を列挙

- PcScaleLibrary

  - 代表スケール: Major / Minor / ChurchModes（Ionian〜Locrian）/ Chromatic / WholeTone / Pentatonic / Blues
  - `JapaneseDegreeNames`: 主音 / 上主音 / 中音 / 下属音 / 属音 / 下中音 / 導音
  - `FindScalesContaining(PitchClass pc)`: 代表スケール集合から包含スケールを列挙

- 調号推定（実験的）

  - `KeySignatureInference.InferKeySignature(IScale scale)`: 7 音スケールをモードパターンと突き合わせ、Ionian（メジャー）基準の調号値（# 本数、負数は ♭）を返す。7 音以外は `null`。

例（Scale の基本）:

```csharp
var major = PcScaleLibrary.Major;                 // [0,2,4,5,7,9,11]
bool hasB = major.Contains(new PitchClass(11));   // true
var dm = major.Transposed(2);                     // D メジャー相当（名前は付加情報）
```

注意:

- `GetSpelledDegreesWithNames` の綴りは候補からの簡易選択です（キー文脈に最適化していません）。
- `InferKeySignature` は 7 音スケールのみ対象です。

### Chord（和音）モデルの仕様（詳細）

以下は汎用コード表記（ルート+シンボル）の最小実装です。Roman/機能和声とは独立して利用できます。

#### データ型

- ChordFormula

  - フィールド:
    - `string Name`（内部名）
    - `string Symbol`（表示シンボル。例: "m", "maj7", "7", "dim" など）
    - `FunctionalInterval[] CoreIntervals`（コア構成音: 3度/5度/7度 等）
    - `FunctionalInterval[] Tensions`（9, 11, 13 等。省略可能）
    - `string[] Aliases`（別名。例: maj7 の "M7"/"Δ7"、m7 の "-7" など）
  - `GetDisplayName()`: 例) `"7(9,11,13)"` のようにシンボル+テンション表示

- ChordName

  - `string Root`（ルート名を任意文字列で保持。例: "C", "F#", "Bb"）
  - `ChordFormula Formula`
  - `ToString() => Root + Formula.Symbol`

#### 組み込みフォーミュラ（抜粋）

- Triad: Major(別名 "maj"/"M"), Minor("m"/"min"/"-"), Diminished("dim"/"o"), Augmented("aug"/"+")
- Seventh: maj7("M7"/"Δ7"), 7, m7("-7"), m7b5("ø7"), dim7
- Extended/Altered: 9, 13, 7alt(b9,#9,#11,b13), maj13("M13"/"Δ13")

実体は `ChordFormulas.All` の配列に定義されています。

#### ユーティリティ API

- `IEnumerable<ChordFormula> ChordFormulas.All`
- `IEnumerable<ChordFormula> MatchByNotes(IEnumerable<int> inputNotes)`
  - 入力は「ルートからの半音距離」の集合（例: メジャートライアドは {4,7}）。
  - 実装は `input ⊆ (CoreIntervals∪Tensions)` を満たすフォーミュラを列挙（部分一致可）。
  - 例: {4,10} は Dominant7(4,7,10) にマッチ（5度が欠けていても許容）。
- `IEnumerable<ChordName> GenerateChordNames(string root)`
  - 指定ルートで全フォーミュラの候補名を生成（例: root="C" → C, Cm, Cdim, Caug, Cmaj7, C7...）。
- `ChordName? ParseChordName(string chordText)`
  - 末尾のシンボル/別名を最長一致で認識し、残りを `Root` として解釈。
  - 例: "Cmaj7" → Root="C", Symbol="maj7"。該当シンボルがない場合は空シンボルのフォーミュラ（Major Triad）へフォールバック。

例（基本）:

```csharp
// 文字列 → 構造
var cmaj7 = ChordFormulas.ParseChordName("Cmaj7");
Console.WriteLine(cmaj7);      // Cmaj7
Console.WriteLine(cmaj7!.Formula.Name); // "Major 7"

// ルートからの半音距離でマッチング（部分一致可）
var matches = ChordFormulas.MatchByNotes(new[]{4,7});
// → Major Triad, maj7, 9, 13, maj13 ...（4,7 を含むもの）

// 一括生成
foreach(var name in ChordFormulas.GenerateChordNames("F#"))
{
    Console.WriteLine(name.ToString());
}
```

注意:

- `Root` は自由形式の文字列です（厳密な綴り検証は行いません）。Pitch/SpelledPitch と連携する場合は別途変換が必要です。
- `MatchByNotes` はコア音の完全一致を強制しません（部分一致を許容）。厳密判定が必要な用途では、呼び出し側で必須度数（例: 3度/7度）を確認してください。
- 本モジュールは汎用コード表記であり、Roman/機能和声や CLI の出力ポリシーとは独立です。

設計のキーポイント:

- Triad 転回は distinct なピッチクラスが3音のときのみ付与（4音以上は Seventh/Tension を優先）
- V9 の誤検出抑止（早期判定 + 最終再チェック）
- Aug6 と bVI7 の曖昧性への棲み分けとオプション（preferMixture7 など）
- Neapolitan（bII）を bII6 に強制するオプション（--enforceN6）
- 一般 6-4（Passing/Pedal）と Cadential 6-4 を区別し、表示抑制オプションあり

---

## 2. セットアップ

前提:

- .NET 8 SDK がインストール済み
- Windows PowerShell 5.1 以降（例示は PowerShell）

手順（最小）:

```powershell
# ビルド
dotnet build -c Release

# テスト（最新ビルドを流用）
dotnet test -c Release --nologo --no-build
```

推奨ツール（任意）:

- カバレッジ HTML 生成: `dotnet tool install -g dotnet-reportgenerator-globaltool`

---

## 3. クイックスタート（CLI）

最小デモ（ローマ数字）:

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "I; V; I" --segments --trace
```

JSON 出力（スキーマに基づくマシン可読）:

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "I; V; I" --json
```

モジュレーション推定（プリセット stable|permissive）:

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release `
  --pcs "0,4,7; 7,11,2; 0,4,7; 2,6,9,0; 7,11,2; 4,7,11; 9,0,4; 2,6,9; 7,11,2" `
  --segments --trace --preset stable
```

VS Code タスク（推奨）: README の「VS Code タスク」節を参照。`dotnet: build` → `dotnet: test (no build)`、各種 `cli: demo`/`cli: json`、`coverage: ...` を用意しています。

---

## 4. CLI 使い方（概要）

入力系:

- `--roman "I; V; I"` ローマ数字列
- `--pcs "0,4,7; 7,11,2; 0,4,7"` ピッチクラス列（0..11）
- `--key C` などのキー指定（pcs 入力の補助）

出力系:

- 既定はテキスト。`--json` 指定で JSON 出力（各和音に warnings/errors 付き）
- スキーマは `--schema`（メイン）/ `--schema util:roman|util:dur|util:pcs`

推定器（キー/区間）:

- `--segments`（区間抽出）/ `--trace`（トレース表示）
- `--preset stable|permissive`（閾値セット）。個別フラグ（`--window` 等）で上書き可能

表示/挙動オプション（抜粋）:

- `--v7n9` V9 を `V7(9)` 表記に切替
- `--maj7Inv` メジャーセブンス転回の表示を `maj` 付きに
- `--hide64` 非カデンツ 6-4（Passing/Pedal）を一覧から隠す
- `--cad64Dominant` Cadential 6-4（I64→V→I）を記譜上 V64-53 にリラベル
- `--enforceN6` Neapolitan（bII triad）を常に bII6 に正規化
- `--preferMixture7` Aug6 と bVI7 の曖昧時に Mixture 7th を優先

ユーティリティ:

- `--romanJson` / `--pcsJson` 入力を JSON 化

クオートの注意（PowerShell）:

- セミコロン区切りの値はダブルクオートで囲む（例: `"I; V; I"`）

---

### 4.1 CLI オプション詳細

主要オプションの意味・既定値・相互作用・JSON 出力への影響を一覧します。具体例は VS Code の各種「cli: demo/json」タスクで検証できます。

- 入力の指定系
  - `--roman "I; V; I"`
    - ローマ数字を直接入力して解析を固定（推定はしない）
    - JSON 影響: `chords[].roman` は入力値に準拠（表示ポリシーで V9 等は変化し得る）
  - `--pcs "0,4,7; 7,11,2; ..."`
    - ピッチクラス列から和音を推定
    - JSON 影響: `chords[].roman` と `function` を推定。`key` が未指定なら C メジャー既定
  - `--key C|Am|...`（pcs 入力の補助）
    - キーコンテキストを与える（推定のベース/相対表記に影響）

- 出力の形式/スキーマ
  - `--json`
    - 決定論的 JSON を出力
    - JSON 影響: `version`/`options`/`chords`/`cadences` ほかを含む
  - `--schema [main|util:roman|util:dur|util:pcs]`
    - スキーマ定義を出力（ユーティリティ系の入出力仕様も含む）

- 推定器（キー/区間）
  - `--segments` / `--trace`
    - キー区間の抽出と詳細ログ
    - JSON 影響: `segments[]`/`keys[]`/`trace[]` が追加される場合あり
  - `--preset stable|permissive`
    - 閾値セット。`stable` は保守的、`permissive` は敏感
    - 上書き可: `--window`, `--minSwitch`, `--prevBias`, `--switchMargin`, `--minSegLen`, `--minSegConf`

- 表示/挙動ポリシー（和声）
  - `--v7n9`
    - V9 を `V7(9)` 表示に変更
    - JSON 影響: `options.v9` が `"V9"`→`"V7(9)"`
  - `--maj7Inv`
    - メジャー7thの転回に `maj` を付与（例: `IV65`→`IVmaj65`）
    - JSON 影響: `options.maj7Inv=true`
  - `--hide64`
    - 一般 6-4（Passing/Pedal）を一覧から抑制
    - JSON 影響: `options.hide64=true`。Cadential は別枠で保持
  - `--cad64Dominant`
    - Cadential 6-4 を V64-53 として再表示
    - JSON 影響: `options.cad64Dominant=true`（`cadences[].cad64` は true のまま）
  - `--enforceN6`
    - Neapolitan（三和音）を bII6 に強制（bII7 には非適用）
    - JSON 影響: `options.enforceN6=true`
  - `--preferMixture7`
    - Aug6 と bVI7 の曖昧時に Mixture 7th を優先
    - JSON 影響: `options.preferMixture7=true`

- 音価ユーティリティ
  - `--dur "Q; E(3:2); 1/8.."` / `--durJson "..."`
    - 音価列のパース・正規化・表示（詳細は 5.2）
  - `--tuplets paren|star`
    - 連符表示の選択（括弧/スター）。JSON の `options.tuplets` に反映

- PowerShell のクオート注意
  - セミコロン区切りや複合値（`--pcs`/`--roman`）は必ずダブルクオートで囲む


## 5. JSON 出力（要点）

- トップに `options`（表記ポリシー）
- `chords[]` に和音ごとの結果（`roman`/`function`/`warnings`/`errors` など）
- `cadences[]` に終止情報（PAC/IAC 近似、Cadential 6-4 フラグ、Passing/Pedal 6-4 など）
- スキーマは `--schema` で取得可能。ユーティリティスキーマ（`util:*`）も提供

例（V9 トグル）:

- 既定: `--pcs 7,11,2,5,9 --json` → `options.v9 == "V9"`
- 切替: `--v7n9` 追加 → `options.v9 == "V7(9)"`

最小スナップショット（I; V; I）:

```json
{
  "version": 1,
  "key": { "tonic": 0, "mode": "major" },
  "options": {
    "hide64": false,
    "cad64Dominant": false,
    "v9": "V9",
    "maj7Inv": false,
    "preferMixture7": false,
    "enforceN6": false,
    "tuplets": "paren"
  },
  "chords": [
    { "index": 0, "roman": "I", "function": "Tonic", "warnings": [], "errors": [] },
    { "index": 1, "roman": "V", "function": "Dominant", "warnings": ["Parallel perfects detected"], "errors": ["Voice overlap"] },
    { "index": 2, "roman": "I", "function": "Tonic", "warnings": ["Parallel perfects detected"], "errors": ["Voice overlap"] }
  ],
  "cadences": [
    { "indexFrom": 0, "type": "Half", "pac": false, "cad64": false, "sixFour": "None" },
    { "indexFrom": 1, "type": "Authentic", "pac": true,  "cad64": false, "sixFour": "None" }
  ]
}
```

出力は `--segments`/`--trace` の指定有無で `segments`/`trace` 節が加わるほか、推定器パラメータ（`estimator`）や `keys` の時系列も含まれる場合があります。

---

### 5.1 Roman/Function/Cadence の仕様（詳細）

この節では、ローマ数字（Roman）、機能（Function）、および終止（Cadence）の近似判定と JSON フィールドの意味を要点のみ整理します。

#### Roman（ローマ数字）

- 入力: `--roman` または `--pcs`（推定）
- 出力フィールド: `chords[].roman`
- 転回表示:
  - Triad: 6/64（distinct=3 音時のみ付与。4音以上は Seventh/Tension を優先）
  - Seventh: 7/65/43/42（必要に応じて）
- 表記ポリシーの例:
  - `--v7n9` により V9 を `V7(9)` 表示へ切替
  - `--maj7Inv` によりメジャー7thの転回に `maj` を付与

#### Function（機能）

- 出力フィールド: `chords[].function`（`Tonic`/`Subdominant`/`Dominant` の3分類）
- Roman と連動し、代理コード・借用和音も近似的に機能へ写像

#### Cadence（終止）

- 出力フィールド: `cadences[]`
  - `type`: `Authentic`/`Half`/`Plagal`/`Deceptive`/`None`（近似）
  - `pac`: 完全終止（PAC）の強い近似を満たす場合 true
  - `cad64`: Cadential 6-4 の随伴がある場合 true（記譜上の V64-53 へのリラベルは `--cad64Dominant`）
  - `sixFour`: `None`/`Passing`/`Pedal`/`Cadential`（6-4 の分類）

6-4 の分類例（実出力）:

- Cadential 6-4（I64→V→I）

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

- Cadential 6-4（V64-53 として表示; `--cad64Dominant`）

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

- Passing 6-4（例: IV; IV64; IV6）

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

- Pedal 6-4（例: IV; IV64; IV）

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

PAC 近似の主な条件（概要）:

- ドミナント → トニックの終止（V→I など）
- オプションで厳格化（Strict PAC）: 素朴三和音のみ／ドミナントの拡張を禁止／終止 I のソプラノ=主音 等

---

実行例（PowerShell / VS Code タスク）:

```powershell
# Cadential 6-4 のデモ（I64→V→I）
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "I64; V; I" --json

# Cadential 6-4 を V64-53（属）として表示
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "I64; V; I" --cad64Dominant --json

# Pedal 6-4 のデモ（IV; IV64; IV）
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "IV; IV64; IV" --json

# VS Code タスク（同等の設定を用意）
# - cli: demo (cadential 6-4)
# - cli: json (cadential 6-4)
# - cli: json (cadential 6-4 as dominant)
# - cli: demo (6-4 pedal)
# - cli: json (6-4 pedal)
# - cli: json (6-4 passing)
```

これらのタスクは VS Code のターミナル上で選択実行できます。詳細は README の「VS Code タスク」節をご参照ください。

### 5.2 音価（Duration/Note/Rest/Tuplet）の仕様（詳細）

この節では、音価モデル（Duration）、音符/休符、付点、連符、記法（文字列）の要点をまとめます。

#### 単位と基礎

- 時間解像度
  - Quarter Note の PPQ=480、Whole Note=1920 ticks（`Duration.TicksPerQuarter=480`, `TicksPerWhole=1920`）。
  - 付点上限は 3（`Duration.MaxDots=3`）。
- 基底音価 `BaseNoteValue`（Whole=1.0 基準の分数）
  - `DoubleWhole=2/1`, `Whole=1/1`, `Half=1/2`, `Quarter=1/4`, `Eighth=1/8`, `Sixteenth=1/16`, `ThirtySecond=1/32`, `SixtyFourth=1/64`, `OneHundredTwentyEighth=1/128`。
  - `BaseNoteValueExtensions.GetFraction()` で分数取得。
- 有理係数 `RationalFactor(num, den)`
  - Whole=1 を基準とした分数。正規化（約分）されます。
  - `Duration.WholeFraction` として内部表現に使用。

#### Duration の生成と加算

- `Duration.FromBase(baseValue, dots=0, Tuplet? tuplet=null)`
  - 付点倍率は $(2 - 1/2^{dots})$ を掛けます（例: 付点四分=四分×1.5）。
  - 連符倍率は `Tuplet(normal/actual)` を掛けます（例: 三連 `(3:2)` は 2/3 倍）。
- `Duration.FromTicks(ticks)` 任意 tick から生成。
- 加算 `a + b` は WholeFraction を和として合成（タイの近似）。
- 表示 `ToString()` は `"num/den (ticks ticks)"` の簡易表示。

補助:

- `Tuplet(actual, normal)` は「Actual 個を Normal 個分に詰める」比率（例: `(3,2)` で 2/3）。`Factor`/`FactorRational` を提供。
- `Note(Duration, Pitch=60, Velocity=100, Channel=0)` / `Rest(Duration)`
  - `Note.Tie(next)` で音価を合算。
  - `DurationFactory`/`RestFactory` による簡易生成（`Quarter(1)`=付点四分 等）。

#### 単純形/分解（付点/連符の判定）

- `TryAsSimple(out baseValue, out dots)`
  - 連符なしで単一の `base + dots(0..3)` で表現できるか検査。
- `TryDecomposeFull(out baseValue, out dots, out tuplet, bool extendedTuplets=false)`
  - 代表的連符（既定は `(3:2),(5:4),(7:4),(5:2),(7:8),(9:8)`）または拡張連符（`extendedTuplets=true` で 2..12 の全列挙）を用いて、`base + dots + tuplet` へ分解を試みます。
  - 複数一致時は慣例（Normal=4→2→その他、baseTicks 小さい順 等）で安定に選好。

シーケンス正規化（休符例）:

- `DurationSequenceUtils.NormalizeRests(seq, advancedSplit=false, additionalTuplets=null, mergeTuplets=false, extendedTuplets=false, allowSplit=false)`
  - 連続休符を合算し、`TryAsSimple`/`TryDecomposeFull`/カスタム連符で縮約。
  - `advancedSplit=true` で付点の展開→再統合を許容。
  - `allowSplit=true` かつ `extendedTuplets` 併用で、分割許容の推論を使い「8分+16分+16分=三連 8分」のような縮約に対応。
- 連符推論ユーティリティ
  - `InferCompositeTuplet(parts, extendedTuplets=false)` と `InferCompositeTupletFlexible(parts, extendedTuplets=false)` を提供。

#### 記法（DurationNotation）

- 役割: 文字列↔音価の往復変換（解析/整形）。
- 主 API
  - `bool DurationNotation.TryParse(string text, out Duration d)`
  - `string DurationNotation.ToNotation(Duration d, bool extendedTuplets=false)`
- サポートする表記（抜粋）
  - 省略記号: `W`(全), `H`(2分), `Q`(4分), `E`(8分) 等。
  - 分数: `1/4`（四分）, `1/8..`（二重付点八分）。
  - 付点: `.` を付加（上限 3）。例: `Q.`（付点四分）。
  - 連符: 既定は括弧で `(actual:normal)`。拡張連符表示（`extendedTuplets=true`）ではスター `*actual:normal`。
- 例（テストに基づく既定値）
  - `"Q" → 480 ticks`
  - `"Q." → 720 ticks`
  - `"E(3:2)" → 160 ticks`（8分三連）
  - `"1/8.." → 420 ticks`（二重付点八分）

注意:

- `ToNotation(d, extendedTuplets:false)` は連符を `(a:n)`、`true` では `*a:n` として出力します。
- 解析と整形はラウンドトリップ可能な範囲で安定化しています（代表的連符/付点に制限）。

#### CLI ユーティリティ（dur / durJson / スキーマ）

- 文字列解析と整形を CLI から試す:

```powershell
# 解析 + 正規整形（既定: tuplets=paren）
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release `
  --dur "Q; Q.; E(3:2); 1/8.."

# スター表記（拡張連符）での整形
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release `
  --dur "E(3:2)" --tuplets star

# JSON で取り出す
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release `
  --durJson "Q; Q.; E(3:2); 1/8.." --tuplets paren
```

- スキーマの取得（ユーティリティ）:

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --schema util:dur
```

- JSON 出力の `options.tuplets` は `"paren"|"star"` を取り、`DurationNotation.ToNotation` の連符表記に反映されます。

### 5.3 音価虫眼鏡ルール（編集UIの指針）

この節は、シーケンサー/譜面エディタ等で「音価の拡大・縮小（ズーム）」を一貫した操作感で提供するための内蔵ルールを要約します。実装は `Theory/Time/NoteValueZoom.cs` および `Theory/Time/NoteValueZoomSelection.cs` にあります（テストは `Tests/MusicTheory.Tests/NoteValueZoom*.cs`）。

- 離散ステップ（Entries）
  - 四分音符を 1.0 とした代表的な音価を、小さい→大きい順で配列化して用います。
  - 例（概念ラベル。実体は tick 値）: 32分, 16分三連, 16分, 8分三連, 付点16分, 8分, 4分三連, 付点8分, 4分, 2分三連, 付点4分, 2分, 付点2分, 全。

- 最近傍スナップ（FindNearestIndex）
  - 任意の Duration を最も近いステップへスナップします（絶対 tick 差の最小）。
  - タイブレーク: ちょうど中間のときは「大きい側」を選びます。

- 単一値ズーム（Zoom, ZoomIn, ZoomOut）
  - 現在値を最近傍へスナップ後、配列インデックスを delta（±1 など）だけ移動して新しい音価を返します。
  - 端でのクランプ: 先頭/末尾を超える移動は境界に張り付きます（配列外へは出ません）。

- 複数選択の操作（NoteValueZoomSelection）
  - 代表ターゲット（ChooseTargetIndexByMedian）
    - 選択中ノートの各最近傍インデックスの「中央値」を採用します（空選択は四分が既定）。
  - 一括スナップ（開始維持）: SnapAllToIndexMaintainStarts
    - 全ノートの長さのみをターゲット音価へ置換し、開始時刻は変更しません。
  - 一括スナップ（全体長維持）: SnapAllToIndexPreserveTotalSpan
    - 全ノートをターゲット音価に揃えつつ、選択の開始→終了のスパンを保存します。
    - 誤差は末尾ノートに集約し、長さは最小 1 tick を保証します。
  - 一括ズーム: ZoomSelection(delta, preserveTotalSpan)
    - 各ノートへ Zoom を適用します。preserveTotalSpan=true の場合は上記「全体長維持」で調整します。

- 端・例外の扱い
  - ステップ外の中間値も、ズーム前に最近傍へ正規化されます。
  - ステップ境界を跨いでも決定論的（同距離時は大きめ優先）に動作します。

実装の要点はテストで検証されています（Entries が小→大の順であること、ズームの上下移動と境界クランプ、選択スナップ時の開始/全体スパン不変性など）。

### 5.4 JSON スキーマ（詳細）

メインスキーマ（`--schema`）の主要フィールド（実出力に準拠）:

- version: number（現在 1）
- key: { tonic: 0..11, mode: "major"|"minor" }
- options: 表示/挙動のポリシー
  - hide64: boolean
  - cad64Dominant: boolean
  - v9: "V9"|"V7(9)"
  - maj7Inv: boolean
  - preferMixture7: boolean
  - enforceN6: boolean
  - tuplets: "paren"|"star"
  - preset?: "stable"|"permissive"
- chords[]: 各和音の分析
  - index: number（0 始まり）
  - roman: string（転回・付加含む）
  - function: string（"Tonic"/"Subdominant"/"Dominant" 等のラベル）
  - warnings: string[]
  - errors: string[]
- cadences[]: 終止情報（任意）
  - indexFrom: number（始点和音インデックス）
  - type: "Authentic"|"Plagal"|"Half"|"Deceptive"|"None"
  - pac: boolean（強い PAC 近似）
  - cad64: boolean（Cadential 6-4 付随）
  - sixFour: "None"|"Passing"|"Pedal"|"Cadential"
- estimator: 推定器の設定（`window`/`minSwitch`/`prevBias`/`initBias`/`switchMargin`/`outPenalty`）。`--segments` 指定時に出力
- keys[]: 各インデックスの推定キー（`--segments` 指定時）
- segments[]: 区間（`confidence` を含む、`--segments` 指定時）
- trace[]: 推定トレース（`--segments --trace` 指定時）

ユーティリティスキーマ（`--schema util:*`）:

- util:roman: 入力 `{ key: string, roman: string }` → パース結果
- util:dur: 入力 `{ list: string, tuplets: "paren"|"star" }` → 正規化された音価列
- util:pcs: 入力 `{ list: string }` → ピッチクラス列

注意:

- 文字列の内容は今後拡張される可能性がありますが、フィールド名/型は後方互換を重視します。
- `cadences` は常時出力されます（空のときも空配列）。

BNF 風（簡易）:

```text
MainResult ::= {
  version: number,
  key: { tonic: 0..11, mode: ("major"|"minor") },
  options: Options,
  chords: ChordResult[],
  cadences: CadenceResult[],
  estimator?: EstimatorConfig,
  keys?: KeyEstimate[],
  segments?: Segment[],
  trace?: TraceEvent[]
}

Options ::= {
  hide64: boolean,
  cad64Dominant: boolean,
  v9: ("V9"|"V7(9)"),
  maj7Inv: boolean,
  preferMixture7: boolean,
  enforceN6: boolean,
  tuplets: ("paren"|"star"),
  preset?: ("stable"|"permissive")
}

ChordResult ::= {
  index: number,
  roman: string,
  function: string,
  warnings: string[],
  errors: string[]
}

CadenceResult ::= {
  indexFrom: number,
  type: ("Authentic"|"Plagal"|"Half"|"Deceptive"|"None"),
  pac: boolean,
  cad64: boolean,
  sixFour: ("None"|"Passing"|"Pedal"|"Cadential")
}

Segment ::= { start: number, end: number, key: { tonic: 0..11, mode: ("major"|"minor") }, confidence: number }
KeyEstimate ::= { index: number, key: { tonic: 0..11, mode: ("major"|"minor") } }
TraceEvent ::= { index: number, max: number, second: number, margin: number }
EstimatorConfig ::= {
  window: number,
  minSwitch: number,
  prevBias: number,
  initBias: number,
  switchMargin: number,
  outPenalty: number
}

// util schemas（省略: util:roman / util:dur / util:pcs は各節参照）
```

ユーティリティスキーマ（実出力要約）:

- util:roman（URN: urn:music-theory:cli:schema:util:roman:v1）
  - 形: array of { input: string, pcs: `number[0..11]`, bassPcHint?: `0..11` }
  - 必須: input, pcs
  - 取得: VS Code タスク「cli: schema (util:roman)」

- util:dur（URN: urn:music-theory:cli:schema:util:dur:v1）
  - 形: array of { input: string, ticks: int>=0, fraction: { numerator:int>=1, denominator:int>=1 }, notation: string }
  - 必須: input, ticks, fraction, notation
  - 取得: VS Code タスク「cli: schema (util:dur)」

- util:pcs（URN: urn:music-theory:cli:schema:util:pcs:v1）
  - 形: array of { input: string, pcs: `number[0..11]` }
  - 必須: input, pcs
  - 取得: VS Code タスク「cli: schema (util:pcs)」

### 5.5 警告/エラー（voice leading・ミクスチャ等）

各和音の `warnings`/`errors` はアナライザが見つけた注意/問題のメッセージです。代表例:

- warnings（助言）
  - Borrowed seventh detected: bVI7/bII7/bVII7/iv7
  - Parallel perfects detected（平行完全）
  - Hidden fifths/hidden octaves（隠れ）
- errors（強い違反）
  - Voice overlap（声部の交差/重なり）
  - Spacing too wide（声部間隔の超過）

ガイダンス:

- warnings は学習/教材向けに提示される助言で、致命的ではありません。
- errors はボイスリーディングの破綻を表し、改善が推奨されます。

### 5.6 Estimator/Segments/Keys 出力

`--segments` を有効にすると、推定器がキーの時系列と区間を出力します（`--trace` で詳細ログ）。

- segments[]: { start, end, key: { tonic, mode }, confidence }
- keys[]: 各インデックスの推定キー
- estimator: { window, minSwitch, prevBias, initBias, switchMargin, outPenalty }
- trace[]: { index, max, second, margin }

プリセット:

- stable: 切替に慎重。短い揺らぎは現キーに吸収
- permissive: 変化に敏感。短い候補も提示

### 5.7 代表例と比較スナップショット

- V9 表示トグル（タスク: cli: json (V9) / cli: json (V7(9))）
  - `--pcs 7,11,2,5,9 --json` → `options.v9 = "V9"`
  - `--v7n9` を追加 → `options.v9 = "V7(9)"`
  例:
  ```json
  { "options": { "v9": "V9" } }
  ```
  → `--v7n9` で
  ```json
  { "options": { "v9": "V7(9)" } }
  ```
  確認ポイント:
  - JSON の `options.v9` が期待の文字列になっているか。
  - `chords[0].roman` が `V9` または `V7(9)` に一致しているか。

- Cadential 6-4 表示（タスク: cli: demo (cadential 6-4 as dominant)）
  - `--roman "I64; V; I" --json --cad64Dominant` → `cadences[].cad64=true` かつ V64-53 表示
  例（要旨）:
  ```json
  { "cadences": [ { "cad64": true } ] }
  ```
  確認ポイント:
  - `cadences` の対象要素で `cad64` が `true`。`sixFour` は `"Cadential"`。
  - `options.cad64Dominant` が `true`。
  - テキスト出力（--json なし）では I64 が V64-53 として表示されること（表示のみの変更）。

- Passing 6-4（タスク: cli: json (6-4 passing)）
  - `--roman "IV; IV64; IV6" --json`
  例（要旨）:
  ```json
  {
    "chords": [
      { "roman": "IV" },
      { "roman": "IV64" },
      { "roman": "IV6" }
    ],
    "cadences": [
      { "indexFrom": 0, "type": "None", "pac": false, "cad64": false, "sixFour": "Passing" }
    ]
  }
  ```
  確認ポイント:
  - `cadences[0].sixFour` が `"Passing"`。
  - `cadences[0].type` は `"None"`（一般6-4は非カデンツエントリに付与）。
  - `chords[].roman` が `IV → IV64 → IV6` の並び。

- Pedal 6-4（タスク: cli: json (6-4 pedal)）
  - `--roman "IV; IV64; IV" --json`
  例（要旨）:
  ```json
  {
    "chords": [
      { "roman": "IV" },
      { "roman": "IV64" },
      { "roman": "IV" }
    ],
    "cadences": [
      { "indexFrom": 0, "type": "None", "pac": false, "cad64": false, "sixFour": "Pedal" }
    ]
  }
  ```
  確認ポイント:
  - `cadences[0].sixFour` が `"Pedal"`。
  - `cadences[0].type` は `"None"`。
  - `chords[].roman` が `IV → IV64 → IV` の並び。

- 非カデンツ 6-4 の隠し（タスク: cli: demo (6-4 passing hide64) / cli: demo (6-4 pedal hide64)）
  - `--roman "IV; IV64; IV6" --hide64`（または `--roman "IV; IV64; IV" --hide64`）
  - テキスト出力で Type==None の 6-4 項目が一覧から消える（Cadential は維持）
  確認ポイント:
  - `options.hide64` が `true`。
  - JSON（--json 指定時）では Type==None かつ `sixFour=Passing|Pedal` の項目が出力から抑制される。
  - Cadential 6-4（I64→V→I）の `cadences[].cad64` は抑制されない（別枠）。

- Neapolitan 強制（タスク: cli: demo (Neapolitan enforceN6)）
  - `--roman "bII; V; I" --json --enforceN6` → bII6 に正規化（bII7 は対象外）
  例（要旨）:
  ```json
  { "options": { "enforceN6": true }, "chords": [ { "roman": "bII6" } ] }
  ```
  確認ポイント:
  - `options.enforceN6` が `true`。
  - 先頭の `chords[0].roman` が `bII6`（bII ではなく 6 に正規化）。

- PAC 近似（タスク: cli: json (roman demo)）
  - `--roman "I; V; I" --json`
  例（要旨）:
  ```json
  {
    "chords": [
      { "roman": "I" },
      { "roman": "V" },
      { "roman": "I" }
    ],
    "cadences": [
      { "indexFrom": 0, "type": "Half", "pac": false, "cad64": false, "sixFour": "None" },
      { "indexFrom": 1, "type": "Authentic", "pac": true,  "cad64": false, "sixFour": "None" }
    ]
  }
  ```
  確認ポイント:
  - `cadences` の後段が `type=Authentic` かつ `pac=true`。
  - Strict PAC オプションを有効にした場合は、ボイシングや拡張の有無により `pac` が false になり得る（設計メモ参照）。

### IAC 近似（Authentic だが pac=false の最小例） {#iac-json}

#### IAC vs Strict PAC（短い対比）

- IAC（Imperfect Authentic）は「Authentic（V→I 系）」だが PAC 条件を満たさないため pac=false になるケースの総称（例: V6→I, V→I6 など）。
- Strict PAC フラグは PAC の必要条件をさらに厳しくするオプション群（素朴 triad 限定／ドミナント拡張禁止／ソプラノ=主音／導音の上行解決）。その結果、通常は pac=true な並びでも pac=false になり得ます。
- 実務ガイド:
  - IAC 確認には本節の最小例（V6→I / V→I6）を使用。
  - 採点・教育用途で PAC を厳格化したい場合は Strict PAC の各タスク（「cli: json (Strict PAC: …)」）で挙動を確認。

- 再現（例1: V6 → I）

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "I; V6; I" --json
```

- 期待される最小出力（抜粋）

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

- 再現（例2: V → I6）

```powershell
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --roman "I; V; I6" --json
```

- 確認ポイント
  - `cadences` の対象要素が `type = "Authentic"` かつ `pac = false`（IAC: 不完全終止の近似）。
  - 一例目では `chords[1].roman = "V6"`、二例目では `chords[2].roman = "I6"`。
  - `sixFour = "None"`, `cad64 = false`（Cadential 6-4 は関与しない）。

- iv7（長調での pcs 入力）（例: `--key C --pcs "5,8,0,3"`）
  - タスク: なし（直接 CLI 実行）
  - コマンド例（PowerShell）:
    
    ```powershell
    dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release --key C --pcs "5,8,0,3" --json
    ```
    
    → Roman 正規化と Mixture-7th 警告
  例（要旨）:
  ```json
  { "chords": [ { "roman": "iv7", "warnings": ["Mixture: iv7 typically resolves to V"] } ] }
  ```
  確認ポイント:
  - `chords[0].roman` が `iv7`（長調での Roman 入力では `IVmaj7` に正規化される点に注意。ここでは pcs 入力で検証）。
  - `chords[0].warnings` に Mixture-7th 系の助言が含まれる。

再現用タスク対応（サマリ）:

- V9 表示トグル: 「cli: json (V9)」/「cli: json (V7(9)）」
- Cadential 6-4 をドミナント表記: 「cli: demo (cadential 6-4 as dominant)」
- Neapolitan 強制: 「cli: demo (Neapolitan enforceN6)」
- iv7（pcs入力）: タスクなし、上記 PowerShell コマンド例を利用
  
- Mixture-7th 警告（タスク: cli: demo (mixture 7th)）
  - `--roman "bVI7; bII7; V; I" --json`
  例（要旨・chordsのみ）:
  ```json
  {
    "chords": [
      { "index": 0, "roman": "bVI7", "warnings": ["Mixture: bVI7 typically resolves to V"], "errors": [] },
      { "index": 1, "roman": "bII7", "warnings": ["Mixture: bII7 (Neapolitan 7) often resolves to V or I6"], "errors": [] }
    ]
  }
  ```
  確認ポイント:
  - `chords[i].warnings` に Mixture-7th 系の助言が含まれる（bVI7/bII7/iv7/bVII7）。
  - iv7 の検証は Roman 入力だと IVmaj7 に正規化されるため、pcs 入力の例（本節の iv7 例）を参照。
- Mixture-7th 警告: 「cli: demo (mixture 7th)」（iv7 は pcs 入力の例を参照）
- IAC 近似（Authentic だが pac=false）: 「cli: json (IAC: V6->I)」「cli: json (IAC: V->I6)」

- モジュレーション JSON（タスク: cli: json (modulation preset permissive|stable)）
  - `--pcs "0,4,7; 7,11,2; 0,4,7; 2,6,9,0; 7,11,2; 4,7,11; 9,0,4; 2,6,9; 7,11,2" --segments --trace --preset permissive --json`
  確認ポイント:
  - `options.preset` が `"permissive"` または `"stable"`。
  - `segments[]` に C→G の区間分割が含まれ、各要素に `confidence` がある。
  - `estimator` に `window/minSwitch/prevBias/initBias/switchMargin/outPenalty` が出力される。
  - `trace[]` に `{ index, max, second, margin }` が出力される（--trace 時）。

## 6. 主要ポリシー（分析設計の要旨）

- Triad 転回は distinct=3 音のときのみ（4音以上は Seventh/Tension を優先）
- V9 は早期判定 + 最終再チェックで誤検出/取りこぼしを抑止
- Aug6（It6/Fr43/Ger65）と bVI7 は voicing とオプションで棲み分け
  - 既定: bass=b6 で Aug6 条件合致なら Aug6 優先。ただしソプラノ=b6 では慣用上 bVI7 を優先表示
  - `--preferMixture7` で曖昧時に Mixture 7th 優先へ反転可能
- Neapolitan（bII 三和音）は `--enforceN6` で bII6 に統一（bII7 には非適用）
- 一般 6-4 と Cadential 6-4 の分離
  - Passing/Pedal は非カデンツの並びにのみ付与
  - Cadential 6-4 は Authentic などのカデンツ項目に付随
- Strict PAC（近似）オプション
  - 素朴三和音のみ/PACでのドミナント拡張禁止/最終 I のソプラノ=主音 等を設定可能

---

## 7. 既知の制限

- 借用 Seventh は iv7 / bVII7 / bII7 / bVI7 の最小集合に対応（その他は未実装）
- V9 は {1,3,b7,9}(+任意5度) の 4–5 音構成のみを対象
- 入力はピッチクラス（0..11）で、オクターブ差は無視（重複は除外）
- 七和音の転回付与には FourPartVoicing が必要（未指定時はルート表記）

---

## 8. テスト / カバレッジ

安定実行の推奨手順:

```powershell
# 安定化の基本順序
dotnet build -c Release

# no-build でテスト（最新ビルドを使用）
dotnet test -c Release --nologo --no-build
```

カバレッジ（Cobertura → HTML 生成）:

```powershell
# 収集（安定化したい場合は VS Code タスクの stable 版を利用）
dotnet test -c Release --nologo --no-build `
  --results-directory Tests/MusicTheory.Tests/TestResults `
  --collect 'XPlat Code Coverage' -- `
  DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

# HTML 生成（ReportGenerator 必要）
reportgenerator -reports:Tests/MusicTheory.Tests/TestResults/**/coverage.cobertura.xml `
  -targetdir:Tests/MusicTheory.Tests/TestResults/coverage-report -reporttypes:Html
```

バッジ表示（Pages 未設定時のフォールバック）:

- `docs/badge_linecoverage.svg` を README から参照
- 将来 Pages を有効化したら `https://<OWNER>.github.io/<REPO>/badge_linecoverage.svg` へ切替可

---

## 9. トラブルシューティング

- テストが稀に不一致/失敗する
  - 直前に `dotnet build -c Release` を挟み、`--no-build` で実行
  - VS Code タスクの「validate: build+test」や「dotnet: test (retry)」を利用
- PowerShell のクオート問題（セミコロンやダブルクオート）
  - `--roman "I; V; I"` のように全体をダブルクオートで囲む
- ReportGenerator が見つからない
  - `dotnet tool install -g dotnet-reportgenerator-globaltool` を実行

---

## 10. FAQ（抜粋）

- Q: Triad の 6/64 が出ない
  - A: distinct PC が3音のときのみ付与します。4音以上だと Seventh/Tension を優先する設計です。
- Q: V9 と `V7(9)` の表記を切り替えたい
  - A: `--v7n9` を指定（機能判定は同一で表示のみ変更）。
- Q: Neapolitan を常に bII6 にしたい
  - A: `--enforceN6` を指定（bII7 は対象外）。
- Q: Aug6 と bVI7 が曖昧に見える
  - A: `--preferMixture7` で Mixture 7th を優先。既定でもソプラノ=b6 のときは bVI7 を優先表示します。

---

## 11. 用語（簡易）

- distinct ピッチクラス: 重複を除いた 0..11 の集合
- Cadential 6-4: I64→V→I で現れる 6-4（一般 6-4 とは区別）
- PAC/IAC: Perfect/Imperfect Authentic Cadence（本ライブラリでは近似判定）

---

## 12. 参考

- 詳細な設計と例は README を参照（ローマ数字例、Aug6/bVI7 の比較、Neapolitan の声部進行ヒント、V9 表記ポリシーなど）。
- CLI スキーマは `--schema` で取得できます（`util:*` スキーマも同様）。

以上です。ご不明点・改善要望は Issue/PR でお知らせください。
