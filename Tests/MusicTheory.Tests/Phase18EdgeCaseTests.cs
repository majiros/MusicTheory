using Xunit;
using TimeSignatureTime = MusicTheory.Theory.Time.TimeSignature;
using MusicTheory.Theory.Time;

namespace MusicTheory.Tests;

/// <summary>
/// Phase 18: Edge case tests targeting final 0.3% to reach 85% (84.7% → 85.0%+)
/// Strategy: Focus on exception paths and boundary conditions
/// Target: TimeSignature, BarBeatTick, RationalFactor, Duration validation branches
/// </summary>
public class Phase18EdgeCaseTests
{
    // TimeSignature Exception Tests (91.6% → Higher)
    // Target: Exception paths in constructor validation

    [Fact]
    public void TimeSignature_Constructor_ThrowsOnInvalidNumerator()
    {
        // Target: numerator <= 0 exception path
        Assert.Throws<ArgumentOutOfRangeException>(() => new TimeSignatureTime(0, 4));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TimeSignatureTime(-1, 4));
    }

    [Fact]
    public void TimeSignature_Constructor_ThrowsOnInvalidDenominator()
    {
        // Target: denominator <= 0 or non-power-of-two exception paths
        Assert.Throws<ArgumentException>(() => new TimeSignatureTime(4, 0));
        Assert.Throws<ArgumentException>(() => new TimeSignatureTime(4, -1));
        Assert.Throws<ArgumentException>(() => new TimeSignatureTime(4, 3)); // Not power of two
        Assert.Throws<ArgumentException>(() => new TimeSignatureTime(4, 6)); // Not power of two
        Assert.Throws<ArgumentException>(() => new TimeSignatureTime(4, 5)); // Not power of two
    }

    // BarBeatTick Exception Tests (87.5% → Higher)
    // Target: Exception path in constructor

    [Fact]
    public void BarBeatTick_Constructor_ThrowsOnNegativeValues()
    {
        // Target: Negative value exception paths (bar<0||beat<0||tickWithinBeat<0)
        Assert.Throws<ArgumentOutOfRangeException>(() => new BarBeatTick(-1, 0, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BarBeatTick(0, -1, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BarBeatTick(0, 0, -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new BarBeatTick(-5, -2, -3)); // Multiple negative
    }

    // RationalFactor Edge Cases (95.2% → Higher)
    // Target: Addition operator

    [Fact]
    public void RationalFactor_Addition_ProducesReducedFraction()
    {
        // Target: operator+ for two RationalFactors
        var third = new RationalFactor(1, 3);
        var sixth = new RationalFactor(1, 6);
        
        var result = third + sixth; // 1/3 + 1/6 = 1/2
        
        Assert.Equal(1, result.Numerator);
        Assert.Equal(2, result.Denominator);
    }

    // Duration Edge Cases (94.7% → Higher)
    // Target: Comparison and ToString methods

    [Fact]
    public void Duration_ToString_ReturnsTicksRepresentation()
    {
        // Target: ToString method
        var quarter = Duration.FromBase(BaseNoteValue.Quarter, 0);
        var str = quarter.ToString();
        
        Assert.NotNull(str);
        Assert.Contains(quarter.Ticks.ToString(), str);
    }

    [Fact]
    public void Duration_GetHashCode_ConsistentForSameValue()
    {
        // Target: GetHashCode method
        var dur1 = Duration.FromBase(BaseNoteValue.Quarter, 0);
        var dur2 = Duration.FromBase(BaseNoteValue.Quarter, 0);
        
        Assert.Equal(dur1.GetHashCode(), dur2.GetHashCode());
    }

    [Fact]
    public void Duration_Equals_WorksWithObject()
    {
        // Target: Equals(object) override
        var dur1 = Duration.FromBase(BaseNoteValue.Quarter, 0);
        object dur2 = Duration.FromBase(BaseNoteValue.Quarter, 0);
        
        Assert.True(dur1.Equals(dur2));
    }
}
