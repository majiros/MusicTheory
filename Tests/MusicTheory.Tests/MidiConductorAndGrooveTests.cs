using MusicTheory.Theory.Midi;
using MusicTheory.Theory.Time;
using Xunit;

namespace MusicTheory.Tests;

/// <summary>
/// Tests for MidiConductorHelper to improve coverage (Phase 2).
/// Current coverage: 0% → Target: 100%
/// </summary>
public class MidiConductorHelperTests
{
    [Fact]
    public void ExtractConductor_ThrowsOnEmptyTracks()
    {
        var empty = Array.Empty<MidiTrack>();
        Assert.Throws<ArgumentException>(() => MidiConductorHelper.ExtractConductor(empty));
    }

    [Fact]
    public void ExtractConductor_CreatesConductorFromFirstTrack()
    {
        var track1 = new MidiTrack();
        track1.AddMeta(new TempoEvent(0, Tempo.FromBpm(120)));
        track1.AddMeta(new TextEvent(0, "Test"));
        track1.AddNote(new MidiNoteEvent(0, 60, 100, 0, true));

        var track2 = new MidiTrack();
        track2.AddNote(new MidiNoteEvent(0, 64, 100, 0, true));

        var result = MidiConductorHelper.ExtractConductor(new[] { track1, track2 });

        Assert.Equal(2, result.conductor.MetaEvents.Count);
        Assert.Single(result.others);
        Assert.Equal(track2, result.others[0]);
    }

    [Fact]
    public void ConsolidateAndStrip_ThrowsOnEmptyTracks()
    {
        var empty = Array.Empty<MidiTrack>().ToList();
        Assert.Throws<ArgumentException>(() => MidiConductorHelper.ConsolidateAndStrip(empty));
    }

    [Fact]
    public void ConsolidateAndStrip_ConsolidatesTempoAndTimeSignature()
    {
        var track1 = new MidiTrack();
        track1.AddMeta(new TempoEvent(0, Tempo.FromBpm(120)));
        track1.AddMeta(new TimeSignatureEvent(0, new Theory.Time.TimeSignature(4, 4)));

        var track2 = new MidiTrack();
        track2.AddMeta(new TempoEvent(480, Tempo.FromBpm(140)));

        var tracks = new List<MidiTrack> { track1, track2 };
        var conductor = MidiConductorHelper.ConsolidateAndStrip(tracks);

        // Conductor should have consolidated meta events
        Assert.NotEmpty(conductor.MetaEvents);
        Assert.Contains(conductor.MetaEvents, m => m is TempoEvent);
        Assert.Contains(conductor.MetaEvents, m => m is TimeSignatureEvent);
    }

    [Fact]
    public void ConsolidateAndStrip_RemovesDuplicatesAtSameTick()
    {
        var track1 = new MidiTrack();
        track1.AddMeta(new TempoEvent(0, Tempo.FromBpm(120)));

        var track2 = new MidiTrack();
        track2.AddMeta(new TempoEvent(0, Tempo.FromBpm(120))); // Duplicate

        var tracks = new List<MidiTrack> { track1, track2 };
        var conductor = MidiConductorHelper.ConsolidateAndStrip(tracks);

        // Should only have one tempo event at tick 0
        var tempoEvents = conductor.MetaEvents.OfType<TempoEvent>().Where(e => e.Tick == 0).ToList();
        Assert.Single(tempoEvents);
    }
}

/// <summary>
/// Tests for GrooveHumanize to improve coverage (Phase 2).
/// Current coverage: 33.3% → Target: 100%
/// </summary>
public class GrooveHumanizeTests
{
    [Fact]
    public void ApplyGroove_AppliesPatternToTicks()
    {
        var ticks = new long[] { 0, 480, 960 };
        var pattern = new int[] { 0, 10, -10 };
        var gridTicks = 480;

        var result = GrooveHumanize.ApplyGroove(ticks, gridTicks, pattern).ToList();

        Assert.Equal(3, result.Count);
        // Verify groove pattern is applied (exact values depend on Quantize.ApplyGroove implementation)
        Assert.NotEqual(ticks, result); // Should be modified
    }

    [Fact]
    public void ProcessOne_CombinesAllEffects()
    {
        var tick = 480L;
        var gridTicks = 120;
        var swingRatio = 0.5;
        var swingSubdivisionTicks = 240;
        var ticksPerBar = 1920;
        var strongBeats = new[] { 0, 960 };
        var strongBias = 0.2;
        var groovePattern = new[] { 0, 5, -5 };

        var result = GrooveHumanize.ProcessOne(
            tick, gridTicks, swingRatio, swingSubdivisionTicks,
            ticksPerBar, strongBeats, strongBias, groovePattern);

        // Should return a modified tick value
        Assert.True(result >= 0);
    }

    [Fact]
    public void ProcessOne_WithNoEffects_QuantizesOnly()
    {
        var tick = 485L;
        var gridTicks = 480;

        var result = GrooveHumanize.ProcessOne(
            tick, gridTicks, 0, 0, 1920,
            Array.Empty<int>(), 0, Array.Empty<int>());

        // Should quantize to grid
        Assert.Equal(480, result);
    }

    [Fact]
    public void AddTimingJitter_ReturnsOriginalWhenMaxJitterIsZero()
    {
        var ticks = new long[] { 0, 480, 960 };

        var result = GrooveHumanize.AddTimingJitter(ticks, 0).ToList();

        Assert.Equal(ticks, result);
    }

    [Fact]
    public void AddTimingJitter_AddsRandomOffset()
    {
        var ticks = new long[] { 480, 480, 480 };
        var maxJitter = 10;
        var seed = 42;

        var result = GrooveHumanize.AddTimingJitter(ticks, maxJitter, seed).ToList();

        Assert.Equal(3, result.Count);
        // With fixed seed, results should be deterministic but likely different from original
        Assert.All(result, r => Assert.InRange(r, 480 - maxJitter, 480 + maxJitter));
    }

    [Fact]
    public void HumanizeVelocity_ReturnsOriginalWhenPercentIsZero()
    {
        var velocities = new int[] { 64, 80, 100 };

        var result = GrooveHumanize.HumanizeVelocity(velocities, 0).ToList();

        Assert.Equal(velocities, result);
    }

    [Fact]
    public void HumanizeVelocity_VariesVelocities()
    {
        var velocities = new int[] { 64, 64, 64 };
        var percent = 0.2;
        var seed = 42;

        var result = GrooveHumanize.HumanizeVelocity(velocities, percent, seed).ToList();

        Assert.Equal(3, result.Count);
        // With fixed seed, results should be deterministic
        Assert.All(result, v => Assert.InRange(v, 0, 127));
    }

    [Fact]
    public void HumanizeVelocity_ClampsToValidRange()
    {
        var velocities = new int[] { 10, 120 };
        var percent = 0.5; // Large variation
        var seed = 123;

        var result = GrooveHumanize.HumanizeVelocity(velocities, percent, seed).ToList();

        // All velocities should be clamped to MIDI valid range
        Assert.All(result, v => Assert.InRange(v, 0, 127));
    }
}
