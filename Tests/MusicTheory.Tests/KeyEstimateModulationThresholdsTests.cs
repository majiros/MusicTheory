using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class KeyEstimateModulationThresholdsTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void When_AllSegmentsFiltered_FallbackFirstSegmentReturned()
    {
        // Build a short sequence with an obvious initial key, then a brief switch
        // Use a very large minLength to ensure all segments are filtered out,
        // so the fallback logic returns the first original segment.
        var pcs = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) }, // C
            new[]{ Pc(60), Pc(64), Pc(67) }, // C
            new[]{ Pc(67), Pc(71), Pc(74) }, // G
            new[]{ Pc(60), Pc(64), Pc(67) }, // C
        };
        var initial = new Key(60, true);
        var opt = new KeyEstimator.Options { Window = 1, CollectTrace = true };

        var (res, segs, keys) = ProgressionAnalyzer.AnalyzeWithKeyEstimateAndModulation(
            pcs, initial, opt, out var trace, voicings: null, minLength: 100, minConfidence: 0.0);

        Assert.NotNull(segs);
        Assert.True(segs.Count >= 1);
        // Fallback should return the first original segment, which starts at 0 and is in the initial key
        Assert.Equal(0, segs[0].start);
        Assert.Equal(Pc(60), Pc(segs[0].key.TonicMidi));
        Assert.True(segs[0].key.IsMajor);
    }
}
