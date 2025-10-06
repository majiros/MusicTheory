using Xunit;
using MusicTheory.Theory.Time;
using System.Linq;

namespace MusicTheory.Tests;

/// <summary>
/// Tests for DurationFactory to improve coverage.
/// </summary>
public class DurationFactoryTests
{
    // ──────────────────────────────────────────────────────────
    // Basic Note Value Creation
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void DoubleWhole_CreatesCorrectDuration()
    {
        var dur = DurationFactory.DoubleWhole();
        Assert.Equal(Duration.TicksPerQuarter * 8, dur.Ticks);
    }

    [Fact]
    public void Whole_CreatesCorrectDuration()
    {
        var dur = DurationFactory.Whole();
        Assert.Equal(Duration.TicksPerQuarter * 4, dur.Ticks);
    }

    [Fact]
    public void Half_CreatesCorrectDuration()
    {
        var dur = DurationFactory.Half();
        Assert.Equal(Duration.TicksPerQuarter * 2, dur.Ticks);
    }

    [Fact]
    public void Quarter_CreatesCorrectDuration()
    {
        var dur = DurationFactory.Quarter();
        Assert.Equal(Duration.TicksPerQuarter, dur.Ticks);
    }

    [Fact]
    public void Eighth_CreatesCorrectDuration()
    {
        var dur = DurationFactory.Eighth();
        Assert.Equal(Duration.TicksPerQuarter / 2, dur.Ticks);
    }

    [Fact]
    public void Sixteenth_CreatesCorrectDuration()
    {
        var dur = DurationFactory.Sixteenth();
        Assert.Equal(Duration.TicksPerQuarter / 4, dur.Ticks);
    }

    [Fact]
    public void ThirtySecond_CreatesCorrectDuration()
    {
        var dur = DurationFactory.ThirtySecond();
        Assert.Equal(Duration.TicksPerQuarter / 8, dur.Ticks);
    }

    [Fact]
    public void SixtyFourth_CreatesCorrectDuration()
    {
        var dur = DurationFactory.SixtyFourth();
        Assert.Equal(Duration.TicksPerQuarter / 16, dur.Ticks);
    }

    [Fact]
    public void OneHundredTwentyEighth_CreatesCorrectDuration()
    {
        var dur = DurationFactory.OneHundredTwentyEighth();
        Assert.Equal(Duration.TicksPerQuarter / 32, dur.Ticks);
    }

    // ──────────────────────────────────────────────────────────
    // Dotted Notes
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void Quarter_OneDot_CorrectTicks()
    {
        var dur = DurationFactory.Quarter(dots: 1);
        // Quarter = 480, dotted = 480 + 240 = 720
        Assert.Equal(720, dur.Ticks);
    }

    [Fact]
    public void Half_TwoDots_CorrectTicks()
    {
        var dur = DurationFactory.Half(dots: 2);
        // Half = 960, 1 dot = 960 + 480 = 1440, 2 dots = 1440 + 240 = 1680
        Assert.Equal(1680, dur.Ticks);
    }

    [Fact]
    public void Eighth_OneDot_CorrectTicks()
    {
        var dur = DurationFactory.Eighth(dots: 1);
        // Eighth = 240, dotted = 240 + 120 = 360
        Assert.Equal(360, dur.Ticks);
    }

    // ──────────────────────────────────────────────────────────
    // Tuplet Creation
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void Tuplet_Triplet_CorrectTicks()
    {
        var triplet = new Tuplet(3, 2); // 3 in space of 2
        var dur = DurationFactory.Tuplet(BaseNoteValue.Quarter, triplet);
        
        // Quarter triplet: (480 * 2) / 3 = 320
        Assert.Equal(320, dur.Ticks);
    }

    [Fact]
    public void Tuplet_Quintuplet_CorrectTicks()
    {
        var quintuplet = new Tuplet(5, 4); // 5 in space of 4
        var dur = DurationFactory.Tuplet(BaseNoteValue.Sixteenth, quintuplet);
        
        // Sixteenth quintuplet: (120 * 4) / 5 = 96
        Assert.Equal(96, dur.Ticks);
    }

