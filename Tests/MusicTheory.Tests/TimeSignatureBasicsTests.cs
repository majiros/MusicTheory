using MusicTheory.Theory.Time;
using Xunit;

namespace MusicTheory.Tests;

public class TimeSignatureBasicsTests
{
    [Fact]
    public void Ticks_Computation_And_ToString()
    {
    var ts = new MusicTheory.Theory.Time.TimeSignature(4, 4);
        Assert.Equal("4/4", ts.ToString());
        Assert.True(ts.TicksPerBeat > 0);
        Assert.True(ts.TicksPerBar > ts.TicksPerBeat);

        var pos = new TimePosition(ts, ts.TicksPerBar + ts.TicksPerBeat + 10);
        Assert.Equal(1, pos.BarBeatTick.Bar);
        Assert.Equal(1, pos.BarBeatTick.Beat);
        Assert.Equal(10, pos.BarBeatTick.TickWithinBeat);

    var pos2 = TimePosition.FromBarBeatTick(ts, new BarBeatTick(2, 0, 0));
        Assert.Equal(2 * ts.TicksPerBar, pos2.AbsoluteTicks);
    }

    [Fact]
    public void TimeSignatureMap_Basic_Change()
    {
    var map = new TimeSignatureMap(new MusicTheory.Theory.Time.TimeSignature(4, 4));
    map.AddChange(1000, new MusicTheory.Theory.Time.TimeSignature(3, 8));
        Assert.Equal("4/4", map.GetAt(999).ToString());
        Assert.Equal("3/8", map.GetAt(1000).ToString());
        var p = map.ToPosition(1001);
        Assert.Equal("3/8", p.Signature.ToString());
    }
}
