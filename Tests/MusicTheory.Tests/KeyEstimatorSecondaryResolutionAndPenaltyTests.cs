using System.Collections.Generic;
using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class KeyEstimatorSecondaryResolutionAndPenaltyTests
{
    [Fact]
    public void SecondaryResolution_Bonus_And_OutOfKey_Penalty_Applied()
    {
        // Key: C major. Sequence: C (I) -> A (V/ii) -> Dm (ii)
        var pcsList = new List<int[]>
        {
            new[] { 0, 4, 7 }, // C major triad (I)
            new[] { 9, 1, 4 }, // A major triad (V/ii) contains C# (1) non-diatonic
            new[] { 2, 5, 9 }, // D minor triad (ii)
        };

        var initial = new Key(60, true); // C major
        var opt = new KeyEstimator.Options
        {
            Window = 1,
            CollectTrace = true,
            SecondaryResolutionBonus = 2,
            OutOfKeyPenaltyPerPc = 1,
            PrevKeyBias = 1,
            SwitchMargin = 1,
            // 初期キー固定（全長）でペナルティ/ボーナスの挙動を決定論的に検証
            MinSwitchIndex = 10
        };

        var keys = KeyEstimator.EstimatePerChord(pcsList, initial, opt, out var trace);

    Assert.Equal(3, keys.Count);
    // 初回は初期キー（Cメジャー）を選ぶことを確認
    Assert.True(keys[0].IsMajor && ((keys[0].TonicMidi % 12 + 12) % 12) == 0);

        // On V/ii chord, one non-diatonic pc (C#) => penalty negative
        Assert.True(trace[1].OutOfKeyPenalty < 0);
        // On resolution (ii), secondary resolution bonus applied
        Assert.True(trace[2].SecondaryResolution > 0);
    }
}
