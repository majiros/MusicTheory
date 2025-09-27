using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class HarmonyInversionHardeningTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void DiatonicSeventh_Inversions_LabelCorrectly()
    {
        var key = new Key(60, true); // C major
        // ii7: D F A C
        var pcs = new[] { Pc(62), Pc(65), Pc(69), Pc(72) };
        // Root position
        var v0 = new FourPartVoicing(86, 81, 74, 62);
        var r0 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v0, null);
        Assert.True(r0.Success);
        Assert.Equal("ii7", r0.RomanText);
        // 1st inversion (65): F in bass
        var v65 = new FourPartVoicing(86, 81, 74, 65);
        var r65 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v65, null);
        Assert.True(r65.Success);
        Assert.Equal("ii65", r65.RomanText);
        // 2nd inversion (43): A in bass
        var v43 = new FourPartVoicing(88, 81, 69, 69);
        var r43 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v43, null);
        Assert.True(r43.Success);
        Assert.Equal("ii43", r43.RomanText);
        // 3rd inversion (42): C in bass
        var v42 = new FourPartVoicing(88, 84, 77, 72);
        var r42 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v42, null);
        Assert.True(r42.Success);
        Assert.Equal("ii42", r42.RomanText);
    }

    [Fact]
    public void MixtureTriads_Inversions_LabelCorrectly()
    {
        var key = new Key(60, true); // C major
        // bVI: Ab C Eb
        var bVI = new[] { Pc(68), Pc(72), Pc(75) };
        // root position: Ab bass
        var v0 = new FourPartVoicing(87, 84, 80, 68);
        var r0 = HarmonyAnalyzer.AnalyzeTriad(bVI, key, v0, null);
        Assert.True(r0.Success);
        Assert.Equal("bVI", r0.RomanText);
        // 1st inversion (6): C bass
        var v6 = new FourPartVoicing(87, 84, 76, 72);
        var r6 = HarmonyAnalyzer.AnalyzeTriad(bVI, key, v6, null);
        Assert.True(r6.Success);
        Assert.Equal("bVI6", r6.RomanText);
        // 2nd inversion (64): Eb bass
        var v64 = new FourPartVoicing(87, 79, 75, 75);
        var r64 = HarmonyAnalyzer.AnalyzeTriad(bVI, key, v64, null);
        Assert.True(r64.Success);
        Assert.Equal("bVI64", r64.RomanText);

        // bII (Neapolitan triad baseline): Db F Ab
        var bII = new[] { Pc(61), Pc(65), Pc(68) };
        var v0_bII = new FourPartVoicing(85, 77, 73, 61);
        var r0_bII = HarmonyAnalyzer.AnalyzeTriad(bII, key, v0_bII, null);
        Assert.True(r0_bII.Success);
        Assert.StartsWith("bII", r0_bII.RomanText);
        // inversion checks
        var v6_bII = new FourPartVoicing(85, 77, 72, 65);
        var r6_bII = HarmonyAnalyzer.AnalyzeTriad(bII, key, v6_bII, null);
        Assert.True(r6_bII.Success);
        Assert.StartsWith("bII6", r6_bII.RomanText);
        var v64_bII = new FourPartVoicing(85, 77, 73, 68);
        var r64_bII = HarmonyAnalyzer.AnalyzeTriad(bII, key, v64_bII, null);
        Assert.True(r64_bII.Success);
        Assert.StartsWith("bII64", r64_bII.RomanText);
    }

    [Fact]
    public void SecondaryLeadingTone_And_Dominant_Seventh_Inversions()
    {
        var key = new Key(60, true); // C major
        // viiø7/ii: C# E G B (half-diminished)
        var vii_half = new[] { Pc(61), Pc(64), Pc(67), Pc(71) };
        var v0 = new FourPartVoicing(86, 79, 71, 61); // root position (C# bass) => viiø7/ii
        var r0 = HarmonyAnalyzer.AnalyzeTriad(vii_half, key, v0, null);
        Assert.True(r0.Success);
        Assert.Equal("viiø7/ii", r0.RomanText);
        var v65 = new FourPartVoicing(86, 79, 71, 64); // 3rd (E) in bass
        var r65 = HarmonyAnalyzer.AnalyzeTriad(vii_half, key, v65, null);
        Assert.True(r65.Success);
        Assert.Equal("viiø65/ii", r65.RomanText);
        var v43 = new FourPartVoicing(86, 79, 71, 67); // 5th (G) in bass
        var r43 = HarmonyAnalyzer.AnalyzeTriad(vii_half, key, v43, null);
        Assert.True(r43.Success);
        Assert.Equal("viiø43/ii", r43.RomanText);
        var v42 = new FourPartVoicing(86, 79, 71, 71); // 7th (B) in bass
        var r42 = HarmonyAnalyzer.AnalyzeTriad(vii_half, key, v42, null);
        Assert.True(r42.Success);
        Assert.Equal("viiø42/ii", r42.RomanText);

        // Secondary dominant seventh: V7/ii = A C# E G
        var V7ii = new[] { Pc(57), Pc(61), Pc(64), Pc(67) };
        var rv0 = HarmonyAnalyzer.AnalyzeTriad(V7ii, key, new FourPartVoicing(86, 79, 71, 57), null);
        Assert.True(rv0.Success);
        Assert.Equal("V7/ii", rv0.RomanText);
        var rv65 = HarmonyAnalyzer.AnalyzeTriad(V7ii, key, new FourPartVoicing(86, 79, 71, 61), null);
        Assert.True(rv65.Success);
        Assert.Equal("V65/ii", rv65.RomanText);
        var rv43 = HarmonyAnalyzer.AnalyzeTriad(V7ii, key, new FourPartVoicing(86, 79, 71, 64), null);
        Assert.True(rv43.Success);
        Assert.Equal("V43/ii", rv43.RomanText);
        var rv42 = HarmonyAnalyzer.AnalyzeTriad(V7ii, key, new FourPartVoicing(86, 79, 71, 67), null);
        Assert.True(rv42.Success);
        Assert.Equal("V42/ii", rv42.RomanText);
    }
}
