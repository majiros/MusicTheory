using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class TriadInversionStrictDistinctMinorTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void Dyad_Does_Not_Label_Triad_Inversion_In_Minor()
    {
        // C minor, dyad C–Eb (2 distinct PCs). Even if bass is the 3rd, triad path is gated to distinct=3.
    var key = new Key(60, false);
        var pcs = new[] { Pc(60), Pc(63) }; // C, Eb
        var v = new FourPartVoicing(84, 75, 63, 51); // S:A:T:B, bass pc = 3 (= third)
        var res = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v, null);
        Assert.False(res.Success);
        Assert.Null(res.RomanText);
    }

    [Fact]
    public void Minor_Triad_With_Bass_Third_Gets_6()
    {
        // C minor, C–Eb–G triad, bass on Eb -> i6
    var key = new Key(60, false);
        var pcs = new[] { Pc(60), Pc(63), Pc(67) }; // C, Eb, G
        var v = new FourPartVoicing(88, 75, 63, 51); // bass Eb3 (pc=3)
        var res = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v, null);
        Assert.True(res.Success);
        Assert.Equal("i6", res.RomanText);
    }
}
