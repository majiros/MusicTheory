using System.Linq;
using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class HarmonyMajorSeventhInversionNotationTests
{
    static int Pc(int midi) => (midi % 12 + 12) % 12;

    [Fact]
    public void IVmaj7_Inversions_Default_NoMajToken()
    {
        var key = new Key(60, true); // C major
        // F A C E
        var pcs = new[] { Pc(65), Pc(69), Pc(72), Pc(76) };

        // 1st inversion (A in bass): IV65
        var v65 = new FourPartVoicing(86, 81, 76, 69);
        var r65 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v65, null);
        Assert.True(r65.Success);
        Assert.Equal("IV65", r65.RomanText);

        // 2nd inversion (C in bass): IV43
        var v43 = new FourPartVoicing(88, 84, 77, 72);
        var r43 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v43, null);
        Assert.True(r43.Success);
        Assert.Equal("IV43", r43.RomanText);

        // 3rd inversion (E in bass): IV42
        var v42 = new FourPartVoicing(88, 84, 81, 76);
        var r42 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v42, null);
        Assert.True(r42.Success);
        Assert.Equal("IV42", r42.RomanText);
    }

    [Fact]
    public void IVmaj7_Inversions_WithOption_IncludeMajToken()
    {
        var key = new Key(60, true); // C major
        var pcs = new[] { Pc(65), Pc(69), Pc(72), Pc(76) }; // F A C E
        var opts = new HarmonyOptions { IncludeMajInSeventhInversions = true };

        // 1st inversion (A in bass): IVmaj65
        var v65 = new FourPartVoicing(86, 81, 76, 69);
        var r65 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v65, null);
        Assert.True(r65.Success);
        Assert.Equal("IVmaj65", r65.RomanText);

        // 2nd inversion (C in bass): IVmaj43
        var v43 = new FourPartVoicing(88, 84, 77, 72);
        var r43 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v43, null);
        Assert.True(r43.Success);
        Assert.Equal("IVmaj43", r43.RomanText);

        // 3rd inversion (E in bass): IVmaj42
        var v42 = new FourPartVoicing(88, 84, 81, 76);
        var r42 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v42, null);
        Assert.True(r42.Success);
        Assert.Equal("IVmaj42", r42.RomanText);
    }

    [Fact]
    public void Minor_IIImaj7_FirstInversion_WithOption()
    {
        var key = new Key(60, false); // C minor (harmonic minor baseline)
        // IIImaj7 in C minor → Eb G Bb D  (63, 67, 70, 74 PCs)
        var pcs = new[] { Pc(63), Pc(67), Pc(70), Pc(74) };
        var opts = new HarmonyOptions { IncludeMajInSeventhInversions = true };

        // First inversion: G in bass (67)
        var v65 = new FourPartVoicing(91, 86, 79, 67);
        var r65 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v65, null);
        Assert.True(r65.Success);
        Assert.Equal("IIImaj65", r65.RomanText);
    }
}
