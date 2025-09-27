using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicTheory.Theory.Analysis;

public static class AnalysisUtils
{
    // Pitch class normalization helpers
    public static int Mod12(int v) => ((v % 12) + 12) % 12;

    // Rotate set so candidateRoot becomes 0
    public static int[] NormalizeToRoot(IEnumerable<int> pcs, int candidateRoot)
        => pcs.Select(pc => Mod12(pc - candidateRoot)).Distinct().OrderBy(x=>x).ToArray();

    // Canonical signature: hyphen-joined sorted pc list (e.g., 0-3-7-10)
    public static string Signature(IEnumerable<int> pcs)
        => string.Join('-', pcs.Select(Mod12).Distinct().OrderBy(x=>x));
}
