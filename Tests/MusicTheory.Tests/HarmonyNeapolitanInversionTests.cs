using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class HarmonyNeapolitanInversionTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Neapolitan_bII6_Is_Labeled_With_Inversion_When_Bass_Is_Third()
    {
        var key = new Key(60, true); // C major
        // Neapolitan triad: Db F Ab => PCs {1,5,8}
        var bII = new[] { Pc(61), Pc(65), Pc(68) };
        // Make bass = F (third of Db major triad) to expect bII6
        var v = new FourPartVoicing(77, 70, 65, 53); // S=A, A=F, T=F, B=F

        var res = HarmonyAnalyzer.AnalyzeTriad(bII, key, v);
        Assert.True(res.Success);
        Assert.Equal("bII6", res.RomanText);
    }

    [Fact]
    public void Neapolitan_bII64_Is_Labeled_With_Inversion_When_Bass_Is_Fifth()
    {
        var key = new Key(60, true); // C major
        var bII = new[] { Pc(61), Pc(65), Pc(68) }; // Db F Ab
        // Bass = Ab (fifth) -> expect bII64
        var v = new FourPartVoicing(77, 73, 80, 56); // S=F, A=Db, T=Ab, B=Ab

        var res = HarmonyAnalyzer.AnalyzeTriad(bII, key, v);
        Assert.True(res.Success);
        Assert.Equal("bII64", res.RomanText);
    }

    [Fact]
    public void Mixture_bVI6_Is_Labeled_With_Inversion_When_Bass_Is_Third()
    {
        var key = new Key(60, true); // C major
        var bVI = new[] { Pc(68), Pc(60), Pc(63) }; // Ab C Eb
        // Bass = C (third) -> expect bVI6
        var v = new FourPartVoicing(80, 68, 75, 48); // S=Ab, A=Ab, T=Eb, B=C

        var res = HarmonyAnalyzer.AnalyzeTriad(bVI, key, v);
        Assert.True(res.Success);
        Assert.Equal("bVI6", res.RomanText);
    }

    [Fact]
    public void Mixture_bVII64_Is_Labeled_With_Inversion_When_Bass_Is_Fifth()
    {
        var key = new Key(60, true); // C major
        var bVII = new[] { Pc(70), Pc(62), Pc(65) }; // Bb D F
        // Bass = F (fifth) -> expect bVII64
        var v = new FourPartVoicing(70, 62, 65, 53); // S=Bb, A=D, T=F, B=F

        var res = HarmonyAnalyzer.AnalyzeTriad(bVII, key, v);
        Assert.True(res.Success);
        Assert.Equal("bVII64", res.RomanText);
    }

    [Fact]
    public void Mixture_bIII64_Is_Labeled_With_Inversion_When_Bass_Is_Fifth()
    {
        var key = new Key(60, true); // C major
        var bIII = new[] { Pc(63), Pc(67), Pc(70) }; // Eb G Bb
        // Bass = Bb (fifth) -> expect bIII64
        var v = new FourPartVoicing(82, 75, 70, 58); // S=Eb, A=G, T=Bb, B=Bb

        var res = HarmonyAnalyzer.AnalyzeTriad(bIII, key, v);
        Assert.True(res.Success);
        Assert.Equal("bIII64", res.RomanText);
    }

    [Fact]
    public void Mixture_bVII6_Is_Labeled_With_Inversion_When_Bass_Is_Third()
    {
        var key = new Key(60, true); // C major
        var bVII = new[] { Pc(70), Pc(62), Pc(65) }; // Bb D F
        // Bass = D (third) -> expect bVII6
        var v = new FourPartVoicing(77, 70, 62, 50); // S=F, A=Bb, T=D, B=D

        var res = HarmonyAnalyzer.AnalyzeTriad(bVII, key, v);
        Assert.True(res.Success);
        Assert.Equal("bVII6", res.RomanText);
    }
}
