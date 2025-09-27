using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MusicTheory.Theory.Time;

namespace MusicTheory.Theory.Midi;

/// <summary>
/// シンプルな SMF (Standard MIDI File) Writer。
/// Format0 (単一) と Format1 (複数同期トラック) をサポート。
/// 対応メタイベント: Tempo(FF51), TimeSignature(FF58), KeySignature(FF59), Text/Name/Marker (FF01/03/06)。
/// </summary>
public static class MidiFileWriter
{
    /// <summary>
    /// トラックを SMF format0 バイト列として書き出す。Tempo/TimeSignature メタイベントは track.MetaEvents から収集。
    /// </summary>
    public static byte[] WriteSingleTrack(MidiTrack track, int division = Duration.TicksPerQuarter, bool useRunningStatus=true)
    {
        if (division <= 0) throw new ArgumentOutOfRangeException(nameof(division));
        track.Sort();
        var events = new List<(long tick, byte[] data, bool channel)>();

        // Meta events
        foreach (var me in track.MetaEvents)
        {
            switch (me)
            {
                case TempoEvent te:
                    // FF 51 03 tttttt
                    int us = te.Tempo.MicrosecondsPerQuarter;
                    events.Add((te.Tick, new byte[]{0xFF,0x51,0x03,(byte)((us>>16)&0xFF),(byte)((us>>8)&0xFF),(byte)(us&0xFF)}, false));
                    break;
                case TimeSignatureEvent tse:
                    // FF 58 04 nn dd cc bb
                    byte nn = (byte)tse.Signature.Numerator;
                    // dd = log2(denominator)
                    byte dd = (byte)Math.Log2(tse.Signature.Denominator);
                    byte cc = 24; // メトロノームクリック (デフォルト)
                    byte bb = 8;  // 32分音符数 (四分音符あたり)
                    events.Add((tse.Tick, new byte[]{0xFF,0x58,0x04, nn, dd, cc, bb}, false));
                    break;
                case KeySignatureEvent kse:
                    // FF 59 02 sf mi (sf=-7..7 flats/sharps, mi=0 major 1 minor)
                    sbyte sf = (sbyte)kse.SharpsFlats;
                    byte mi = (byte)(kse.IsMinor ? 1:0);
                    events.Add((kse.Tick, new byte[]{0xFF,0x59,0x02,(byte)sf, mi}, false));
                    break;
                case TextEvent txt:
                    events.Add((txt.Tick, BuildTextMeta(0x01, txt.Text), false));
                    break;
                case TrackNameEvent tname:
                    events.Add((tname.Tick, BuildTextMeta(0x03, tname.Name), false));
                    break;
                case MarkerEvent mark:
                    events.Add((mark.Tick, BuildTextMeta(0x06, mark.Label), false));
                    break;
            }
        }

        // Note events
        foreach (var ne in track.Notes)
        {
            int statusBase = ne.IsNoteOn ? 0x90 : 0x80; // 0x9x / 0x8x
            byte status = (byte)(statusBase | (ne.Channel & 0x0F));
            byte pitch = (byte)Math.Clamp(ne.Pitch, 0, 127);
            byte velocity = (byte)Math.Clamp(ne.Velocity, 0, 127);
            events.Add((ne.Tick, new byte[]{status, pitch, velocity}, true));
        }

        // End Of Track (tick = 最後のイベント tick と同じ or 0)
        long endTick = events.Count > 0 ? events.Max(e => e.tick) : 0;
        events.Add((endTick, new byte[]{0xFF,0x2F,0x00}, false));

        events.Sort((a,b)=> a.tick.CompareTo(b.tick));

        var trackData = new MemoryStream();
        long prevTick = 0;
        byte? lastStatus = null;
        foreach (var ev in events)
        {
            long delta = ev.tick - prevTick;
            WriteVarLen(trackData, delta);
            prevTick = ev.tick;
            if (useRunningStatus && ev.channel)
            {
                byte status = ev.data[0];
                if (lastStatus.HasValue && lastStatus.Value == status)
                {
                    // omit status
                    trackData.Write(ev.data, 1, ev.data.Length-1);
                }
                else
                {
                    trackData.Write(ev.data, 0, ev.data.Length);
                    lastStatus = status;
                }
            }
            else
            {
                trackData.Write(ev.data, 0, ev.data.Length); if (ev.channel) lastStatus = ev.data[0];
            }
        }

        byte[] trackBytes = trackData.ToArray();

        // Header (MThd)
        var ms = new MemoryStream();
        using (var bw = new BinaryWriter(ms, System.Text.Encoding.ASCII, leaveOpen:true))
        {
            bw.Write(new byte[]{(byte)'M',(byte)'T',(byte)'h',(byte)'d'});
            bw.Write(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(6))); // length
            bw.Write(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder((short)0))); // format 0
            bw.Write(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder((short)1))); // ntrks=1
            bw.Write(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder((short)division))); // division

            // Track chunk
            bw.Write(new byte[]{(byte)'M',(byte)'T',(byte)'r',(byte)'k'});
            bw.Write(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(trackBytes.Length)));
            bw.Write(trackBytes);
        }
        return ms.ToArray();
    }

    /// <summary>複数トラック (Format1)。各 MidiTrack は独立にソートされ、先頭テンポ/拍子は最初のトラック(インデックス0)に集約することを推奨。</summary>
    public static byte[] WriteMultipleTracks(IReadOnlyList<MidiTrack> tracks, int division = Duration.TicksPerQuarter, bool useRunningStatus=true, bool consolidateConductor=true)
    {
        if (tracks==null || tracks.Count==0) throw new ArgumentException("tracks empty");
        if (consolidateConductor && tracks.Count>1)
        {
            // 先頭トラックに Tempo/TimeSig/KeySig を集約 (tick=0 のもの優先) し他トラックから除去
            var primary = tracks[0];
            var others = tracks.Skip(1);
            var conductorTypes = new HashSet<Type>{ typeof(TempoEvent), typeof(TimeSignatureEvent), typeof(KeySignatureEvent) };
            foreach (var ot in others)
            {
                var moving = ot.MetaEvents.Where(m=>conductorTypes.Contains(m.GetType())).ToList();
                foreach (var m in moving)
                {
                    // 既に primary に同種 tick==m.Tick が無ければ追加
                    if (!primary.MetaEvents.Any(x=> x.GetType()==m.GetType() && x.Tick==m.Tick)) primary.AddMeta(m);
                }
                // 除去 (Tempo/TimeSig/KeySig のみ)
                RemoveMetaInternal(ot, conductorTypes);
            }
        }
        foreach (var t in tracks) t.Sort();
        var trackChunks = new List<byte[]>();
        foreach (var track in tracks)
            trackChunks.Add(BuildTrackChunk(track, useRunningStatus));

        var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms, System.Text.Encoding.ASCII, leaveOpen:true);
        // Header
        bw.Write(new byte[]{(byte)'M',(byte)'T',(byte)'h',(byte)'d'});
        bw.Write(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(6)));
        bw.Write(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder((short)1))); // format1
        bw.Write(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder((short)trackChunks.Count))); // ntrks
        bw.Write(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder((short)division)));
        // Tracks
        foreach (var chunk in trackChunks)
            bw.Write(chunk);
        return ms.ToArray();
    }

    private static byte[] BuildTrackChunk(MidiTrack track, bool useRunningStatus)
    {
        var events = new List<(long tick, byte[] data, bool channel)>();
        foreach (var me in track.MetaEvents)
        {
            switch (me)
            {
                case TempoEvent te:
                    int us = te.Tempo.MicrosecondsPerQuarter;
                    events.Add((te.Tick, new byte[]{0xFF,0x51,0x03,(byte)((us>>16)&0xFF),(byte)((us>>8)&0xFF),(byte)(us&0xFF)}, false));
                    break;
                case TimeSignatureEvent tse:
                    byte nn = (byte)tse.Signature.Numerator; byte dd = (byte)Math.Log2(tse.Signature.Denominator); byte cc=24; byte bb=8;
                    events.Add((tse.Tick, new byte[]{0xFF,0x58,0x04, nn, dd, cc, bb}, false));
                    break;
                case KeySignatureEvent kse:
                    sbyte sf = (sbyte)kse.SharpsFlats; byte mi = (byte)(kse.IsMinor?1:0);
                    events.Add((kse.Tick, new byte[]{0xFF,0x59,0x02,(byte)sf, mi}, false));
                    break;
                case TextEvent txt:
                    events.Add((txt.Tick, BuildTextMeta(0x01, txt.Text), false));
                    break;
                case TrackNameEvent tname:
                    events.Add((tname.Tick, BuildTextMeta(0x03, tname.Name), false));
                    break;
                case MarkerEvent mark:
                    events.Add((mark.Tick, BuildTextMeta(0x06, mark.Label), false));
                    break;
            }
        }
        foreach (var ne in track.Notes)
        {
            int statusBase = ne.IsNoteOn ? 0x90 : 0x80; byte status = (byte)(statusBase | (ne.Channel & 0x0F));
            byte pitch = (byte)Math.Clamp(ne.Pitch, 0, 127); byte velocity = (byte)Math.Clamp(ne.Velocity, 0, 127);
            events.Add((ne.Tick, new byte[]{status, pitch, velocity}, true));
        }
        long endTick = events.Count>0 ? events.Max(e=>e.tick) : 0;
        events.Add((endTick, new byte[]{0xFF,0x2F,0x00}, false));
        events.Sort((a,b)=> a.tick.CompareTo(b.tick));
        var ms = new MemoryStream();
        long prev=0; byte? lastStatus=null; foreach (var ev in events){ long d=ev.tick-prev; WriteVarLen(ms,d); prev=ev.tick; if (useRunningStatus && ev.channel){ var status=ev.data[0]; if (lastStatus.HasValue && lastStatus.Value==status){ ms.Write(ev.data,1,ev.data.Length-1);} else { ms.Write(ev.data,0,ev.data.Length); lastStatus=status; } } else { ms.Write(ev.data,0,ev.data.Length); if (ev.channel) lastStatus=ev.data[0]; } }        
        var data = ms.ToArray();
        var chunk = new MemoryStream();
        using var bw = new BinaryWriter(chunk, System.Text.Encoding.ASCII, leaveOpen:true);
        bw.Write(new byte[]{(byte)'M',(byte)'T',(byte)'r',(byte)'k'});
        bw.Write(BitConverter.GetBytes(System.Net.IPAddress.HostToNetworkOrder(data.Length)));
        bw.Write(data);
        return chunk.ToArray();
    }

    private static byte[] BuildTextMeta(byte type, string text)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text ?? "");
        var ms = new MemoryStream();
        ms.WriteByte(0xFF); ms.WriteByte(type); WriteVarLen(ms, bytes.Length); ms.Write(bytes,0,bytes.Length);
        return ms.ToArray();
    }

    private static void WriteVarLen(Stream s, long value)
    {
        // 最大 4 バイト想定
        uint buffer = (uint)value & 0x7F;
        while ((value >>= 7) > 0)
        {
            buffer <<= 8;
            buffer |= (uint)(((uint)value & 0x7F) | 0x80);
        }
        while (true)
        {
            s.WriteByte((byte)buffer);
            if ((buffer & 0x80) != 0) buffer >>= 8; else break;
        }
    }
    private static void RemoveMetaInternal(MidiTrack track, HashSet<Type> types)
    {
        var keep = track.MetaEvents.Where(m=>!types.Contains(m.GetType())).ToList();
        var field = typeof(MidiTrack).GetField("_meta", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
        if (field!=null && field.GetValue(track) is List<MetaEvent> list)
        {
            list.Clear(); list.AddRange(keep);
        }
    }
}

