using Xunit;
using MusicTheory.Theory.Harmony;
using MusicTheory.Theory.Time;

namespace MusicTheory.Tests;

/// <summary>
/// Phase 17: Final push to 85% coverage (84.7% → 85.0%+)
/// Target: Remaining small gaps in Chord, VoiceRanges, Duration, RationalFactor
/// Strategy: Focus on uncovered branches and edge cases in 80%+ classes
/// </summary>
public class Phase17Final85PercentTests
{
    // Chord Tests (85.7% → Higher)
    // Target: PitchClasses default case for ChordQuality.Unknown

    [Fact]
    public void Chord_PitchClasses_UnknownQuality_ReturnsEmpty()
    {
        // Target: Default case in PitchClasses switch (line ~25)
        var unknownChord = new Chord(0, ChordQuality.Unknown);
        
        var pcs = unknownChord.PitchClasses();
        
        Assert.Empty(pcs); // Should return empty array for unknown quality
    }

    // VoiceRanges Tests (95.2% → 100%)
    // Target: Test all voices to ensure complete coverage

    [Fact]
    public void VoiceRanges_AllVoices_HaveValidRanges()
    {
        // Target: Ensure all voice enum values have valid ranges
        foreach (Voice voice in Enum.GetValues(typeof(Voice)))
        {
            var range = VoiceRanges.ForVoice(voice);
            
            Assert.True(range.MinMidi < range.MaxMidi);
            Assert.True(range.WarnMinMidi <= range.MinMidi);
            Assert.True(range.WarnMaxMidi >= range.MaxMidi);
        }
    }

    // BaseNoteValueExtensions Tests (92.3% → Higher)
    // Target: Test GetFraction for all enum values

    [Fact]
    public void BaseNoteValueExtensions_GetFraction_AllValues()
    {
        // Target: Ensure all enum values have valid fraction representations
        foreach (BaseNoteValue value in Enum.GetValues(typeof(BaseNoteValue)))
        {
            var (num, den) = value.GetFraction();
            
            Assert.True(num > 0);
            Assert.True(den > 0);
        }
    }

    // Duration Tests (94.7% → Higher)
    // Target: Uncovered edge cases

    [Fact]
    public void Duration_Equality_WithDifferentFractions_SameValue()
    {
        // Target: Test equivalence of different fraction representations
        var dur1 = new Duration(new RationalFactor(1, 4)); // Quarter note
        var dur2 = new Duration(new RationalFactor(2, 8)); // Same as 1/4
        
        Assert.Equal(dur1.Ticks, dur2.Ticks); // Should have same tick value
    }

    [Fact]
    public void Duration_FromBase_WithMaxDots()
    {
        // Target: Test maximum dots (3) edge case
        var dottedDuration = Duration.FromBase(BaseNoteValue.Quarter, Duration.MaxDots);
        
        Assert.True(dottedDuration.Ticks > Duration.FromBase(BaseNoteValue.Quarter, 0).Ticks);
        // 1/4 with 3 dots = 1/4 * (2^4-1)/2^3 = 1/4 * 15/8
    }

    // RationalFactor Tests (95.2% → Higher)
    // Target: Test operator* and reduction

    [Fact]
    public void RationalFactor_Multiplication_ProducesReducedFraction()
    {
        // Target: Test operator* and automatic reduction
        var half = new RationalFactor(1, 2);
        var third = new RationalFactor(1, 3);
        
        var result = half * third;
        
        Assert.Equal(1, result.Numerator);
        Assert.Equal(6, result.Denominator); // 1/2 * 1/3 = 1/6
    }

    // TimeSignature Tests (91.6% → Higher)
    // Target: Test constructor with various inputs

    [Fact]
    public void TimeSignature_Constructor_ValidatesInput()
    {
        // Target: Test constructor with valid inputs
        var ts = new MusicTheory.Theory.Time.TimeSignature(3, 4); // 3/4 time
        
        Assert.Equal(3, ts.Numerator);
        Assert.Equal(4, ts.Denominator);
        Assert.True(ts.TicksPerBeat > 0);
        Assert.True(ts.TicksPerBar > 0);
    }

    [Fact]
    public void TimeSignature_CompoundTime_CalculatesCorrectly()
    {
        // Target: Test 6/8 compound time
        var ts = new MusicTheory.Theory.Time.TimeSignature(6, 8); // 6/8 time
        
        Assert.Equal(6, ts.Numerator);
        Assert.Equal(8, ts.Denominator);
        Assert.True(ts.TicksPerBar == ts.TicksPerBeat * ts.Numerator);
    }
}
