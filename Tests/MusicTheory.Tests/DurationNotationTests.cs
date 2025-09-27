using Xunit;
using MusicTheory.Theory.Time;

namespace MusicTheory.Tests;

public class DurationNotationTests
{
    [Theory]
    [InlineData("Q", 480)]
    [InlineData("Q.", 480 + 240)]
    [InlineData("E(3:2)", 160)] // 8分三連の一要素
    [InlineData("1/4", 480)]
    [InlineData("1/8..", 480 * 7 / 8)] // 8分ダブルドット = 1/8*(2 - 1/4) = 7/8 of a quarter = 420 ticks
    public void Parse_Notation_To_Ticks(string text, int expectedTicks)
    {
        var ok = DurationNotation.TryParse(text, out var d);
        Assert.True(ok);
        Assert.Equal(expectedTicks, d.Ticks);
    }

    [Fact]
    public void Roundtrip_Abbrev_Dots_Tuplet()
    {
        var d = Duration.FromBase(BaseNoteValue.Eighth, dots: 1, tuplet: new Tuplet(3, 2)); // 付点8分の三連
        var s = DurationNotation.ToNotation(d);
        var ok = DurationNotation.TryParse(s, out var d2);
        Assert.True(ok);
        Assert.Equal(d.Ticks, d2.Ticks);
    }

    [Fact]
    public void ToNotation_Uses_Star_When_ExtendedTuplets()
    {
        var d = Duration.FromBase(BaseNoteValue.Eighth, dots: 0, tuplet: new Tuplet(5, 4)); // 8分5:4 連符
        var s1 = DurationNotation.ToNotation(d, extendedTuplets: true);
        Assert.Equal("E*5:4", s1);
        Assert.True(DurationNotation.TryParse(s1, out var d1));
        Assert.Equal(d.Ticks, d1.Ticks);

        var s2 = DurationNotation.ToNotation(d, extendedTuplets: false);
        Assert.Equal("E(5:4)", s2);
        Assert.True(DurationNotation.TryParse(s2, out var d2));
        Assert.Equal(d.Ticks, d2.Ticks);
    }
}
