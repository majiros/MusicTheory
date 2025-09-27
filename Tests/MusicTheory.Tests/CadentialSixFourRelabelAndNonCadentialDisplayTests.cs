using System.Collections.Generic;
using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class CadentialSixFourRelabelAndNonCadentialDisplayTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void PreferCadentialSixFourAsDominant_Relabels_I64_To_V6453()
    {
        var key = new Key(60, true); // C major
        var pcs = new List<int[]>
        {
            new[] { Pc(60), Pc(64), Pc(67) }, // I (will be voiced as 64)
            new[] { Pc(67), Pc(71), Pc(62) }, // V
            new[] { Pc(60), Pc(64), Pc(67) }, // I
        };
        var voicings = new List<FourPartVoicing?>
        {
            new FourPartVoicing(S:79, A:72, T:76, B:67), // G in bass -> I64
            new FourPartVoicing(S:79, A:71, T:67, B:55), // V in root
            new FourPartVoicing(S:72, A:64, T:60, B:48), // I in root
        };
        var opt = new HarmonyOptions { PreferCadentialSixFourAsDominant = true };
        var (res, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(pcs, key, opt, voicings);

        Assert.True(cadences.Count >= 1);
        // The first chord (index 0) should be relabeled to V64-53
        Assert.Equal("V64-53", res.Chords[0].RomanText);
        Assert.Equal(RomanNumeral.V, res.Chords[0].Roman);
    }

    [Fact]
    public void NonCadential_SixFour_Suppressed_When_Option_False_And_Shown_When_True()
    {
        var key = new Key(60, true); // C major
        // IV -> IV64 -> IV6 pattern
        var pcs = new List<int[]>
        {
            new[] { Pc(65), Pc(69), Pc(60) }, // IV (F A C)
            new[] { Pc(65), Pc(69), Pc(60) }, // IV64 by voicing
            new[] { Pc(65), Pc(69), Pc(60) }, // IV6 by voicing
        };
        var voicings = new List<FourPartVoicing?>
        {
            new FourPartVoicing(S:77, A:69, T:65, B:53), // IV root
            new FourPartVoicing(S:77, A:69, T:65, B:60), // bass on C -> 64
            new FourPartVoicing(S:77, A:69, T:65, B:57), // bass on A -> 6
        };

        var optHide = new HarmonyOptions { ShowNonCadentialSixFour = false };
        var (_res1, cadsHidden) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(pcs, key, optHide, voicings);
        Assert.True(cadsHidden.Count == 0);

        var optShow = new HarmonyOptions { ShowNonCadentialSixFour = true };
        var (_res2, cadsShown) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(pcs, key, optShow, voicings);
        Assert.True(cadsShown.Count >= 1);
        Assert.Contains(cadsShown, ci => ci.SixFour == SixFourType.Passing && ci.Type == CadenceType.None);
    }
}
