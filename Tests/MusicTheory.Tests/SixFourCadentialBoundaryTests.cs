using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class SixFourCadentialBoundaryTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void I64_V_I_Classified_As_Cadential_No_NonCadential_Emissions()
    {
        var key = new Key(60, true); // C major

        var I = new[] { Pc(60), Pc(64), Pc(67) };
        var V = new[] { Pc(67), Pc(71), Pc(62) };
        var seq = new[] { I, V, I };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 76, 72, 67), // I64（Gベース）
            new FourPartVoicing(83, 79, 74, 67), // V root
            new FourPartVoicing(84, 76, 72, 60), // I root
        };

        var (res, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);

        Assert.Contains(cadences, c => c.Type == CadenceType.Authentic);
        Assert.DoesNotContain(cadences, c => c.Type == CadenceType.None);
        Assert.Contains(cadences, c => c.SixFour == SixFourType.Cadential || c.HasCadentialSixFour);
    }

    [Fact]
    public void I64_V_I_With_HideNonCadential_Still_Cadential_No_None_Entries()
    {
        var key = new Key(60, true); // C major

        var I = new[] { Pc(60), Pc(64), Pc(67) };
        var V = new[] { Pc(67), Pc(71), Pc(62) };
        var seq = new[] { I, V, I };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 76, 72, 67), // I64（Gベース）
            new FourPartVoicing(83, 79, 74, 67), // V root
            new FourPartVoicing(84, 76, 72, 60), // I root
        };
        var opts = new HarmonyOptions { ShowNonCadentialSixFour = false };

        var (res, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, opts, voicings);

        Assert.Contains(cadences, c => c.Type == CadenceType.Authentic);
        Assert.DoesNotContain(cadences, c => c.Type == CadenceType.None);
        Assert.Contains(cadences, c => c.SixFour == SixFourType.Cadential || c.HasCadentialSixFour);
    }
}
