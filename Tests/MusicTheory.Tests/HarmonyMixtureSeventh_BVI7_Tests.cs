using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class HarmonyMixtureSeventh_BVI7_Tests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Borrowed_bVI7_RootLabel_Without_Voicing()
    {
        var key = new Key(60, true); // C major
        // Ab C Eb Gb => Ab7 (bVI7)
        var pcs = new[] { Pc(68), Pc(72), Pc(75), Pc(66) }; // Ab C Eb Gb
    var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key);
        Assert.True(r.Success);
        Assert.Equal("bVI7", r.RomanText);
    }

    [Fact]
    public void Borrowed_bVI7_Inversions_With_Voicing()
    {
        var key = new Key(60, true); // C major
        var pcs = new[] { Pc(68), Pc(72), Pc(75), Pc(66) }; // Ab C Eb Gb

        // Root position (Ab bass): bVI7
        var v7 = new FourPartVoicing(92, 84, 75, 68);
    var opts = new HarmonyOptions { DisallowAugmentedSixthWhenSopranoFlat6 = true, PreferAugmentedSixthOverMixtureWhenBassFlat6 = true };
    var r7 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v7, null);
        Assert.True(r7.Success);
        Assert.Equal("bVI7", r7.RomanText);

        // 1st inversion (third in bass: C): bVI65
        var v65 = new FourPartVoicing(91, 84, 79, 72);
    var r65 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v65, null);
        Assert.True(r65.Success);
        Assert.Equal("bVI65", r65.RomanText);

        // 2nd inversion (fifth in bass: Eb): bVI43
        var v43 = new FourPartVoicing(91, 84, 79, 75);
    var r43 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v43, null);
        Assert.True(r43.Success);
        Assert.Equal("bVI43", r43.RomanText);

        // 3rd inversion (seventh in bass: Gb): bVI42
        var v42 = new FourPartVoicing(90, 81, 78, 66);
    var r42 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v42, null);
        Assert.True(r42.Success);
        Assert.Equal("bVI42", r42.RomanText);
    }
}
