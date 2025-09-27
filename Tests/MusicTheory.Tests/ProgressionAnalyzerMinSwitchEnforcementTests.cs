using System.Collections.Generic;
using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class ProgressionAnalyzerMinSwitchEnforcementTests
{
    [Fact]
    public void MinSwitchIndex_Enforces_Stay_On_Previous_Key_In_Trace_Overload()
    {
        var pcs = new List<int[]>
        {
            new[] { 0, 4, 7 },
            new[] { 7, 11, 2 },
            new[] { 0, 4, 7 },
        };
        var key = new Key(60, true);
        var opt = new KeyEstimator.Options { CollectTrace = true, Window = 1, MinSwitchIndex = 10 };

        var (_res, _segs, keys) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(pcs, key, opt, out var _trace, voicings: null);
        Assert.Equal(3, keys.Count);
        Assert.All(keys, k => Assert.Equal(0, ((k.TonicMidi % 12) + 12) % 12));
    }

    [Fact]
    public void MinSwitchIndex_Enforces_Stay_In_OptionsPlusHarmonyOptions_Overload()
    {
        var pcs = new List<int[]>
        {
            new[] { 0, 4, 7 },
            new[] { 7, 11, 2 },
            new[] { 0, 4, 7 },
        };
        var key = new Key(60, true);
        var opt = new KeyEstimator.Options { CollectTrace = false, Window = 1, MinSwitchIndex = 10 };
        var hopt = new HarmonyOptions();

        var (_res, _segs, keys) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(pcs, key, opt, hopt, out var _trace, voicings: null);
        Assert.Equal(3, keys.Count);
        Assert.All(keys, k => Assert.True(k.IsMajor));
    }
}
