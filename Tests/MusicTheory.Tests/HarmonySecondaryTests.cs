using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class HarmonySecondaryTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void SecondaryDominant_Triad_And_Seventh_RootLabels()
    {
        var key = new Key(60, true); // C major
        // D major triad: D F# A => V/V
        var dMaj = new[]{ Pc(62), Pc(66), Pc(69) };
        var r1 = HarmonyAnalyzer.AnalyzeTriad(dMaj, key, null, null);
        Assert.True(r1.Success);
        Assert.Equal("V/V", r1.RomanText);

        // D7: D F# A C => V7/V
        var d7 = new[]{ Pc(62), Pc(66), Pc(69), Pc(60) }; // D F# A C
        var r2 = HarmonyAnalyzer.AnalyzeTriad(d7, key, null, null);
        Assert.True(r2.Success);
        Assert.Equal("V7/V", r2.RomanText);
    }

    [Fact]
    public void SecondaryDominant_Seventh_Inversions_With_Voicing()
    {
        var key = new Key(60, true); // C major
        var pcs = new[]{ Pc(62), Pc(66), Pc(69), Pc(60) }; // D F# A C (D7)

        // Root position: D in bass => V7/V
        var v0 = new FourPartVoicing(83, 79, 74, 62);
        var r0 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v0, null);
        Assert.True(r0.Success);
        Assert.Equal("V7/V", r0.RomanText);

        // 1st inversion: third (F#) in bass => V65/V
        var v65 = new FourPartVoicing(86, 81, 74, 66);
        var r65 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v65, null);
        Assert.True(r65.Success);
        Assert.Equal("V65/V", r65.RomanText);

        // 2nd inversion: fifth (A) in bass => V43/V
        var v43 = new FourPartVoicing(86, 81, 74, 69);
        var r43 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v43, null);
        Assert.True(r43.Success);
        Assert.Equal("V43/V", r43.RomanText);

        // 3rd inversion: seventh (C) in bass => V42/V
        var v42 = new FourPartVoicing(86, 81, 74, 60);
        var r42 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v42, null);
        Assert.True(r42.Success);
        Assert.Equal("V42/V", r42.RomanText);
    }

    [Fact]
    public void SecondaryLeadingTone_Triad_And_Seventh()
    {
        var key = new Key(60, true); // C major
        // vii°/ii triad: C# E G
        var ltTri = new[]{ Pc(61), Pc(64), Pc(67) };
        var r1 = HarmonyAnalyzer.AnalyzeTriad(ltTri, key, null, null);
        Assert.True(r1.Success);
        Assert.Equal("vii°/ii", r1.RomanText);

        // viiø7/ii: C# E G B
        var lt7_half = new[]{ Pc(61), Pc(64), Pc(67), Pc(71) };
        var r2 = HarmonyAnalyzer.AnalyzeTriad(lt7_half, key, null, null);
        Assert.True(r2.Success);
        Assert.Equal("viiø7/ii", r2.RomanText);

        // vii°7/V: F# A C Eb
        var lt7_full = new[]{ Pc(66), Pc(69), Pc(72), Pc(75) };
        var r3 = HarmonyAnalyzer.AnalyzeTriad(lt7_full, key, null, null);
        Assert.True(r3.Success);
        Assert.Equal("vii°7/V", r3.RomanText);
    }
}
