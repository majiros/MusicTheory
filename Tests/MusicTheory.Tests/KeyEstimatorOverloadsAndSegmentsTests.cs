using System.Collections.Generic;
using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class KeyEstimatorOverloadsAndSegmentsTests
{
    [Fact]
    public void EstimatePerChord_WindowOverload_Works()
    {
        var pcs = new List<int[]> { new[] { 0, 4, 7 }, new[] { 7, 11, 2 }, new[] { 0, 4, 7 } };
        var keys = KeyEstimator.EstimatePerChord(pcs, new Key(60, true), window: 1);
        Assert.Equal(3, keys.Count);
    }

    [Fact]
    public void EstimatePerChord_WithOptions_OutTrace_Works()
    {
        var pcs = new List<int[]> { new[] { 0, 4, 7 }, new[] { 7, 11, 2 }, new[] { 0, 4, 7 } };
        var opt = new KeyEstimator.Options { CollectTrace = true, Window = 1 };
        var keys = KeyEstimator.EstimatePerChord(pcs, new Key(60, true), opt, out var trace);
        Assert.Equal(3, keys.Count);
        Assert.Equal(3, trace.Count);
    }

    [Fact]
    public void AnalyzeWithKeyEstimateAndModulation_Fallback_When_All_Filtered()
    {
        var pcs = new List<int[]>
        {
            new[] { 0, 4, 7 },
            new[] { 2, 5, 9 },
            new[] { 0, 4, 7 },
            new[] { 7, 11, 2 },
        };
        var opt = new KeyEstimator.Options { CollectTrace = true, Window = 1 };
        var (res, segs, keys) = ProgressionAnalyzer.AnalyzeWithKeyEstimateAndModulation(pcs, new Key(60, true), opt, out var trace, null, minLength: 5, minConfidence: 0.9);
        Assert.True(segs.Count >= 1); // fallback guarantees at least one
    }
}
