using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class RomanNumeralUtilsTests
{
    // ──────────────────────────────────────────────────────────
    // FunctionOf - Tonic
    // ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(RomanNumeral.I, TonalFunction.Tonic)]
    [InlineData(RomanNumeral.i, TonalFunction.Tonic)]
    [InlineData(RomanNumeral.III, TonalFunction.Tonic)]
    [InlineData(RomanNumeral.iii, TonalFunction.Tonic)]
    [InlineData(RomanNumeral.VI, TonalFunction.Tonic)]
    [InlineData(RomanNumeral.vi, TonalFunction.Tonic)]
    public void FunctionOf_TonicNumerals_ReturnsTonic(RomanNumeral rn, TonalFunction expected)
    {
        Assert.Equal(expected, RomanNumeralUtils.FunctionOf(rn));
    }

    // ──────────────────────────────────────────────────────────
    // FunctionOf - Subdominant
    // ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(RomanNumeral.IV, TonalFunction.Subdominant)]
    [InlineData(RomanNumeral.iv, TonalFunction.Subdominant)]
    [InlineData(RomanNumeral.II, TonalFunction.Subdominant)]
    [InlineData(RomanNumeral.ii, TonalFunction.Subdominant)]
    public void FunctionOf_SubdominantNumerals_ReturnsSubdominant(RomanNumeral rn, TonalFunction expected)
    {
        Assert.Equal(expected, RomanNumeralUtils.FunctionOf(rn));
    }

    // ──────────────────────────────────────────────────────────
    // FunctionOf - Dominant
    // ──────────────────────────────────────────────────────────

    [Theory]
    [InlineData(RomanNumeral.V, TonalFunction.Dominant)]
    [InlineData(RomanNumeral.v, TonalFunction.Dominant)]
    [InlineData(RomanNumeral.VII, TonalFunction.Dominant)]
    [InlineData(RomanNumeral.vii, TonalFunction.Dominant)]
    public void FunctionOf_DominantNumerals_ReturnsDominant(RomanNumeral rn, TonalFunction expected)
    {
        Assert.Equal(expected, RomanNumeralUtils.FunctionOf(rn));
    }
}

public class VoiceRangesTests
{
    // ──────────────────────────────────────────────────────────
    // Predefined Voice Ranges
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void Soprano_ReturnsValidRange()
    {
        var range = VoiceRanges.Soprano;
        Assert.True(range.MinMidi >= 0);
        Assert.True(range.MaxMidi > range.MinMidi);
        Assert.True(range.WarnMinMidi < range.MinMidi);
        Assert.True(range.WarnMaxMidi > range.MaxMidi);
    }

    [Fact]
    public void Alto_ReturnsValidRange()
    {
        var range = VoiceRanges.Alto;
        Assert.True(range.MinMidi >= 0);
        Assert.True(range.MaxMidi > range.MinMidi);
        Assert.True(range.WarnMinMidi < range.MinMidi);
        Assert.True(range.WarnMaxMidi > range.MaxMidi);
    }

    [Fact]
    public void Tenor_ReturnsValidRange()
    {
        var range = VoiceRanges.Tenor;
        Assert.True(range.MinMidi >= 0);
        Assert.True(range.MaxMidi > range.MinMidi);
        Assert.True(range.WarnMinMidi < range.MinMidi);
        Assert.True(range.WarnMaxMidi > range.MaxMidi);
    }

    [Fact]
    public void Bass_ReturnsValidRange()
    {
        var range = VoiceRanges.Bass;
        Assert.True(range.MinMidi >= 0);
        Assert.True(range.MaxMidi > range.MinMidi);
        Assert.True(range.WarnMinMidi < range.MinMidi);
        Assert.True(range.WarnMaxMidi > range.MaxMidi);
    }

    [Fact]
    public void Soprano_ExpectedMidiRange()
    {
        var range = VoiceRanges.Soprano;
        // C4 (60) to A5 (81), warn ±4 semitones
        Assert.Equal(60, range.MinMidi);
        Assert.Equal(81, range.MaxMidi);
        Assert.Equal(56, range.WarnMinMidi);
        Assert.Equal(85, range.WarnMaxMidi);
    }

