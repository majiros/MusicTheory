using System.Collections.Generic;
using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class SixFourCombinedOptionsTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void CombinedOptions_Suppresses_NonCadential_Passing_SixFour()
    {
        var key = new Key(60, true); // C major
        // IV -> IV64 -> IV6 (passing)
        var pcs = new List<int[]>
        {
            new[] { Pc(65), Pc(69), Pc(60) },
            new[] { Pc(65), Pc(69), Pc(60) },
            new[] { Pc(65), Pc(69), Pc(60) },
        };
        var voicings = new List<FourPartVoicing?>
        {
            new FourPartVoicing(S:77, A:69, T:65, B:53), // IV root (F bass)
            new FourPartVoicing(S:77, A:69, T:65, B:60), // IV64 (C bass)
            new FourPartVoicing(S:77, A:69, T:65, B:57), // IV6 (A bass)
        };

        var opt = new HarmonyOptions
        {
            ShowNonCadentialSixFour = false,
            PreferCadentialSixFourAsDominant = true,
        };

        var (_res, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(pcs, key, opt, voicings);

        // Non-cadential events should be suppressed entirely
        Assert.True(cadences.Count == 0);
    }

    [Fact]
    public void CombinedOptions_Relabels_Cadential_I64_As_Dominant_And_Retains_Cadence()
    {
        var key = new Key(60, true); // C major
        // I64 -> V -> I (cadential 6-4)
        var pcs = new List<int[]>
        {
            new[] { Pc(60), Pc(64), Pc(67) }, // I (voiced as 64)
            new[] { Pc(67), Pc(71), Pc(62) }, // V
            new[] { Pc(60), Pc(64), Pc(67) }, // I
        };
        var voicings = new List<FourPartVoicing?>
        {
            new FourPartVoicing(S:79, A:72, T:76, B:67), // I64 (G bass)
            new FourPartVoicing(S:79, A:71, T:67, B:55), // V root
            new FourPartVoicing(S:72, A:64, T:60, B:48), // I root
        };

        var opt = new HarmonyOptions
        {
            ShowNonCadentialSixFour = false,
            PreferCadentialSixFourAsDominant = true,
        };

        var (res, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(pcs, key, opt, voicings);

        // Cadential 6-4 should remain and be relabeled as dominant (V64-53)
        Assert.True(cadences.Count >= 1);
        Assert.Contains(cadences, c => c.Type == CadenceType.Authentic && (c.SixFour == SixFourType.Cadential || c.HasCadentialSixFour));
        Assert.Equal(RomanNumeral.V, res.Chords[0].Roman);
        Assert.Equal("V64-53", res.Chords[0].RomanText);
    }
}
