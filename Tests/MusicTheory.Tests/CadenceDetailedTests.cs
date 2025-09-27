using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class CadenceDetailedTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Detects_Cadential_SixFour_And_PAC()
    {
        var key = new Key(60, true); // C major
        // I64 -> V -> I progression (approximate PCs):
        // I64 (C/E/G with G in bass and C,E above) — we'll just pass I triad and rely on RomanText heuristics not perfect;
        // To ensure RomanText, we feed voicing with bass=G for I triad to simulate I64.
        var iTriad = new[]{ Pc(60), Pc(64), Pc(67) };
        var vTriad = new[]{ Pc(67), Pc(71), Pc(62) };
        var seq = new[] { iTriad, vTriad, iTriad };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 76, 72, 67), // bass G -> I64-ish
            new FourPartVoicing(83, 79, 74, 67), // V root
            new FourPartVoicing(84, 76, 72, 60), // I root
        };
        var (res, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        Assert.True(cadences.Count >= 1);
        var cad = cadences[0];
        Assert.Equal(CadenceType.Authentic, cad.Type);
        Assert.True(cad.HasCadentialSixFour);
        Assert.True(cad.IsPerfectAuthentic);
    }

    [Fact]
    public void Cadential_SixFour_Does_Not_Emit_Separate_None_Entry()
    {
        var key = new Key(60, true); // C major
        int Pc(int m) => ((m % 12) + 12) % 12;
        var I = new[]{ Pc(60), Pc(64), Pc(67) };
        var V = new[]{ Pc(67), Pc(71), Pc(62) };
        var seq = new[] { I, V, I };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 76, 72, 67), // I64（Gベース）
            new FourPartVoicing(83, 79, 74, 67), // V root
            new FourPartVoicing(84, 76, 72, 60), // I root
        };
        var (res, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        // 期待: Authentic 1件のみ（Type=None の I64→V 抑止項目はコレクションに含めない）
        Assert.Single(cadences);
        Assert.Equal(CadenceType.Authentic, cadences[0].Type);
        Assert.Equal(SixFourType.Cadential, cadences[0].SixFour);
        Assert.True(cadences[0].HasCadentialSixFour);
    }

    [Fact]
    public void Classifies_Passing_SixFour_In_NonCadential_Transition()
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
        var (_, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        Assert.Contains(cadences, c => c.Type == CadenceType.None && c.SixFour == SixFourType.Passing);
    }

    [Fact]
    public void Classifies_Pedal_SixFour_In_NonCadential_Transition()
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
        var (_, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        Assert.Contains(cadences, c => c.Type == CadenceType.None && c.SixFour == SixFourType.Pedal);
    }

    [Fact]
    public void Suppresses_NonCadential_SixFour_When_Option_Disabled()
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
        var opts = new HarmonyOptions { ShowNonCadentialSixFour = false };
        var (_, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, opts, voicings);
        // 非カデンツ 6-4 は抑制されるため Type=None のエントリは存在しない
        Assert.DoesNotContain(cadences, c => c.Type == CadenceType.None);
    }
}
