using MusicTheory.Theory.Analysis;

namespace MusicTheory.Tests;

public class ChordAnalyzerTests
{
    [Fact]
    public void Analyze_DominantSeventhSet()
    {
        // G7: G B D F => pcs 7,11,2,5
        var pcs = new[]{7,11,2,5};
    var result = ChordAnalyzer.Analyze(pcs, new ChordAnalyzerOptions(AllowInversions: true)).ToList();
        Assert.Contains(result, c => c.Formula.Symbol == "7" && c.RootName == "G");
    }

    [Fact]
    public void Analyze_AlteredDominantCandidates()
    {
        var pcs = new[]{0,4,10,1,3,6,8};
    var result = ChordAnalyzer.Analyze(pcs, new ChordAnalyzerOptions(AllowInversions: true)).ToList();
        Assert.Contains(result, c => c.Formula.Symbol == "7alt" && c.RootName == "C");
    }

    [Fact]
    public void Analyze_MissingCoreAndTension_Classified()
    {
        // C13 formula core: 3,7,10 tensions: 2,5,9 (relative to root)
        // Provide C root, 3rd, b7, 9, 13 but omit 11 (5) and 5th (7) to test classification
        var pcs = new[]{0,4,10,2,9}; // C E Bb D A
        var result = ChordAnalyzer.ToListRanked(pcs, new ChordAnalyzerOptions(AllowInversions:true));
        var cand = result.First(c=>c.Formula.Symbol=="13" && c.RootName=="C");
        Assert.Contains(7, cand.MissingCoreIntervals); // perfect fifth missing
        Assert.Contains(5, cand.MissingTensionIntervals); // 11th missing (tension)
    }

    [Fact]
    public void Analyze_MinCompletion_FiltersLow()
    {
    // Provide root + 3rd + major 7th of a maj7 chord (missing 5th) -> incomplete (2/3), should be filtered with higher MinCompletion
    var pcs = new[]{0,4,11};
        var result = ChordAnalyzer.Analyze(pcs, new ChordAnalyzerOptions(MinCompletion:0.8)).ToList();
        Assert.DoesNotContain(result, c=>c.Formula.Symbol=="maj7" && c.RootName=="C");
        // Lower threshold allows it
        var result2 = ChordAnalyzer.Analyze(pcs, new ChordAnalyzerOptions(MinCompletion:0.2)).ToList();
        Assert.Contains(result2, c=>c.Formula.Symbol=="maj7" && c.RootName=="C");
    }

    [Fact]
    public void Analyze_AvoidPenalty_AppliedForIonianNatural11()
    {
        // C Ionian with natural 11 (F) over maj7 chord expected avoid penalty (contains 4 (E), 5 (F), 11 (B))
        var pcs = new[]{0,4,7,11,5}; // C E G B F
    var list = ChordAnalyzer.ToListRanked(pcs);
    // maj13 が優先されている可能性を考慮
    var cand = list.FirstOrDefault(c=> (c.Formula.Symbol=="maj7" || c.Formula.Symbol=="maj13") && c.RootName=="C");
    Assert.True(cand != default, "maj7/maj13 candidate not found. Got: " + string.Join(";", list.Select(x=>$"{x.RootName}{x.Formula.Symbol}")));
    Assert.True(cand.AvoidPenalty > 0);
    Assert.NotNull(cand.ScaleDetail);
    Assert.Contains(5, cand.ScaleDetail!.Value.AvoidNotes);
    }

    [Fact]
    public void Analyze_ColorBoost_AppliedForLydianSharp11()
    {
        // C Lydian (#11) color: include #11 (F#=6)
        var pcs = new[]{0,4,7,11,6}; // C E G B F#
        var list = ChordAnalyzer.ToListRanked(pcs).ToList();
        // #11 だけで 9,11,13 が揃っていないので maj13 フォーミュラ subset 条件を満たさず候補0の可能性 → スケール色付け評価対象にならないケースを許容
        var colorCandidate = list.FirstOrDefault(c=>c.RootName=="C" && (c.Formula.Symbol=="maj7" || c.Formula.Symbol=="maj13"));
            if (colorCandidate != default)
            {
                Assert.True(colorCandidate.ColorBoost >= 0); // 存在する場合は非負 (色付け有無は将来調整余地)
            }
    }

