using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicTheory.Theory.Time;

/// <summary>
/// 音価虫眼鏡ツール: 四分音符[*1] を基準に、定義済みの代表音価ステップに沿って拡大(+) / 縮小(-) する。
/// 例: 8分三連符[*0.33] → 付点16分[*0.375] → 8分[*0.5] → 4分三連符[*0.66] → 付点8分[*0.75] → 4分[*1] → ...
/// </summary>
public static class NoteValueZoom
{
    public readonly record struct Entry(string Label, Func<Duration> Factory)
    {
        public Duration Create() => Factory();
        public double FactorToQuarter => Create().Ticks / (double)Duration.TicksPerQuarter; // 四分音符=1.0
        public int Ticks => Create().Ticks;
    }

    // 昇順（小→大）。四分音符[*1] を中心に、画像で示されたリストに準拠。
    private static readonly Entry[] _entries = new[]
    {
        new Entry("32分音符[*0.125]", () => DurationFactory.ThirtySecond()),
        new Entry("16分三連符[*0.165]", () => DurationFactory.Tuplet(BaseNoteValue.Sixteenth, new Tuplet(3,2))),
        new Entry("16分音符[*0.25]", () => DurationFactory.Sixteenth()),
        new Entry("8分三連符[*0.33]", () => DurationFactory.Tuplet(BaseNoteValue.Eighth, new Tuplet(3,2))),
        new Entry("付点16分音符[*0.375]", () => DurationFactory.Sixteenth(1)),
        new Entry("8分音符[*0.5]", () => DurationFactory.Eighth()),
        new Entry("4分三連符[*0.66]", () => DurationFactory.Tuplet(BaseNoteValue.Quarter, new Tuplet(3,2))),
        new Entry("付点8分音符[*0.75]", () => DurationFactory.Eighth(1)),
        new Entry("4分音符[*1]", () => DurationFactory.Quarter()),
        new Entry("2分三連符[*1.32]", () => DurationFactory.Tuplet(BaseNoteValue.Half, new Tuplet(3,2))),
        new Entry("付点4分音符[*1.5]", () => DurationFactory.Quarter(1)),
        new Entry("2分音符[*2]", () => DurationFactory.Half()),
        new Entry("付点2分音符[*3]", () => DurationFactory.Half(1)),
        new Entry("全音符[*4]", () => DurationFactory.Whole()),
    };

    public static IReadOnlyList<Entry> Entries => _entries;

    /// <summary>四分音符[*1]基準の倍率（ticks/480）で現在位置に最も近いインデックスを返す（同距離は「大きい方」を優先）。</summary>
    public static int FindNearestIndex(Duration current)
    {
        var ticks = current.Ticks;
        int bestIdx = 0; int bestDiff = Math.Abs(_entries[0].Ticks - ticks);
        for (int i=1;i<_entries.Length;i++)
        {
            int diff = Math.Abs(_entries[i].Ticks - ticks);
            if (diff < bestDiff || (diff==bestDiff && _entries[i].Ticks > _entries[bestIdx].Ticks))
                (bestIdx, bestDiff) = (i, diff);
        }
        return bestIdx;
    }

    /// <summary>ズーム（delta>0:大きく, delta<0:小さく）。範囲外は端にクランプ。</summary>
    public static Duration Zoom(Duration current, int delta)
    {
        int idx = FindNearestIndex(current);
        int next = Math.Clamp(idx + delta, 0, _entries.Length - 1);
        return _entries[next].Create();
    }

    public static Duration ZoomIn(Duration current) => Zoom(current, +1);
    public static Duration ZoomOut(Duration current) => Zoom(current, -1);
}
