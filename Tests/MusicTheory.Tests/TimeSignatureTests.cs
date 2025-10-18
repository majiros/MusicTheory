using Xunit;
using MusicTheory.Theory.Time;
using System.Linq;
using TimeSignature = MusicTheory.Theory.Time.TimeSignature;

namespace MusicTheory.Tests;

/// <summary>
/// Tests for TimeSignature to improve coverage.
/// </summary>
public class TimeSignatureTests
{
    // ──────────────────────────────────────────────────────────
    // Constructor & Validation
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ValidTimeSignature_CreatesCorrectly()
    {
        var ts = new TimeSignature(4, 4);
        Assert.Equal(4, ts.Numerator);
        Assert.Equal(4, ts.Denominator);
    }

    [Fact]
    public void Constructor_ZeroNumerator_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TimeSignature(0, 4));
    }

    [Fact]
    public void Constructor_NegativeNumerator_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new TimeSignature(-1, 4));
    }

    [Fact]
    public void Constructor_ZeroDenominator_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new TimeSignature(4, 0));
    }

    [Fact]
    public void Constructor_NegativeDenominator_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new TimeSignature(4, -4));
    }

    [Fact]
    public void Constructor_NonPowerOfTwoDenominator_ThrowsArgumentException()
    {
        // 3, 5, 6, 7, 9 are not powers of 2
        Assert.Throws<ArgumentException>(() => new TimeSignature(4, 3));
        Assert.Throws<ArgumentException>(() => new TimeSignature(4, 5));
        Assert.Throws<ArgumentException>(() => new TimeSignature(4, 6));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(4)]
    [InlineData(8)]
    [InlineData(16)]
    [InlineData(32)]
    public void Constructor_PowerOfTwoDenominator_Valid(int denominator)
    {
        var ts = new TimeSignature(4, denominator);
        Assert.Equal(denominator, ts.Denominator);
    }

    // ──────────────────────────────────────────────────────────
    // Ticks Calculation
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void TicksPerBeat_FourFour_QuarterNoteTicks()
    {
        var ts = new TimeSignature(4, 4);
        Assert.Equal(Duration.TicksPerQuarter, ts.TicksPerBeat);
    }

    [Fact]
    public void TicksPerBeat_ThreeEight_EighthNoteTicks()
    {
        var ts = new TimeSignature(3, 8);
        Assert.Equal(Duration.TicksPerQuarter / 2, ts.TicksPerBeat);
    }

    [Fact]
    public void TicksPerBeat_SixEight_EighthNoteTicks()
    {
        var ts = new TimeSignature(6, 8);
        Assert.Equal(Duration.TicksPerQuarter / 2, ts.TicksPerBeat);
    }

    [Fact]
    public void TicksPerBar_FourFour_WholeNoteTicks()
    {
        var ts = new TimeSignature(4, 4);
        Assert.Equal(Duration.TicksPerQuarter * 4, ts.TicksPerBar);
    }

    [Fact]
    public void TicksPerBar_ThreeFour_DottedHalfTicks()
    {
        var ts = new TimeSignature(3, 4);
        Assert.Equal(Duration.TicksPerQuarter * 3, ts.TicksPerBar);
    }

    [Fact]
    public void TicksPerBar_SixEight_DottedQuarterTicks()
    {
        var ts = new TimeSignature(6, 8);
        Assert.Equal(Duration.TicksPerQuarter * 3, ts.TicksPerBar);
    }

    // ──────────────────────────────────────────────────────────
    // Equality & ToString
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void Equals_SameTimeSignature_ReturnsTrue()
    {
        var a = new TimeSignature(4, 4);
        var b = new TimeSignature(4, 4);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentNumerator_ReturnsFalse()
    {
        var a = new TimeSignature(4, 4);
        var b = new TimeSignature(3, 4);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentDenominator_ReturnsFalse()
    {
        var a = new TimeSignature(4, 4);
        var b = new TimeSignature(4, 8);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void GetHashCode_SameTimeSignature_SameHash()
    {
        var a = new TimeSignature(4, 4);
        var b = new TimeSignature(4, 4);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var ts = new TimeSignature(3, 8);
        Assert.Equal("3/8", ts.ToString());
    }
}

/// <summary>
/// Tests for BarBeatTick to improve coverage.
/// </summary>
public class BarBeatTickTests
{
    [Fact]
    public void Constructor_ValidValues_CreatesCorrectly()
    {
        var bbt = new BarBeatTick(2, 3, 120);
        Assert.Equal(2, bbt.Bar);
        Assert.Equal(3, bbt.Beat);
        Assert.Equal(120, bbt.TickWithinBeat);
    }

    [Fact]
    public void Constructor_ZeroValues_Valid()
    {
        var bbt = new BarBeatTick(0, 0, 0);
        Assert.Equal(0, bbt.Bar);
        Assert.Equal(0, bbt.Beat);
        Assert.Equal(0, bbt.TickWithinBeat);
    }

    [Fact]
    public void Constructor_NegativeBar_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BarBeatTick(-1, 0, 0));
    }

    [Fact]
    public void Constructor_NegativeBeat_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BarBeatTick(0, -1, 0));
    }

    [Fact]
    public void Constructor_NegativeTick_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new BarBeatTick(0, 0, -1));
    }

    [Fact]
    public void Equals_SameBarBeatTick_ReturnsTrue()
    {
        var a = new BarBeatTick(1, 2, 100);
        var b = new BarBeatTick(1, 2, 100);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentBar_ReturnsFalse()
    {
        var a = new BarBeatTick(1, 2, 100);
        var b = new BarBeatTick(2, 2, 100);
        Assert.False(a.Equals(b));
    }

    [Fact]
    public void GetHashCode_SameBarBeatTick_SameHash()
    {
        var a = new BarBeatTick(1, 2, 100);
        var b = new BarBeatTick(1, 2, 100);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var bbt = new BarBeatTick(2, 3, 120);
        Assert.Equal("2:3+120", bbt.ToString());
    }
}

