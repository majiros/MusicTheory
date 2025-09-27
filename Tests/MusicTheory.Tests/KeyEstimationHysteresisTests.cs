using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class KeyEstimationHysteresisTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Hysteresis_Can_Delay_Switch_When_Advantage_Is_Small()
    {
        // Sequence that momentarily hints G major but with small margin; expect to stay in C due to SwitchMargin
        var seq = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) }, // C
            new[]{ Pc(67), Pc(71), Pc(74) }, // G (I of G)
            new[]{ Pc(60), Pc(64), Pc(67) }, // back to C
        };
        var options = new KeyEstimator.Options { Window = 0, CollectTrace = true, SwitchMargin = 1, PrevKeyBias = 1 };
        var initialKey = new Key(60, true);
        var keys = KeyEstimator.EstimatePerChord(seq, initialKey, options, out var trace);
        Assert.Equal(seq.Length, keys.Count);
    // First is C; second should also remain C (either because score is higher or due to hysteresis)
    Assert.True(keys[0].IsMajor && Pc(keys[0].TonicMidi) == Pc(60));
    Assert.True(keys[1].IsMajor && Pc(keys[1].TonicMidi) == Pc(60));
    }

    [Fact]
    public void Hysteresis_Flag_Is_Set_On_Tie()
    {
        // Make scores tie at index 1 and verify we stay due to hysteresis (flag=true)
        var seq = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) }, // C
            new[]{ Pc(67), Pc(71), Pc(74) }, // G
        };
        var options = new KeyEstimator.Options
        {
            Window = 0,
            PrevKeyBias = 0,
            InitialKeyBias = 0,
            DominantTriadBonus = 0,
            DominantSeventhBonus = 0,
            CadenceBonus = 0,
            SwitchMargin = 1,
            PivotChordBonus = 0,
            SecondaryDominantTriadBonus = 0,
            SecondaryDominantSeventhBonus = 0,
            CollectTrace = true,
        };
        var initialKey = new Key(60, true);
        var keys = KeyEstimator.EstimatePerChord(seq, initialKey, options, out var trace);
        Assert.Equal(seq.Length, keys.Count);
        // index 1: stayed due to hysteresis, not due to prevKey tie-break only
        Assert.True(trace[1].StayedDueToHysteresis);
        Assert.True(keys[1].IsMajor && Pc(keys[1].TonicMidi) == Pc(60));
    }
}
