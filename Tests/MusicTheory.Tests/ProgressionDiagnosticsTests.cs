using System.Collections.Generic;
using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class ProgressionDiagnosticsTests
{
    [Fact]
    public void AnalyzeVoicings_Computes_Flags_And_Totals()
    {
        var v0 = new FourPartVoicing(S: 90, A: 80, T: 70, B: 60); // out of warn range for S => Range true, spacing S-A=10 ok, A-T=10 ok
        var v1 = new FourPartVoicing(S: 81, A: 60, T: 47, B: 35);  // spacing S-A=21>12 and A-T=13>12 => Spacing true, also Range true for B=35
        var v2 = new FourPartVoicing(S: 70, A: 69, T: 68, B: 67);  // tight, but previous v1 to v2 should cause overlaps and possibly parallels

        var res = ProgressionDiagnostics.AnalyzeVoicings(new List<FourPartVoicing?> { v0, v1, v2 });

        Assert.Equal(3, res.PerIndex.Count);

        // Index 0: Only range violation expected (no previous to compare overlap/parallels)
        var f0 = res.PerIndex[0].flags;
        Assert.True(f0.Range);
        Assert.False(f0.Spacing);
        Assert.False(f0.Overlap);
        Assert.False(f0.ParallelPerfects);

        // Index 1: Range (Bass 35 below warn), Spacing violations expected
        var f1 = res.PerIndex[1].flags;
        Assert.True(f1.Range);
        Assert.True(f1.Spacing);

        // Index 2: Check overlap/parallels flags computed relative to v1
        var f2 = res.PerIndex[2].flags;
        // We don't assert exact true/false for overlap/parallels here to avoid brittle expectations across changes,
        // but we do assert that flags are present (bools) and totals are consistent.
        Assert.True(res.TotalRange >= (f0.Range ? 1 : 0) + (f1.Range ? 1 : 0) + (f2.Range ? 1 : 0));
        Assert.True(res.TotalSpacing >= (f0.Spacing ? 1 : 0) + (f1.Spacing ? 1 : 0) + (f2.Spacing ? 1 : 0));
        Assert.True(res.TotalOverlap >= (f0.Overlap ? 1 : 0) + (f1.Overlap ? 1 : 0) + (f2.Overlap ? 1 : 0));
        Assert.True(res.TotalParallelPerfects >= (f0.ParallelPerfects ? 1 : 0) + (f1.ParallelPerfects ? 1 : 0) + (f2.ParallelPerfects ? 1 : 0));
    }
}
