using System.Collections.Generic;

namespace MusicTheory.Theory.Harmony;

public readonly record struct VoiceLeadingFlags(bool Range, bool Spacing, bool Overlap, bool ParallelPerfects);

public readonly record struct ProgressionDiagnosticsResult(
    List<(int index, VoiceLeadingFlags flags)> PerIndex,
    int TotalRange,
    int TotalSpacing,
    int TotalOverlap,
    int TotalParallelPerfects
);

public static class ProgressionDiagnostics
{
    public static ProgressionDiagnosticsResult AnalyzeVoicings(IReadOnlyList<FourPartVoicing?> voicings)
    {
        var list = new List<(int, VoiceLeadingFlags)>(voicings.Count);
        int sumR = 0, sumS = 0, sumO = 0, sumP = 0;
        for (int i = 0; i < voicings.Count; i++)
        {
            bool r = false, s = false, o = false, p = false;
            if (voicings[i] is FourPartVoicing v)
            {
                r = VoiceLeadingRules.HasRangeViolation(v);
                s = VoiceLeadingRules.HasSpacingViolations(v);
                if (i > 0 && voicings[i - 1] is FourPartVoicing prev)
                {
                    o = VoiceLeadingRules.HasOverlap(prev, v);
                    p = VoiceLeadingRules.HasParallelPerfects(prev, v);
                }
            }
            if (r) sumR++;
            if (s) sumS++;
            if (o) sumO++;
            if (p) sumP++;
            list.Add((i, new VoiceLeadingFlags(r, s, o, p)));
        }
        return new ProgressionDiagnosticsResult(list, sumR, sumS, sumO, sumP);
    }
}
