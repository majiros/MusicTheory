using System.Linq;
using MusicTheory.Theory.Time;

namespace MusicTheory.Tests;

public class QuantizeAndLayoutTests
{
    [Fact]
    public void StrongBeatQuantize_BiasesToBeat()
    {
        int grid = Duration.TicksPerQuarter/2; // 8分
        int barTicks = Duration.TicksPerQuarter*4;
        var strong = new[]{0, Duration.TicksPerQuarter*2}; // 1拍目 & 3拍目
        long nearStrongTick = Duration.TicksPerQuarter*2 + grid/3; // 3拍目付近
        var q = Quantize.ToGridStrongBeat(nearStrongTick, grid, barTicks, strong, bias:0.8);
        Assert.Equal(Duration.TicksPerQuarter*2, q); // 強拍に吸着
    }

    [Fact]
    public void GroovePattern_AppliesOffsets()
    {
        int grid = Duration.TicksPerQuarter/4; // 16分
        var pattern = new[]{0,5,0,-5}; // 軽いシンコペーション
        long tick = grid * 5; // 6個目 (index5 -> pattern index1)
        long g = Quantize.ApplyGroove(tick, grid, pattern);
        Assert.Equal(tick + 5, g);
    }

    [Fact]
    public void SequenceLayout_InsertsRest()
    {
        var notes = new[]{ (start:0L, note:new Note(DurationFactory.Quarter())), (start:(long)Duration.TicksPerQuarter*2, note:new Note(DurationFactory.Quarter())) };
        var seq = SequenceLayout.InsertRests(notes);
        Assert.Equal(3, seq.Count);
    Assert.Contains(seq, e => e.start==Duration.TicksPerQuarter && e.entity is Rest);
    }

    [Fact]
    public void SequenceLayout_SplitsAtInferredTupletBoundaries()
    {
        // 入力: 8分(240) + 16分(120) + 16分(120) = 480 を 3:2 連符として等分 160 ずつに切り分け
        var n1 = new Note(DurationFactory.Eighth());
        var n2 = new Note(DurationFactory.Sixteenth());
        var n3 = new Note(DurationFactory.Sixteenth());
        var seq = SequenceLayout.InsertRests(new[]{ (0L, n1), ((long)240, n2), ((long)360, n3) }, normalizeRests: true, advancedSplit: false, allowSplitTuplets: true);
        // 160,160,160 の3要素連続になるはず
        Assert.Equal(3, seq.Count);
        Assert.All(seq, e => Assert.Equal(160, e.entity.Duration.Ticks));
        Assert.Equal(new[]{0,160,320}, seq.Select(e=> (int)e.start).ToArray());
    }

    [Fact]
    public void SequenceLayout_SplitsAtExtendedTuplet_5over2()
    {
        // 合計 2分音符長 (960) を 5:2 で 5 等分 => 各要素 192 ticks
        var a = new Note(DurationFactory.Quarter());   // 480
        var b = new Note(DurationFactory.Quarter());   // 480 => 960
        var seq = SequenceLayout.InsertRests(new[]{ (0L, a), ((long)480, b) }, normalizeRests: true, advancedSplit: false, allowSplitTuplets: true, extendedTuplets: true);
        Assert.Equal(5, seq.Count);
        Assert.All(seq, e => Assert.Equal(192, e.entity.Duration.Ticks));
        Assert.Equal(new[]{0,192,384,576,768}, seq.Select(e=> (int)e.start).ToArray());
    }

    [Fact]
    public void SequenceLayout_Flexible_7over8_With_StrongBeat_Quantize()
    {
        // 例: 8分(240)+16分(120)+16分(120)+8分(240)+8分(240) = 合計 960
        // 7:8 の柔軟推論が働く前提で強拍吸着グリッド(240)を通し、その後の分割が総長を保つことを確認
        var parts = new[]{ DurationFactory.Eighth(), DurationFactory.Sixteenth(), DurationFactory.Sixteenth(), DurationFactory.Eighth(), DurationFactory.Eighth() };
        long t=0; var notes = new System.Collections.Generic.List<(long, Note)>();
        foreach (var d in parts) { notes.Add((t, new Note(d))); t += d.Ticks; }
        // 強拍クオンタイズ（4/4, bar=1920, 強拍=0,960）で開始位置を軽く整える
        int grid = 120; int barTicks=1920; var strong = new[]{0,960};
        var quantized = notes.Select(n => (Quantize.ToGridStrongBeat(n.Item1, grid, barTicks, strong, bias:0.5), n.Item2)).ToList();
        // 分割（柔軟推論 + extended）
        var seq = SequenceLayout.InsertRests(quantized, normalizeRests: true, advancedSplit: false, allowSplitTuplets: true, extendedTuplets: true);
        // 総長は 960 を維持し要素の合計も一致
        var total = seq.Sum(e=>e.entity.Duration.Ticks);
        Assert.Equal(960, total);
        // セグメント個数が過小（1や2）ではないこと（7:8 の近似的等分が行われ分割数が増える）
        Assert.True(seq.Count >= 4);
    }

    [Fact]
    public void SequenceLayout_LongRun_Performance_Smoke()
    {
        // 4/4 で 64小節相当の長尺を 8分と16分のランダム列で生成し、分割が完了し総長を保持することのみ確認
        var rnd = new System.Random(123);
        var parts = new System.Collections.Generic.List<Duration>();
        int barTicks = 1920; int totalBars = 64; int target = barTicks * totalBars; int acc=0;
        while (acc < target)
        {
            var d = (rnd.NextDouble() < 0.5) ? DurationFactory.Eighth() : DurationFactory.Sixteenth();
            if (acc + d.Ticks > target) d = Duration.FromTicks(target - acc);
            parts.Add(d); acc += d.Ticks;
        }
        long t=0; var notes = new System.Collections.Generic.List<(long, Note)>();
        foreach (var d in parts){ notes.Add((t, new Note(d))); t+=d.Ticks; }
        var seq = SequenceLayout.InsertRests(notes, normalizeRests: true, advancedSplit:false, allowSplitTuplets:true, extendedTuplets:true);
        var total = seq.Sum(e=>e.entity.Duration.Ticks);
        Assert.Equal(target, total);
    }
}
