using System.Collections.Generic;
using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class TraceExporterTests
{
    [Fact]
    public void ToMarkdown_Emits_Header_And_Rows()
    {
        var trace = new List<KeyEstimator.TraceEntry>
        {
            new KeyEstimator.TraceEntry
            {
                Index = 0,
                ChosenKey = new Key(60, true), // C major => M@0
                BaseWindowScore = 0,
                PrevKeyBias = 0,
                InitialKeyBias = 0,
                DominantTriad = 0,
                DominantSeventh = 0,
                Cadence = 0,
                Pivot = 0,
                SecondaryDominantTriad = 0,
                SecondaryDominantSeventh = 0,
                SecondaryResolution = 0,
                Total = 10,
                StayedDueToHysteresis = true,
                MaxScore = 9,
                SecondBestScore = 8,
                NumTopKeys = 2,
                TopKeysSummary = "M@0:9,m@9:9",
                OutOfKeyPenalty = -1,
                VLRangeViolation = false,
                VLSpacingViolation = false,
                VLOverlap = false,
                VLParallelPerfects = false,
            },
            new KeyEstimator.TraceEntry
            {
                Index = 1,
                ChosenKey = new Key(61, false), // C# minor => m@1
                BaseWindowScore = 0,
                PrevKeyBias = 0,
                InitialKeyBias = 0,
                DominantTriad = 0,
                DominantSeventh = 0,
                Cadence = 0,
                Pivot = 0,
                SecondaryDominantTriad = 0,
                SecondaryDominantSeventh = 0,
                SecondaryResolution = 0,
                Total = 5,
                StayedDueToHysteresis = false,
                MaxScore = 7,
                SecondBestScore = 6,
                NumTopKeys = 1,
                TopKeysSummary = "m@1:7",
                OutOfKeyPenalty = 0,
                VLRangeViolation = true,
                VLSpacingViolation = true,
                VLOverlap = true,
                VLParallelPerfects = true,
            }
        };

        var md = TraceExporter.ToMarkdown(trace);

        Assert.Contains("| idx | key | total | stay | max | 2nd | tops | out | vl |", md);
        Assert.Contains("|---:|:---:|---:|:---:|---:|---:|---:|---:|:---:|", md);
        Assert.Contains("| 0 | M@0 | 10 | 1 | 9 | 8 | 2 | -1 | r=0,s=0,o=0,p=0 |", md);
        Assert.Contains("| 1 | m@1 | 5 | 0 | 7 | 6 | 1 | 0 | r=1,s=1,o=1,p=1 |", md);
    }
}
