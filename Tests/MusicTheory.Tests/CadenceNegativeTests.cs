using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class CadenceNegativeTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void Ger65_To_I_Is_Not_Authentic()
    {
        var key = new Key(60, true); // C major
        // Ger65 in C: A C Eb F# (enharmonic), here use pitch classes: A(69)->9, C(60)->0, Eb(63)->3, F#(66)->6
        var Ger65 = new[] { Pc(69), Pc(60), Pc(63), Pc(66) };
        var I = new[] { Pc(60), Pc(64), Pc(67) };
        var seq = new[] { Ger65, I };
        var (_, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key);
        Assert.DoesNotContain(cads, c => c.Type == CadenceType.Authentic);
    }

    [Fact]
    public void V_To_vi_Is_Deceptive_Not_PAC()
    {
        var key = new Key(60, true); // C major
        var V = new[] { Pc(67), Pc(71), Pc(74) }; // G B D
        var vi = new[] { Pc(69), Pc(72), Pc(76) }; // A C E
        var seq = new[] { V, vi };
        var (_, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key);
        var cad = Assert.Single(cads);
        Assert.Equal(CadenceType.Deceptive, cad.Type);
        Assert.False(cad.IsPerfectAuthentic);
    }

    [Fact]
    public void V9_To_I6_Is_Authentic_But_Not_PAC()
    {
        var key = new Key(60, true);
        // V9 (omit5) example: G B F A (67,71,65,69) plus D optional; we'll include 9 (A) and 7 (F)
        var V9 = new[] { Pc(67), Pc(71), Pc(65), Pc(69) };
        var I = new[] { Pc(60), Pc(64), Pc(67) };
        var seq = new[] { V9, I };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(S:81, A:77, T:72, B:67), // treat as root position dominant extended
            new FourPartVoicing(S:84, A:76, T:72, B:64)  // I6 (bass E)
        };
        var (_, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        var cad = Assert.Single(cads);
        Assert.Equal(CadenceType.Authentic, cad.Type);
        Assert.False(cad.IsPerfectAuthentic); // inversion on tonic prevents PAC
    }

    [Fact]
    public void V7Paren9_To_I6_Is_Authentic_But_Not_PAC()
    {
        var key = new Key(60, true);
        // Simulate V7(9) text path via options flag in actual CLI; here just supply pcs akin to V9
        var V7n9 = new[] { Pc(67), Pc(71), Pc(65), Pc(69) }; // same set as above
        var I = new[] { Pc(60), Pc(64), Pc(67) };
        var seq = new[] { V7n9, I };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(S:81, A:77, T:72, B:67),
            new FourPartVoicing(S:84, A:76, T:72, B:64)  // I6
        };
        var (_, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        var cad = Assert.Single(cads);
        Assert.Equal(CadenceType.Authentic, cad.Type);
        Assert.False(cad.IsPerfectAuthentic);
    }

    [Fact]
    public void Ger65_To_V_To_I_Yields_Single_Authentic_On_VI()
    {
        var key = new Key(60, true);
        var Ger65 = new[] { Pc(69), Pc(60), Pc(63), Pc(66) }; // Ger65
        var V = new[] { Pc(67), Pc(71), Pc(74) }; // V
        var I = new[] { Pc(60), Pc(64), Pc(67) }; // I
        var seq = new[] { Ger65, V, I };
        var (_, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key);
        // Expect only the final V->I authentic cadence, not Ger65->V authentic
        Assert.Single(cads);
        Assert.Equal(CadenceType.Authentic, cads[0].Type);
    }
}
