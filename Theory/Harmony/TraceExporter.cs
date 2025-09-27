using System.Collections.Generic;
using System.Text;

namespace MusicTheory.Theory.Harmony;

public static class TraceExporter
{
    public static string ToMarkdown(IReadOnlyList<KeyEstimator.TraceEntry> trace)
    {
        var sb = new StringBuilder();
        sb.AppendLine("| idx | key | total | stay | max | 2nd | tops | out | vl |");
        sb.AppendLine("|---:|:---:|---:|:---:|---:|---:|---:|---:|:---:|");
        foreach (var t in trace)
        {
            var keyStr = (t.ChosenKey.IsMajor ? "M" : "m") + "@" + (((t.ChosenKey.TonicMidi % 12) + 12) % 12);
            var vl = $"r={(t.VLRangeViolation ? 1 : 0)},s={(t.VLSpacingViolation ? 1 : 0)},o={(t.VLOverlap ? 1 : 0)},p={(t.VLParallelPerfects ? 1 : 0)}";
            sb.AppendLine($"| {t.Index} | {keyStr} | {t.Total} | {(t.StayedDueToHysteresis ? 1 : 0)} | {t.MaxScore} | {t.SecondBestScore} | {t.NumTopKeys} | {t.OutOfKeyPenalty} | {vl} |");
        }
        return sb.ToString();
    }
}
