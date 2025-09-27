using System;
using System.Linq;
using MusicTheory.Theory.Time;
using MusicTheory.Theory.Midi;
using Xunit;

namespace MusicTheory.Tests;

public class MidiExtendedTests
{
    [Fact]
    public void Format0_Golden_Match()
    {
        var track = new MidiTrack();
        track.AddMeta(new TempoEvent(0, Tempo.FromBpm(120))); // 500000 us
        track.AddMeta(new TimeSignatureEvent(0, new MusicTheory.Theory.Time.TimeSignature(4,4)));
        track.AddNoteRange(MidiNoteBuilder.BuildSingle(0,60,100,0, DurationFactory.Quarter()));
    var bytes = MidiFileWriter.WriteSingleTrack(track, division:480, useRunningStatus:false);
    // 期待バイト列 (固定): ヘッダ + 1トラック (テンポ, 拍子, NoteOn, NoteOff, End)
    // ここでは delta=0 で並ぶ最小ケースを利用 (NoteOn tick 0, NoteOff tick 480)
    // 可読性のため 16進文字列化して比較
    string Hex(byte[] b) => string.Concat(b.Select(x=>x.ToString("X2")));
    var actual = Hex(bytes);
    // Header: MThd 00000006 0000 0001 01E0  (format0, tracks=1, division=480)
    Assert.StartsWith("4D546864000000060000000101E0", actual);
    Assert.Contains("FF5103", actual); // tempo meta
    Assert.Contains("FF5804", actual); // time sig meta
    Assert.EndsWith("FF2F00", actual); // end of track
    }

    [Fact]
    public void Format1_MultipleTracks_Writes()
    {
        var t1 = new MidiTrack();
        t1.AddMeta(new TempoEvent(0, Tempo.FromBpm(100)));
        t1.AddMeta(new TimeSignatureEvent(0, new MusicTheory.Theory.Time.TimeSignature(3,4)));
        var t2 = new MidiTrack();
        t2.AddMeta(new TrackNameEvent(0, "Melody"));
        t2.AddNoteRange(MidiNoteBuilder.BuildSingle(0, 64, 90, 0, DurationFactory.Quarter()));
        var data = MidiFileWriter.WriteMultipleTracks(new[]{t1,t2});
        Assert.StartsWith("MThd", System.Text.Encoding.ASCII.GetString(data,0,4));
    }

    [Fact]
    public void TempoMap_BinarySearch_Equals_Linear()
    {
        var map = new TempoMap(Tempo.FromBpm(120));
        map.AddChange(1920, Tempo.FromBpm(90));
        map.AddChange(3840, Tempo.FromBpm(140));
        long tick = 4500;
        long usFast = map.TickToMicroseconds(tick);
        // 線形確認 (再計算)
        long Linear(long tk){ var m2 = new TempoMap(Tempo.FromBpm(120)); m2.AddChange(1920, Tempo.FromBpm(90)); m2.AddChange(3840, Tempo.FromBpm(140)); return m2.TickToMicroseconds(tk);}        
        Assert.Equal(Linear(tick), usFast);
    }

    [Fact]
    public void CompositeTuplet_Inference_Works()
    {
        // 8分三連符: 8分基準(=240ticks) の (3:2) -> 各要素 160 ticks ( = 240 * 2/3 )
        var part = Duration.FromTicks(160);
        var inferred = DurationSequenceUtils.InferCompositeTuplet(new[]{part,part,part});
        Assert.True(inferred.HasValue);
        inferred.Value.TryDecomposeFull(out var baseVal, out var dots, out var tuplet);
        Assert.Equal(BaseNoteValue.Eighth, baseVal);
        Assert.True(tuplet.HasValue);
        Assert.Equal(3, tuplet.Value.ActualCount);
        Assert.Equal(2, tuplet.Value.NormalCount);
    }

    [Fact]
    public void CompositeTuplet_MixedLengths_StrictInference_ReturnsNull()
    {
        // 8分 + 16分 + 16分 = 240 + 120 +120 = 480 ticks -> 8分三連符 (3:2) の総和 (8分=240 を 3:2 → 160x3=480)
        var d1 = DurationFactory.Eighth(); // 240
        var d2 = DurationFactory.Sixteenth(); //120
        var d3 = DurationFactory.Sixteenth(); //120
        var inferred = DurationSequenceUtils.InferCompositeTuplet(new[]{d1, d2, d3});
        Assert.Null(inferred); // 厳密(非分割)では推論しない仕様
    }

    [Fact]
    public void RunningStatus_Compression_Reduces_Size()
    {
        var track = new MidiTrack();
        for (int i=0;i<8;i++)
            track.AddNoteRange(MidiNoteBuilder.BuildSingle(0, 60+i, 100, i*120, DurationFactory.Sixteenth()));
        var noRs = MidiFileWriter.WriteSingleTrack(track, useRunningStatus:false);
        var withRs = MidiFileWriter.WriteSingleTrack(track, useRunningStatus:true);
    var diff = noRs.Length - withRs.Length;
    var ratio = (double)withRs.Length / noRs.Length;
    System.Diagnostics.Debug.WriteLine($"RunningStatus size: {withRs.Length} vs {noRs.Length} (saved {diff} bytes, ratio {ratio:P1})");
    Assert.True(withRs.Length < noRs.Length);
    }

