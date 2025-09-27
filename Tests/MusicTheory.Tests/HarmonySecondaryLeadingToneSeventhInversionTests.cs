using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class HarmonySecondaryLeadingToneSeventhInversionTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void SecondaryLeadingToneSeventh_V_of_V_Inversions_LabelCorrectly()
    {
        var key = new Key(60, true); // C major
        // F# A C Eb
        var pcs = new[] { Pc(66), Pc(69), Pc(72), Pc(75) };

        // Root position (F# in bass): vii°7/V
        var v0 = new FourPartVoicing(87, 81, 72, 66);
        var r0 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v0, null);
        Assert.True(r0.Success);
        Assert.Equal("vii°7/V", r0.RomanText);

        // 1st inversion (A in bass): vii°65/V
        var v65 = new FourPartVoicing(89, 84, 76, 69);
        var r65 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v65, null);
        Assert.True(r65.Success);
        Assert.Equal("vii°65/V", r65.RomanText);

        // 2nd inversion (C in bass): vii°43/V
        var v43 = new FourPartVoicing(89, 84, 76, 60);
        var r43 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v43, null);
        Assert.True(r43.Success);
        Assert.Equal("vii°43/V", r43.RomanText);

        // 3rd inversion (Eb in bass): vii°42/V
        var v42 = new FourPartVoicing(89, 84, 77, 63);
        var r42 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v42, null);
        Assert.True(r42.Success);
        Assert.Equal("vii°42/V", r42.RomanText);
    }
}
