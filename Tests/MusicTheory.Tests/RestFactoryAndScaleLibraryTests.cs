using MusicTheory.Theory.Pitch;
using MusicTheory.Theory.Scale;
using MusicTheory.Theory.Time;
using Xunit;

namespace MusicTheory.Tests;

/// <summary>
/// Tests for RestFactory utility methods to improve coverage.
/// </summary>
public class RestFactoryTests
{
    [Fact]
    public void RestFactory_DoubleWhole_CreatesRest()
    {
        var rest = RestFactory.DoubleWhole();
        Assert.Equal(Duration.TicksPerQuarter * 8, rest.Duration.Ticks);
    }

    [Fact]
    public void RestFactory_Whole_CreatesRest()
    {
        var rest = RestFactory.Whole();
        Assert.Equal(Duration.TicksPerQuarter * 4, rest.Duration.Ticks);
    }

    [Fact]
    public void RestFactory_Half_CreatesRest()
    {
        var rest = RestFactory.Half();
        Assert.Equal(Duration.TicksPerQuarter * 2, rest.Duration.Ticks);
    }

    [Fact]
    public void RestFactory_Quarter_CreatesRest()
    {
        var rest = RestFactory.Quarter();
        Assert.Equal(Duration.TicksPerQuarter, rest.Duration.Ticks);
    }

    [Fact]
    public void RestFactory_Eighth_CreatesRest()
    {
        var rest = RestFactory.Eighth();
        Assert.Equal(Duration.TicksPerQuarter / 2, rest.Duration.Ticks);
    }

    [Fact]
    public void RestFactory_Sixteenth_CreatesRest()
    {
        var rest = RestFactory.Sixteenth();
        Assert.Equal(Duration.TicksPerQuarter / 4, rest.Duration.Ticks);
    }

    [Fact]
    public void RestFactory_ThirtySecond_CreatesRest()
    {
        var rest = RestFactory.ThirtySecond();
        Assert.Equal(Duration.TicksPerQuarter / 8, rest.Duration.Ticks);
    }

    [Fact]
    public void RestFactory_SixtyFourth_CreatesRest()
    {
        var rest = RestFactory.SixtyFourth();
        Assert.Equal(Duration.TicksPerQuarter / 16, rest.Duration.Ticks);
    }

    [Fact]
    public void RestFactory_OneHundredTwentyEighth_CreatesRest()
    {
        var rest = RestFactory.OneHundredTwentyEighth();
        Assert.Equal(Duration.TicksPerQuarter / 32, rest.Duration.Ticks);
    }

    [Fact]
    public void RestFactory_WithDots_CreatesRest()
    {
        var rest = RestFactory.Quarter(dots: 1);
        // Dotted quarter = quarter + eighth
        Assert.Equal(Duration.TicksPerQuarter + Duration.TicksPerQuarter / 2, rest.Duration.Ticks);
    }

    [Fact]
    public void RestFactory_FromTicks_CreatesRest()
    {
        var rest = RestFactory.FromTicks(1000);
        Assert.Equal(1000, rest.Duration.Ticks);
    }
}

/// <summary>
/// Tests for PcScaleLibrary to improve coverage.
/// </summary>
public class PcScaleLibraryTests
{
    [Fact]
    public void PcScaleLibrary_Major_HasCorrectDegrees()
    {
        var major = PcScaleLibrary.Major;
        Assert.Equal("Major", major.Name);
        Assert.Equal(7, major.Degrees.Length);
        Assert.Contains(major.Degrees, d => d.Pc == 0); // C
        Assert.Contains(major.Degrees, d => d.Pc == 4); // E
        Assert.Contains(major.Degrees, d => d.Pc == 7); // G
    }

    [Fact]
    public void PcScaleLibrary_Minor_HasCorrectDegrees()
    {
        var minor = PcScaleLibrary.Minor;
        Assert.Equal("Minor", minor.Name);
        Assert.Equal(7, minor.Degrees.Length);
        Assert.Contains(minor.Degrees, d => d.Pc == 9); // A
        Assert.Contains(minor.Degrees, d => d.Pc == 0); // C
        Assert.Contains(minor.Degrees, d => d.Pc == 4); // E
    }

    [Fact]
    public void PcScaleLibrary_ChurchModes_HasSevenModes()
    {
        var modes = PcScaleLibrary.ChurchModes;
        Assert.Equal(7, modes.Length);

        var names = modes.Select(m => m.Name).ToList();
        Assert.Contains("Ionian", names);
        Assert.Contains("Dorian", names);
        Assert.Contains("Phrygian", names);
        Assert.Contains("Lydian", names);
        Assert.Contains("Mixolydian", names);
        Assert.Contains("Aeolian", names);
        Assert.Contains("Locrian", names);
    }

    [Fact]
    public void PcScaleLibrary_Chromatic_HasTwelveDegrees()
    {
        var chromatic = PcScaleLibrary.Chromatic;
        Assert.Equal("Chromatic", chromatic.Name);
        Assert.Equal(12, chromatic.Degrees.Length);
    }

    [Fact]
    public void PcScaleLibrary_WholeTone_HasSixDegrees()
    {
        var wholeTone = PcScaleLibrary.WholeTone;
        Assert.Equal("Whole Tone", wholeTone.Name);
        Assert.Equal(6, wholeTone.Degrees.Length);

        // All degrees should be 2 semitones apart
        for (int i = 0; i < wholeTone.Degrees.Length; i++)
        {
            Assert.Equal(i * 2, wholeTone.Degrees[i].Pc);
        }
    }

    [Fact]
    public void PcScaleLibrary_PentatonicMajor_HasFiveDegrees()
    {
        var pentatonic = PcScaleLibrary.PentatonicMajor;
        Assert.Equal("Pentatonic Major", pentatonic.Name);
        Assert.Equal(5, pentatonic.Degrees.Length);
    }

    [Fact]
    public void PcScaleLibrary_PentatonicMinor_HasFiveDegrees()
    {
        var pentatonic = PcScaleLibrary.PentatonicMinor;
        Assert.Equal("Pentatonic Minor", pentatonic.Name);
        Assert.Equal(5, pentatonic.Degrees.Length);
    }

    [Fact]
    public void PcScaleLibrary_Blues_HasSixDegrees()
    {
        var blues = PcScaleLibrary.Blues;
        Assert.Equal("Blues", blues.Name);
        Assert.Equal(6, blues.Degrees.Length);
    }

    [Fact]
    public void PcScaleLibrary_FindScalesContaining_ReturnsScales()
    {
        var pc = new PitchClass(0); // C
        var scales = PcScaleLibrary.FindScalesContaining(pc).ToList();

        Assert.NotEmpty(scales);

        // C should be in Major scale
        Assert.Contains(scales, s => s.Name == "Major");
    }

    [Fact]
    public void PcScaleLibrary_JapaneseDegreeNames_HasSevenNames()
    {
        var names = PcScaleLibrary.JapaneseDegreeNames;
        Assert.Equal(7, names.Length);
        Assert.Equal("主音", names[0]);
        Assert.Equal("導音", names[6]);
    }
}
