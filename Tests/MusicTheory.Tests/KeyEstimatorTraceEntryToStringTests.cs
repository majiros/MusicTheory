using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class KeyEstimatorTraceEntryToStringTests
{
    [Fact]
    public void ToString_Contains_Key_And_Fields()
    {
        var t = new KeyEstimator.TraceEntry
        {
            Index = 3,
            ChosenKey = new Key(67, true), // G major => M@7
            BaseWindowScore = 4,
            PrevKeyBias = 1,
            InitialKeyBias = 0,
            DominantTriad = 2,
            DominantSeventh = 0,
            Cadence = 0,
            Pivot = 1,
            SecondaryDominantTriad = 0,
            SecondaryDominantSeventh = 0,
            SecondaryResolution = 0,
            Total = 8,
            StayedDueToHysteresis = true,
            MaxScore = 9,
            SecondBestScore = 8,
            NumTopKeys = 1,
            TopKeysSummary = "M@7:9",
            OutOfKeyPenalty = 0,
            VLRangeViolation = false,
            VLSpacingViolation = false,
            VLOverlap = false,
            VLParallelPerfects = false,
        };

        var s = t.ToString();
        Assert.Contains("idx=3", s);
        Assert.Contains("key=M@7", s);
        Assert.Contains("base=4", s);
        Assert.Contains("prev=1", s);
        Assert.Contains("total=8", s);
        Assert.Contains("max=9", s);
        Assert.Contains("2nd=8", s);
    }
}
