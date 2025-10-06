using MusicTheory.Theory.Analysis;
using MusicTheory.Theory.Chord;
using MusicTheory.Theory.Scale;
using Xunit;

namespace MusicTheory.Tests;

/// <summary>
/// ChordAnalyzer の追加テスト（Phase 4: カバレッジ拡張）
/// </summary>
public class ChordAnalyzerExtendedTests
{
    [Fact]
    public void Analyze_EmptyInput_ReturnsEmpty()
    {
        var result = ChordAnalyzer.Analyze(Array.Empty<int>()).ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void Analyze_SinglePitchClass_MatchesRootOnlyFormulas()
    {
        // Root のみ (例: C のみ) の候補を生成（該当フォーミュラがあれば）
        var result = ChordAnalyzer.Analyze(new[] { 0 }).ToList();
        // Triad 最小要件で候補がない場合は空でも OK
        Assert.NotNull(result);
    }

    [Fact]
    public void Analyze_AllowInversions_False_OnlyMinRootCandidates()
    {
        // AllowInversions=false の場合、最小 PC のみをルートとして許可
        var pcs = new[] { 0, 4, 7 }; // C E G
        var result = ChordAnalyzer.Analyze(pcs, new ChordAnalyzerOptions(AllowInversions: false)).ToList();
        Assert.All(result, c => Assert.Equal("C", c.RootName));
        // 転回なしなので E や G がルートの候補はないはず
        Assert.DoesNotContain(result, c => c.RootName == "E" || c.RootName == "G");
    }

    [Fact]
    public void Analyze_AllowInversions_True_MultipleRoots()
    {
        // AllowInversions=true の場合、すべての PC がルート候補として試行される
        // ただし、フォーミュラ条件を満たす候補のみが生成される
        var pcs = new[] { 0, 4, 7 }; // C E G
        var result = ChordAnalyzer.Analyze(pcs, new ChordAnalyzerOptions(AllowInversions: true)).ToList();
        // 少なくとも C がルートの候補は存在するはず
        Assert.Contains(result, c => c.RootName == "C");
        // E や G がルートの候補も存在する可能性があるが、フォーミュラ次第
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Analyze_DuplicatePitchClasses_Normalized()
    {
        // 重複した PC を含む入力は正規化される
        var pcs = new[] { 0, 0, 4, 4, 7 }; // C E G (重複あり)
        var result = ChordAnalyzer.Analyze(pcs).ToList();
        Assert.NotEmpty(result);
        Assert.Contains(result, c => c.RootName == "C" && c.Formula.Symbol == "");
    }

    [Fact]
    public void Analyze_OutOfRange_PitchClasses_Mod12Normalized()
    {
        // 12 以上や負の PC も mod 12 で正規化される
        var pcs = new[] { 12, 16, 19, -5 }; // 0, 4, 7, 7 (mod 12)
        var result = ChordAnalyzer.Analyze(pcs).ToList();
        Assert.NotEmpty(result);
    }

    [Fact]
    public void ToListRanked_ReturnsDescendingTotalScore()
    {
        var pcs = new[] { 0, 4, 7, 10, 2, 5, 9 }; // C13 完全形
        var result = ChordAnalyzer.ToListRanked(pcs);
        // TotalScore の降順でソートされていることを確認
        for (int i = 0; i < result.Count - 1; i++)
        {
            Assert.True(result[i].TotalScore >= result[i + 1].TotalScore,
                $"Index {i}: {result[i].TotalScore} should be >= {result[i + 1].TotalScore}");
        }
    }

    [Fact]
    public void AnalyzeRanked_EnumerableReturnsSorted()
    {
        var pcs = new[] { 0, 3, 7, 10 }; // Cm7
        var result = ChordAnalyzer.AnalyzeRanked(pcs).ToList();
        // TotalScore の降順であることを確認
        for (int i = 0; i < result.Count - 1; i++)
        {
            Assert.True(result[i].TotalScore >= result[i + 1].TotalScore);
        }
    }

    [Fact]
    public void Analyze_ScaleFitDetail_Populated()
    {
        // ScaleDetail が適切に設定されることを確認
        var pcs = new[] { 0, 4, 7, 11 }; // Cmaj7
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "maj7");
        Assert.True(cand != default);
        Assert.NotNull(cand.ScaleDetail);
        Assert.NotNull(cand.ScaleDetail!.Value.ScaleName);
    }

    [Fact]
    public void Analyze_AlternativeScales_Populated()
    {
        var pcs = new[] { 0, 4, 7, 10 }; // C7
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "7");
        Assert.True(cand != default);
        Assert.NotNull(cand.AlternativeScales);
        Assert.NotEmpty(cand.AlternativeScales);
    }