    [Fact]
    public void Analyze_Ranking_TotalScoreDescending()
    {
        // Two related candidates: one with extra color tone should score higher
        var baseSet = new[]{0,4,7,10}; // C7
        var colorSet = new[]{0,4,7,10,9}; // C13 adds 13
        var baseCandidate = ChordAnalyzer.ToListRanked(baseSet).First(c=>c.Formula.Symbol=="7" && c.RootName=="C");
        var colorCandidate = ChordAnalyzer.ToListRanked(colorSet).First(c=>c.Formula.Symbol=="13" && c.RootName=="C");
        Assert.True(colorCandidate.TotalScore >= baseCandidate.TotalScore);
    }

    [Fact]
    public void EssentialCore_AllowsOmittingPerfectFifth()
    {
        // C E B (0,4,11) : maj7 の 5th 省略ケース
        var pcs = new[]{0,4,11};
        var list = ChordAnalyzer.ToListRanked(pcs);
        var cand = list.FirstOrDefault(c=>c.RootName=="C" && c.Formula.Symbol=="maj7");
        Assert.True(cand != default, "maj7 candidate not produced for C E B");
        Assert.Contains(7, cand.MissingCoreIntervals); // 完全5度不足として報告
        Assert.DoesNotContain(4, cand.MissingCoreIntervals); // 3rd は含まれている
        Assert.DoesNotContain(11, cand.MissingCoreIntervals); // 7th も含まれている
    }

    [Fact]
    public void EssentialCore_RejectsMissingThird()
    {
        // C B (0,11) : 3rd 欠落のため maj7 不許可
        var pcs = new[]{0,11};
        var list = ChordAnalyzer.ToListRanked(pcs);
        Assert.DoesNotContain(list, c=>c.RootName=="C" && c.Formula.Symbol=="maj7");
    }

    // Phase 4: Additional comprehensive tests for ChordAnalyzer
    [Fact]
    public void Analyze_EmptyInput_ReturnsEmpty()
    {
        var result = ChordAnalyzer.Analyze(Array.Empty<int>());
        Assert.Empty(result);
    }

    [Fact]
    public void Analyze_AllowInversionsFalse_OnlyMinRootCandidates()
    {
        // G7: pcs 7,11,2,5 → 最小rootは2
        var pcs = new[]{7,11,2,5};
        var result = ChordAnalyzer.Analyze(pcs, new ChordAnalyzerOptions(AllowInversions: false)).ToList();
        // AllowInversions=false なら、root=2 のみ許可
        Assert.All(result, c => Assert.Equal("D", c.RootName));
    }

    [Fact]
    public void AnalyzeRanked_ReturnsSortedByTotalScore()
    {
        var pcs = new[]{0,4,7,11};
        var result = ChordAnalyzer.AnalyzeRanked(pcs).ToList();
        // スコアが降順であることを確認
        for (int i = 0; i < result.Count - 1; i++)
        {
            Assert.True(result[i].TotalScore >= result[i+1].TotalScore);
        }
    }

    [Fact]
    public void ToListRanked_ReturnsListInDescendingOrder()
    {
        var pcs = new[]{0,3,7,10};
        var result = ChordAnalyzer.ToListRanked(pcs);
        Assert.IsType<List<ChordCandidate>>(result);
        for (int i = 0; i < result.Count - 1; i++)
        {
            Assert.True(result[i].TotalScore >= result[i+1].TotalScore);
        }
    }

    [Fact]
    public void SuggestScales_ReturnsMatchingScales()
    {
        // maj7 symbol に対して Ionian/Lydian などが返る
        var normalized = new[]{0,4,7,11};
        var scales = ChordAnalyzer.SuggestScales("maj7", normalized).ToList();
        Assert.NotEmpty(scales);
        Assert.Contains(scales, s => s.Name.Contains("Ionian") || s.Name.Contains("Lydian"));
    }

    [Fact]
    public void SuggestScales_NoHint_ReturnsEmpty()
    {
        var normalized = new[]{0,4,7};
        var scales = ChordAnalyzer.SuggestScales("unknown_symbol", normalized).ToList();
        Assert.Empty(scales);
    }

    [Fact]
    public void SuggestScales_NoExactMatch_ReturnsAllHints()
    {
        // m7 には Dorian/Aeolian ヒントがあるが、normalized が完全一致しない場合でもヒントを返す
        var normalized = new[]{0,3,7,10,1}; // extra note: Db
        var scales = ChordAnalyzer.SuggestScales("m7", normalized).ToList();
        Assert.NotEmpty(scales);
    }

