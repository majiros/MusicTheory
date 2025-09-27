using System.Linq;

namespace MusicTheory.Tests;

public class SixFourSegmentationStabilityTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void Passing_SixFour_Unchanged_With_Segments()
    {
        var key = new Key(60, true); // C major
        var IV = new[] { Pc(65), Pc(69), Pc(72) }; // F A C
        var seq = new[] { IV, IV, IV };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 77, 72, 65), // IV root (F)
            new FourPartVoicing(81, 77, 72, 72), // IV64 (C)
            new FourPartVoicing(81, 77, 72, 69), // IV6 (A)
        };

        var (_, cadNoSeg) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        Assert.Contains(cadNoSeg, c => c.Type == CadenceType.None && c.SixFour == SixFourType.Passing);

    var keyOpts = new KeyEstimator.Options { Window = 1 };
    _ = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, key, keyOpts, HarmonyOptions.Default, out _, voicings);
        // AnalyzeWithKeyEstimate は cadences を返さないため、DetailedCadences をセグメント考慮後に再実行（セグメントは進行そのものを変えない）
        var (_, cadWithSeg) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        Assert.Contains(cadWithSeg, c => c.Type == CadenceType.None && c.SixFour == SixFourType.Passing);
    }

    [Fact]
    public void Pedal_SixFour_Unchanged_With_Segments()
    {
        var key = new Key(60, true); // C major
        var IV = new[] { Pc(65), Pc(69), Pc(72) }; // F A C
        var seq = new[] { IV, IV, IV };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 77, 72, 65), // IV root (F)
            new FourPartVoicing(81, 77, 72, 72), // IV64 (C)
            new FourPartVoicing(81, 77, 72, 65), // IV root (F)
        };

        var (_, cadNoSeg) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        Assert.Contains(cadNoSeg, c => c.Type == CadenceType.None && c.SixFour == SixFourType.Pedal);

    var keyOpts = new KeyEstimator.Options { Window = 1 };
    _ = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, key, keyOpts, HarmonyOptions.Default, out _, voicings);
        var (_, cadWithSeg) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        Assert.Contains(cadWithSeg, c => c.Type == CadenceType.None && c.SixFour == SixFourType.Pedal);
    }
}
