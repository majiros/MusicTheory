using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class CadentialSixFourDominantPreferenceTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Cadential_I64_Is_Relabeled_As_V64_53_When_Option_Enabled()
    {
        var key = new Key(60, true); // C major
        // I64 -> V -> I progression with voicings (bass indicates inversion)
        // I64: C-E-G with G in bass
        var I = new[]{ Pc(60), Pc(64), Pc(67) };
        var V = new[]{ Pc(67), Pc(71), Pc(74) };
        var seq = new[] { I, V, I };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(76, 72, 67, 55), // I64 (G in bass)
            new FourPartVoicing(79, 74, 71, 55), // V (root in bass)
            new FourPartVoicing(76, 72, 67, 48), // I (root in bass)
        };

        var opts = new HarmonyOptions { PreferCadentialSixFourAsDominant = true };
        var (result, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, opts, voicings);

        // Ensure a cadence is detected
        Assert.Contains(cadences, c => c.Type == CadenceType.Authentic);

        // First chord should be relabeled to V64-53 while keeping Dominant function
        Assert.Equal("V64-53", result.Chords[0].RomanText);
        Assert.Equal(TonalFunction.Dominant, result.Chords[0].Function);
    }
}
