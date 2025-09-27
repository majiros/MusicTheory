using System;
using MusicTheory.Tests;

namespace MusicTheory.Tests;

public class HarmonySecondaryTriadInversionTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void Secondary_Dominant_Triad_Inversions_V_over_ii()
    {
    var key = new Key(60, true); // C major
        var v_over_ii = new[] { Pc(9), Pc(1), Pc(4) }; // A C# E

        // Root position (A as bass) -> V/ii
        var vRoot = new FourPartVoicing(81, 69, 57, 69); // Bass=A(69)
        var rRoot = HarmonyAnalyzer.AnalyzeTriad(v_over_ii, key, vRoot, null);
        Assert.Equal("V/ii", rRoot.RomanText);

        // 1st inversion (C# as bass) -> V6/ii
        var v6 = new FourPartVoicing(85, 73, 69, 61); // Bass=C#(61)
        var r6 = HarmonyAnalyzer.AnalyzeTriad(v_over_ii, key, v6, null);
        Assert.Equal("V6/ii", r6.RomanText);

        // 2nd inversion (E as bass) -> V64/ii
        var v64 = new FourPartVoicing(88, 76, 72, 64); // Bass=E(64)
        var r64 = HarmonyAnalyzer.AnalyzeTriad(v_over_ii, key, v64, null);
        Assert.Equal("V64/ii", r64.RomanText);
    }

    [Fact]
    public void Secondary_LeadingTone_Triad_Inversions_viiDim_over_V()
    {
    var key = new Key(60, true); // C major
        var viio_over_V = new[] { Pc(6), Pc(9), Pc(0) }; // F# A C

        // Root position (F# as bass) -> vii°/V
        var vRoot = new FourPartVoicing(78, 74, 66, 66); // Bass=F#(66)
        var rRoot = HarmonyAnalyzer.AnalyzeTriad(viio_over_V, key, vRoot, null);
        Assert.Equal("vii°/V", rRoot.RomanText);

        // 1st inversion (A as bass) -> vii°6/V
        var v6 = new FourPartVoicing(81, 76, 69, 69); // Bass=A(69)
        var r6 = HarmonyAnalyzer.AnalyzeTriad(viio_over_V, key, v6, null);
        Assert.Equal("vii°6/V", r6.RomanText);

        // 2nd inversion (C as bass) -> vii°64/V
        var v64 = new FourPartVoicing(84, 72, 72, 60); // Bass=C(60)
        var r64 = HarmonyAnalyzer.AnalyzeTriad(viio_over_V, key, v64, null);
        Assert.Equal("vii°64/V", r64.RomanText);
    }
}

public class HarmonySecondaryTriadInversionAdditionalTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void Secondary_Dominant_Triad_Inversions_V_over_vi()
    {
        var key = new Key(60, true); // C major
        var v_over_vi = RomanInputParser.Parse("V/vi", key)[0].Pcs; // E G# B (V/vi)

        // Root (E)
    var vRoot = new FourPartVoicing(88, 83, 80, 64); // Bass=E(64)
        var rRoot = HarmonyAnalyzer.AnalyzeTriad(v_over_vi, key, vRoot, null);
        Assert.Equal("V/vi", rRoot.RomanText);

        // 1st inv (G#)
    var v6 = new FourPartVoicing(92, 83, 76, 68); // Bass=G#(68)
        var r6 = HarmonyAnalyzer.AnalyzeTriad(v_over_vi, key, v6, null);
        Assert.Equal("V6/vi", r6.RomanText);

        // 2nd inv (B)
    var v64 = new FourPartVoicing(88, 80, 76, 71); // Bass=B(71)
        var r64 = HarmonyAnalyzer.AnalyzeTriad(v_over_vi, key, v64, null);
        Assert.Equal("V64/vi", r64.RomanText);
    }

    [Fact]
    public void Secondary_LeadingTone_Triad_Inversions_viiDim_over_ii()
    {
        var key = new Key(60, true); // C major
        var viio_over_ii = new[] { Pc(1), Pc(4), Pc(7) }; // C# E G (vii°/ii)

        // Root (C#)
        var vRoot = new FourPartVoicing(85, 73, 61, 61); // Bass=C#(61)
        var rRoot = HarmonyAnalyzer.AnalyzeTriad(viio_over_ii, key, vRoot, null);
        Assert.Equal("vii°/ii", rRoot.RomanText);

        // 1st inv (E)
        var v6 = new FourPartVoicing(88, 76, 72, 64); // Bass=E(64)
        var r6 = HarmonyAnalyzer.AnalyzeTriad(viio_over_ii, key, v6, null);
        Assert.Equal("vii°6/ii", r6.RomanText);

        // 2nd inv (G)
        var v64 = new FourPartVoicing(91, 79, 75, 67); // Bass=G(67)
        var r64 = HarmonyAnalyzer.AnalyzeTriad(viio_over_ii, key, v64, null);
        Assert.Equal("vii°64/ii", r64.RomanText);
    }

    [Fact]
    public void Secondary_Dominant_Triad_Inversions_V_over_vii()
    {
        var key = new Key(60, true); // C major
        // V/vii = F# A# C# （vii=Bm の属: F#）
        var v_over_vii = new[] { Pc(6), Pc(10), Pc(1) }; // F# A# C#

        // Root (F#)
        var vRoot = new FourPartVoicing(78, 66, 54, 66); // Bass=F#(66)
        var rRoot = HarmonyAnalyzer.AnalyzeTriad(v_over_vii, key, vRoot, null);
        Assert.Equal("V/vii", rRoot.RomanText);

        // 1st inv (A#)
        var v6 = new FourPartVoicing(82, 70, 58, 70); // Bass=A#(70)
        var r6 = HarmonyAnalyzer.AnalyzeTriad(v_over_vii, key, v6, null);
        Assert.Equal("V6/vii", r6.RomanText);

        // 2nd inv (C#)
        var v64 = new FourPartVoicing(85, 73, 61, 61); // Bass=C#(61)
        var r64 = HarmonyAnalyzer.AnalyzeTriad(v_over_vii, key, v64, null);
        Assert.Equal("V64/vii", r64.RomanText);
    }

    [Fact]
    public void Not_Label_I_As_V_over_IV()
    {
        var key = new Key(60, true); // C major
        var I = new[] { Pc(0), Pc(4), Pc(7) }; // C E G

        // 任意のベース配置でも I として優先され、V/IV と誤認しない
        var vRoot = new FourPartVoicing(84, 76, 72, 60); // Bass=C
        var rRoot = HarmonyAnalyzer.AnalyzeTriad(I, key, vRoot, null);
        Assert.Equal("I", rRoot.RomanText);

        var v6 = new FourPartVoicing(81, 76, 72, 64); // Bass=E
        var r6 = HarmonyAnalyzer.AnalyzeTriad(I, key, v6, null);
        Assert.Equal("I6", r6.RomanText);

        var v64 = new FourPartVoicing(81, 76, 72, 67); // Bass=G
        var r64 = HarmonyAnalyzer.AnalyzeTriad(I, key, v64, null);
        Assert.Equal("I64", r64.RomanText);
    }
}
