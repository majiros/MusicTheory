using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class CadenceExtendedDominantInversionTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;
    private static Key Cmaj = new Key(60, true);

    [Theory]
    // Corrected chord tone sets (include essential chord tones: G B D F (A for 9th)) and bass ordering to reflect inversion
    [InlineData("V65",      new int[]{71,74,77,79},     new int[]{60,64,67}, false)] // 1st inversion (third B in bass) V65 -> I (IAC)
    [InlineData("V43",      new int[]{74,77,79,83},     new int[]{60,64,67}, false)] // 2nd inversion (fifth D in bass) V43 -> I (IAC)
    [InlineData("V42",      new int[]{65,67,71,74},     new int[]{60,64,67}, false)] // 3rd inversion (seventh F in bass) V42 -> I (IAC)
    [InlineData("V9inv",    new int[]{71,74,77,79,81},  new int[]{60,64,67}, false)] // Inverted V9 (omit low root) -> I
    [InlineData("V7(9)inv", new int[]{71,74,77,79,81},  new int[]{60,64,67}, false)] // Same pcs; label difference via options in CLI normally
    public void Inversion_Dominants_To_I_Are_Authentic_Not_PAC(string label, int[] dominant, int[] tonic, bool expectPac)
    {
        var pcsDom = Array.ConvertAll(dominant, Pc);
        var pcsI   = Array.ConvertAll(tonic, Pc);
        var seq = new[] { pcsDom, pcsI };
        // Provide voicings to enforce inversion (bass = first element of "dominant" array)
        var bassDom = dominant[0];
        var bassI = tonic[0];
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(S:bassDom+12, A:bassDom+7, T:bassDom+5, B:bassDom),
            new FourPartVoicing(S:bassI+24, A:bassI+12, T:bassI+7, B:bassI) // I root
        };
        var (_, cad) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, Cmaj, voicings);
        if (cad.Count == 0)
        {
            var (resFull, _) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, Cmaj, voicings);
            var labels = string.Join(",", resFull.Chords.Select(c => c.RomanText ?? "?"));
            Xunit.Assert.Fail($"No cadence detected. RomanText sequence: {labels}");
        }
        var c = Assert.Single(cad);
        Assert.Equal(CadenceType.Authentic, c.Type);
        Assert.Equal(expectPac, c.IsPerfectAuthentic);
        // touch label param so test framework sees it as used
        Assert.False(string.IsNullOrEmpty(label));
    }

    [Fact]
    public void I64_V_I_Yields_Suppressed_Half_Then_Authentic()
    {
        var I64 = new[]{ Pc(60), Pc(64), Pc(67) }; // triad; voicing will force 64 via bass
        var V   = new[]{ Pc(67), Pc(71), Pc(74) };
        var I   = new[]{ Pc(60), Pc(64), Pc(67) };
        var seq = new[]{ I64, V, I };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(S:84, A:76, T:72, B:67), // I64 (bass = G)
            new FourPartVoicing(S:83, A:79, T:74, B:67), // V root
            new FourPartVoicing(S:84, A:76, T:72, B:60), // I root
        };
        var (_, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, Cmaj, voicings);
        // Expect only one cadence (final Authentic), and no intermediate Half.
        Assert.Single(cadences);
        Assert.Equal(CadenceType.Authentic, cadences[0].Type);
        Assert.True(cadences[0].IsPerfectAuthentic);
        Assert.True(cadences[0].HasCadentialSixFour);
    }
}
