using System.Linq;
using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class KeyEstimatorOutOfKeyPenaltyTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void Trace_Includes_OutOfKeyPenalty_When_Enabled()
    {
        var key = new Key(60, true); // C major
        // Chord with two non-diatonic pcs in C major: C E F# A (F#=6, A=9 → A is diatonic, F#のみ非ダイアトニック)
        // もう一つ外音として Db(1) を加えて 2 つの非ダイアトニックにする: C E F# Db
        var seq = new[] { new[] { Pc(60), Pc(64), Pc(66), Pc(61) } }; // {0,4,6,1}
        var opt = new KeyEstimator.Options { Window = 0, CollectTrace = true, OutOfKeyPenaltyPerPc = 1 };
        var keys = KeyEstimator.EstimatePerChord(seq, key, opt, out var trace);

        Assert.Single(keys);
        Assert.Single(trace);
        // C major diatonic set = {0,2,4,5,7,9,11}; non-diatonic in chord = {1,6} → 2個 → out=-2
        Assert.Equal(-2, trace[0].OutOfKeyPenalty);
        // Total should include this penalty
        Assert.Equal(trace[0].BaseWindowScore + trace[0].PrevKeyBias + trace[0].InitialKeyBias
            + trace[0].DominantTriad + trace[0].DominantSeventh + trace[0].Cadence + trace[0].Pivot
            + trace[0].SecondaryDominantTriad + trace[0].SecondaryDominantSeventh + trace[0].OutOfKeyPenalty,
            trace[0].Total);
    }
}