/// <summary>コンダクタートラック生成 / 集約ヘルパ。</summary>
public static class MidiConductorHelper
{
    public static (MidiTrack conductor, IList<MidiTrack> others) ExtractConductor(IReadOnlyList<MidiTrack> tracks)
    {
        if (tracks.Count==0) throw new ArgumentException("tracks empty");
        var conductor = new MidiTrack();
        foreach (var m in tracks[0].MetaEvents) conductor.AddMeta(m);
        return (conductor, tracks.Skip(1).ToList());
    }
    public static MidiTrack ConsolidateAndStrip(IList<MidiTrack> tracks)
    {
        if (tracks.Count==0) throw new ArgumentException("tracks empty");
        // consolidate by calling writer helper indirectly not exposed; replicate logic here
        var conductor = new MidiTrack();
        var types = new HashSet<Type>{ typeof(TempoEvent), typeof(TimeSignatureEvent), typeof(KeySignatureEvent) };
        foreach (var t in tracks)
        {
            var moving = t.MetaEvents.Where(m=>types.Contains(m.GetType())).ToList();
            foreach (var m in moving)
                if (!conductor.MetaEvents.Any(x=>x.GetType()==m.GetType() && x.Tick==m.Tick)) conductor.AddMeta(m);
            // strip
            var field = typeof(MidiTrack).GetField("_meta", System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
            if (field!=null && field.GetValue(t) is List<MetaEvent> list)
            {
                list.RemoveAll(m=>types.Contains(m.GetType()));
            }
        }
        return conductor;
    }
}
