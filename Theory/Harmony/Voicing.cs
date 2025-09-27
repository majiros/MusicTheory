using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicTheory.Theory.Harmony;

public readonly record struct FourPartVoicing(int S, int A, int T, int B)
{
    public IEnumerable<(Voice voice, int midi)> Notes() => new[]
    {
        (Voice.Soprano, S), (Voice.Alto, A), (Voice.Tenor, T), (Voice.Bass, B)
    };

    public bool IsOrderedTopDown() => S >= A && A >= T && T >= B;
}

public static class VoiceLeadingRules
{
    public static bool HasSpacingViolations(FourPartVoicing v)
    {
        // Spacing: S-A and A-T <= octave; T-B no hard limit (common rule: <= octave too, but looser)
        return (v.S - v.A) > 12 || (v.A - v.T) > 12;
    }

    public static bool HasRangeViolation(FourPartVoicing v)
    {
        foreach (var (voice, midi) in v.Notes())
        {
            var r = VoiceRanges.ForVoice(voice);
            if (!r.InWarnRange(midi)) return true; // hard out of warn range
        }
        return false;
    }

    public static bool HasOverlap(FourPartVoicing prev, FourPartVoicing next)
    {
        // No voice overlapping: each voice must stay within its neighbors' previous positions.
        return next.S < prev.A || next.A > prev.S || next.A < prev.T || next.T > prev.A || next.T < prev.B || next.B > prev.T;
    }

    public static bool HasParallelPerfects(FourPartVoicing a, FourPartVoicing b)
    {
        // Check pairs for P5/P8 parallels by motion direction and interval equality.
        var pairs = new (int a1, int a2, int b1, int b2)[]
        {
            (a.S, a.A, b.S, b.A), (a.S, a.T, b.S, b.T), (a.S, a.B, b.S, b.B),
            (a.A, a.T, b.A, b.T), (a.A, a.B, b.A, b.B),
            (a.T, a.B, b.T, b.B)
        };

        foreach (var (x1,y1,x2,y2) in pairs)
        {
            var intv1 = Math.Abs((x1 - y1) % 12);
            var intv2 = Math.Abs((x2 - y2) % 12);
            // Treat only P8 and P5 as perfect for parallel checks
            bool isPerfect1 = (intv1 % 12) == 0 || (intv1 % 12) == 7;
            bool isPerfect2 = (intv2 % 12) == 0 || (intv2 % 12) == 7;
            if (!isPerfect1 || !isPerfect2) continue;
            int dir1 = Math.Sign(x2 - x1);
            int dir2 = Math.Sign(y2 - y1);
            if (dir1 == dir2 && dir1 != 0) return true;
        }
        return false;
    }
}
