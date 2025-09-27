using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class KeyEstimateConfidenceTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void AnalyzeWithKeyEstimate_WithConfidence_Yields_Confidence_In_Range()
    {
        var key = new Key(60, true); // C major
        // Simple cadence-heavy sequence to ensure non-zero segments
        var I = new[] { Pc(60), Pc(64), Pc(67) };
        var V = new[] { Pc(67), Pc(71), Pc(74) };
        var seq = new[] { I, V, I, I, V, I };

        var opts = new KeyEstimator.Options { Window = 1, CollectTrace = true };
        var (res, segments, keys) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, key, opts, out var trace, voicings: null, withConfidence: true);

        Assert.NotEmpty(segments);
        foreach (var s in segments)
        {
            Assert.InRange(s.confidence, 0.0, 1.0);
        }
    }
}