    [Fact]
    public void Tuplet_WithDots_CorrectTicks()
    {
        var triplet = new Tuplet(3, 2);
        var dur = DurationFactory.Tuplet(BaseNoteValue.Quarter, triplet, dots: 1);
        
        // Quarter triplet = 320, dotted = 320 + 160 = 480
        Assert.Equal(480, dur.Ticks);
    }
}

public class DurationSequenceUtilsTests
{
    // ──────────────────────────────────────────────────────────
    // NormalizeRests - Basic Functionality
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void NormalizeRests_NoRests_ReturnsOriginalSequence()
    {
        var seq = new IDurationEntity[]
        {
            new Note(DurationFactory.Quarter(), 60),
            new Note(DurationFactory.Eighth(), 64)
        };

        var result = DurationSequenceUtils.NormalizeRests(seq);

        Assert.Equal(2, result.Count);
        Assert.IsType<Note>(result[0]);
        Assert.IsType<Note>(result[1]);
    }

    [Fact]
    public void NormalizeRests_SingleRest_ReturnsUnchanged()
    {
        var seq = new IDurationEntity[]
        {
            RestFactory.Quarter()
        };

        var result = DurationSequenceUtils.NormalizeRests(seq);

        Assert.Single(result);
        Assert.IsType<Rest>(result[0]);
        Assert.Equal(Duration.TicksPerQuarter, result[0].Duration.Ticks);
    }

    [Fact]
    public void NormalizeRests_ConsecutiveRests_Merged()
    {
        var seq = new IDurationEntity[]
        {
            RestFactory.Quarter(),
            RestFactory.Quarter()
        };

        var result = DurationSequenceUtils.NormalizeRests(seq);

        // Should merge into one half rest (or equivalent)
        Assert.Single(result);
        Assert.IsType<Rest>(result[0]);
        Assert.Equal(Duration.TicksPerQuarter * 2, result[0].Duration.Ticks);
    }

    [Fact]
    public void NormalizeRests_MixedNotesAndRests_OnlyMergesRests()
    {
        var seq = new IDurationEntity[]
        {
            new Note(DurationFactory.Quarter(), 60),
            RestFactory.Eighth(),
            RestFactory.Eighth(),
            new Note(DurationFactory.Quarter(), 64)
        };

        var result = DurationSequenceUtils.NormalizeRests(seq);

        // Rests should be merged, notes preserved
        Assert.Equal(3, result.Count);
        Assert.IsType<Note>(result[0]);
        Assert.IsType<Rest>(result[1]);
        Assert.IsType<Note>(result[2]);
        Assert.Equal(Duration.TicksPerQuarter, result[1].Duration.Ticks); // Merged to quarter
    }

    [Fact]
    public void NormalizeRests_MultipleRestRuns_EachRunMergedIndependently()
    {
        var seq = new IDurationEntity[]
        {
            RestFactory.Eighth(),
            RestFactory.Eighth(),
            new Note(DurationFactory.Quarter(), 60),
            RestFactory.Quarter(),
            RestFactory.Quarter()
        };

        var result = DurationSequenceUtils.NormalizeRests(seq);

        Assert.Equal(3, result.Count);
        Assert.IsType<Rest>(result[0]);
        Assert.Equal(Duration.TicksPerQuarter, result[0].Duration.Ticks); // First run: 2 eighths
        Assert.IsType<Note>(result[1]);
        Assert.IsType<Rest>(result[2]);
        Assert.Equal(Duration.TicksPerQuarter * 2, result[2].Duration.Ticks); // Second run: 2 quarters
    }

    [Fact]
    public void NormalizeRests_EmptySequence_ReturnsEmpty()
    {
        var seq = System.Array.Empty<IDurationEntity>();
        var result = DurationSequenceUtils.NormalizeRests(seq);
        Assert.Empty(result);
    }

    // ──────────────────────────────────────────────────────────
    // NormalizeRests - Advanced Options
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void NormalizeRests_AdvancedSplit_DottedRests_KeptAsSimple()
    {
        // advancedSplit splits when duration cannot be expressed as simple form
        // Single dotted quarter CAN be simple (Quarter with dots=1), so kept as-is
        var seq = new IDurationEntity[]
        {
            RestFactory.Quarter(dots: 1) // Dotted quarter is valid simple form
        };

        var result = DurationSequenceUtils.NormalizeRests(seq, advancedSplit: true);

        // Should keep as single dotted rest (already simple)
        Assert.Single(result);
        Assert.IsType<Rest>(result[0]);
        Assert.Equal(Duration.TicksPerQuarter + Duration.TicksPerQuarter / 2, result[0].Duration.Ticks);
    }