/// <summary>
/// Tests for TimePosition to improve coverage.
/// </summary>
public class TimePositionTests
{
    [Fact]
    public void Constructor_ZeroTicks_FirstBarFirstBeat()
    {
        var ts = new TimeSignature(4, 4);
        var pos = new TimePosition(ts, 0);

        Assert.Equal(0, pos.AbsoluteTicks);
        Assert.Equal(0, pos.BarBeatTick.Bar);
        Assert.Equal(0, pos.BarBeatTick.Beat);
        Assert.Equal(0, pos.BarBeatTick.TickWithinBeat);
    }

    [Fact]
    public void Constructor_OneBar_SecondBar()
    {
        var ts = new TimeSignature(4, 4);
        var pos = new TimePosition(ts, ts.TicksPerBar);

        Assert.Equal(1, pos.BarBeatTick.Bar);
        Assert.Equal(0, pos.BarBeatTick.Beat);
        Assert.Equal(0, pos.BarBeatTick.TickWithinBeat);
    }

    [Fact]
    public void Constructor_OneBeat_SecondBeat()
    {
        var ts = new TimeSignature(4, 4);
        var pos = new TimePosition(ts, ts.TicksPerBeat);

        Assert.Equal(0, pos.BarBeatTick.Bar);
        Assert.Equal(1, pos.BarBeatTick.Beat);
        Assert.Equal(0, pos.BarBeatTick.TickWithinBeat);
    }

    [Fact]
    public void Constructor_PartialBeat_CorrectTickWithinBeat()
    {
        var ts = new TimeSignature(4, 4);
        var pos = new TimePosition(ts, 100);

        Assert.Equal(0, pos.BarBeatTick.Bar);
        Assert.Equal(0, pos.BarBeatTick.Beat);
        Assert.Equal(100, pos.BarBeatTick.TickWithinBeat);
    }

    [Fact]
    public void Constructor_NegativeTicks_ThrowsArgumentOutOfRange()
    {
        var ts = new TimeSignature(4, 4);
        Assert.Throws<ArgumentOutOfRangeException>(() => new TimePosition(ts, -1));
    }

