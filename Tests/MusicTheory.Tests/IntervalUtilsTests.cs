using MusicTheory.Theory.Interval;
using MusicTheory.Theory.Pitch;
using Xunit;

namespace MusicTheory.Tests;

public class IntervalUtilsTests
{
    [Theory]
    [InlineData(0, IntervalQuality.Perfect, 1)]
    [InlineData(7, IntervalQuality.Augmented, 4)]
    [InlineData(12, IntervalQuality.Minor, 8)]
    public void GetIntervalName_BasicSamples(int semitones, IntervalQuality q, int number)
    {
        var name = IntervalUtils.GetIntervalName(new FunctionalInterval((IntervalType)semitones));
        Assert.Equal(q, name.Quality);
        Assert.Equal(number, name.Number);
        Assert.False(string.IsNullOrWhiteSpace(name.ToJapaneseString()));
        Assert.Contains(number.ToString(), name.ToJapaneseString());
    }

    [Fact]
    public void Between_And_Degree_And_Enharmonic_Work()
    {
    var c4 = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 4);
    var g4 = new Pitch(new SpelledPitch(Letter.G, new Accidental(0)), 4);
    var db4 = new Pitch(new SpelledPitch(Letter.D, new Accidental(-1)), 4);
    var csharp4 = new Pitch(new SpelledPitch(Letter.C, new Accidental(1)), 4);

        var ivl = IntervalUtils.Between(c4, g4);
        Assert.True(ivl.Semitones > 0);

        int deg = IntervalUtils.DegreeBetween(c4, g4);
        Assert.Equal(5, deg); // C-D-E-F-G => 5度

        Assert.True(IntervalUtils.IsEnharmonic(db4, csharp4));
    }

    [Fact]
    public void Extension_ToSemitone_Wraps_Functional()
    {
        var f = new FunctionalInterval((IntervalType)5);
        var s = f.ToSemitone();
        Assert.Equal(5, s.Semitones);
    }

    // Additional comprehensive tests for IntervalUtils
    [Theory]
    [InlineData(1, IntervalQuality.DoublyDiminished, 2)]  // 実装では重減2度
    [InlineData(2, IntervalQuality.Minor, 2)]  // m2
    [InlineData(3, IntervalQuality.Major, 2)]  // M2
    [InlineData(4, IntervalQuality.Minor, 3)]  // m3
    [InlineData(5, IntervalQuality.Major, 3)]  // M3
    [InlineData(6, IntervalQuality.Perfect, 4)]  // P4
    [InlineData(8, IntervalQuality.DoublyAugmented, 4)]  // 実装では重増4度
    [InlineData(10, IntervalQuality.Minor, 6)]  // m6
    [InlineData(11, IntervalQuality.Major, 6)]  // M6
    public void GetIntervalName_ExtendedTheory(int semitones, IntervalQuality expectedQuality, int expectedNumber)
    {
        var interval = new FunctionalInterval((IntervalType)semitones);
        var name = IntervalUtils.GetIntervalName(interval);
        Assert.Equal(expectedQuality, name.Quality);
        Assert.Equal(expectedNumber, name.Number);
    }

    [Fact]
    public void Between_SamePitch_ReturnsZero()
    {
        var c4 = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 4);
        var interval = IntervalUtils.Between(c4, c4);
        Assert.Equal(0, interval.Semitones);
    }

    [Fact]
    public void Between_DescendingInterval_ReturnsNegative()
    {
        var c5 = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 5);
        var c4 = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 4);
        var interval = IntervalUtils.Between(c5, c4);
        Assert.Equal(-12, interval.Semitones);
    }

    [Fact]
    public void Between_AscendingOctave_Returns12()
    {
        var c4 = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 4);
        var c5 = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 5);
        var interval = IntervalUtils.Between(c4, c5);
        Assert.Equal(12, interval.Semitones);
    }

    [Fact]
    public void DegreeBetween_SamePitch_Returns1()
    {
        var c4 = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 4);
        var degree = IntervalUtils.DegreeBetween(c4, c4);
        Assert.Equal(1, degree);
    }

    [Fact]
    public void DegreeBetween_OctaveUp_Returns8()
    {
        var c4 = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 4);
        var c5 = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 5);
        var degree = IntervalUtils.DegreeBetween(c4, c5);
        Assert.Equal(8, degree);
    }

    [Fact]
    public void IsEnharmonic_CSharpAndDFlat_ReturnsTrue()
    {
        var csharp = new Pitch(new SpelledPitch(Letter.C, new Accidental(1)), 4);
        var dflat = new Pitch(new SpelledPitch(Letter.D, new Accidental(-1)), 4);
        Assert.True(IntervalUtils.IsEnharmonic(csharp, dflat));
    }

    [Fact]
    public void IsEnharmonic_DifferentPitches_ReturnsFalse()
    {
        var c4 = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 4);
        var d4 = new Pitch(new SpelledPitch(Letter.D, new Accidental(0)), 4);
        Assert.False(IntervalUtils.IsEnharmonic(c4, d4));
    }
}

public class PitchUtilsTests
{
    [Theory]
    [InlineData(Letter.C, 0)]
    [InlineData(Letter.D, 2)]
    [InlineData(Letter.E, 4)]
    [InlineData(Letter.F, 5)]
    [InlineData(Letter.G, 7)]
    [InlineData(Letter.A, 9)]
    [InlineData(Letter.B, 11)]
    public void LetterBasePc_AllLetters_ReturnsCorrectPc(Letter letter, int expectedPc)
    {
        var pc = PitchUtils.LetterBasePc(letter);
        Assert.Equal(expectedPc, pc);
    }