    [Fact]
    public void NormalizeRests_AdvancedSplit_IrregularDuration_AttemptsSplit()
    {
        // Create an irregular duration that cannot be expressed as simple form
        // Use FromTicks to create a non-standard duration
        var irregularDur = Duration.FromTicks(500); // Not a standard note value
        var seq = new IDurationEntity[]
        {
            new Rest(irregularDur)
        };

        var result = DurationSequenceUtils.NormalizeRests(seq, advancedSplit: true);

        // Should attempt to handle irregular duration (kept as-is if no decomposition)
        Assert.Single(result);
        Assert.IsType<Rest>(result[0]);
        Assert.Equal(500, result[0].Duration.Ticks);
    }

    [Fact]
    public void NormalizeRests_WithoutAdvancedSplit_KeepsDottedRests()
    {
        var seq = new IDurationEntity[]
        {
            RestFactory.Quarter(dots: 1)
        };

        var result = DurationSequenceUtils.NormalizeRests(seq, advancedSplit: false);

        // Should keep as single dotted rest
        Assert.Single(result);
        Assert.IsType<Rest>(result[0]);
        Assert.Equal(Duration.TicksPerQuarter + Duration.TicksPerQuarter / 2, result[0].Duration.Ticks);
    }

    [Fact]
    public void NormalizeRests_CustomTuplets_RecognizesAdditionalTuplets()
    {
        var customTuplets = new[] { new Tuplet(5, 4) }; // Quintuplet
        var quintupletRest = RestFactory.Tuplet(BaseNoteValue.Sixteenth, new Tuplet(5, 4));
        
        var seq = new IDurationEntity[] { quintupletRest };

        var result = DurationSequenceUtils.NormalizeRests(seq, additionalTuplets: customTuplets);

        Assert.Single(result);
        Assert.IsType<Rest>(result[0]);
        Assert.Equal(quintupletRest.Duration.Ticks, result[0].Duration.Ticks);
    }

    // ──────────────────────────────────────────────────────────
    // Note Tie Functionality
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void Note_Tie_CombinesDurations()
    {
        var note1 = new Note(DurationFactory.Quarter(), 60);
        var note2 = new Note(DurationFactory.Eighth(), 60);

        var tied = note1.Tie(note2);

        Assert.Equal(DurationFactory.Quarter().Ticks + DurationFactory.Eighth().Ticks, tied.Duration.Ticks);
        Assert.Equal(60, tied.Pitch);
    }

    [Fact]
    public void Note_Tie_PreservesFirstNotePitch()
    {
        var note1 = new Note(DurationFactory.Quarter(), 60);
        var note2 = new Note(DurationFactory.Quarter(), 64); // Different pitch

        var tied = note1.Tie(note2);

        Assert.Equal(60, tied.Pitch); // First note's pitch preserved
    }

    [Fact]
    public void Note_Tie_MultipleTies_AccumulatesDuration()
    {
        var note1 = new Note(DurationFactory.Quarter(), 60);
        var note2 = new Note(DurationFactory.Quarter(), 60);
        var note3 = new Note(DurationFactory.Half(), 60);

        var tied = note1.Tie(note2).Tie(note3);

        // Quarter + Quarter + Half = Whole
        Assert.Equal(Duration.TicksPerQuarter * 4, tied.Duration.Ticks);
        Assert.Equal(60, tied.Pitch);
    }
}

