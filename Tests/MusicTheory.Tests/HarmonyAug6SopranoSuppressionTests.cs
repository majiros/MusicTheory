using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class HarmonyAug6SopranoSuppressionTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void DefaultSuppressesAug6_WhenSopranoAlsoFlat6()
    {
    var key = new Key(60, true); // C major
        // Ab C Eb Gb  (Ger65 set)
        var pcs = new[] { Pc(68), Pc(60), Pc(63), Pc(66) };
        // S=A♭(b6), A=C, T=E♭, B=A♭ → soprano=b6 & bass=b6
        var v = new FourPartVoicing(80, 72, 63, 68);

        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, HarmonyOptions.Default, v);
        Assert.True(r.Success);
        Assert.StartsWith("bVI", r.RomanText);
    }
}
