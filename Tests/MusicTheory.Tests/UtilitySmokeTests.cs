using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class UtilitySmokeTests
{
    [Fact]
    public void RomanNumeral_FunctionOf_Maps_As_Expected()
    {
        Assert.Equal(TonalFunction.Tonic, RomanNumeralUtils.FunctionOf(RomanNumeral.I));
        Assert.Equal(TonalFunction.Subdominant, RomanNumeralUtils.FunctionOf(RomanNumeral.ii));
        Assert.Equal(TonalFunction.Dominant, RomanNumeralUtils.FunctionOf(RomanNumeral.V));
        Assert.Equal(TonalFunction.Unknown, RomanNumeralUtils.FunctionOf((RomanNumeral)999));
    }

    [Fact]
    public void VoiceRanges_ForVoice_Returns_Range_With_Warn()
    {
        var s = VoiceRanges.Soprano;
        Assert.True(s.InWarnRange(s.WarnMinMidi));
        Assert.True(s.InWarnRange(s.WarnMaxMidi));

        var b = VoiceRanges.ForVoice(Voice.Bass);
        Assert.True(b.InWarnRange(b.WarnMinMidi));
        Assert.True(b.InWarnRange(b.WarnMaxMidi));
    }
}