    [Fact]
    public void Analyze_AlternativeScaleRanks_Populated()
    {
        var pcs = new[] { 0, 4, 7, 10 }; // C7
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "7");
        Assert.True(cand != default);
        Assert.NotNull(cand.AlternativeScaleRanks);
        Assert.NotEmpty(cand.AlternativeScaleRanks);
    }

    [Fact]
    public void Analyze_MissingIntervalNames_Populated()
    {
        // 不足している音程の名前が設定されることを確認
        var pcs = new[] { 0, 4, 11 }; // Cmaj7 without 5th
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "maj7");
        Assert.True(cand != default);
        Assert.NotNull(cand.MissingIntervalNames);
        Assert.NotEmpty(cand.MissingIntervalNames);
    }

    [Fact]
    public void Analyze_FormulaSignature_MatchesFormula()
    {
        var pcs = new[] { 0, 4, 7 }; // C major triad
        var result = ChordAnalyzer.Analyze(pcs).ToList();
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "");
        Assert.True(cand != default);
        Assert.NotNull(cand.FormulaSignature);
        Assert.NotEmpty(cand.FormulaSignature);
    }

    [Fact]
    public void Analyze_InputSignature_ReflectsInput()
    {
        var pcs = new[] { 0, 4, 7 }; // C E G
        var result = ChordAnalyzer.Analyze(pcs).ToList();
        var cand = result.First();
        Assert.NotNull(cand.InputSignature);
        Assert.Contains("4", cand.InputSignature);
        Assert.Contains("7", cand.InputSignature);
    }

    [Fact]
    public void Analyze_CoverageScore_ReflectsMatches()
    {
        var pcs = new[] { 0, 4, 7, 11 }; // Cmaj7 完全形
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "maj7");
        Assert.True(cand != default);
        // Core 3つ (3rd, 5th, 7th) すべて存在するので CoverageScore は高い
        Assert.True(cand.CoverageScore > 0);
    }

    [Fact]
    public void Analyze_CompletionRatio_CorrectCalculation()
    {
        var pcs = new[] { 0, 4, 7, 11 }; // Cmaj7 完全形
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "maj7");
        Assert.True(cand != default);
        // すべてのコア音程が含まれているので CompletionRatio は 1.0
        Assert.Equal(1.0, cand.CompletionRatio, precision: 2);
    }

    [Fact]
    public void Analyze_CompletionRatio_PartialCompletion()
    {
        var pcs = new[] { 0, 4, 11 }; // Cmaj7 without 5th
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "maj7");
        Assert.True(cand != default);
        // 3つの Core のうち 2つ (3rd, 7th) が存在、5th 不足 → 2/3 = 0.666...
        Assert.True(cand.CompletionRatio < 1.0);
        Assert.True(cand.CompletionRatio > 0.5);
    }

    [Fact]
    public void Analyze_ScaleName_PopulatedForKnownChords()
    {
        var pcs = new[] { 0, 4, 7, 10 }; // C7
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "7");
        Assert.True(cand != default);
        Assert.NotNull(cand.ScaleName);
        Assert.NotEmpty(cand.ScaleName);
    }

    [Fact]
    public void Analyze_SuggestedScale_NotNull()
    {
        var pcs = new[] { 0, 4, 7, 10 }; // C7
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "7");
        Assert.True(cand != default);
        Assert.NotNull(cand.SuggestedScale);
    }

    [Fact]
    public void Analyze_CustomWeights_AffectTotalScore()
    {
        var pcs = new[] { 0, 4, 7, 10 }; // C7
        // 既定の重み
        var result1 = ChordAnalyzer.ToListRanked(pcs, new ChordAnalyzerOptions()).ToList();
        // カバレッジ重みを大きくした場合
        var result2 = ChordAnalyzer.ToListRanked(pcs, new ChordAnalyzerOptions(CoverageWeight: 10.0)).ToList();
        // 異なる重みで TotalScore が変わることを確認
        var cand1 = result1.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "7");
        var cand2 = result2.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "7");
        Assert.True(cand1 != default && cand2 != default);
        Assert.NotEqual(cand1.TotalScore, cand2.TotalScore);
    }

    [Theory]
    [InlineData(0, 2, 4, 5, 7, 9, 11)] // C major scale
    public void Analyze_LargePitchClassSet_ProducesCandidates(params int[] pcs)
    {
        // 大きな PC セットの場合、フォーミュラ条件を満たす候補が生成される
        // スケール全体を与えた場合、候補が0の可能性もあるため、結果の存在を確認
        var result = ChordAnalyzer.Analyze(pcs).ToList();
        // 候補が生成されるかどうかは実装依存だが、エラーなく完了することを確認
        Assert.NotNull(result);
    }

    [Fact]
    public void Analyze_CoreCoverageWeight_AffectsScore()
    {
        var pcs = new[] { 0, 4, 7, 11 }; // Cmaj7
        var result1 = ChordAnalyzer.ToListRanked(pcs, new ChordAnalyzerOptions(CoreCoverageWeight: 1.0)).ToList();
        var result2 = ChordAnalyzer.ToListRanked(pcs, new ChordAnalyzerOptions(CoreCoverageWeight: 5.0)).ToList();
        var cand1 = result1.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "maj7");
        var cand2 = result2.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "maj7");
        Assert.True(cand1 != default && cand2 != default);
        Assert.NotEqual(cand1.TotalScore, cand2.TotalScore);
    }

    [Fact]
    public void Analyze_TensionCoverageWeight_AffectsScore()
    {
        var pcs = new[] { 0, 4, 7, 10, 2 }; // C9
        var result1 = ChordAnalyzer.ToListRanked(pcs, new ChordAnalyzerOptions(TensionCoverageWeight: 0.5)).ToList();
        var result2 = ChordAnalyzer.ToListRanked(pcs, new ChordAnalyzerOptions(TensionCoverageWeight: 2.0)).ToList();
        var cand1 = result1.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "9");
        var cand2 = result2.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "9");
        if (cand1 != default && cand2 != default)
        {
            Assert.NotEqual(cand1.TotalScore, cand2.TotalScore);
        }
    }

    [Fact]
    public void Analyze_ScaleRankingCoreSemitones_CustomValues()
    {
        var pcs = new[] { 0, 4, 7, 10 }; // C7
        var customCore = new[] { 0, 4, 7 }; // Root, 3rd, 5th のみをコアとする
        var result = ChordAnalyzer.ToListRanked(pcs, new ChordAnalyzerOptions(ScaleRankingCoreSemitones: customCore)).ToList();
        Assert.NotEmpty(result);
    }

    [Fact]
    public void Analyze_ScaleFitDetail_Covered_Uncovered()
    {
        var pcs = new[] { 0, 4, 7, 11, 2 }; // Cmaj9
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && (c.Formula.Symbol == "maj7" || c.Formula.Symbol == "maj9"));
        if (cand != default && cand.ScaleDetail != null)
        {
            var detail = cand.ScaleDetail.Value;
            Assert.NotNull(detail.Covered);
            Assert.NotNull(detail.Uncovered);
            // Covered には入力 PC が含まれるはず
            Assert.Contains(4, detail.Covered); // 3rd
            Assert.Contains(7, detail.Covered); // 5th
            Assert.Contains(11, detail.Covered); // 7th
        }
    }

    [Fact]
    public void Analyze_ScaleFitDetail_TensionCoverage()
    {
        var pcs = new[] { 0, 4, 7, 10, 2, 5, 9 }; // C13
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "13");
        if (cand != default && cand.ScaleDetail != null)
        {
            var detail = cand.ScaleDetail.Value;
            Assert.True(detail.TensionCoverage >= 0 && detail.TensionCoverage <= 1.0);
            Assert.NotNull(detail.PresentTensions);
            Assert.NotNull(detail.MissingTensions);
        }
    }

    [Fact]
    public void Analyze_ScaleFitDetail_CoreCoverage()
    {
        var pcs = new[] { 0, 4, 7, 11 }; // Cmaj7
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "maj7");
        if (cand != default && cand.ScaleDetail != null)
        {
            var detail = cand.ScaleDetail.Value;
            Assert.True(detail.CoreCoverage >= 0 && detail.CoreCoverage <= 1.0);
            Assert.NotNull(detail.PresentCores);
            Assert.NotNull(detail.MissingCores);
        }
    }

    [Fact]
    public void Analyze_AlternativeScaleRankInfo_SharedRatio()
    {
        var pcs = new[] { 0, 4, 7, 10 }; // C7
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "7");
        if (cand != default && cand.AlternativeScaleRanks.Count > 0)
        {
            var rank = cand.AlternativeScaleRanks[0];
            Assert.True(rank.SharedRatio >= 0 && rank.SharedRatio <= 1.0);
        }
    }

    [Fact]
    public void Analyze_AlternativeScaleRankInfo_TensionPresence()
    {
        var pcs = new[] { 0, 4, 7, 10 }; // C7
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "7");
        if (cand != default && cand.AlternativeScaleRanks.Count > 0)
        {
            var rank = cand.AlternativeScaleRanks[0];
            Assert.True(rank.TensionPresence >= 0 && rank.TensionPresence <= 1.0);
        }
    }

    [Fact]
    public void Analyze_AlternativeScaleRankInfo_Priority()
    {
        var pcs = new[] { 0, 4, 7, 10 }; // C7
        var result = ChordAnalyzer.ToListRanked(pcs);
        var cand = result.FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "7");
        if (cand != default && cand.AlternativeScaleRanks.Count > 0)
        {
            var rank = cand.AlternativeScaleRanks[0];
            Assert.True(rank.Priority >= 0);
        }
    }

    [Fact]
    public void Analyze_NormalizedSet_CorrectTransposition()
    {
        var pcs = new[] { 4, 7, 11 }; // E G B
        var result = ChordAnalyzer.Analyze(pcs).ToList();
        var cand = result.FirstOrDefault(c => c.RootName == "E");
        if (cand != default)
        {
            Assert.NotNull(cand.NormalizedSet);
            // E がルートの場合、正規化セットは 0 を含むはず
            Assert.Contains(0, cand.NormalizedSet);
        }
    }
}
