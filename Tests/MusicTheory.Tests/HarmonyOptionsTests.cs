using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class HarmonyOptionsTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Default_Disallows_Aug6_When_Soprano_b6_Prefers_bVI7()
    {
        var key = new Key(60, true); // C major
        // Ger65 pitch set: Ab C Eb Gb (68, 60, 63, 66)
        var pcs = new[] { Pc(68), Pc(60), Pc(63), Pc(66) };
        // Voicing with Ab in bass (b6) and ALSO Ab in soprano to trigger the disallow rule
        var v = new FourPartVoicing(S:80, A:72, T:68, B:68);
        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v, null);
        Assert.True(r.Success);
        // By default, soprano=b6 suppresses Aug6; mixture bVI7 should remain
        Assert.StartsWith("bVI", r.RomanText);
    }

    [Fact]
    public void Options_Allow_Aug6_When_Soprano_b6_Yields_Ger65()
    {
        var key = new Key(60, true); // C major
        var pcs = new[] { Pc(68), Pc(60), Pc(63), Pc(66) };
        var v = new FourPartVoicing(S:80, A:72, T:68, B:68);
        var opt = new HarmonyOptions
        {
            DisallowAugmentedSixthWhenSopranoFlat6 = false,
            PreferAugmentedSixthOverMixtureWhenBassFlat6 = true
        };
        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opt, v, null);
        Assert.True(r.Success);
        Assert.Equal("Ger65", r.RomanText);
    }
}