/// <summary>
/// Tests for BaseNoteValueExtensions to improve coverage.
/// </summary>
public class BaseNoteValueExtensionsTests
{
    [Theory]
    [InlineData(BaseNoteValue.DoubleWhole, 2, 1)]
    [InlineData(BaseNoteValue.Whole, 1, 1)]
    [InlineData(BaseNoteValue.Half, 1, 2)]
    [InlineData(BaseNoteValue.Quarter, 1, 4)]
    [InlineData(BaseNoteValue.Eighth, 1, 8)]
    [InlineData(BaseNoteValue.Sixteenth, 1, 16)]
    [InlineData(BaseNoteValue.ThirtySecond, 1, 32)]
    [InlineData(BaseNoteValue.SixtyFourth, 1, 64)]
    [InlineData(BaseNoteValue.OneHundredTwentyEighth, 1, 128)]
    public void GetFraction_ReturnsCorrectFraction(BaseNoteValue value, int expectedNum, int expectedDen)
    {
        var (num, den) = value.GetFraction();
        Assert.Equal(expectedNum, num);
        Assert.Equal(expectedDen, den);
    }

    [Fact]
    public void GetFraction_DoubleWhole_IsTwiceWhole()
    {
        var (num, den) = BaseNoteValue.DoubleWhole.GetFraction();
        var whole = BaseNoteValue.Whole.GetFraction();
        
        // DoubleWhole (2/1) should be twice Whole (1/1)
        Assert.Equal(2 * whole.Item1, num * whole.Item2);
    }

    [Fact]
    public void GetFraction_EachValueHalvesPrevious()
    {
        var values = new[]
        {
            BaseNoteValue.Whole,
            BaseNoteValue.Half,
            BaseNoteValue.Quarter,
            BaseNoteValue.Eighth,
            BaseNoteValue.Sixteenth
        };

        for (int i = 1; i < values.Length; i++)
        {
            var prev = values[i - 1].GetFraction();
            var curr = values[i].GetFraction();
            
            // Current should be half of previous
            Assert.Equal(prev.Item1 * curr.Item2, prev.Item2 * curr.Item1 * 2);
        }
    }
}

/// <summary>
/// Tests for Tuplet structure to improve coverage.
/// </summary>
public class TupletTests
{
    [Fact]
    public void Tuplet_Constructor_StoresValues()
    {
        var tuplet = new Tuplet(3, 2);
        Assert.Equal(3, tuplet.ActualCount);
        Assert.Equal(2, tuplet.NormalCount);
    }

    [Fact]
    public void Tuplet_Factor_CalculatesCorrectly()
    {
        var triplet = new Tuplet(3, 2);
        Assert.Equal(2.0 / 3.0, triplet.Factor, precision: 5);
    }

    [Fact]
    public void Tuplet_Quintuplet_Factor()
    {
        var quintuplet = new Tuplet(5, 4);
        Assert.Equal(4.0 / 5.0, quintuplet.Factor, precision: 5);
    }

    [Fact]
    public void Tuplet_Create_ReturnsSameAsDirect()
    {
        var direct = new Tuplet(3, 2);
        var created = Tuplet.Create(3, 2);
        
        Assert.Equal(direct.ActualCount, created.ActualCount);
        Assert.Equal(direct.NormalCount, created.NormalCount);
    }

    [Fact]
    public void Tuplet_ToString_FormatsCorrectly()
    {
        var tuplet = new Tuplet(3, 2);
        Assert.Equal("3:2", tuplet.ToString());
    }

    [Fact]
    public void Tuplet_FactorRational_CalculatesCorrectly()
    {
        var triplet = new Tuplet(3, 2);
        var rational = triplet.FactorRational;
        
        Assert.Equal(2, rational.Numerator);
        Assert.Equal(3, rational.Denominator);
    }
}

/// <summary>
/// Tests for RationalFactor to improve coverage.
/// </summary>
public class RationalFactorTests
{
    // ──────────────────────────────────────────────────────────
    // Constructor & Simplification
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_SimplifiesFraction()
    {
        var rf = new RationalFactor(6, 8);
        Assert.Equal(3, rf.Numerator);
        Assert.Equal(4, rf.Denominator);
    }

    [Fact]
    public void Constructor_ZeroNumerator_ResultsIn0Over1()
    {
        var rf = new RationalFactor(0, 5);
        Assert.Equal(0, rf.Numerator);
        Assert.Equal(1, rf.Denominator);
    }

    [Fact]
    public void Constructor_ZeroDenominator_ThrowsDivideByZero()
    {
        Assert.Throws<DivideByZeroException>(() => new RationalFactor(5, 0));
    }