    [Fact]
    public void FromBarBeatTick_ConvertsCorrectly()
    {
        var ts = new TimeSignature(4, 4);
        var bbt = new BarBeatTick(1, 2, 100);
        var pos = TimePosition.FromBarBeatTick(ts, bbt);

        long expected = ts.TicksPerBar + 2 * ts.TicksPerBeat + 100;
        Assert.Equal(expected, pos.AbsoluteTicks);
        Assert.Equal(bbt, pos.BarBeatTick);
    }

    [Fact]
    public void FromBarBeatTick_BeatExceedsNumerator_ThrowsArgumentOutOfRange()
    {
        var ts = new TimeSignature(4, 4);
        var bbt = new BarBeatTick(0, 4, 0); // Beat 4 in 4/4 is invalid (0-3 valid)

        Assert.Throws<ArgumentOutOfRangeException>(() => TimePosition.FromBarBeatTick(ts, bbt));
    }

    [Fact]
    public void FromBarBeatTick_TickExceedsTicksPerBeat_ThrowsArgumentOutOfRange()
    {
        var ts = new TimeSignature(4, 4);
        var bbt = new BarBeatTick(0, 0, ts.TicksPerBeat);

        Assert.Throws<ArgumentOutOfRangeException>(() => TimePosition.FromBarBeatTick(ts, bbt));
    }

    [Fact]
    public void Add_AddsDuration()
    {
        var ts = new TimeSignature(4, 4);
        var pos = new TimePosition(ts, 0);
        var dur = DurationFactory.Quarter();

        var newPos = pos.Add(dur);

        Assert.Equal(dur.Ticks, newPos.AbsoluteTicks);
        Assert.Equal(0, newPos.BarBeatTick.Bar);
        Assert.Equal(1, newPos.BarBeatTick.Beat);
    }

    [Fact]
    public void ToString_IncludesBarBeatTickAndTicks()
    {
        var ts = new TimeSignature(4, 4);
        var pos = new TimePosition(ts, 100);
        var str = pos.ToString();

        Assert.Contains("0:0+100", str);
        Assert.Contains("100 ticks", str);
    }
}

/// <summary>
/// Tests for TupletPatterns to improve coverage.
/// </summary>
public class TupletPatternsTests
{
    [Fact]
    public void Generate_DefaultParams_GeneratesTuplets()
    {
        var tuplets = TupletPatterns.Generate().ToList();

        Assert.NotEmpty(tuplets);
        Assert.Contains(tuplets, t => t.ActualCount == 3 && t.NormalCount == 2); // Triplet
    }

    [Fact]
    public void Generate_ExcludesSameActualAndNormal()
    {
        var tuplets = TupletPatterns.Generate().ToList();

        Assert.DoesNotContain(tuplets, t => t.ActualCount == t.NormalCount);
    }

    [Fact]
    public void Generate_CustomRange_LimitsCorrectly()
    {
        var tuplets = TupletPatterns.Generate(maxActual: 5, maxNormal: 4).ToList();

        Assert.All(tuplets, t => Assert.True(t.ActualCount <= 5));
        Assert.All(tuplets, t => Assert.True(t.NormalCount <= 4));
    }

    [Fact]
    public void Generate_ContainsCommonTuplets()
    {
        var tuplets = TupletPatterns.Generate().ToList();

        // Common tuplets
        Assert.Contains(tuplets, t => t.ActualCount == 3 && t.NormalCount == 2); // Triplet
        Assert.Contains(tuplets, t => t.ActualCount == 5 && t.NormalCount == 4); // Quintuplet
        Assert.Contains(tuplets, t => t.ActualCount == 7 && t.NormalCount == 4); // Septuplet
    }
}

/// <summary>
/// Tests for MidiNoteEvent to improve coverage.
/// </summary>
public class MidiNoteEventTests
{
    [Fact]
    public void Constructor_StoresAllProperties()
    {
        var evt = new MidiNoteEvent(1, 60, 100, 480, true);

        Assert.Equal(1, evt.Channel);
        Assert.Equal(60, evt.Pitch);
        Assert.Equal(100, evt.Velocity);
        Assert.Equal(480, evt.Tick);
        Assert.True(evt.IsNoteOn);
    }

