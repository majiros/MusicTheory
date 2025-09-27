using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicTheory.Theory.Time;

/// <summary>
/// NoteValueZoom を複数ノート選択へ適用するための補助ユーティリティ。
/// </summary>
public static class NoteValueZoomSelection
{
    /// <summary>選択の中央値（近傍インデックス）をターゲットとして返す。</summary>
    public static int ChooseTargetIndexByMedian(IEnumerable<Note> notes)
    {
        var idxs = notes.Select(n => NoteValueZoom.FindNearestIndex(n.Duration)).OrderBy(x=>x).ToArray();
        if (idxs.Length == 0) return NoteValueZoom.FindNearestIndex(DurationFactory.Quarter());
        return idxs[idxs.Length/2];
    }

    /// <summary>全ノートを同一ステップへスナップ（開始位置は維持）。</summary>
    public static IEnumerable<(long start, Note note)> SnapAllToIndexMaintainStarts(IEnumerable<(long start, Note note)> sel, int targetIndex)
    {
        var entry = NoteValueZoom.Entries[Math.Clamp(targetIndex, 0, NoteValueZoom.Entries.Count-1)];
        var d = entry.Create();
        foreach (var (start, n) in sel)
            yield return (start, new Note(d, n.Pitch, n.Velocity, n.Channel));
    }

    /// <summary>
    /// 全ノートを同一ステップへスナップ（選択の総スパン長を保持）。
    /// 先頭開始から最終ノート終端までの長さを保持し、最後のノートで差分を吸収（最小1tickにクランプ）。
    /// </summary>
    public static IEnumerable<(long start, Note note)> SnapAllToIndexPreserveTotalSpan(IEnumerable<(long start, Note note)> sel, int targetIndex)
    {
        var list = sel.OrderBy(x=>x.start).ToList(); if (list.Count==0) return list;
        long spanStart = list.First().start;
        long spanEnd = list.Last().start + list.Last().note.Duration.Ticks;
        long span = Math.Max(0, spanEnd - spanStart);

        var entry = NoteValueZoom.Entries[Math.Clamp(targetIndex, 0, NoteValueZoom.Entries.Count-1)];
        int stepTicks = entry.Create().Ticks;

        var output = new List<(long, Note)>();
        for (int i=0;i<list.Count;i++)
        {
            var (s, n) = list[i];
            int durTicks = (i==list.Count-1)
                ? (int)Math.Max(1, span - (s - spanStart) - (long)stepTicks * (list.Count-1==0?0:list.Count-1)) // fallback if single note
                : stepTicks;
            // 再計算: 最終ノート以外は stepTicks、最終ノートはスパンを満たすよう調整
            if (i < list.Count-1)
                output.Add((s, new Note(Duration.FromTicks(stepTicks), n.Pitch, n.Velocity, n.Channel)));
            else
            {
                long usedBefore = 0;
                for (int k=0;k<list.Count-1;k++) usedBefore += stepTicks;
                int lastTicks = (int)Math.Max(1, span - (list[i].start - spanStart) - (long)0); // recompute below
                // 最終長さは spanEnd' = lastStart + lastTicks になる。ここでは lastTicks = span - (lastStart - spanStart).
                lastTicks = (int)Math.Max(1, span - (list[i].start - spanStart));
                output.Add((s, new Note(Duration.FromTicks(lastTicks), n.Pitch, n.Velocity, n.Channel)));
            }
        }
        return output;
    }

    /// <summary>選択へ一括ズーム（delta）。preserveTotalSpan=true で総スパン保持。</summary>
    public static IEnumerable<(long start, Note note)> ZoomSelection(IEnumerable<(long start, Note note)> sel, int delta, bool preserveTotalSpan=false)
    {
        if (!preserveTotalSpan)
        {
            foreach (var (s,n) in sel)
                yield return (s, new Note(NoteValueZoom.Zoom(n.Duration, delta), n.Pitch, n.Velocity, n.Channel));
            yield break;
        }
        var list = sel.OrderBy(x=>x.start).ToList(); if (list.Count==0){ yield break; }
        long spanStart = list.First().start;
        long spanEnd = list.Last().start + list.Last().note.Duration.Ticks;
        long span = Math.Max(0, spanEnd - spanStart);
        for (int i=0;i<list.Count;i++)
        {
            var (s,n) = list[i];
            var z = NoteValueZoom.Zoom(n.Duration, delta);
            if (i < list.Count-1)
                yield return (s, new Note(z, n.Pitch, n.Velocity, n.Channel));
            else
            {
                int lastTicks = (int)Math.Max(1, span - (s - spanStart));
                yield return (s, new Note(Duration.FromTicks(lastTicks), n.Pitch, n.Velocity, n.Channel));
            }
        }
    }
}
