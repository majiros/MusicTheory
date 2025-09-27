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
}
