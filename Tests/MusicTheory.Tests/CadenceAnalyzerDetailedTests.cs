using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class CadenceAnalyzerDetailedTests
{
    [Fact]
    public void Half_Suppressed_When_I64_Precedes_V_And_PAC_Detected()
    {
        // Step 1: I64 -> V (Half would be suppressed and reported as None with Cadential 6-4 flag)
        var sup = CadenceAnalyzer.DetectDetailed(indexFrom: 0,
            prev: RomanNumeral.I, curr: RomanNumeral.V, isMajor: true, tonicPc: 0,
            prevText: "I64", currText: "V", prevPrevText: null);
        Assert.Equal(CadenceType.None, sup.Type);
        Assert.True(sup.HasCadentialSixFour);
        Assert.Equal(SixFourType.Cadential, sup.SixFour);

        // Step 2: V -> I authentic; PAC if both are root position
        var pac = CadenceAnalyzer.DetectDetailed(indexFrom: 1,
            prev: RomanNumeral.V, curr: RomanNumeral.I, isMajor: true, tonicPc: 0,
            prevText: "V", currText: "I", prevPrevText: "I64");
        Assert.Equal(CadenceType.Authentic, pac.Type);
        Assert.True(pac.IsPerfectAuthentic);
        Assert.True(pac.HasCadentialSixFour);
        Assert.Equal(SixFourType.Cadential, pac.SixFour);
    }
}
