using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class KeyEstimateWithConfidenceAndOptionsTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void AnalyzeWithKeyEstimate_WithConfidence_And_HarmonyOptions_Works()
    {
        var key = new Key(60, true); // C major
        var seq = new[]
        {
            new[] { Pc(60), Pc(64), Pc(67) },   // C
            new[] { Pc(67), Pc(71), Pc(74) },   // G
            new[] { Pc(60), Pc(64), Pc(67) },   // C
            new[] { Pc(65), Pc(69), Pc(72) },   // F
            new[] { Pc(68), Pc(60), Pc(63), Pc(66) }, // Ab C Eb Gb (Ambiguous Ger65/bVI7)
        };
        var voicings = new FourPartVoicing?[]
        {
            null,
            null,
            null,
            null,
            new FourPartVoicing(75, 63, 60, 68) // S=Eb (b3), A=Eb, T=C, B=Ab(b6)
        };

        // Keep estimator anchored to initial key (C) to avoid switching to Db on Ab7
        var est = new KeyEstimator.Options
        {
            // Use minimal context window to reinforce staying in C across the phrase
            Window = 1,
            CollectTrace = true,
            // Strongly discourage key switching in this smoke test
            PrevKeyBias = 100,
            InitialKeyBias = 100,
            SwitchMargin = 1000,
            DominantSeventhBonus = 0,
            DominantTriadBonus = 0,
            CadenceBonus = 0,
            SecondaryDominantTriadBonus = 0,
            SecondaryDominantSeventhBonus = 0,
            PivotChordBonus = 0,
            // Stronger penalty to decisively suppress spurious Db reinterpretation on Ab7
            OutOfKeyPenaltyPerPc = 8,
            // Lock switching for all indices in this sequence
            MinSwitchIndex = 999,
        };
        var hopts = new HarmonyOptions { DisallowAugmentedSixthWhenSopranoFlat6 = false };

        var (res, segs, keys) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(
            seq, key, est, hopts, out var trace, voicings, withConfidence: true);

        Assert.Equal(seq.Length, res.Chords.Count);
        Assert.NotEmpty(segs);
        // confidence range check
        foreach (var s in segs)
            Assert.InRange(s.confidence, 0.0, 1.0);

    // Stay in the initial key (C major) and prefer Aug6 label per options
    Assert.True(keys[^1].IsMajor && ((keys[^1].TonicMidi % 12 + 12) % 12) == 0);
        Assert.Equal("Ger65", res.Chords[^1].RomanText);
        // Trace length equals chords
        Assert.Equal(seq.Length, trace.Count);
    }
}
