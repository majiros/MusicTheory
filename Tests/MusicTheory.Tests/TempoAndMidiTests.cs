using System;
using System.Linq;
using MusicTheory.Theory.Time;
using MusicTheory.Theory.Midi;
using TimeSig = MusicTheory.Theory.Time.TimeSignature;

namespace MusicTheory.Tests;

public class TempoAndMidiTests
{
    [Fact]
    public void TempoMap_ConvertsTicksToMicroseconds()
    {
        var map = new TempoMap(Tempo.FromBpm(120)); // 500000 us/quarter
        map.AddChange(480*4, Tempo.FromBpm(60)); // 1 小節後 BPM 半減
        long usBefore = map.TickToMicroseconds(480*4); // 1 bar (4 quarters) => 4 * 500000 = 2,000,000
        Assert.Equal(2_000_000, usBefore);
        long usAfter = map.TickToMicroseconds(480*6);
        Assert.Equal(4_000_000, usAfter);
    }

    [Fact]
    public void MidiFileWriter_BuildsFormat0()
    {
        var track = new MidiTrack();
        track.AddMeta(new TempoEvent(0, Tempo.FromBpm(120)));
    track.AddMeta(new TimeSignatureEvent(0, new TimeSig(4,4)));
        var noteDur = DurationFactory.Quarter();
        foreach (var e in MidiNoteBuilder.BuildSingle(0, 60, 100, 0, noteDur)) track.AddNote(e);
        var bytes = MidiFileWriter.WriteSingleTrack(track);
        Assert.Equal((byte)'M', bytes[0]);
        Assert.Equal((byte)'T', bytes[1]);
        Assert.Equal((byte)'h', bytes[2]);
        Assert.Equal((byte)'d', bytes[3]);
        // EndOfTrack (FF 2F 00) がどこかに含まれること
        bool eot = false;
        for (int i=0;i<bytes.Length-2;i++)
            if (bytes[i]==0xFF && bytes[i+1]==0x2F && bytes[i+2]==0x00) { eot=true; break; }
        Assert.True(eot, "EndOfTrack meta not found");
    }

}
