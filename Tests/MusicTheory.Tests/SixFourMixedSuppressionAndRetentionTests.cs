using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class SixFourMixedSuppressionAndRetentionTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Suppress_NonCadential_Passing_But_Retain_Cadential_In_Mixed_Sequence()
    {
        var key = new Key(60, true); // C major

        // Segment A (non-cadential Passing): IV -> IV64 -> IV6
        var IV = new[] { Pc(65), Pc(69), Pc(72) }; // F A C
        var voA0 = new FourPartVoicing(77, 69, 65, 53); // IV root (F bass)
        var voA1 = new FourPartVoicing(77, 69, 65, 60); // IV64 (C bass)
        var voA2 = new FourPartVoicing(77, 69, 65, 57); // IV6 (A bass)

        // Segment B (cadential 6-4): I64 -> V -> I
        var I = new[] { Pc(60), Pc(64), Pc(67) }; // C E G
        var V = new[] { Pc(67), Pc(71), Pc(62) }; // G B D
        var voB0 = new FourPartVoicing(81, 76, 72, 67); // I64 (G bass)
        var voB1 = new FourPartVoicing(83, 79, 74, 67); // V root (G bass)
        var voB2 = new FourPartVoicing(84, 76, 72, 60); // I root (C bass)

        var pcs = new[] { IV, IV, IV, I, V, I };
        var voicing = new FourPartVoicing?[] { voA0, voA1, voA2, voB0, voB1, voB2 };

        var opts = new HarmonyOptions { ShowNonCadentialSixFour = false };
        var (res, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(pcs, key, opts, voicing);

        // Non-cadential (Passing/Pedal) six-four entries must be suppressed entirely
        Assert.DoesNotContain(cadences, c => c.Type == CadenceType.None && (c.SixFour == SixFourType.Passing || c.SixFour == SixFourType.Pedal));

        // Cadential 6-4 must remain attached to the authentic cadence
        Assert.Contains(cadences, c => c.Type == CadenceType.Authentic && (c.SixFour == SixFourType.Cadential || c.HasCadentialSixFour));
    }
}