    [Fact]
    public void Constructor_NegativeNumerator_PreservesSign()
    {
        var rf = new RationalFactor(-3, 4);
        Assert.Equal(-3, rf.Numerator);
        Assert.Equal(4, rf.Denominator);
    }

    [Fact]
    public void Constructor_NegativeDenominator_MovesSignToNumerator()
    {
        var rf = new RationalFactor(3, -4);
        Assert.Equal(-3, rf.Numerator);
        Assert.Equal(4, rf.Denominator);
    }

    [Fact]
    public void Constructor_BothNegative_ResultsPositive()
    {
        var rf = new RationalFactor(-6, -8);
        Assert.Equal(3, rf.Numerator);
        Assert.Equal(4, rf.Denominator);
    }

    [Fact]
    public void FromFraction_CreatesSameAsConstructor()
    {
        var rf1 = new RationalFactor(3, 4);
        var rf2 = RationalFactor.FromFraction(3, 4);
        
        Assert.Equal(rf1.Numerator, rf2.Numerator);
        Assert.Equal(rf1.Denominator, rf2.Denominator);
    }

    // ──────────────────────────────────────────────────────────
    // Addition
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void Addition_SameDenominator_AddsNumerators()
    {
        var a = new RationalFactor(1, 4);
        var b = new RationalFactor(1, 4);
        var result = a + b;
        
        Assert.Equal(1, result.Numerator);
        Assert.Equal(2, result.Denominator);
    }

    [Fact]
    public void Addition_DifferentDenominators_FindsCommonDenominator()
    {
        var a = new RationalFactor(1, 2); // 1/2
        var b = new RationalFactor(1, 3); // 1/3
        var result = a + b;
        
        // 1/2 + 1/3 = 3/6 + 2/6 = 5/6
        Assert.Equal(5, result.Numerator);
        Assert.Equal(6, result.Denominator);
    }

    [Fact]
    public void Addition_ResultSimplified()
    {
        var a = new RationalFactor(1, 4);
        var b = new RationalFactor(3, 4);
        var result = a + b;
        
        // 1/4 + 3/4 = 4/4 = 1/1
        Assert.Equal(1, result.Numerator);
        Assert.Equal(1, result.Denominator);
    }

    // ──────────────────────────────────────────────────────────
    // Multiplication
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void Multiplication_MultipliesNumeratorsAndDenominators()
    {
        var a = new RationalFactor(2, 3);
        var b = new RationalFactor(3, 4);
        var result = a * b;
        
        // 2/3 * 3/4 = 6/12 = 1/2
        Assert.Equal(1, result.Numerator);
        Assert.Equal(2, result.Denominator);
    }

    [Fact]
    public void Multiplication_ByInteger_MultipliesNumerator()
    {
        var a = new RationalFactor(3, 4);
        var result = a * 2;
        
        // 3/4 * 2 = 6/4 = 3/2
        Assert.Equal(3, result.Numerator);
        Assert.Equal(2, result.Denominator);
    }

    [Fact]
    public void Multiplication_ByZero_ResultsInZero()
    {
        var a = new RationalFactor(3, 4);
        var result = a * 0;
        
        Assert.Equal(0, result.Numerator);
        Assert.Equal(1, result.Denominator);
    }

    // ──────────────────────────────────────────────────────────
    // Division
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void Division_ByInteger_MultipliesDenominator()
    {
        var a = new RationalFactor(3, 4);
        var result = a / 2;
        
        // 3/4 / 2 = 3/8
        Assert.Equal(3, result.Numerator);
        Assert.Equal(8, result.Denominator);
    }

    [Fact]
    public void Division_ResultSimplified()
    {
        var a = new RationalFactor(6, 4);
        var result = a / 2;
        
        // 6/4 / 2 = 6/8 = 3/4
        Assert.Equal(3, result.Numerator);
        Assert.Equal(4, result.Denominator);
    }

    // ──────────────────────────────────────────────────────────
    // Conversion & Equality
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void ToDouble_ConvertsCorrectly()
    {
        var rf = new RationalFactor(3, 4);
        Assert.Equal(0.75, rf.ToDouble(), precision: 10);
    }

