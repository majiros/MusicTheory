using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicTheory.Theory.Time;

/// <summary>
/// グルーヴ / ヒューマナイズ適用ユーティリティ。
/// </summary>
public static class GrooveHumanize
{
    /// <summary>位置配列にグルーヴテンプレート (tick オフセット) を適用。</summary>
    public static IEnumerable<long> ApplyGroove(IEnumerable<long> ticks, int gridTicks, IReadOnlyList<int> pattern)
        => ticks.Select(t => Quantize.ApplyGroove(t, gridTicks, pattern));

    /// <summary>スイング + グルーヴ + 強拍バイアスをまとめて適用。</summary>
    public static long ProcessOne(long tick, int gridTicks, double swingRatio, int swingSubdivisionTicks, int ticksPerBar, IReadOnlyList<int> strongBeats, double strongBias, IReadOnlyList<int> groovePattern)
    {
        var q = Quantize.ToGrid(tick, gridTicks);
        if (swingRatio>0 && swingSubdivisionTicks>0)
            q = Quantize.ApplySwing(q, swingSubdivisionTicks, swingRatio);
        if (strongBeats.Count>0)
            q = Quantize.ToGridStrongBeat(q, gridTicks, ticksPerBar, strongBeats, strongBias);
        if (groovePattern.Count>0)
            q = Quantize.ApplyGroove(q, gridTicks, groovePattern);
        return q;
    }

    /// <summary>タイミングジッター (±maxJitterTicks の整数一様) を付与。</summary>
    public static IEnumerable<long> AddTimingJitter(IEnumerable<long> ticks, int maxJitterTicks, int? seed=null)
    {
        if (maxJitterTicks<=0) return ticks;
        var rnd = seed.HasValue? new Random(seed.Value) : new Random();
        return ticks.Select(t => t + rnd.Next(-maxJitterTicks, maxJitterTicks+1));
    }

    /// <summary>ベロシティヒューマナイズ (割合 ±percent / clamp 0..127)。</summary>
    public static IEnumerable<int> HumanizeVelocity(IEnumerable<int> velocities, double percent, int? seed=null)
    {
        if (percent<=0) return velocities;
        var rnd = seed.HasValue? new Random(seed.Value) : new Random();
        return velocities.Select(v => (int)Math.Clamp(v + v * (rnd.NextDouble()*2-1)*percent, 0, 127));
    }
}

/// <summary>量子化プリセット (スイングとグルーヴ合成) を提供。</summary>
public static class QuantizePresets
{
    public record Preset(string Name, int GridTicks, double SwingRatio, int SwingSubdivisionTicks, IReadOnlyList<int> GroovePattern, IReadOnlyList<int> StrongBeats, double StrongBias);

    public static Preset Straight16 => new("Straight16", Duration.TicksPerQuarter/4, 0, 0, Array.Empty<int>(), Array.Empty<int>(), 0);
    public static Preset Swing8 => new("Swing8", Duration.TicksPerQuarter/2, 0.5, Duration.TicksPerQuarter/2, Array.Empty<int>(), Array.Empty<int>(), 0);
    public static Preset Funk16 => new("Funk16", Duration.TicksPerQuarter/4, 0.0, 0, new[]{0,5,0,-5}, new[]{0, Duration.TicksPerQuarter*2}, 0.2);

    public static long Apply(Preset p, long tick, int ticksPerBar)
        => GrooveHumanize.ProcessOne(tick, p.GridTicks, p.SwingRatio, p.SwingSubdivisionTicks, ticksPerBar, p.StrongBeats, p.StrongBias, p.GroovePattern);
}
