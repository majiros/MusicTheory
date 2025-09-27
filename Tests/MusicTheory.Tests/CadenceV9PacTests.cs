using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class CadenceV9PacTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void V9_To_I_Is_PAC()
    {
        var key = new Key(60, true); // C major
        // V9 (G B D F A) → I (C E G)
        var v9 = new[] { Pc(67), Pc(71), Pc(74), Pc(65), Pc(69) };
        var I = new[] { Pc(60), Pc(64), Pc(67) };
        var seq = new[] { v9, I };
        var (chords, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key);
        Assert.Single(cadences);
        var c = cadences[0];
        Assert.Equal(CadenceType.Authentic, c.Type);
        Assert.True(c.IsPerfectAuthentic); // V9 を root ドミナント扱いで PAC
    }

    [Fact]
    public void V7Paren9_To_I_Is_PAC()
    {
        var key = new Key(60, true); // C major
        var opts = new HarmonyOptions { PreferV7Paren9OverV9 = true };
        var v9 = new[] { Pc(67), Pc(71), Pc(74), Pc(65), Pc(69) }; // V9 同構成
        var I = new[] { Pc(60), Pc(64), Pc(67) };
        var seq = new[] { v9, I };
        var (chords, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, opts);
        Assert.Single(cadences);
        var c = cadences[0];
        Assert.Equal(CadenceType.Authentic, c.Type);
        Assert.True(c.IsPerfectAuthentic);
    }
}
