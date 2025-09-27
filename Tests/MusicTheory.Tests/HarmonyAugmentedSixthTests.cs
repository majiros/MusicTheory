using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class HarmonyAugmentedSixthTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Italian_Sixth_Label()
    {
        var key = new Key(60, true); // C major
        // It6: Ab C F#; require Ab in bass
        var pcs = new[]{ Pc(68), Pc(60), Pc(66) };
        var v = new FourPartVoicing(84, 76, 72, 68);
    var opts = new HarmonyOptions { DisallowAugmentedSixthWhenSopranoFlat6 = false, PreferAugmentedSixthOverMixtureWhenBassFlat6 = true };
    var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v, null);
        Assert.True(r.Success);
        Assert.Equal("It6", r.RomanText);
        Assert.Null(r.Roman);
    }

    [Fact]
    public void French_Sixth_Label()
    {
        var key = new Key(60, true); // C major
        // Fr43: Ab C D F#; require Ab in bass
        var pcs = new[]{ Pc(68), Pc(60), Pc(62), Pc(66) };
        var v = new FourPartVoicing(86, 81, 74, 68);
    var opts = new HarmonyOptions { DisallowAugmentedSixthWhenSopranoFlat6 = false, PreferAugmentedSixthOverMixtureWhenBassFlat6 = true };
    var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v, null);
        Assert.True(r.Success);
        Assert.Equal("Fr43", r.RomanText);
    }

    [Fact]
    public void German_Sixth_Label_Prefers_Over_bVI7_When_Bass_b6()
    {
        var key = new Key(60, true); // C major
        // Ger65: Ab C Eb Gb; require Ab in bass
        var pcs = new[]{ Pc(68), Pc(60), Pc(63), Pc(66) };
        var v = new FourPartVoicing(86, 81, 75, 68);
    var opts = new HarmonyOptions { DisallowAugmentedSixthWhenSopranoFlat6 = false, PreferAugmentedSixthOverMixtureWhenBassFlat6 = true };
    var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v, null);
        Assert.True(r.Success);
        Assert.Equal("Ger65", r.RomanText);
    }
}