    [Fact]
    public void Constructor_NoteOff_IsNoteOnFalse()
    {
        var evt = new MidiNoteEvent(0, 64, 0, 960, false);

        Assert.False(evt.IsNoteOn);
        Assert.Equal(0, evt.Velocity);
    }

    [Fact]
    public void ToString_NoteOn_ContainsOn()
    {
        var evt = new MidiNoteEvent(0, 60, 100, 0, true);
        var str = evt.ToString();

        Assert.Contains("On", str);
        Assert.Contains("p60", str);
        Assert.Contains("v100", str);
    }

    [Fact]
    public void ToString_NoteOff_ContainsOff()
    {
        var evt = new MidiNoteEvent(0, 60, 0, 480, false);
        var str = evt.ToString();

        Assert.Contains("Off", str);
    }
}

/// <summary>
/// Tests for MidiNoteBuilder to improve coverage.
/// </summary>
public class MidiNoteBuilderTests
{
    [Fact]
    public void BuildSingle_GeneratesNoteOnAndOff()
    {
        var events = MidiNoteBuilder.BuildSingle(0, 60, 100, 0, DurationFactory.Quarter()).ToList();

        Assert.Equal(2, events.Count);
        Assert.True(events[0].IsNoteOn);
        Assert.False(events[1].IsNoteOn);
    }

    [Fact]
    public void BuildSingle_CorrectTiming()
    {
        var dur = DurationFactory.Quarter();
        var events = MidiNoteBuilder.BuildSingle(0, 60, 100, 0, dur).ToList();

        Assert.Equal(0, events[0].Tick);
        Assert.Equal(dur.Ticks, events[1].Tick);
    }

    [Fact]
    public void BuildSingle_NoteOffVelocityZero()
    {
        var events = MidiNoteBuilder.BuildSingle(0, 60, 100, 0, DurationFactory.Quarter()).ToList();

        Assert.Equal(0, events[1].Velocity);
    }

    [Fact]
    public void BuildMany_GeneratesSortedEvents()
    {
        var notes = new[]
        {
            (pitch: 60, start: 480L, dur: DurationFactory.Quarter(), velocity: 100, channel: 0),
            (pitch: 64, start: 0L, dur: DurationFactory.Half(), velocity: 90, channel: 0)
        };

        var events = MidiNoteBuilder.BuildMany(notes).ToList();

        Assert.Equal(4, events.Count);
        // Events should be sorted by tick
        Assert.True(events[0].Tick <= events[1].Tick);
        Assert.True(events[1].Tick <= events[2].Tick);
        Assert.True(events[2].Tick <= events[3].Tick);
    }

    [Fact]
    public void BuildMany_NoteOffBeforeNoteOnAtSameTick()
    {
        var notes = new[]
        {
            (pitch: 60, start: 0L, dur: DurationFactory.Quarter(), velocity: 100, channel: 0),
            (pitch: 64, start: 480L, dur: DurationFactory.Quarter(), velocity: 90, channel: 0)
        };

        var events = MidiNoteBuilder.BuildMany(notes).ToList();

        // At tick 480, NoteOn (pitch 64) should come before NoteOff (pitch 60) due to ThenBy(!e.IsNoteOn) sorting
        var tick480Events = events.Where(e => e.Tick == 480).ToList();
        Assert.Equal(2, tick480Events.Count);
        Assert.True(tick480Events[0].IsNoteOn); // NoteOn first (ThenBy false < true)
        Assert.False(tick480Events[1].IsNoteOn); // NoteOff second
    }
}

/// <summary>
/// Tests for TimeSignatureMap to improve coverage.
/// </summary>
public class TimeSignatureMapTests
{
    [Fact]
    public void Constructor_InitialSignatureAtZero()
    {
        var ts = new TimeSignature(4, 4);
        var map = new TimeSignatureMap(ts);

        Assert.Equal(ts, map.GetAt(0));
    }

    [Fact]
    public void AddChange_AddsNewSignature()
    {
        var ts1 = new TimeSignature(4, 4);
        var ts2 = new TimeSignature(3, 4);
        var map = new TimeSignatureMap(ts1);

        map.AddChange(1920, ts2);

        Assert.Equal(ts1, map.GetAt(0));
        Assert.Equal(ts2, map.GetAt(1920));
    }

