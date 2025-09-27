using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class HarmonyMixtureSeventhInversionTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Borrowed_iv7_Inversions_With_Voicing()
    {
        var key = new Key(60, true); // C major
        // F Ab C Eb (iv7)
        var pcs = new[] { Pc(65), Pc(68), Pc(72), Pc(75) };

        // 1st inversion (third in bass: Ab) => iv65
        var v65 = new FourPartVoicing(88, 81, 76, 68);
        var r65 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v65, null);
        Assert.True(r65.Success);
        Assert.Equal("iv65", r65.RomanText);

        // 2nd inversion (fifth in bass: C) => iv43
        var v43 = new FourPartVoicing(88, 81, 76, 72);
        var r43 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v43, null);
        Assert.True(r43.Success);
        Assert.Equal("iv43", r43.RomanText);
    }

    [Fact]
    public void Borrowed_bVII7_Inversions_With_Voicing()
    {
        var key = new Key(60, true); // C major
        // Bb D F Ab (bVII7)
        var pcs = new[] { Pc(70), Pc(62), Pc(65), Pc(68) };

        // 2nd inversion (fifth in bass: F) => bVII43
        var v43 = new FourPartVoicing(89, 82, 77, 65);
        var r43 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v43, null);
        Assert.True(r43.Success);
        Assert.Equal("bVII43", r43.RomanText);

        // 3rd inversion (seventh in bass: Ab) => bVII42
        var v42 = new FourPartVoicing(89, 82, 77, 68);
        var r42 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v42, null);
        Assert.True(r42.Success);
        Assert.Equal("bVII42", r42.RomanText);
    }
}
