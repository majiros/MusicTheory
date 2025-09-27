using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class KeyEstimatorTraceFlagTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void SecondaryDominant_Triad_Flag_Is_Set_When_V_of_V_Triad_In_C_Major()
    {
        // C major → D major triad (V/V) で、キーは C に留まるが trace に V/x が立つことを確認
        var seq = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) }, // C E G
            new[]{ Pc(62), Pc(66), Pc(69) }, // D F# A (V/V in C)
        };
        var options = new KeyEstimator.Options
        {
            Window = 0,
            PrevKeyBias = 2, // 直前キー保持に寄せる
            InitialKeyBias = 0,
            SwitchMargin = 1,
            CollectTrace = true,
        };
        var initialKey = new Key(60, true); // C major
        var keys = KeyEstimator.EstimatePerChord(seq, initialKey, options, out var trace);

        Assert.True(keys[1].IsMajor);
        Assert.Equal(Pc(60), Pc(keys[1].TonicMidi)); // C に留まる
        Assert.True(trace[1].SecondaryDominantTriad > 0); // V/x が加点されている
        Assert.Equal(0, trace[1].DominantTriad); // 選択キー(C)に対しては V ではない
    }

    [Fact]
    public void SecondaryDominant_Seventh_Flag_Is_Set_When_V_of_V7_In_C_Major()
    {
        // C major → D7 (V/V7) で、キーは C に留まるが trace に V/x7 が立つことを確認
        var seq = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) },       // C E G
            new[]{ Pc(62), Pc(66), Pc(69), Pc(60) } // D F# A C (D7)
        };
        var options = new KeyEstimator.Options
        {
            Window = 0,
            PrevKeyBias = 2,
            InitialKeyBias = 0,
            SwitchMargin = 0,
            CollectTrace = true,
        };
        var initialKey = new Key(60, true); // C major
        var keys = KeyEstimator.EstimatePerChord(seq, initialKey, options, out var trace);

        Assert.True(keys[1].IsMajor);
        Assert.Equal(Pc(60), Pc(keys[1].TonicMidi)); // C に留まる（タイは前キー優先）
        Assert.True(trace[1].SecondaryDominantSeventh > 0); // V/x7 が加点
        Assert.Equal(0, trace[1].DominantSeventh);
    }

    [Fact]
    public void Pivot_Flag_Is_Set_On_Switch_From_C_To_G_Via_D7_Cadence()
    {
        // C → Em(共通和音) → D7 → G と進行。終止で G に転じ、G での和音(最後)が C と G の両方に属するので pivot フラグが立つ。
        var seq = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) },             // C E G (C)
            new[]{ Pc(64), Pc(67), Pc(71) },             // E G B (Em) 共通和音
            new[]{ Pc(62), Pc(66), Pc(69), Pc(60) },     // D F# A C (D7)
            new[]{ Pc(67), Pc(71), Pc(62) },             // G B D (G)
        };
        var options = new KeyEstimator.Options
        {
            Window = 0,
            PrevKeyBias = 0,
            InitialKeyBias = 0,
            SwitchMargin = 0, // 証拠があれば即スイッチ
            CollectTrace = true,
        };
        var initialKey = new Key(60, true); // C major
        var keys = KeyEstimator.EstimatePerChord(seq, initialKey, options, out var trace);

        // 2つ目(Em)はタイブレークで C のままになりやすい（prev 優先/低トニックPC優先）
        Assert.True(keys[1].IsMajor);
        Assert.Equal(Pc(60), Pc(keys[1].TonicMidi));

        // D7 → G で G にスイッチし、cadence と pivot が立つことを確認
        Assert.True(keys[3].IsMajor);
        Assert.Equal(Pc(67), Pc(keys[3].TonicMidi)); // G major
        Assert.True(trace[3].Cadence > 0);           // 前が V(7)、現 I で加点
        Assert.True(trace[3].Pivot > 0);             // G 和音は C と G の双方に属する
    }
}
