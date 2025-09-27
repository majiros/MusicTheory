using System.Linq;
using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class KeyEstimatorHysteresisStayTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void StayDueToHysteresis_When_TopTie_Includes_Prev()
    {
        // 構成: C major 初期→ 同点になるようにウィンドウ0 & 同一被覆数で C と G を作る
        // seq[0]: C triad（初期キーCを選ぶ）
        // seq[1]: G triad（CとGで同点になる状況を作り、prev=C を維持）
        var initial = new Key(60, true); // C major
        var seq = new[]
        {
            new[] { Pc(60), Pc(64), Pc(67) }, // C
            new[] { Pc(67), Pc(71), Pc(74) }, // G
        };
        var opt = new KeyEstimator.Options
        {
            Window = 0,
            SwitchMargin = 1,
            CollectTrace = true,
            PrevKeyBias = 0,
            InitialKeyBias = 0,
            DominantTriadBonus = 0,
            DominantSeventhBonus = 0,
            CadenceBonus = 0,
            PivotChordBonus = 0,
            SecondaryDominantTriadBonus = 0,
            SecondaryDominantSeventhBonus = 0,
        };
        var keys = KeyEstimator.EstimatePerChord(seq, initial, opt, out var trace);

        Assert.Equal(2, keys.Count);
        // 1音目は初期C、2音目は tie のトップに prev(C) が含まれるため C に留まる（StayedDueToHysteresis=true）
        Assert.True(trace[1].StayedDueToHysteresis);
        Assert.True(keys[1].IsMajor);
        Assert.Equal(Pc(60), keys[1].TonicMidi % 12);
    }
}
