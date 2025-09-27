using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class KeyEstimatorTieBreakTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void TieBreak_Prefers_Major_When_Scores_Tie_With_Relative()
    {
        // Chord A minor triad (A C E) is diatonic in both C major and A minor
        var seq = new[]
        {
            new[]{ Pc(61), Pc(65), Pc(68) }, // dummy chord (C# major-ish), not relevant
            new[]{ Pc(57), Pc(60), Pc(64) }, // A C E
        };
        var options = new KeyEstimator.Options
        {
            Window = 0,
            PrevKeyBias = 0,
            InitialKeyBias = 0,
            DominantTriadBonus = 0,
            DominantSeventhBonus = 0,
            CadenceBonus = 0,
            SwitchMargin = 0,
            PivotChordBonus = 0,
            SecondaryDominantTriadBonus = 0,
            SecondaryDominantSeventhBonus = 0,
            CollectTrace = true,
        };
        // Choose an initial key that's NOT among top ties to avoid prev-key preference
        var initialKey = new Key(61, true); // C# major
        var keys = KeyEstimator.EstimatePerChord(seq, initialKey, options, out var trace);
        var chosen = keys[1];
        // Expect C major due to Major preference over A minor when tied
        Assert.True(chosen.IsMajor);
        Assert.Equal(Pc(60), Pc(chosen.TonicMidi));
        // sanity: trace reflects the chosen key
        Assert.Equal(Pc(60), Pc(trace[1].ChosenKey.TonicMidi));
        Assert.True(trace[1].ChosenKey.IsMajor);
    }

    [Fact]
    public void TieBreak_Prefers_Lower_TonicPc_Among_Majors()
    {
        // C major triad (C E G) is diatonic in both C major and G major → tie among majors
        var seq = new[]
        {
            new[]{ Pc(61), Pc(65), Pc(68) }, // dummy chord
            new[]{ Pc(60), Pc(64), Pc(67) }, // C E G
        };
        var options = new KeyEstimator.Options
        {
            Window = 0,
            PrevKeyBias = 0,
            InitialKeyBias = 0,
            DominantTriadBonus = 0,
            DominantSeventhBonus = 0,
            CadenceBonus = 0,
            SwitchMargin = 0,
            PivotChordBonus = 0,
            SecondaryDominantTriadBonus = 0,
            SecondaryDominantSeventhBonus = 0,
            CollectTrace = true,
        };
        var initialKey = new Key(61, true); // C# major (not tied)
        var keys = KeyEstimator.EstimatePerChord(seq, initialKey, options, out var trace);
        var chosen = keys[1];
        // Among tied majors (C and G), prefer lower tonic pc => C (0)
        Assert.True(chosen.IsMajor);
        Assert.Equal(Pc(60), Pc(chosen.TonicMidi));
        Assert.Equal(Pc(60), Pc(trace[1].ChosenKey.TonicMidi));
    }

    [Fact]
    public void TieBreak_Prefers_Major_When_Same_Tonic_Modes_Tie()
    {
        // G major triad (G B D) is diatonic in both C major and C harmonic minor → same-tonic tie exists
        var seq = new[]
        {
            new[]{ Pc(61), Pc(65), Pc(68) }, // dummy chord (avoid prev-key influence)
            new[]{ Pc(67), Pc(71), Pc(62) }, // G B D
        };
        var options = new KeyEstimator.Options
        {
            Window = 0,
            PrevKeyBias = 0,
            InitialKeyBias = 0,
            DominantTriadBonus = 0,
            DominantSeventhBonus = 0,
            CadenceBonus = 0,
            SwitchMargin = 0,
            PivotChordBonus = 0,
            SecondaryDominantTriadBonus = 0,
            SecondaryDominantSeventhBonus = 0,
            CollectTrace = true,
        };
        var initialKey = new Key(61, true); // C# major (not among ties)
        var keys = KeyEstimator.EstimatePerChord(seq, initialKey, options, out var trace);
        var chosen = keys[1];
        // Expect major selected for tonic C when C major and C minor tie
        Assert.True(chosen.IsMajor);
        Assert.Equal(Pc(60), Pc(chosen.TonicMidi));
        Assert.True(trace[1].ChosenKey.IsMajor);
        Assert.Equal(Pc(60), Pc(trace[1].ChosenKey.TonicMidi));
    }
}
