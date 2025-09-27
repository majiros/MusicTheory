using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class CadenceIacBaselineTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void V6_To_I_Is_Authentic_But_Not_PAC()
    {
        var key = new Key(60, true); // C major
        var V = new[] { Pc(67), Pc(71), Pc(74) }; // G B D
        var I = new[] { Pc(60), Pc(64), Pc(67) }; // C E G
        var seq = new[] { V, I };
        var voicings = new FourPartVoicing?[]
        {
            // V6: bass = B (71)
            new FourPartVoicing(S:79, A:74, T:67, B:71),
            // I root
            new FourPartVoicing(S:84, A:76, T:72, B:60)
        };
        var (_, cad) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        Assert.Single(cad);
        Assert.Equal(CadenceType.Authentic, cad[0].Type);
        Assert.False(cad[0].IsPerfectAuthentic); // inversion present -> IAC
    }

    [Fact]
    public void V_To_I6_Is_Authentic_But_Not_PAC()
    {
        var key = new Key(60, true);
        var V = new[] { Pc(67), Pc(71), Pc(74) };
        var I = new[] { Pc(60), Pc(64), Pc(67) };
        var seq = new[] { V, I };
        var voicings = new FourPartVoicing?[]
        {
            // V root
            new FourPartVoicing(S:83, A:79, T:74, B:67),
            // I6: bass = E (64)
            new FourPartVoicing(S:84, A:76, T:72, B:64)
        };
        var (_, cad) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        Assert.Single(cad);
        Assert.Equal(CadenceType.Authentic, cad[0].Type);
        Assert.False(cad[0].IsPerfectAuthentic);
    }

    [Fact]
    public void V7_To_I6_Is_Authentic_But_Not_PAC()
    {
        var key = new Key(60, true);
        // V7: G B D F
        var V7 = new[] { Pc(67), Pc(71), Pc(74), Pc(65) };
        var I = new[] { Pc(60), Pc(64), Pc(67) };
        var seq = new[] { V7, I };
        var voicings = new FourPartVoicing?[]
        {
            // V7 root (bass G)
            new FourPartVoicing(S:85, A:79, T:74, B:67),
            // I6 (bass E)
            new FourPartVoicing(S:84, A:76, T:72, B:64)
        };
        var (_, cad) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
        Assert.Single(cad);
        Assert.Equal(CadenceType.Authentic, cad[0].Type);
        Assert.False(cad[0].IsPerfectAuthentic);
    }

    [Fact]
    public void IV_To_I_Is_Plagal_Not_PAC()
    {
        var key = new Key(60, true);
        var IV = new[] { Pc(65), Pc(69), Pc(72) }; // F A C
        var I = new[] { Pc(60), Pc(64), Pc(67) };
        var seq = new[] { IV, I };
        var (chords, cad) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key);
        Assert.Single(cad);
        Assert.Equal(CadenceType.Plagal, cad[0].Type);
        Assert.False(cad[0].IsPerfectAuthentic);
    }

    [Fact]
    public void V_To_Imaj7_Is_PAC()
    {
        var key = new Key(60, true);
        var V = new[] { Pc(67), Pc(71), Pc(74), Pc(65) }; // treat as V7
        var Imaj7 = new[] { Pc(60), Pc(64), Pc(67), Pc(71) }; // C E G B
        var seq = new[] { V, Imaj7 };
        var (chords, cad) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key);
        Assert.Single(cad);
        Assert.Equal(CadenceType.Authentic, cad[0].Type);
        Assert.True(cad[0].IsPerfectAuthentic); // Imaj7 root counts for PAC
    }
}
