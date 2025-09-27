using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class KeyEstimatorVoiceLeadingTraceTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void Trace_Includes_VoiceLeading_Flags_When_Voicings_Provided()
    {
        // pcs: C major triad -> D major triad (arbitrary); focus is on voicing motion only
        var pcsList = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) }, // C
            new[]{ Pc(62), Pc(66), Pc(69) }, // D
        };

        // Create parallel 5ths between S and A: (C5,F4)->(D5,G4)
        FourPartVoicing v0 = new FourPartVoicing(S:72, A:65, T:60, B:48);
        FourPartVoicing v1 = new FourPartVoicing(S:74, A:67, T:60, B:48);
        var voicings = new FourPartVoicing?[]{ v0, v1 };

        var opt = new KeyEstimator.Options { Window = 0, CollectTrace = true };
        var initial = new Key(60, true);

        // Use the overload that annotates voice-leading into trace
        var keys = KeyEstimator.EstimatePerChord(pcsList, initial, opt, out var trace, voicings);

        Assert.Equal(2, keys.Count);
        Assert.Equal(2, trace.Count);
        // Index 1 should report parallel perfects; spacing/range/overlap should be false in this setup
        Assert.False(trace[0].VLParallelPerfects);
        Assert.True(trace[1].VLParallelPerfects);
        Assert.False(trace[1].VLRangeViolation);
        Assert.False(trace[1].VLSpacingViolation);
        Assert.False(trace[1].VLOverlap);
    }
}
