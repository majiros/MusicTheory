using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicTheory.Theory.Time;

/// <summary>
/// Note シーケンスのレイアウト: ギャップを自動的に Rest で埋め、必要なら正規化。
/// 現状は単声 (モノフォニック) 前提。重なりがある場合は開始順に優先し後続を切り詰めずそのまま並列とする。
/// </summary>
public static class SequenceLayout
{
    public static IReadOnlyList<(long start, IDurationEntity entity)> InsertRests(
        IEnumerable<(long start, Note note)> notes,
        bool normalizeRests = true,
        bool advancedSplit = false,
        bool allowSplitTuplets = false,
        bool extendedTuplets = false)
    {
        var ordered = notes.OrderBy(n=>n.start).ToList();
        var result = new List<(long, IDurationEntity)>();
        long cursor = 0;
        foreach (var (start, note) in ordered)
        {
            if (start > cursor)
            {
                var gap = start - cursor;
                var rest = RestFactory.FromTicks((int)gap);
                var restSeq = new List<IDurationEntity>{ rest };
                if (normalizeRests)
                    restSeq = DurationSequenceUtils.NormalizeRests(restSeq, advancedSplit: advancedSplit, allowSplit: allowSplitTuplets, extendedTuplets: extendedTuplets).ToList();
                foreach (var r in restSeq)
                    result.Add((cursor, r));
            }
            result.Add((start, note));
            var end = start + note.Duration.Ticks;
            if (end > cursor) cursor = end; // 単声前提: 重なる場合は長い方が cursor を進める
        }
        // 連続区間に対して連符推論に基づく等分割 (allowSplitTuplets) を適用
        if (allowSplitTuplets && result.Count > 0)
            result = SplitRunsByInferredTuplet(result, extendedTuplets).ToList();
        return result;
    }

    private static IEnumerable<(long start, IDurationEntity entity)> SplitRunsByInferredTuplet(
        IReadOnlyList<(long start, IDurationEntity entity)> timeline,
        bool extendedTuplets)
    {
        var output = new List<(long, IDurationEntity)>();
        int i = 0;
        while (i < timeline.Count)
        {
            // 連続ラン収集: start が直前 end と等しいものをひとかたまりにする (重なりや飛びは境界)
            var run = new List<(long start, IDurationEntity entity)>();
            long runStart = timeline[i].start;
            long cursor = runStart;
            int j = i;
            while (j < timeline.Count)
            {
                var (s, e) = timeline[j];
                var durTicks = e.Duration.Ticks;
                if (s != cursor) break; // ギャップ or オーバーラップ -> ラン切断
                run.Add((s, e));
                cursor = s + durTicks;
                j++;
            }

            // 推論して分割適用
            var parts = run.Select(x => x.entity.Duration).ToList();
            // 単一要素相当のランは対象外
            if (parts.Count == 0) { i = j; continue; }
            int totalTicks = parts.Sum(p=>p.Ticks);
            int minTicks = parts.Min(p=>p.Ticks);
            if (totalTicks < minTicks * 2)
            {
                for (int k = i; k < j; k++) output.Add(timeline[k]);
                i = j; continue;
            }
            var inferred = DurationSequenceUtils.InferCompositeTupletFlexible(parts, extendedTuplets);
            if (inferred.HasValue)
            {
                int total = totalTicks;
                int elemTicks = inferred.Value.Ticks; // 要素長
                if (TryChooseGroupTuplet(total, elemTicks, extendedTuplets, out var baseVal, out var dots, out Tuplet? tupletNullable))
                {
                    var tuplet = tupletNullable!.Value;
                    long segStart = runStart;
                    for (int seg = 0; seg < tuplet.ActualCount; seg++)
                    {
                        var ent = FindEntityAt(run, segStart);
                        var segDur = DurationFactory.Tuplet(baseVal, tuplet, dots);
                        output.Add((segStart, RebuildEntity(ent, segDur)));
                        segStart += elemTicks;
                    }
                    i = j; continue;
                }
                // フォールバック: 純粋に elemTicks で等分割
                if (elemTicks > 0 && total % elemTicks == 0)
                {
                    int segCount = total / elemTicks;
                    long segStart = runStart;
                    for (int seg = 0; seg < segCount; seg++)
                    {
                        var ent = FindEntityAt(run, segStart);
                        var segDur = Duration.FromTicks(elemTicks);
                        output.Add((segStart, RebuildEntity(ent, segDur)));
                        segStart += elemTicks;
                    }
                    i = j; continue;
                }
                // 決定不能ならそのまま通す
                for (int k = i; k < j; k++) output.Add(timeline[k]);
                i = j; continue;
            }

            // 分割しない場合はそのまま出力
            for (int k = i; k < j; k++) output.Add(timeline[k]);
            i = j;
        }
        return output;
    }

