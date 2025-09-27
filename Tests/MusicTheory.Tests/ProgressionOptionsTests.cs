using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class ProgressionOptionsTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Analyze_With_HarmonyOptions_Overload_Runs_And_Returns_Same_Count()
    {
    var key = new Key(60, true); // C major
        var seq = new[]
        {
            new[] { Pc(60), Pc(64), Pc(67) }, // C
            new[] { Pc(67), Pc(71), Pc(74) }, // G
            new[] { Pc(60), Pc(64), Pc(67) }, // C
        };
        var opts = new HarmonyOptions();

        var res = ProgressionAnalyzer.Analyze(seq, key, opts, voicings: null);
        Assert.Equal(seq.Length, res.Chords.Count);
        Assert.NotNull(res.Chords[1].RomanText); // middle chord should be labeled (e.g., V)
    }

    [Fact]
    public void AnalyzeWithKeyEstimate_Combined_Overload_Respects_Aug6_Option_Toggle()
    {
    var key = new Key(60, true); // C major
        // Ambiguous set: {b6, 1, b3, #4} = {Ab, C, Eb, F#} => Ger65 or bVI7
        var pcs = new[] { Pc(68), Pc(60), Pc(63), Pc(66) }; // -> {8,0,3,6}
        var seq = new[] { pcs };
        // Voicing with bass=b6 and soprano=b6 to trigger the soprano=b6 suppression by default
        var v = new FourPartVoicing(
            S: 68, // Ab (b6)
            A: 63, // Eb (b3)
            T: 60, // C (1)
            B: 44  // Ab (b6)
        );
        var voicings = new FourPartVoicing?[] { v };

        var keyOpts = new KeyEstimator.Options { Window = 0 };

        // Default options: DisallowAugmentedSixthWhenSopranoFlat6 = true => prefer bVI7 label in this voicing
        var (r1, segs1, keys1) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, key, keyOpts, HarmonyOptions.Default, out var trace1, voicings);
        Assert.Single(r1.Chords);
        Assert.Equal("bVI7", r1.Chords[0].RomanText);

        // Turn off soprano=b6 suppression -> prefer Ger65 under same voicing
        var opts = new HarmonyOptions { DisallowAugmentedSixthWhenSopranoFlat6 = false };
        var (r2, segs2, keys2) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, key, keyOpts, opts, out var trace2, voicings);
        Assert.Single(r2.Chords);
        Assert.Equal("Ger65", r2.Chords[0].RomanText);
    }
}
