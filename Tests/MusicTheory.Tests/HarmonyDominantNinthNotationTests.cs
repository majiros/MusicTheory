using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class HarmonyDominantNinthNotationTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DominantNinth_Label_Default_Is_V9(bool omitFifth)
    {
        var key = new Key(60, true); // C major
        int v = Pc(67); // G
        var pcs = omitFifth
            ? new[] { v, Pc(v + 4), Pc(v + 10), Pc(v + 14) } // 1,3,b7,9 (no 5th)
            : new[] { v, Pc(v + 4), Pc(v + 7), Pc(v + 10), Pc(v + 14) }; // 1,3,5,b7,9

        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key);
        Assert.True(r.Success);
        Assert.Equal("V9", r.RomanText);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DominantNinth_Label_Option_V7Paren9_When_Enabled(bool omitFifth)
    {
        var key = new Key(60, true); // C major
        int v = Pc(67); // G
        var pcs = omitFifth
            ? new[] { v, Pc(v + 4), Pc(v + 10), Pc(v + 14) }
            : new[] { v, Pc(v + 4), Pc(v + 7), Pc(v + 10), Pc(v + 14) };

        var opts = new HarmonyOptions { PreferV7Paren9OverV9 = true };
        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts);
        Assert.True(r.Success);
        Assert.Equal("V7(9)", r.RomanText);
    }
}