    private static IDurationEntity SliceEntity(IDurationEntity entity, int sliceTicks)
    {
        var d = Duration.FromTicks(sliceTicks);
        return entity switch
        {
            Note n => new Note(d, n.Pitch, n.Velocity, n.Channel),
            Rest => new Rest(d),
            _ => entity
        };
    }

    private static IDurationEntity RebuildEntity(IDurationEntity entity, Duration duration)
    {
        return entity switch
        {
            Note n => new Note(duration, n.Pitch, n.Velocity, n.Channel),
            Rest => new Rest(duration),
            _ => entity
        };
    }

    private static bool TryChooseGroupTuplet(int totalTicks, int elemTicks, bool extendedTuplets, out BaseNoteValue baseVal, out int dots, out Tuplet? tuplet)
    {
        baseVal = default; dots = 0; tuplet = null;
        if (elemTicks <= 0 || totalTicks <= 0) return false;
        // 候補連符集合
        var tuplets = extendedTuplets
            ? GenerateTupletsExtended()
            : GenerateTupletsCommon();
        // 優先: normal=4 -> 2 -> その他、かつ ActualCount に好みの順序を付与 (5,3,7...)
        int PrefNormal(int n) => n==4?0 : n==2?1 : 2;
        int IndexActual(int a)
        {
            int[] pref = new[]{5,3,7,9,11,4,6,8,10,12,2};
            int idx = Array.IndexOf(pref, a);
            return idx >= 0 ? idx : pref.Length + a;
        }
        foreach (var t in tuplets
                     .OrderBy(x=>PrefNormal(x.NormalCount))
                     .ThenBy(x=>IndexActual(x.ActualCount)))
        {
            if (totalTicks % t.NormalCount != 0) continue;
            int baseTicks = totalTicks / t.NormalCount;
            // elem 一致判定
            double ideal = baseTicks * t.Factor; int idealTicks = (int)Math.Round(ideal);
            if (Math.Abs(ideal - idealTicks) > 0.0001) continue;
            if (idealTicks != elemTicks) continue;
            // baseTicks を (baseValue + dots) で表現可能か
            if (TryFindBaseByTicks(baseTicks, out baseVal, out dots)) { tuplet = t; return true; }
        }
        return false;
    }

    private static bool TryFindBaseByTicks(int ticks, out BaseNoteValue baseVal, out int dots)
    {
        foreach (BaseNoteValue v in Enum.GetValues(typeof(BaseNoteValue)))
        {
            for (int d=0; d<=Duration.MaxDots; d++)
            {
                if (Duration.FromBase(v, d).Ticks == ticks)
                { baseVal = v; dots = d; return true; }
            }
        }
        baseVal = default; dots = 0; return false;
    }

    private static Tuplet[] GenerateTupletsCommon() => new[]{ new Tuplet(3,2), new Tuplet(5,4), new Tuplet(7,4), new Tuplet(5,2), new Tuplet(7,8), new Tuplet(9,8)};
    private static Tuplet[] GenerateTupletsExtended()
    {
        var list = new List<Tuplet>();
        for (int normal=2; normal<=12; normal++) for (int actual=2; actual<=12; actual++) if (actual!=normal) list.Add(new Tuplet(actual, normal));
        return list.ToArray();
    }

    private static IDurationEntity FindEntityAt(List<(long start, IDurationEntity entity)> run, long time)
    {
        // run は start 昇順・連続。最後の start<=time の要素を返す。
        IDurationEntity ent = run[0].entity;
        foreach (var (s,e) in run)
        {
            if (s > time) break;
            ent = e;
        }
        return ent;
    }
}
