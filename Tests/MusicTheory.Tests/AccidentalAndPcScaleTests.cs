using MusicTheory.Theory.Pitch;
using MusicTheory.Theory.Scale;
using Xunit;

namespace MusicTheory.Tests;

/// <summary>
/// Tests for Accidental to improve coverage (Phase 1).
/// Current coverage: 4.8% → Target: 100%
/// </summary>
public class AccidentalTests
{
    [Fact]
    public void Accidental_Constructor_StoresSemitones()
    {
        var acc = new Accidental(2);
        Assert.Equal(2, acc.Semitones);
    }

    [Fact]
    public void Accidental_AccidentalValue_ClampsToValidRange()
    {
        Assert.Equal(-3, new Accidental(-5).AccidentalValue);
        Assert.Equal(-3, new Accidental(-3).AccidentalValue);
        Assert.Equal(0, new Accidental(0).AccidentalValue);
        Assert.Equal(3, new Accidental(3).AccidentalValue);
        Assert.Equal(3, new Accidental(5).AccidentalValue);
    }

    [Theory]
    [InlineData(-3, "♭♭♭")]
    [InlineData(-2, "♭♭")]
    [InlineData(-1, "♭")]
    [InlineData(0, "")]
    [InlineData(1, "♯")]
    [InlineData(2, "𝄪")]
    [InlineData(3, "♯𝄪")]
    public void Accidental_ToString_Unicode_ProducesCorrectSymbols(int semitones, string expected)
    {
        Accidental.Notation = AccidentalNotation.Unicode;
        var acc = new Accidental(semitones);
        Assert.Equal(expected, acc.ToString());
    }

    [Theory]
    [InlineData(-3, "bbb")]
    [InlineData(-2, "bb")]
    [InlineData(-1, "b")]
    [InlineData(0, "")]
    [InlineData(1, "#")]
    [InlineData(2, "##")]
    [InlineData(3, "###")]
    public void Accidental_ToString_ASCII_ProducesCorrectSymbols(int semitones, string expected)
    {
        Accidental.Notation = AccidentalNotation.ASCII;
        var acc = new Accidental(semitones);
        Assert.Equal(expected, acc.ToString());
    }

    [Theory]
    [InlineData(-3, "bbb")]
    [InlineData(-2, "bb")]
    [InlineData(-1, "b")]
    [InlineData(0, "")]
    [InlineData(1, "#")]
    [InlineData(2, "x")]
    [InlineData(3, "#x")]
    public void Accidental_ToString_ASCII_X_ProducesCorrectSymbols(int semitones, string expected)
    {
        Accidental.Notation = AccidentalNotation.ASCII_X;
        var acc = new Accidental(semitones);
        Assert.Equal(expected, acc.ToString());
    }

    [Fact]
    public void Accidental_Notation_CanBeChanged()
    {
        var acc = new Accidental(1);

        Accidental.Notation = AccidentalNotation.Unicode;
        Assert.Equal("♯", acc.ToString());

        Accidental.Notation = AccidentalNotation.ASCII;
        Assert.Equal("#", acc.ToString());

        Accidental.Notation = AccidentalNotation.ASCII_X;
        Assert.Equal("#", acc.ToString());

        // Reset to default
        Accidental.Notation = AccidentalNotation.Unicode;
    }

    [Fact]
    public void SpelledPitch_ToString_CombinesLetterAndAccidental()
    {
        Accidental.Notation = AccidentalNotation.Unicode;
        var pitch = new SpelledPitch(Letter.C, new Accidental(1));
        Assert.Equal("C♯", pitch.ToString());

        var flatPitch = new SpelledPitch(Letter.B, new Accidental(-1));
        Assert.Equal("B♭", flatPitch.ToString());
    }
}

/// <summary>
/// Tests for PcScale to improve coverage (Phase 1).
/// Current coverage: 21.4% → Target: 100%
/// </summary>
public class PcScaleTests
{
    [Fact]
    public void PcScale_Constructor_StoresNameAndDegrees()
    {
        var degrees = new[] { new PitchClass(0), new PitchClass(2), new PitchClass(4) };
        var scale = new PcScale("Test Scale", degrees);

        Assert.Equal("Test Scale", scale.Name);
        Assert.Equal(3, scale.Degrees.Length);
        Assert.Contains(scale.Degrees, d => d.Pc == 0);
    }

    [Fact]
    public void PcScale_Contains_DetectsPitchClass()
    {
        var scale = PcScaleLibrary.Major; // C major: 0,2,4,5,7,9,11

        Assert.True(scale.Contains(new PitchClass(0))); // C
        Assert.True(scale.Contains(new PitchClass(4))); // E
        Assert.False(scale.Contains(new PitchClass(1))); // C#
        Assert.False(scale.Contains(new PitchClass(3))); // Eb
    }

    [Fact]
    public void PcScale_ToString_ShowsNameAndDegrees()
    {
        var degrees = new[] { new PitchClass(0), new PitchClass(2) };
        var scale = new PcScale("Test", degrees);

        var str = scale.ToString();
        Assert.Contains("Test", str);
        Assert.Contains("0", str);
        Assert.Contains("2", str);
    }

    [Fact]
    public void PcScale_Transposed_ShiftsAllDegrees()
    {
        var cMajor = PcScaleLibrary.Major; // 0,2,4,5,7,9,11
        var dMajor = cMajor.Transposed(2); // Should be 2,4,6,7,9,11,1

        Assert.Contains("Root: 2", dMajor.Name);
        Assert.Contains(dMajor.Degrees, d => d.Pc == 2); // D
        Assert.Contains(dMajor.Degrees, d => d.Pc == 6); // F#
        Assert.Contains(dMajor.Degrees, d => d.Pc == 9); // A
    }

    [Fact]
    public void PcScale_GetJapaneseDegreeNames_ReturnsSevenNames()
    {
        var scale = PcScaleLibrary.Major;
        var names = scale.GetJapaneseDegreeNames();

        Assert.Equal(7, names.Length);
        Assert.Equal("主音", names[0]);
        Assert.Equal("導音", names[6]);
    }

    [Fact]
    public void PcScale_GetSpelledDegreesWithNames_GeneratesSpellings()
    {
        var scale = PcScaleLibrary.Major;
        var root = new SpelledPitch(Letter.C, new Accidental(0));

        var spelledDegrees = scale.GetSpelledDegreesWithNames(root).ToList();

        Assert.Equal(7, spelledDegrees.Count);
        Assert.Equal("主音", spelledDegrees[0].Item2);
    }

    [Fact]
    public void PcScale_GetSemitoneSet_ReturnsAllPitchClasses()
    {
        var major = PcScaleLibrary.Major;
        var semitones = major.GetSemitoneSet();

        Assert.Equal(7, semitones.Count);
        Assert.Contains(0, semitones); // C
        Assert.Contains(4, semitones); // E
        Assert.Contains(7, semitones); // G
    }

    [Fact]
    public void PcScale_IScale_ContainsSemitone_ChecksMembership()
    {
        IScale scale = PcScaleLibrary.Major;

        Assert.True(scale.ContainsSemitone(0)); // C
        Assert.True(scale.ContainsSemitone(12)); // C (octave equivalent)
        Assert.False(scale.ContainsSemitone(1)); // C#
    }
}