    [Fact]
    public void Alto_ExpectedMidiRange()
    {
        var range = VoiceRanges.Alto;
        // G3 (55) to D5 (74), warn ±4 semitones
        Assert.Equal(55, range.MinMidi);
        Assert.Equal(74, range.MaxMidi);
        Assert.Equal(51, range.WarnMinMidi);
        Assert.Equal(78, range.WarnMaxMidi);
    }

    [Fact]
    public void Tenor_ExpectedMidiRange()
    {
        var range = VoiceRanges.Tenor;
        // C3 (48) to A4 (69), warn ±4 semitones
        Assert.Equal(48, range.MinMidi);
        Assert.Equal(69, range.MaxMidi);
        Assert.Equal(44, range.WarnMinMidi);
        Assert.Equal(73, range.WarnMaxMidi);
    }

    [Fact]
    public void Bass_ExpectedMidiRange()
    {
        var range = VoiceRanges.Bass;
        // F2 (41) to D4 (62), warn ±4 semitones
        Assert.Equal(41, range.MinMidi);
        Assert.Equal(62, range.MaxMidi);
        Assert.Equal(37, range.WarnMinMidi);
        Assert.Equal(66, range.WarnMaxMidi);
    }

    // ──────────────────────────────────────────────────────────
    // ForVoice Method
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void ForVoice_Soprano_ReturnsSopranoRange()
    {
        var range = VoiceRanges.ForVoice(Voice.Soprano);
        Assert.Equal(VoiceRanges.Soprano.MinMidi, range.MinMidi);
        Assert.Equal(VoiceRanges.Soprano.MaxMidi, range.MaxMidi);
    }

    [Fact]
    public void ForVoice_Alto_ReturnsAltoRange()
    {
        var range = VoiceRanges.ForVoice(Voice.Alto);
        Assert.Equal(VoiceRanges.Alto.MinMidi, range.MinMidi);
        Assert.Equal(VoiceRanges.Alto.MaxMidi, range.MaxMidi);
    }

    [Fact]
    public void ForVoice_Tenor_ReturnsTenorRange()
    {
        var range = VoiceRanges.ForVoice(Voice.Tenor);
        Assert.Equal(VoiceRanges.Tenor.MinMidi, range.MinMidi);
        Assert.Equal(VoiceRanges.Tenor.MaxMidi, range.MaxMidi);
    }

    [Fact]
    public void ForVoice_Bass_ReturnsBassRange()
    {
        var range = VoiceRanges.ForVoice(Voice.Bass);
        Assert.Equal(VoiceRanges.Bass.MinMidi, range.MinMidi);
        Assert.Equal(VoiceRanges.Bass.MaxMidi, range.MaxMidi);
    }

    // ──────────────────────────────────────────────────────────
    // VoiceRange Instance Methods
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void InHardRange_WithinRange_ReturnsTrue()
    {
        var range = VoiceRanges.Soprano;
        Assert.True(range.InHardRange(60)); // C4
        Assert.True(range.InHardRange(70)); // A#4
        Assert.True(range.InHardRange(81)); // A5
    }

    [Fact]
    public void InHardRange_OutsideRange_ReturnsFalse()
    {
        var range = VoiceRanges.Soprano;
        Assert.False(range.InHardRange(59)); // B3 (below min)
        Assert.False(range.InHardRange(82)); // A#5 (above max)
    }

    [Fact]
    public void InWarnRange_WithinWarnRange_ReturnsTrue()
    {
        var range = VoiceRanges.Soprano;
        Assert.True(range.InWarnRange(56)); // Warn min
        Assert.True(range.InWarnRange(70)); // Middle
        Assert.True(range.InWarnRange(85)); // Warn max
    }

    [Fact]
    public void InWarnRange_OutsideWarnRange_ReturnsFalse()
    {
        var range = VoiceRanges.Soprano;
        Assert.False(range.InWarnRange(55)); // Below warn min
        Assert.False(range.InWarnRange(86)); // Above warn max
    }

    [Fact]
    public void InWarnRange_WiderThanHardRange()
    {
        var range = VoiceRanges.Soprano;
        // Warn range should be wider than hard range
        Assert.True(range.WarnMinMidi < range.MinMidi);
        Assert.True(range.WarnMaxMidi > range.MaxMidi);

        // Values just outside hard range should be in warn range
        Assert.True(range.InWarnRange(range.MinMidi - 1));
        Assert.True(range.InWarnRange(range.MaxMidi + 1));
    }
}