    [Fact]
    public void Equals_SameFraction_ReturnsTrue()
    {
        var a = new RationalFactor(3, 4);
        var b = new RationalFactor(6, 8); // Simplifies to 3/4
        
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentFraction_ReturnsFalse()
    {
        var a = new RationalFactor(3, 4);
        var b = new RationalFactor(2, 3);
        
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void GetHashCode_SameFraction_SameHash()
    {
        var a = new RationalFactor(3, 4);
        var b = new RationalFactor(6, 8);
        
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var rf = new RationalFactor(3, 4);
        Assert.Equal("3/4", rf.ToString());
    }
}

/// <summary>
/// Tests for Duration advanced methods to improve coverage.
/// </summary>
public class DurationAdvancedTests
{
    // ──────────────────────────────────────────────────────────
    // FromTicks
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void FromTicks_CreatesCorrectDuration()
    {
        var dur = Duration.FromTicks(480);
        Assert.Equal(480, dur.Ticks);
    }

    [Fact]
    public void FromTicks_NegativeTicks_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Duration.FromTicks(-1));
    }

    [Fact]
    public void FromTicks_ZeroTicks_Valid()
    {
        var dur = Duration.FromTicks(0);
        Assert.Equal(0, dur.Ticks);
    }

    // ──────────────────────────────────────────────────────────
    // FromBase Validation
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void FromBase_NegativeDots_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => 
            Duration.FromBase(BaseNoteValue.Quarter, dots: -1));
    }

    [Fact]
    public void FromBase_TooManyDots_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => 
            Duration.FromBase(BaseNoteValue.Quarter, dots: 4));
    }

    // ──────────────────────────────────────────────────────────
    // Duration Addition
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void Addition_CombinesDurations()
    {
        var quarter = DurationFactory.Quarter();
        var eighth = DurationFactory.Eighth();
        var result = quarter + eighth;
        
        Assert.Equal(Duration.TicksPerQuarter + Duration.TicksPerQuarter / 2, result.Ticks);
    }

    [Fact]
    public void Addition_ZeroDuration_ReturnsOriginal()
    {
        var quarter = DurationFactory.Quarter();
        var zero = Duration.FromTicks(0);
        var result = quarter + zero;
        
        Assert.Equal(quarter.Ticks, result.Ticks);
    }

    // ──────────────────────────────────────────────────────────
    // TryAsSimple
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void TryAsSimple_Quarter_ReturnsTrue()
    {
        var dur = DurationFactory.Quarter();
        var success = dur.TryAsSimple(out var baseValue, out var dots);
        
        Assert.True(success);
        Assert.Equal(BaseNoteValue.Quarter, baseValue);
        Assert.Equal(0, dots);
    }

    [Fact]
    public void TryAsSimple_DottedHalf_ReturnsTrue()
    {
        var dur = DurationFactory.Half(dots: 1);
        var success = dur.TryAsSimple(out var baseValue, out var dots);
        
        Assert.True(success);
        Assert.Equal(BaseNoteValue.Half, baseValue);
        Assert.Equal(1, dots);
    }

    [Fact]
    public void TryAsSimple_Triplet_ReturnsFalse()
    {
        var dur = DurationFactory.Tuplet(BaseNoteValue.Quarter, new Tuplet(3, 2));
        var success = dur.TryAsSimple(out _, out _);
        
        Assert.False(success);
    }

    [Fact]
    public void TryAsSimple_IrregularTicks_ReturnsFalse()
    {
        var dur = Duration.FromTicks(500); // Not a standard note value
        var success = dur.TryAsSimple(out _, out _);
        
        Assert.False(success);
    }

    // ──────────────────────────────────────────────────────────
    // Equality & ToString
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void Equals_SameDuration_ReturnsTrue()
    {
        var a = DurationFactory.Quarter();
        var b = DurationFactory.Quarter();
        
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentDuration_ReturnsFalse()
    {
        var a = DurationFactory.Quarter();
        var b = DurationFactory.Half();
        
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void GetHashCode_SameDuration_SameHash()
    {
        var a = DurationFactory.Quarter();
        var b = DurationFactory.Quarter();
        
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_IncludesFractionAndTicks()
    {
        var dur = DurationFactory.Quarter();
        var str = dur.ToString();
        
        Assert.Contains("1/4", str);
        Assert.Contains("480", str);
    }
}
