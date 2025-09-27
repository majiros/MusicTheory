# PAC/導音解決と抑止規則（設計ノート）

このノートは CadenceAnalyzer.DetectDetailed の設計メモです。実装に対する高レベル方針と、テストでカバーしている抑止規則を要約します。

## 目的

- PAC (Perfect Authentic Cadence) を教育的・採点用途で厳格に判定できるよう、任意のフラグでゲートする。
- 誤検出になりやすい経路（I64→V の Half、Aug6→V の Half、vii…/V→I の直接解決など）を抑止し、終止の重複・取りこぼしを防止する。

## PAC ゲート（主なフラグ）

- StrictPacPlainTriadsOnly: PAC は三和音のみ（7th/Tensions を含む場合は PAC にはしない）。
- StrictPacDisallowDominantExtensions: ドミナントに拡張音（7/9 等）が含まれている場合は PAC にはしない。
- StrictPacRequireSopranoTonic: 最終 I のソプラノが主音であることを要求（FourPartVoicing 必須）。
- StrictPacRequireSopranoLeadingToneResolution: 直前 V のソプラノが導音だった場合、主音へ上行解決を要求（FourPartVoicing 必須）。
- 追加要件（ボイシング有りのとき）: ドミナントは原位（root）であること。反転ドミナントは Authentic 可だが PAC ではない。

## 抑止規則（誤検出防止）

- I64→V の Half を抑止し、後続の V→I Authentic に集約（Cadential 6-4）。
- Augmented Sixth（It6/Fr43/Ger65）→V の Half を抑止。
- 二次導音（vii…/V ファミリ）→I の直接遷移は Authentic にしない（Ger65 の異名同音経路も含む）。
- Authentic のテキストフォールバックは厳密に V→I のヘッド一致に限定（vii°7/V の誤読を防止）。

## 実装メモ

- ProgressionAnalyzer から DetectDetailed に `tonicPc`、`RomanText`、`voicings` を受け取る。
- PAC 判定はまずドミナント原位（voicing 有りの場合）を確認し、Strict フラグに従って PAC フラグを付与する。
- 6-4 は Cadential/Passing/Pedal を区別し、終止項目（Type!=None）には一般 6-4 を付与しない。

## テスト

- 代表テスト
  - I64→V→I: Authentic, Cadential Six-Four, PAC 条件満たせば PAC。
  - Ger65→I: Authentic にならない。
  - Ger65→V→I: Half を抑止し、最終 Authentic のみ。
  - 反転ドミナント/拡張ドミナント: Authentic 可、PAC ではない。
  - V9 表記トグル: 表示のみの差分（V9 vs V7(9)）。
  - モジュレーションプリセット差分: JSON Segments がプリセットに応じて変化。

## 既知の制約

- Strict PAC の一部条件（ソプラノ確認）はボイシングが必要です。CLI で PAC を評価する場合は JSON の `cadences[].isPac` を参照してください。
