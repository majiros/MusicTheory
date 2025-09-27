using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class TriadInversionStrictnessTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void DoesNot_Assign_Triad_Inversion_When_More_Than_Three_Distinct_PCs()
    {
        var key = new Key(60, true); // C major
        // C major triad plus duplicated third (E) in another octave and added 7th to force 4 distinct
        var pcs = new[] { Pc(60), Pc(64), Pc(67), Pc(71) }; // C E G B => actually implies a seventh (Cmaj7)
        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, new FourPartVoicing(88, 76, 72, 60), null);
        Assert.NotNull(r.RomanText);
        Assert.DoesNotContain("64", r.RomanText);
        Assert.DoesNotContain("6 ", r.RomanText);
        Assert.DoesNotMatch(".*6$", r.RomanText ?? string.Empty);
    }

    [Fact]
    public void Assigns_Triad_Inversion_When_Exactly_Three_Distinct_PCs()
    {
        var key = new Key(60, true);
        var pcs = new[] { Pc(60), Pc(64), Pc(67) }; // C E G
        var v = new FourPartVoicing(84, 76, 72, 64); // Bass=E -> first inversion
        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v, null);
        Assert.Equal("I6", r.RomanText);
    }

    [Fact]
    public void Secondary_Dominant_Triad_Inversion_Still_Applies_With_Three_Distinct()
    {
        var key = new Key(60, true); // C major
        // V/ii = A C# E (distinct 3)
        int PcLocal(int m) => ((m % 12) + 12) % 12;
        var pcs = new[] { PcLocal(9), PcLocal(1), PcLocal(4) }; // A C# E
        var v = new FourPartVoicing(85, 73, 69, 61); // Bass=C# -> V6/ii expected
        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v, null);
        Assert.Equal("V6/ii", r.RomanText);
    }
}
