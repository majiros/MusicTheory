using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class TriadInversionStrictDistinctTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void Dyad_Does_Not_Label_Triad_Inversion()
    {
        // C major, dyad C–E (2 distinct PCs). Even if bass is the 3rd, triad path is gated to distinct=3.
        var key = new Key(60, true);
        var pcs = new[] { Pc(60), Pc(64) }; // C, E
        var v = new FourPartVoicing(84, 76, 64, 52); // S:A:T:B = C6, E5, E4, E3 (bass pc = 4 = third)
        var res = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v, null);
        Assert.False(res.Success);
        Assert.Null(res.RomanText);
    }

    [Fact]
    public void Triad_With_Bass_Third_Gets_6()
    {
        // C major, C–E–G triad, bass on E -> I6
        var key = new Key(60, true);
        var pcs = new[] { Pc(60), Pc(64), Pc(67) }; // C, E, G
        var v = new FourPartVoicing(88, 76, 64, 52); // Bass E3 (pc=4)
        var res = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v, null);
        Assert.True(res.Success);
        Assert.Equal("I6", res.RomanText);
    }
}
