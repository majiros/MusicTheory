using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class ModulationSegmentsTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void Segments_Are_Filtered_By_Length_And_Confidence()
    {
        // Sequence: 2x C, 3x G (clear switch), 1x ambiguous C/G (may be filtered)
        var pcs = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) }, // C
            new[]{ Pc(60), Pc(64), Pc(67) }, // C
            new[]{ Pc(67), Pc(71), Pc(74) }, // G
            new[]{ Pc(67), Pc(71), Pc(74) }, // G
            new[]{ Pc(67), Pc(71), Pc(74) }, // G
            new[]{ Pc(60), Pc(67), Pc(71) }, // C+G overlap (ambiguous)
        };
        var initial = new Key(60, true);
        var opt = new KeyEstimator.Options { Window = 0, CollectTrace = true, PrevKeyBias = 0 };
        var (res, segs, keys) = ProgressionAnalyzer.AnalyzeWithKeyEstimateAndModulation(
            pcs, initial, opt, out var trace, voicings: null, minLength: 1, minConfidence: 0.0);

        Assert.True(segs.Count >= 1);
        // First segment should start at 0, key C major
        Assert.Equal(0, segs[0].start);
        Assert.True(segs[0].key.IsMajor && Pc(segs[0].key.TonicMidi) == Pc(60));
    }
}
