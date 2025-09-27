using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class SixFourClassificationTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Classifies_Passing_SixFour()
    {
        var key = new Key(60, true); // C major
        // Pattern: IV -> IV64 -> IV6 (passing 6-4 within subdominant harmony)
        var IV = new[]{ Pc(65), Pc(69), Pc(72) };   // F A C (IV)
        var seq = new[] { IV, IV, IV };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 77, 72, 65), // IV root (F in bass)
            new FourPartVoicing(81, 77, 72, 72), // IV64 (C in bass)
            new FourPartVoicing(81, 77, 72, 69), // IV6 (A in bass)
        };

        var (res, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        // We expect a non-cadential six-four classification at index 1 (between 0->1)
        Assert.Contains(cadences, c => c.SixFour == SixFourType.Passing);
    }

    [Fact]
    public void Classifies_Passing_SixFour_Descending()
    {
        var key = new Key(60, true); // C major
        // Pattern: IV6 -> IV64 -> IV (descending passing 6-4 within subdominant harmony)
        var IV = new[]{ Pc(65), Pc(69), Pc(72) };   // F A C (IV)
        var seq = new[] { IV, IV, IV };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 77, 72, 69), // IV6 (A in bass)
            new FourPartVoicing(81, 77, 72, 72), // IV64 (C in bass)
            new FourPartVoicing(81, 77, 72, 65), // IV root (F in bass)
        };

        var (_, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        Assert.Contains(cadences, c => c.SixFour == SixFourType.Passing && c.Type == CadenceType.None);
    }

    [Fact]
    public void Classifies_Passing_SixFour_Descending_Is_Stable_On_Reanalysis()
    {
        var key = new Key(60, true); // C major
        var IV = new[]{ Pc(65), Pc(69), Pc(72) };   // F A C (IV)
        var seq = new[] { IV, IV, IV };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 77, 72, 69), // IV6 (A in bass)
            new FourPartVoicing(81, 77, 72, 72), // IV64 (C in bass)
            new FourPartVoicing(81, 77, 72, 65), // IV root (F in bass)
        };

        var (_, cad1) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        var (_, cad2) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);

        Assert.Contains(cad1, c => c.SixFour == SixFourType.Passing && c.Type == CadenceType.None);
        Assert.Contains(cad2, c => c.SixFour == SixFourType.Passing && c.Type == CadenceType.None);
        // Ensure no Pedal misclassification sneaks in
        Assert.DoesNotContain(cad2, c => c.SixFour == SixFourType.Pedal && c.Type == CadenceType.None);
    }

    [Fact]
    public void DoesNot_Classify_PassingOrPedal_When_Neighbors_Differ()
    {
        var key = new Key(60, true); // C major
        // Pattern: IV -> IV64 -> V (neighbors differ around the 6-4)
        var IV = new[]{ Pc(65), Pc(69), Pc(72) };   // F A C
        var V  = new[]{ Pc(67), Pc(71), Pc(62) };   // G B D
        var seq = new[] { IV, IV, V };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 77, 72, 65), // IV root (F in bass)
            new FourPartVoicing(81, 77, 72, 72), // IV64 (C in bass)
            new FourPartVoicing(83, 79, 74, 67), // V root (G in bass)
        };

        var (_, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        // Non-cadential passing/pedal should not be emitted when neighbors differ
        Assert.DoesNotContain(cadences, c => c.Type == CadenceType.None && (c.SixFour == SixFourType.Passing || c.SixFour == SixFourType.Pedal));
    }
    [Fact]
    public void Classifies_Pedal_SixFour()
    {
        var key = new Key(60, true); // C major
        // Pattern: IV -> IV64 -> IV (pedal 6-4, neighbors around the same bass)
        var IV = new[]{ Pc(65), Pc(69), Pc(72) }; // F A C

        var seq = new[] { IV, IV, IV };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 77, 72, 65), // IV root (F in bass)
            new FourPartVoicing(81, 77, 72, 72), // IV64 (C in bass)
            new FourPartVoicing(81, 77, 72, 65), // IV root again (F in bass)
        };

        var (res, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        Assert.Contains(cadences, c => c.SixFour == SixFourType.Pedal);
    }

    [Fact]
    public void Suppresses_NonCadential_SixFour_When_OptionDisabled()
    {
        var key = new Key(60, true); // C major
        var I = new[]{ Pc(60), Pc(64), Pc(67) };
        var V = new[]{ Pc(67), Pc(71), Pc(62) };
        var seq = new[] { I, V, I };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 76, 72, 60), // I root
            new FourPartVoicing(81, 79, 74, 62), // V64
            new FourPartVoicing(81, 76, 72, 64), // I6
        };
        var opts = new HarmonyOptions { ShowNonCadentialSixFour = false };
    var (res, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, opts, voicings);
    // Option is meant to suppress only non-cadential 6-4 entries; cadential entries may still carry SixFour classification.
    Assert.DoesNotContain(cadences, c => c.Type == CadenceType.None && (c.SixFour == SixFourType.Passing || c.SixFour == SixFourType.Pedal));
    }
}
