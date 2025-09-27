using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class KeyEstimatorMinSwitchIndexTests
{
    [Fact]
    public void FullLock_When_MinSwitchIndex_GreaterOrEqual_Length()
    {
        int Pc(int m) => ((m % 12) + 12) % 12;
        var seq = new[]
        {
            new[] { Pc(60), Pc(64), Pc(67) },    // C
            new[] { Pc(67), Pc(71), Pc(74) },    // G
            new[] { Pc(65), Pc(69), Pc(72) },    // F
        };

    var initial = new Key(60, true); // C major
        var opt = new KeyEstimator.Options
        {
            Window = 1,
            PrevKeyBias = 0,
            InitialKeyBias = 0,
            SwitchMargin = 0,
            OutOfKeyPenaltyPerPc = 0,
            MinSwitchIndex = 999, // >= length -> full lock
            CollectTrace = true,
        };

        var keys = KeyEstimator.EstimatePerChord(seq, initial, opt, out var trace);

        Assert.Equal(seq.Length, keys.Count);
        Assert.All(keys, k => Assert.True(k.IsMajor && ((k.TonicMidi % 12 + 12) % 12) == 0)); // all C major

        Assert.Equal(seq.Length, trace.Count);
        // index 0 は stayed=false, それ以降は stayed=true（ロック継続）
        Assert.False(trace[0].StayedDueToHysteresis);
        for (int i = 1; i < trace.Count; i++)
            Assert.True(trace[i].StayedDueToHysteresis);
    }
}
