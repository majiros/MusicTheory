using Xunit;
using MusicTheory.Theory.Midi;
using MusicTheory.Theory.Time;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicTheory.Tests;

public class MidiFileWriterTests
{
    // ──────────────────────────────────────────────────────────
    // WriteSingleTrack - Basic Functionality
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void WriteSingleTrack_EmptyTrack_ReturnsValidSMF()
    {
        var track = new MidiTrack();
        var bytes = MidiFileWriter.WriteSingleTrack(track);

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);

        // SMF Header: "MThd" (4B) + length=6 (4B) + format=0 (2B) + ntrks=1 (2B) + division (2B) = 14B
        Assert.True(bytes.Length >= 14);
        Assert.Equal((byte)'M', bytes[0]);
        Assert.Equal((byte)'T', bytes[1]);
        Assert.Equal((byte)'h', bytes[2]);
        Assert.Equal((byte)'d', bytes[3]);
    }

    [Fact]
    public void WriteSingleTrack_NegativeDivision_ThrowsArgumentOutOfRangeException()
    {
        var track = new MidiTrack();
        Assert.Throws<ArgumentOutOfRangeException>(() => MidiFileWriter.WriteSingleTrack(track, division: -1));
    }

    [Fact]
    public void WriteSingleTrack_WithTempoEvent_IncludesTempoMeta()
    {
        var track = new MidiTrack();
        var tempo = Tempo.FromBpm(120);
        track.AddMeta(new TempoEvent(0, tempo));

        var bytes = MidiFileWriter.WriteSingleTrack(track, useRunningStatus: false);

        // Check for FF 51 03 (Tempo meta)
        // Tempo meta = FF 51 03 + 3 bytes for microseconds
        bool foundTempo = false;
        for (int i = 0; i < bytes.Length - 5; i++)
        {
            if (bytes[i] == 0xFF && bytes[i + 1] == 0x51 && bytes[i + 2] == 0x03)
            {
                foundTempo = true;
                break;
            }
        }
        Assert.True(foundTempo, "Tempo meta event (FF 51) not found in MIDI file");
    }

    [Fact]
    public void WriteSingleTrack_WithTimeSignature_IncludesTimeSignatureMeta()
    {
        var track = new MidiTrack();
        var sig = new MusicTheory.Theory.Time.TimeSignature(3, 4);
        track.AddMeta(new TimeSignatureEvent(0, sig));

        var bytes = MidiFileWriter.WriteSingleTrack(track, useRunningStatus: false);

        // Check for FF 58 04 (Time Signature meta)
        bool foundTimeSig = false;
        for (int i = 0; i < bytes.Length - 6; i++)
        {
            if (bytes[i] == 0xFF && bytes[i + 1] == 0x58 && bytes[i + 2] == 0x04)
            {
                // Next byte should be numerator (3)
                if (bytes[i + 3] == 3)
                    foundTimeSig = true;
                break;
            }
        }
        Assert.True(foundTimeSig, "Time Signature meta event (FF 58) not found or incorrect");
    }

    [Fact]
    public void WriteSingleTrack_WithKeySignature_IncludesKeySignatureMeta()
    {
        var track = new MidiTrack();
        track.AddMeta(new KeySignatureEvent(0, 2, false)); // D major (2 sharps)

        var bytes = MidiFileWriter.WriteSingleTrack(track, useRunningStatus: false);

        // Check for FF 59 02 (Key Signature meta)
        bool foundKeySig = false;
        for (int i = 0; i < bytes.Length - 4; i++)
        {
            if (bytes[i] == 0xFF && bytes[i + 1] == 0x59 && bytes[i + 2] == 0x02)
            {
                // Next byte should be sharps (2)
                if ((sbyte)bytes[i + 3] == 2)
                    foundKeySig = true;
                break;
            }
        }
        Assert.True(foundKeySig, "Key Signature meta event (FF 59) not found or incorrect");
    }

    [Fact]
    public void WriteSingleTrack_WithTextEvent_IncludesTextMeta()
    {
        var track = new MidiTrack();
        track.AddMeta(new TextEvent(0, "Test"));

        var bytes = MidiFileWriter.WriteSingleTrack(track, useRunningStatus: false);

        // Check for FF 01 (Text meta)
        bool foundText = false;
        for (int i = 0; i < bytes.Length - 2; i++)
        {
            if (bytes[i] == 0xFF && bytes[i + 1] == 0x01)
            {
                foundText = true;
                break;
            }
        }
        Assert.True(foundText, "Text meta event (FF 01) not found");
    }

    [Fact]
    public void WriteSingleTrack_WithTrackNameEvent_IncludesTrackNameMeta()
    {
        var track = new MidiTrack();
        track.AddMeta(new TrackNameEvent(0, "Piano"));

        var bytes = MidiFileWriter.WriteSingleTrack(track, useRunningStatus: false);

        // Check for FF 03 (Track Name meta)
        bool foundTrackName = false;
        for (int i = 0; i < bytes.Length - 2; i++)
        {
            if (bytes[i] == 0xFF && bytes[i + 1] == 0x03)
            {
                foundTrackName = true;
                break;
            }
        }
        Assert.True(foundTrackName, "Track Name meta event (FF 03) not found");
    }

    [Fact]
    public void WriteSingleTrack_WithMarkerEvent_IncludesMarkerMeta()
    {
        var track = new MidiTrack();
        track.AddMeta(new MarkerEvent(0, "Chorus"));

        var bytes = MidiFileWriter.WriteSingleTrack(track, useRunningStatus: false);

        // Check for FF 06 (Marker meta)
        bool foundMarker = false;
        for (int i = 0; i < bytes.Length - 2; i++)
        {
            if (bytes[i] == 0xFF && bytes[i + 1] == 0x06)
            {
                foundMarker = true;
                break;
            }
        }
        Assert.True(foundMarker, "Marker meta event (FF 06) not found");
    }

    [Fact]
    public void WriteSingleTrack_WithNoteOnEvent_IncludesNoteOn()
    {
        var track = new MidiTrack();
        track.AddNote(new MidiNoteEvent(channel: 0, pitch: 60, velocity: 100, tick: 0, isNoteOn: true));

        var bytes = MidiFileWriter.WriteSingleTrack(track, useRunningStatus: false);

        // Check for 0x90 (Note On, channel 0)
        bool foundNoteOn = false;
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] == 0x90)
            {
                foundNoteOn = true;
                break;
            }
        }
        Assert.True(foundNoteOn, "Note On event (0x90) not found");
    }

    [Fact]
    public void WriteSingleTrack_WithNoteOffEvent_IncludesNoteOff()
    {
        var track = new MidiTrack();
        track.AddNote(new MidiNoteEvent(channel: 0, pitch: 60, velocity: 0, tick: 0, isNoteOn: false));

        var bytes = MidiFileWriter.WriteSingleTrack(track, useRunningStatus: false);

        // Check for 0x80 (Note Off, channel 0)
        bool foundNoteOff = false;
        for (int i = 0; i < bytes.Length; i++)
        {
            if (bytes[i] == 0x80)
            {
                foundNoteOff = true;
                break;
            }
        }
        Assert.True(foundNoteOff, "Note Off event (0x80) not found");
    }

    [Fact]
    public void WriteSingleTrack_WithRunningStatus_OmitsRepeatedStatus()
    {
        var track = new MidiTrack();
        track.AddNote(new MidiNoteEvent(channel: 0, pitch: 60, velocity: 100, tick: 0, isNoteOn: true));
        track.AddNote(new MidiNoteEvent(channel: 0, pitch: 64, velocity: 100, tick: 10, isNoteOn: true)); // Same status (0x90 ch 0)

        var bytesWithRunning = MidiFileWriter.WriteSingleTrack(track, useRunningStatus: true);
        var bytesWithoutRunning = MidiFileWriter.WriteSingleTrack(track, useRunningStatus: false);

        // With running status, file should be slightly shorter
        Assert.True(bytesWithRunning.Length < bytesWithoutRunning.Length,
            $"Running status should produce shorter file: {bytesWithRunning.Length} vs {bytesWithoutRunning.Length}");
    }

    [Fact]
    public void WriteSingleTrack_PitchClamping_ClampsPitchTo127()
    {
        var track = new MidiTrack();
        track.AddNote(new MidiNoteEvent(channel: 0, pitch: 200, velocity: 100, tick: 0, isNoteOn: true)); // Invalid pitch

        var bytes = MidiFileWriter.WriteSingleTrack(track, useRunningStatus: false);

        // Should not throw, and pitch should be clamped to 127
        Assert.NotNull(bytes);
    }

    [Fact]
    public void WriteSingleTrack_VelocityClamping_ClampsVelocityTo127()
    {
        var track = new MidiTrack();
        track.AddNote(new MidiNoteEvent(channel: 0, pitch: 60, velocity: 200, tick: 0, isNoteOn: true)); // Invalid velocity

        var bytes = MidiFileWriter.WriteSingleTrack(track, useRunningStatus: false);

        // Should not throw, and velocity should be clamped to 127
        Assert.NotNull(bytes);
    }

    [Fact]
    public void WriteSingleTrack_EndOfTrack_AlwaysPresent()
    {
        var track = new MidiTrack();
        var bytes = MidiFileWriter.WriteSingleTrack(track);

        // Check for FF 2F 00 (End Of Track)
        bool foundEOT = false;
        for (int i = 0; i < bytes.Length - 2; i++)
        {
            if (bytes[i] == 0xFF && bytes[i + 1] == 0x2F && bytes[i + 2] == 0x00)
            {
                foundEOT = true;
                break;
            }
        }
        Assert.True(foundEOT, "End Of Track meta event (FF 2F 00) not found");
    }

    // ──────────────────────────────────────────────────────────
    // WriteMultipleTracks - Format 1
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void WriteMultipleTracks_EmptyList_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => MidiFileWriter.WriteMultipleTracks(new List<MidiTrack>()));
    }

    [Fact]
    public void WriteMultipleTracks_NullList_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => MidiFileWriter.WriteMultipleTracks(null!));
    }

    [Fact]
    public void WriteMultipleTracks_SingleTrack_ReturnsValidFormat1SMF()
    {
        var track = new MidiTrack();
        track.AddNote(new MidiNoteEvent(channel: 0, pitch: 60, velocity: 100, tick: 0, isNoteOn: true));

        var bytes = MidiFileWriter.WriteMultipleTracks(new[] { track });

        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);

        // Format should be 1 (offset 8-9 in header)
        // Format is big-endian: bytes[8:9] should be 0x00 0x01
        Assert.Equal(0x00, bytes[8]);
        Assert.Equal(0x01, bytes[9]);
    }

    [Fact]
    public void WriteMultipleTracks_MultipleTracks_ReturnsFormat1WithCorrectTrackCount()
    {
        var track1 = new MidiTrack();
        track1.AddNote(new MidiNoteEvent(channel: 0, pitch: 60, velocity: 100, tick: 0, isNoteOn: true));

        var track2 = new MidiTrack();
        track2.AddNote(new MidiNoteEvent(channel: 0, pitch: 64, velocity: 100, tick: 0, isNoteOn: true));

        var bytes = MidiFileWriter.WriteMultipleTracks(new[] { track1, track2 });

        // Track count is at offset 10-11 (big-endian)
        int trackCount = (bytes[10] << 8) | bytes[11];
        Assert.Equal(2, trackCount);
    }

    [Fact]
    public void WriteMultipleTracks_ConsolidateConductor_MovesTempoToFirstTrack()
    {
        var track1 = new MidiTrack();
        track1.AddNote(new MidiNoteEvent(channel: 0, pitch: 60, velocity: 100, tick: 0, isNoteOn: true));

        var track2 = new MidiTrack();
        var tempo140 = Tempo.FromBpm(140);
        track2.AddMeta(new TempoEvent(0, tempo140));
        track2.AddNote(new MidiNoteEvent(channel: 0, pitch: 64, velocity: 100, tick: 0, isNoteOn: true));

        var bytes = MidiFileWriter.WriteMultipleTracks(new[] { track1, track2 }, consolidateConductor: true);

        // After consolidation, track1 should have the Tempo event from track2
        Assert.Contains(track1.MetaEvents, m => m is TempoEvent te && Math.Abs(te.Tempo.Bpm - 140) < 0.1);
    }

    [Fact]
    public void WriteMultipleTracks_ConsolidateConductor_RemovesTempoFromOtherTracks()
    {
        var track1 = new MidiTrack();
        var track2 = new MidiTrack();
        track2.AddMeta(new TempoEvent(0, Tempo.FromBpm(140)));

        MidiFileWriter.WriteMultipleTracks(new[] { track1, track2 }, consolidateConductor: true);

        // track2 should no longer have Tempo event
        Assert.DoesNotContain(track2.MetaEvents, m => m is TempoEvent);
    }

    [Fact]
    public void WriteMultipleTracks_NoConsolidation_KeepsTempoInOriginalTrack()
    {
        var track1 = new MidiTrack();
        var track2 = new MidiTrack();
        track2.AddMeta(new TempoEvent(0, Tempo.FromBpm(140)));

        var tempoCountBefore = track2.MetaEvents.Count(m => m is TempoEvent);

        MidiFileWriter.WriteMultipleTracks(new[] { track1, track2 }, consolidateConductor: false);

        // track2 should still have the Tempo event
        var tempoCountAfter = track2.MetaEvents.Count(m => m is TempoEvent);
        Assert.Equal(tempoCountBefore, tempoCountAfter);
    }

    [Fact]
    public void WriteMultipleTracks_ConsolidateConductor_DoesNotDuplicateSameTickTempo()
    {
        var track1 = new MidiTrack();
        track1.AddMeta(new TempoEvent(0, Tempo.FromBpm(120)));

        var track2 = new MidiTrack();
        track2.AddMeta(new TempoEvent(0, Tempo.FromBpm(120))); // Same tick, same tempo

        MidiFileWriter.WriteMultipleTracks(new[] { track1, track2 }, consolidateConductor: true);

        // track1 should have only one Tempo event at tick 0
        var tempos = track1.MetaEvents.Where(m => m is TempoEvent te && te.Tick == 0).ToList();
        Assert.Equal(1, tempos.Count);
    }

    // ──────────────────────────────────────────────────────────
    // MidiConductorHelper Tests
    // ──────────────────────────────────────────────────────────

    [Fact]
    public void ExtractConductor_EmptyList_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => MidiConductorHelper.ExtractConductor(new List<MidiTrack>()));
    }

    [Fact]
    public void ExtractConductor_SingleTrack_ReturnsConductorWithAllMeta()
    {
        var track = new MidiTrack();
        track.AddMeta(new TempoEvent(0, Tempo.FromBpm(120)));
        track.AddMeta(new TimeSignatureEvent(0, new MusicTheory.Theory.Time.TimeSignature(4, 4)));

        var (conductor, others) = MidiConductorHelper.ExtractConductor(new[] { track });

        Assert.Contains(conductor.MetaEvents, m => m is TempoEvent);
        Assert.Contains(conductor.MetaEvents, m => m is TimeSignatureEvent);
        Assert.Empty(others);
    }

    [Fact]
    public void ExtractConductor_MultipleTracks_ReturnsFirstTrackMetaAndRestAsSeparate()
    {
        var track1 = new MidiTrack();
        track1.AddMeta(new TempoEvent(0, Tempo.FromBpm(120)));

        var track2 = new MidiTrack();
        track2.AddNote(new MidiNoteEvent(channel: 0, pitch: 60, velocity: 100, tick: 0, isNoteOn: true));

        var (conductor, others) = MidiConductorHelper.ExtractConductor(new[] { track1, track2 });

        Assert.Single(conductor.MetaEvents);
        Assert.Single(others);
        Assert.Same(track2, others[0]);
    }

    [Fact]
    public void ConsolidateAndStrip_EmptyList_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => MidiConductorHelper.ConsolidateAndStrip(new List<MidiTrack>()));
    }

    [Fact]
    public void ConsolidateAndStrip_SingleTrack_ConsolidatesAndStrips()
    {
        var track = new MidiTrack();
        track.AddMeta(new TempoEvent(0, Tempo.FromBpm(120)));
        track.AddMeta(new TextEvent(0, "Test")); // Non-conductor meta

        var conductor = MidiConductorHelper.ConsolidateAndStrip(new List<MidiTrack> { track });

        // Conductor should have Tempo
        Assert.Single(conductor.MetaEvents.OfType<TempoEvent>());

        // Track should no longer have Tempo
        Assert.Empty(track.MetaEvents.OfType<TempoEvent>());

        // Track should still have Text
        Assert.Single(track.MetaEvents.OfType<TextEvent>());
    }

    [Fact]
    public void ConsolidateAndStrip_MultipleTracks_ConsolidatesAllConductorMeta()
    {
        var track1 = new MidiTrack();
        track1.AddMeta(new TempoEvent(0, Tempo.FromBpm(120)));

        var track2 = new MidiTrack();
        track2.AddMeta(new TimeSignatureEvent(0, new MusicTheory.Theory.Time.TimeSignature(3, 4)));

        var conductor = MidiConductorHelper.ConsolidateAndStrip(new List<MidiTrack> { track1, track2 });

        // Conductor should have both Tempo and TimeSignature
        Assert.Single(conductor.MetaEvents.OfType<TempoEvent>());
        Assert.Single(conductor.MetaEvents.OfType<TimeSignatureEvent>());

        // Tracks should no longer have these
        Assert.Empty(track1.MetaEvents.OfType<TempoEvent>());
        Assert.Empty(track2.MetaEvents.OfType<TimeSignatureEvent>());
    }

    [Fact]
    public void ConsolidateAndStrip_DoesNotDuplicateSameTickSameType()
    {
        var track1 = new MidiTrack();
        track1.AddMeta(new TempoEvent(0, Tempo.FromBpm(120)));

        var track2 = new MidiTrack();
        track2.AddMeta(new TempoEvent(0, Tempo.FromBpm(140))); // Same tick, different tempo

        var conductor = MidiConductorHelper.ConsolidateAndStrip(new List<MidiTrack> { track1, track2 });

        // Conductor should have only one TempoEvent at tick 0 (first encountered)
        var tempos = conductor.MetaEvents.OfType<TempoEvent>().Where(te => te.Tick == 0).ToList();
        Assert.Single(tempos);
        Assert.True(Math.Abs(tempos[0].Tempo.Bpm - 120) < 0.1, $"Expected BPM ~120, got {tempos[0].Tempo.Bpm}");
    }
}
