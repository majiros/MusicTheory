using System.Collections.Generic;
using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class ProgressionAnalyzerOverloadsTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void AnalyzeWithDetailedCadences_NoOptions_Captures_Cadential64_And_Authentic()
    {
        var key = new Key(60, true);
        var pcs = new List<int[]>
        {
            new[] { Pc(60), Pc(64), Pc(67) },
            new[] { Pc(67), Pc(71), Pc(62) },
            new[] { Pc(60), Pc(64), Pc(67) },
        };
        var voicings = new List<FourPartVoicing?>
        {
            new FourPartVoicing(S:79, A:72, T:76, B:67),
            new FourPartVoicing(S:79, A:71, T:67, B:55),
            new FourPartVoicing(S:72, A:64, T:60, B:48),
        };

        var (res, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(pcs, key, voicings);
        Assert.Single(cadences);
        Assert.Equal(CadenceType.Authentic, cadences[0].Type);
        Assert.True(cadences[0].HasCadentialSixFour);
        // No relabel when no options passed
        Assert.Equal("I64", res.Chords[0].RomanText);
    }

    [Fact]
    public void AnalyzeWithKeyEstimate_WithTrace_Overload_Works()
    {
        var key = new Key(60, true);
        var pcs = new List<int[]>
        {
            new[] { 0, 4, 7 }, new[] { 0, 4, 7 }, new[] { 0, 4, 7 }
        };
        var opt = new KeyEstimator.Options { CollectTrace = true, Window = 1 };
        var (_res, segs, keys) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(pcs, key, opt, out var trace, voicings: null);
        Assert.Equal(3, keys.Count);
        Assert.Equal(3, trace.Count);
        Assert.True(segs.Count >= 1);
    }
}
