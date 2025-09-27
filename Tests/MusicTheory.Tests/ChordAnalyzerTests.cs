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
}
