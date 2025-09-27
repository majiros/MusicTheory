using Xunit;
using MusicTheory.Theory.Scale;
using MusicTheory.Theory.Pitch;

namespace MusicTheory.Tests;

public class ScaleTests
{
    [Fact]
    public void MajorScaleSemitones()
    {
        var major = PcScaleLibrary.Major;
        var set = major.GetSemitoneSet();
        Assert.Equal(new[]{0,2,4,5,7,9,11}, set.OrderBy(x=>x));
    }

    [Fact]
    public void LydianDominantContainsSharp11AndFlat7()
    {
        var lydDom = ExtendedScales.LydianDominant;
        var set = lydDom.GetSemitoneSet();
        Assert.Contains(6, set); // #11
        Assert.Contains(10, set); // b7
    }

    [Fact]
    public void BebopDominantIsEightNotes()
    {
        var bebopDom = ExtendedScales.BebopDominant;
        Assert.Equal(8, bebopDom.GetSemitoneSet().Count);
    }
}
