using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class SixFourCadentialRetentionTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Cadential_SixFour_Remains_Attached_When_NonCadential_Suppressed()
    {
        var key = new Key(60, true); // C major
        // I64 → V → I で Authentic + Cadential 6-4 を期待
        var I = new[]{ Pc(60), Pc(64), Pc(67) };   // C E G
        var V = new[]{ Pc(67), Pc(71), Pc(62) };   // G B D

        var seq = new[] { I, V, I };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 76, 72, 67), // I64 (G bass)
            new FourPartVoicing(83, 79, 74, 67), // V root (G bass)
            new FourPartVoicing(84, 76, 72, 60), // I root (C bass)
        };

        var opts = new HarmonyOptions { ShowNonCadentialSixFour = false };
        var (res, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, opts, voicings);

        // 期待: Authentic が1件、かつ Cadential 6-4 情報が付随
        Assert.Contains(cadences, c => c.Type == CadenceType.Authentic && (c.SixFour == SixFourType.Cadential || c.HasCadentialSixFour));
    }
}
