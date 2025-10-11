using Xunit;
using MusicTheory.Theory.Time;
using System;

namespace MusicTheory.Tests;

/// <summary>
/// Tests for TempoMap to improve coverage.
/// </summary>
public class TempoMapTests
{
    [Fact]
    public void Constructor_InitialTempo_SetsAtTick0()
    {
        var tempo = new Tempo(500000); // 120 BPM
        var map = new TempoMap(tempo);
        
        Assert.Equal(tempo, map.GetAt(0));
        Assert.Single(map.Entries);
    }

    [Fact]
    public void AddChange_ValidTick_AddsTempo()
    {
        var map = new TempoMap(new Tempo(500000));
        var newTempo = new Tempo(400000); // 150 BPM
        
        map.AddChange(1920, newTempo);
        
        Assert.Equal(2, map.Entries.Count);
        Assert.Equal(newTempo, map.GetAt(1920));
    }

    [Fact]
    public void AddChange_NegativeTick_Throws()
    {
        var map = new TempoMap(new Tempo(500000));
        
        Assert.Throws<ArgumentOutOfRangeException>(() => map.AddChange(-1, new Tempo(400000)));
    }

    [Fact]
    public void AddChange_SameTick_Replaces()
    {
        var map = new TempoMap(new Tempo(500000));
        map.AddChange(1920, new Tempo(400000));
        map.AddChange(1920, new Tempo(300000)); // Replace
        
        Assert.Equal(2, map.Entries.Count);
        Assert.Equal(new Tempo(300000), map.GetAt(1920));
    }

    [Fact]
    public void GetAt_BeforeFirstChange_ReturnsInitial()
    {
        var initial = new Tempo(500000);
        var map = new TempoMap(initial);
        map.AddChange(1920, new Tempo(400000));
        
        Assert.Equal(initial, map.GetAt(0));
        Assert.Equal(initial, map.GetAt(1919));
    }

    [Fact]
    public void GetAt_AfterChange_ReturnsNewTempo()
    {
        var map = new TempoMap(new Tempo(500000));
        var newTempo = new Tempo(400000);
        map.AddChange(1920, newTempo);
        
        Assert.Equal(newTempo, map.GetAt(1920));
        Assert.Equal(newTempo, map.GetAt(3000));
    }

    [Fact]
    public void GetAt_NegativeTick_Throws()
    {
        var map = new TempoMap(new Tempo(500000));
        
        Assert.Throws<ArgumentOutOfRangeException>(() => map.GetAt(-1));
    }

    [Fact]
    public void GetAt_MultipleChanges_ReturnsCorrectTempo()
    {
        var t1 = new Tempo(500000); // 120 BPM
        var t2 = new Tempo(400000); // 150 BPM
        var t3 = new Tempo(300000); // 200 BPM
        var map = new TempoMap(t1);
        map.AddChange(1920, t2);
        map.AddChange(3840, t3);
        
        Assert.Equal(t1, map.GetAt(1000));
        Assert.Equal(t2, map.GetAt(2500));
        Assert.Equal(t3, map.GetAt(4000));
    }

    [Fact]
    public void TickToMicroseconds_SingleTempo_Calculates()
    {
        var map = new TempoMap(new Tempo(500000)); // 120 BPM, 500000 µs/quarter
        
        // 1 quarter = 480 ticks = 500000 µs
        long us = map.TickToMicroseconds(480);
        Assert.Equal(500000, us);
    }

    [Fact]
    public void TickToMicroseconds_HalfQuarter_Calculates()
    {
        var map = new TempoMap(new Tempo(500000));
        
        // 0.5 quarter = 240 ticks = 250000 µs
        long us = map.TickToMicroseconds(240);
        Assert.Equal(250000, us);
    }

    [Fact]
    public void TickToMicroseconds_MultipleTempos_Accumulates()
    {
        var map = new TempoMap(new Tempo(500000)); // 120 BPM at tick 0
        map.AddChange(1920, new Tempo(400000)); // 150 BPM at tick 1920 (4 beats)
        
        // Time at 1920: 4 quarters at 500000 µs/quarter = 2000000 µs
        long us1920 = map.TickToMicroseconds(1920);
        Assert.Equal(2000000, us1920);
        
        // Time at 2400 (1920 + 480): 1 more quarter at 400000 µs/quarter
        long us2400 = map.TickToMicroseconds(2400);
        Assert.Equal(2400000, us2400); // 2000000 + 400000
    }

    [Fact]
    public void TickToMicroseconds_NegativeTick_Throws()
    {
        var map = new TempoMap(new Tempo(500000));
        
        Assert.Throws<ArgumentOutOfRangeException>(() => map.TickToMicroseconds(-1));
    }

    [Fact]
    public void TickToTimeSpan_Converts()
    {
        var map = new TempoMap(new Tempo(500000));
        
        // 480 ticks = 500000 µs = 500 ms
        var span = map.TickToTimeSpan(480);
        Assert.Equal(500, span.TotalMilliseconds);
    }

    [Fact]
    public void MicrosecondsToTick_SingleTempo_Calculates()
    {
        var map = new TempoMap(new Tempo(500000)); // 500000 µs/quarter
        
        // 500000 µs = 480 ticks
        long ticks = map.MicrosecondsToTick(500000);
        Assert.Equal(480, ticks);
    }

    [Fact]
    public void MicrosecondsToTick_HalfQuarter_Calculates()
    {
        var map = new TempoMap(new Tempo(500000));
        
        // 250000 µs = 240 ticks
        long ticks = map.MicrosecondsToTick(250000);
        Assert.Equal(240, ticks);
    }

    [Fact]
    public void MicrosecondsToTick_MultipleTempos_Converts()
    {
        var map = new TempoMap(new Tempo(500000)); // 120 BPM
        map.AddChange(1920, new Tempo(400000)); // 150 BPM
        
        // 2400000 µs: 2000000 (4 quarters) + 400000 (1 quarter) = tick 2400
        long ticks = map.MicrosecondsToTick(2400000);
        Assert.Equal(2400, ticks);
    }

    [Fact]
    public void MicrosecondsToTick_NegativeMicroseconds_Throws()
    {
        var map = new TempoMap(new Tempo(500000));
        
        Assert.Throws<ArgumentOutOfRangeException>(() => map.MicrosecondsToTick(-1));
    }

    [Fact]
    public void RoundTrip_TickToMicroseconds_ToTick_Consistent()
    {
        var map = new TempoMap(new Tempo(500000));
        long original = 960;
        
        long us = map.TickToMicroseconds(original);
        long converted = map.MicrosecondsToTick(us);
        
        Assert.Equal(original, converted);
    }

    [Fact]
    public void RoundTrip_MultipleTempos_Consistent()
    {
        var map = new TempoMap(new Tempo(500000));
        map.AddChange(1920, new Tempo(400000));
        long original = 2400;
        
        long us = map.TickToMicroseconds(original);
        long converted = map.MicrosecondsToTick(us);
        
        Assert.Equal(original, converted);
    }

    [Fact]
    public void Entries_ReturnsReadOnlyList()
    {
        var map = new TempoMap(new Tempo(500000));
        map.AddChange(1920, new Tempo(400000));
        
        var entries = map.Entries;
        Assert.Equal(2, entries.Count);
        Assert.Equal(0, entries[0].tick);
        Assert.Equal(1920, entries[1].tick);
    }
}