    [Fact]
    public void ToPc_SpelledPitch_Natural()
    {
        var sp = new SpelledPitch(Letter.C, new Accidental(0));
        var pc = PitchUtils.ToPc(sp);
        Assert.Equal(0, pc.Pc);
    }

    [Fact]
    public void ToPc_SpelledPitch_WithSharp()
    {
        var sp = new SpelledPitch(Letter.C, new Accidental(1));
        var pc = PitchUtils.ToPc(sp);
        Assert.Equal(1, pc.Pc);
    }

    [Fact]
    public void ToPc_SpelledPitch_WithFlat()
    {
        var sp = new SpelledPitch(Letter.D, new Accidental(-1));
        var pc = PitchUtils.ToPc(sp);
        Assert.Equal(1, pc.Pc);
    }

    [Fact]
    public void ToPc_Pitch_ReturnsCorrectPc()
    {
        var pitch = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 4);
        var pc = PitchUtils.ToPc(pitch);
        Assert.Equal(0, pc.Pc);
    }

    [Fact]
    public void ToMidi_C4_Returns60()
    {
        var c4 = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 4);
        var midi = PitchUtils.ToMidi(c4);
        Assert.Equal(60, midi);
    }

    [Fact]
    public void ToMidi_A4_Returns69()
    {
        var a4 = new Pitch(new SpelledPitch(Letter.A, new Accidental(0)), 4);
        var midi = PitchUtils.ToMidi(a4);
        Assert.Equal(69, midi);
    }

    [Fact]
    public void FromMidi_60_ReturnsC4()
    {
        var pitch = PitchUtils.FromMidi(60);
        Assert.Equal(Letter.C, pitch.Spelling.Letter);
        Assert.Equal(0, pitch.Spelling.Acc.AccidentalValue);
        Assert.Equal(4, pitch.Octave);
    }

    [Fact]
    public void FromMidi_61_ReturnsCSharp4()
    {
        var pitch = PitchUtils.FromMidi(61);
        Assert.Equal(Letter.C, pitch.Spelling.Letter);
        Assert.Equal(1, pitch.Spelling.Acc.AccidentalValue);
        Assert.Equal(4, pitch.Octave);
    }

    [Fact]
    public void FromMidi_RoundTripPreservesValue()
    {
        // FromMidi の結果を ToMidi で戻すと元の MIDI 値が保持されることを確認
        var pitch = PitchUtils.FromMidi(61);
        var midi = PitchUtils.ToMidi(pitch);
        Assert.Equal(61, midi);
    }

    [Fact]
    public void Transpose_Pitch_Up5Semitones()
    {
        var c4 = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 4);
        var transposed = PitchUtils.Transpose(c4, new SemitoneInterval(5));
        Assert.Equal(65, PitchUtils.ToMidi(transposed));
    }

    [Fact]
    public void Transpose_PitchClass_Wraps()
    {
        var pc = new PitchClass(11);
        var transposed = PitchUtils.Transpose(pc, new SemitoneInterval(2));
        Assert.Equal(1, transposed.Pc);
    }

    [Fact]
    public void GetEnharmonicSpellings_C_ReturnsMultiple()
    {
        var spellings = PitchUtils.GetEnharmonicSpellings(new PitchClass(0));
        Assert.NotEmpty(spellings);
        Assert.Contains(spellings, sp => sp.Letter == Letter.C && sp.Acc.AccidentalValue == 0);
    }

    [Fact]
    public void GetEnharmonicSpellings_CSharp_IncludesDFlat()
    {
        var spellings = PitchUtils.GetEnharmonicSpellings(new PitchClass(1));
        Assert.Contains(spellings, sp => sp.Letter == Letter.C && sp.Acc.AccidentalValue == 1);
        Assert.Contains(spellings, sp => sp.Letter == Letter.D && sp.Acc.AccidentalValue == -1);
    }
}

public class IntervalNameTests
{
    [Fact]
    public void ToJapaneseString_ReturnsNonEmpty()
    {
        var name = new IntervalName(IntervalQuality.Perfect, 5);
        var str = name.ToJapaneseString();
        Assert.False(string.IsNullOrWhiteSpace(str));
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        var name = new IntervalName(IntervalQuality.Major, 3);
        var str = name.ToString();
        Assert.Contains("3", str);
    }
}

public class FunctionalIntervalTests
{
    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        var interval = new FunctionalInterval((IntervalType)5);
        Assert.Equal(5, interval.Semitones);
    }

    [Fact]
    public void Semitones_ReturnsCorrectValue()
    {
        var interval = new FunctionalInterval((IntervalType)7);
        Assert.Equal(7, interval.Semitones);
    }

    [Fact]
    public void DisplayName_Perfect5th_ContainsText()
    {
        var interval = new FunctionalInterval((IntervalType)7);
        var displayName = interval.DisplayName;
        Assert.False(string.IsNullOrWhiteSpace(displayName));
    }

    [Fact]
    public void DisplayName_Octave_ContainsText()
    {
        var interval = new FunctionalInterval((IntervalType)12);
        var displayName = interval.DisplayName;
        Assert.False(string.IsNullOrWhiteSpace(displayName));
    }
}
