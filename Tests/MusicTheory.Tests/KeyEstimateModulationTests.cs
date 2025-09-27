using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class KeyEstimateModulationTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void AnalyzeWithKeyEstimateAndModulation_Returns_AtLeastOneSegment()
    {
        var key = new Key(60, true); // C major
        var I = new[] { Pc(60), Pc(64), Pc(67) };
        var V = new[] { Pc(67), Pc(71), Pc(74) };
        var G_I = new[] { Pc(67), Pc(71), Pc(74) }; // G major I (same as V in C)
        var seq = new[] { I, V, I, G_I, G_I, I };

        var opts = new KeyEstimator.Options { Window = 1, CollectTrace = true };
        var (res, segments, keys) = ProgressionAnalyzer.AnalyzeWithKeyEstimateAndModulation(seq, key, opts, out var trace, voicings: null, minLength: 1, minConfidence: 0.0);

        Assert.NotEmpty(segments);
        foreach (var s in segments)
        {
            Assert.InRange(s.confidence, 0.0, 1.0);
        }
    }
}