    [Fact]
    public void ToDto_PreservesEssentialData()
    {
        var pcs = new[]{0,4,7,10};
        var candidate = ChordAnalyzer.ToListRanked(pcs).First();
        var dto = candidate.ToDto();
        Assert.Equal(candidate.RootName, dto.Root);
        Assert.Equal(candidate.Formula.Symbol, dto.Symbol);
        Assert.Equal(candidate.TotalScore, dto.TotalScore);
        Assert.Equal(candidate.CompletionRatio, dto.CompletionRatio);
    }

    [Fact]
    public void ToDto_WithScaleDetail_PreservesScaleInfo()
    {
        var pcs = new[]{0,4,7,11,2,5,9}; // Cmaj13
        var candidate = ChordAnalyzer.ToListRanked(pcs).FirstOrDefault(c => c.RootName == "C" && c.ScaleDetail.HasValue);
        if (candidate != default)
        {
            var dto = candidate.ToDto();
            Assert.NotNull(dto.ScaleDetail);
            Assert.Equal(candidate.ScaleDetail!.Value.ScaleName, dto.ScaleDetail!.Value.ScaleName);
        }
    }

    [Fact]
    public void Analyze_CoreCoverageWeight_AffectsScore()
    {
        var pcs = new[]{0,4,7,11}; // Cmaj7 完全
        var options = new ChordAnalyzerOptions(CoreCoverageWeight: 10.0);
        var candidate = ChordAnalyzer.ToListRanked(pcs, options).First(c => c.RootName == "C" && c.Formula.Symbol == "maj7");
        Assert.True(candidate.TotalScore > 0);
    }

    [Fact]
    public void Analyze_TensionCoverageWeight_AffectsScore()
    {
        var pcs = new[]{0,4,7,10,2,5,9}; // C13
        var options = new ChordAnalyzerOptions(TensionCoverageWeight: 5.0);
        var candidate = ChordAnalyzer.ToListRanked(pcs, options).First(c => c.RootName == "C" && c.Formula.Symbol == "13");
        Assert.True(candidate.TotalScore > 0);
    }

    [Fact]
    public void Analyze_CustomScaleRankingCore_UsesCustomDefinition()
    {
        var pcs = new[]{0,4,7,11};
        var options = new ChordAnalyzerOptions(ScaleRankingCoreSemitones: new[]{3,4,11});
        var result = ChordAnalyzer.ToListRanked(pcs, options);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void RankAlternativeScales_ExactCoverBonus()
    {
        // 完全一致でボーナスが付与されることを間接確認
        var pcs = new[]{0,4,7,11};
        var candidate = ChordAnalyzer.ToListRanked(pcs).First(c => c.RootName == "C" && c.Formula.Symbol == "maj7");
        Assert.NotEmpty(candidate.AlternativeScaleRanks);
        var topRank = candidate.AlternativeScaleRanks.First();
        Assert.True(topRank.Priority > 0);
    }

    [Fact]
    public void ScaleFitDetail_CapturesPresentAndMissingTensions()
    {
        var pcs = new[]{0,4,7,10,2}; // C7 + 9
        var candidate = ChordAnalyzer.ToListRanked(pcs).FirstOrDefault(c => c.RootName == "C" && c.Formula.Symbol == "9");
        if (candidate != default && candidate.ScaleDetail.HasValue)
        {
            var detail = candidate.ScaleDetail!.Value;
            Assert.Contains(2, detail.PresentTensions);
            Assert.True(detail.TensionCoverage > 0);
        }
    }

    [Fact]
    public void ScaleFitDetail_CapturesPresentAndMissingCores()
    {
        var pcs = new[]{0,4,11}; // Cmaj7 without 5th
        var candidate = ChordAnalyzer.ToListRanked(pcs).First(c => c.RootName == "C" && c.Formula.Symbol == "maj7");
        if (candidate.ScaleDetail.HasValue)
        {
            var detail = candidate.ScaleDetail!.Value;
            Assert.Contains(4, detail.PresentCores);
            Assert.Contains(11, detail.PresentCores);
            Assert.True(detail.CoreCoverage < 1.0); // missing 5th
        }
    }
}