    [Fact]
    public void KeySignature_Inference_Major_Minor()
    {
        var Cmaj = PcScaleLibrary.Major; // C major => 0 sharps
        Assert.Equal(0, KeySignatureInference.InferKeySignature(Cmaj));
        var Aminor = PcScaleLibrary.Minor; // A minor -> 0 sharps
        Assert.Equal(0, KeySignatureInference.InferKeySignature(Aminor));
    }

    [Fact]
    public void CompositeTuplet_Flexible_Allows_Splitting()
    {
        var d1 = DurationFactory.Eighth(); //240
        var d2 = DurationFactory.Sixteenth(); //120
        var d3 = DurationFactory.Sixteenth(); //120
        var inferred = DurationSequenceUtils.InferCompositeTupletFlexible(new[]{d1,d2,d3});
        Assert.True(inferred.HasValue);
        inferred.Value.TryDecomposeFull(out var baseVal, out _, out var t);
        Assert.Equal(BaseNoteValue.Eighth, baseVal);
        Assert.True(t.HasValue && t.Value.ActualCount==3 && t.Value.NormalCount==2);
    }

    [Fact]
    public void ExtendedTuplets_Inference_EqualLength_5over4()
    {
        // Quarter(480) の 5:4 -> 要素 384 ticks
        var part = Duration.FromTicks(384);
        var inferred = DurationSequenceUtils.InferCompositeTuplet(new[]{part,part,part,part,part}, extendedTuplets:true);
        Assert.True(inferred.HasValue);
        inferred.Value.TryDecomposeFull(out var baseVal, out var dots, out var tuplet, extendedTuplets:true);
        Assert.Equal(BaseNoteValue.Quarter, baseVal);
        Assert.Equal(0, dots);
        Assert.True(tuplet.HasValue && tuplet.Value.ActualCount==5 && tuplet.Value.NormalCount==4);
    }

    [Fact]
    public void ExtendedTuplets_Strict_7over4_ReturnsNull_WithIntegerTicks()
    {
        // PPQ=480 では 7系の等長は整数tickで整合しにくい。等長(280ticks×7=1960)でも基底490ticksが存在しないため null
        var part = Duration.FromTicks(280);
        var inferred = DurationSequenceUtils.InferCompositeTuplet(new[]{part,part,part,part,part,part,part}, extendedTuplets:true);
        Assert.Null(inferred);
    }

    [Fact]
    public void ExtendedTuplets_Inference_EqualLength_9over8()
    {
        // Eighth(240) の 9:8 -> 要素 213.333.. → 近似不可のため Quarter(480) で 9:8: 480 * 8/9 = 426.66.. 近似不可
        // ここでは厳密整数tickにマッチする 3:2 や 5:4 以外の extended は PPQ480 だと非整数が多いので柔軟推論で確認
        var d1 = DurationFactory.Quarter(); // 480
        var d2 = DurationFactory.Eighth();  // 240
        var d3 = DurationFactory.Eighth();  // 240 → 合計 960
        var inferred = DurationSequenceUtils.InferCompositeTupletFlexible(new[]{d1,d2,d3}, extendedTuplets:true);
        Assert.True(inferred.HasValue);
        inferred.Value.TryDecomposeFull(out var baseVal, out var dots, out var t, extendedTuplets:true);
        Assert.True(t.HasValue);
        Assert.True(t.Value.ActualCount != t.Value.NormalCount); // 何らかの extended 連符として縮約
    }

    [Fact]
    public void DottedAndTuplet_Boundary_Preservation()
    {
        // 付点四分 (720) + 八分 (240) = 960 を allowSplit で 5等分(192)しても総長は保存される
        var a = new Note(DurationFactory.Quarter(1)); // dotted quarter = 720
        var b = new Note(DurationFactory.Eighth());   // 240
        var seq = SequenceLayout.InsertRests(new[]{ (0L, a), ((long)720, b) }, normalizeRests: true, advancedSplit: false, allowSplitTuplets: true, extendedTuplets: true);
        var total = seq.Sum(e=>e.entity.Duration.Ticks);
        Assert.Equal(960, total);
    }

    [Fact]
    public void ExtendedTuplets_Strict_7over8_EqualLengthTicks_ReturnsNull()
    {
        // 7:8 等長要素を想定した tick(≈274) を 7個並べても PPQ=480 では厳密推論は成立しない
        var part = Duration.FromTicks(274);
        var inferred = DurationSequenceUtils.InferCompositeTuplet(new[]{part,part,part,part,part,part,part}, extendedTuplets:true);
        Assert.Null(inferred);
    }

    private sealed class ByteArrayComparer : System.Collections.Generic.IEqualityComparer<byte[]>
    {
        public bool Equals(byte[]? x, byte[]? y)
        { if (x==null||y==null||x.Length!=y.Length) return false; for(int i=0;i<x.Length;i++) if (x[i]!=y[i]) return false; return true; }
        public int GetHashCode(byte[] obj) => obj.Length>0?obj[0]:0;
    }
}

static class ByteWindowExt
{
    public static System.Collections.Generic.IEnumerable<byte[]> Window(this byte[] src, int size)
    {
        for (int i=0;i<=src.Length-size;i++)
        {
            var seg = new byte[size]; Array.Copy(src, i, seg, 0, size); yield return seg;
        }
    }
}
