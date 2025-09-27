using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class HarmonyAug6MixturePreferenceTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void DefaultPrefersAug6_WhenBassFlat6_AndNoSopranoFlat6()
    {
    var key = new Key(60, true); // C major
        // Ab C Eb Gb  (Ger65 set)
        var pcs = new[] { Pc(68), Pc(60), Pc(63), Pc(66) };
        // Bass = Ab(b6), soprano ≠ b6 （E♭）
        var v = new FourPartVoicing(75, 72, 63, 68);

        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, HarmonyOptions.Default, v);
        Assert.True(r.Success);
        Assert.Equal("Ger65", r.RomanText);
    }

    [Fact]
    public void OptionPrefersMixture7_WhenAmbiguous_UnderSameVoicing()
    {
    var key = new Key(60, true); // C major
        // Ab C Eb Gb  (Ger65 set)
        var pcs = new[] { Pc(68), Pc(60), Pc(63), Pc(66) };
        // Bass = Ab(b6), soprano ≠ b6 （E♭）
        var v = new FourPartVoicing(75, 72, 63, 68);

        var opts = new HarmonyOptions { PreferMixtureSeventhOverAugmentedSixthWhenAmbiguous = true };
        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v);
        Assert.True(r.Success);
        Assert.StartsWith("bVI", r.RomanText);
    }
}
