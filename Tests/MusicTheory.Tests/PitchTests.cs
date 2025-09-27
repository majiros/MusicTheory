using Xunit;
using MusicTheory.Theory.Pitch;
using MusicTheory.Theory.Interval;

namespace MusicTheory.Tests;

public class PitchTests
{
    [Fact]
    public void MidiRoundTrip()
    {
        var sp = new SpelledPitch(Letter.C, new Accidental(0));
        var p = new Pitch(sp, 4); // C4 -> 60
        int midi = PitchUtils.ToMidi(p);
        Assert.Equal(60, midi);
    }

    [Fact]
    public void TransposeInterval()
    {
        var p = new Pitch(new SpelledPitch(Letter.C, new Accidental(0)), 4);
    var m3 = new SemitoneInterval(3);
    var transposed = PitchUtils.Transpose(p, m3);
        Assert.Equal(63, PitchUtils.ToMidi(transposed)); // C4 60 + 3 = 63
    }
}
