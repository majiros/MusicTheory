using System.Collections.Generic;
using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class KeyEstimatorMinSwitchLockTests
{
    [Fact]
    public void Early_Lock_MinSwitchIndex_Emits_Trivial_Trace_And_Stays()
    {
        var pcsList = new List<int[]>
        {
            new[] { 0, 4, 7 },
            new[] { 7, 11, 2 },
            new[] { 0, 4, 7 },
        };
        var initial = new Key(60, true); // C major
        var opt = new KeyEstimator.Options
        {
            Window = 1,
            CollectTrace = true,
            MinSwitchIndex = 10 // larger than sequence length triggers early full-lock
        };

        var keys = KeyEstimator.EstimatePerChord(pcsList, initial, opt, out var trace);

        Assert.Equal(3, keys.Count);
        Assert.All(keys, k => Assert.True(k.IsMajor && ((k.TonicMidi % 12 + 12) % 12) == 0));

        Assert.Equal(3, trace.Count);
        Assert.False(trace[0].StayedDueToHysteresis);
        Assert.True(trace[1].StayedDueToHysteresis);
        Assert.True(trace[2].StayedDueToHysteresis);

        Assert.Equal(0, trace[0].MaxScore);
        Assert.Equal(0, trace[0].SecondBestScore);
    }
}