    [Fact]
    public void AddChange_NegativeTick_ThrowsArgumentOutOfRange()
    {
        var map = new TimeSignatureMap(new TimeSignature(4, 4));

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            map.AddChange(-1, new TimeSignature(3, 4)));
    }

    [Fact]
    public void AddChange_ReplacesExistingAtSameTick()
    {
        var ts1 = new TimeSignature(4, 4);
        var ts2 = new TimeSignature(3, 4);
        var ts3 = new TimeSignature(6, 8);
        var map = new TimeSignatureMap(ts1);

        map.AddChange(1920, ts2);
        map.AddChange(1920, ts3); // Replace

        Assert.Equal(ts3, map.GetAt(1920));
    }

    [Fact]
    public void GetAt_BeforeFirstChange_ReturnsInitial()
    {
        var ts1 = new TimeSignature(4, 4);
        var ts2 = new TimeSignature(3, 4);
        var map = new TimeSignatureMap(ts1);
        map.AddChange(1920, ts2);

        Assert.Equal(ts1, map.GetAt(1000));
    }

    [Fact]
    public void GetAt_AfterChange_ReturnsNewSignature()
    {
        var ts1 = new TimeSignature(4, 4);
        var ts2 = new TimeSignature(3, 4);
        var map = new TimeSignatureMap(ts1);
        map.AddChange(1920, ts2);

        Assert.Equal(ts2, map.GetAt(2000));
    }

    [Fact]
    public void GetAt_MultipleChanges_ReturnsCorrectSignature()
    {
        var ts1 = new TimeSignature(4, 4);
        var ts2 = new TimeSignature(3, 4);
        var ts3 = new TimeSignature(6, 8);
        var map = new TimeSignatureMap(ts1);
        map.AddChange(1920, ts2);
        map.AddChange(3840, ts3);

        Assert.Equal(ts1, map.GetAt(1000));
        Assert.Equal(ts2, map.GetAt(2500));
        Assert.Equal(ts3, map.GetAt(4000));
    }

    [Fact]
    public void ToPosition_CreatesTimePositionWithCorrectSignature()
    {
        var ts1 = new TimeSignature(4, 4);
        var ts2 = new TimeSignature(3, 4);
        var map = new TimeSignatureMap(ts1);
        map.AddChange(1920, ts2);

        var pos = map.ToPosition(2000);

        Assert.Equal(ts2, pos.Signature);
        Assert.Equal(2000, pos.AbsoluteTicks);
    }
}

/// <summary>
/// Tests for Tempo to improve coverage.
/// </summary>
public class TempoTests
{
    [Fact]
    public void Constructor_ValidValue_Stores()
    {
        var tempo = new Tempo(500000);
        Assert.Equal(500000, tempo.MicrosecondsPerQuarter);
    }

    [Fact]
    public void Constructor_ZeroOrNegative_ThrowsArgumentOutOfRange()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Tempo(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Tempo(-1));
    }

    [Fact]
    public void Bpm_CalculatesCorrectly()
    {
        var tempo = new Tempo(500000); // 120 BPM
        Assert.Equal(120.0, tempo.Bpm, precision: 2);
    }

    [Fact]
    public void FromBpm_ConvertsCorrectly()
    {
        var tempo = Tempo.FromBpm(120);
        Assert.Equal(500000, tempo.MicrosecondsPerQuarter);
    }

    [Fact]
    public void FromBpm_RoundTrip_Consistent()
    {
        var original = 140.0;
        var tempo = Tempo.FromBpm(original);
        Assert.Equal(original, tempo.Bpm, precision: 1);
    }

    [Fact]
    public void Equals_SameTempo_ReturnsTrue()
    {
        var a = new Tempo(500000);
        var b = new Tempo(500000);
        Assert.True(a.Equals(b));
    }

    [Fact]
    public void Equals_DifferentTempo_ReturnsFalse()
    {
        var a = new Tempo(500000);
        var b = new Tempo(600000);
        Assert.False(a.Equals(b));
    }
}
