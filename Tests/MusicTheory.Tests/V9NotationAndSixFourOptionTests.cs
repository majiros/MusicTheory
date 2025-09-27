using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class V9NotationAndSixFourOptionTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void DominantNinth_Label_Toggles_Between_V9_And_V7Paren9()
    {
        var key = new Key(60, true); // C major
        // G9: G B D F A -> {7, 11, 2, 5, 9} relative to C major
        var v9 = new[] { Pc(67), Pc(71), Pc(74), Pc(65), Pc(69) };

        var def = HarmonyAnalyzer.AnalyzeTriad(v9, key);
        Assert.True(def.Success);
        Assert.Equal("V9", def.RomanText);

        var opt = new HarmonyOptions { PreferV7Paren9OverV9 = true };
        var paren = HarmonyAnalyzer.AnalyzeTriad(v9, key, opt);
        Assert.True(paren.Success);
        Assert.Equal("V7(9)", paren.RomanText);
    }

    [Fact]
    public void ShowNonCadentialSixFour_False_Suppresses_Passing_Pedal_Items()
    {
        var key = new Key(60, true); // C major
        // IV -> IV64 -> IV6 (Passing 6-4)
        var IV = new[]{ Pc(65), Pc(69), Pc(72) };
        var seq = new[] { IV, IV, IV };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 77, 72, 65), // IV
            new FourPartVoicing(81, 77, 72, 72), // IV64
            new FourPartVoicing(81, 77, 72, 69), // IV6
        };

        var hide = new HarmonyOptions { ShowNonCadentialSixFour = false };
        var (_, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, hide, voicings);
        Assert.DoesNotContain(cadences, c => c.Type == CadenceType.None && (c.SixFour == SixFourType.Passing || c.SixFour == SixFourType.Pedal));
    }
}
