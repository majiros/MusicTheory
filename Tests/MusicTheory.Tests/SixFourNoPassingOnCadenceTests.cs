namespace MusicTheory.Tests;

public class SixFourNoPassingOnCadenceTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void CadenceEntries_NeverCarry_PassingOrPedal()
    {
        var key = new Key(60, true); // C major

        // 1) Authentic with cadential 6-4 present: I64 -> V -> I
        var I = new[] { Pc(60), Pc(64), Pc(67) };
        var V = new[] { Pc(67), Pc(71), Pc(62) };
        var seqAuth = new[] { I, V, I };
        var voicingsAuth = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 76, 72, 67), // I64 (bass=G)
            new FourPartVoicing(83, 79, 74, 67), // V root
            new FourPartVoicing(84, 76, 72, 60), // I root
        };
        var (_, cadAuth) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seqAuth, key, voicingsAuth);
        Assert.Contains(cadAuth, c => c.Type == CadenceType.Authentic);
        Assert.DoesNotContain(cadAuth, c => c.Type != CadenceType.None && (c.SixFour == SixFourType.Passing || c.SixFour == SixFourType.Pedal));

        // 2) Plagal: IV -> I
        var IV = new[] { Pc(65), Pc(69), Pc(72) };
        var seqPlagal = new[] { IV, I };
        var voicingsPlagal = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 77, 72, 65), // IV root
            new FourPartVoicing(84, 76, 72, 60), // I root
        };
        var (_, cadPlagal) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seqPlagal, key, voicingsPlagal);
        Assert.Contains(cadPlagal, c => c.Type == CadenceType.Plagal);
        Assert.DoesNotContain(cadPlagal, c => c.Type != CadenceType.None && (c.SixFour == SixFourType.Passing || c.SixFour == SixFourType.Pedal));

        // 3) Half: ii -> V (ensure no prior I64 so Half isn’t suppressed)
        var ii = new[] { Pc(62), Pc(65), Pc(69) }; // D F A
        var seqHalf = new[] { ii, V };
        var voicingsHalf = new FourPartVoicing?[]
        {
            new FourPartVoicing(83, 77, 69, 62), // ii root
            new FourPartVoicing(83, 79, 74, 67), // V root
        };
        var (_, cadHalf) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seqHalf, key, voicingsHalf);
        Assert.Contains(cadHalf, c => c.Type == CadenceType.Half);
        Assert.DoesNotContain(cadHalf, c => c.Type != CadenceType.None && (c.SixFour == SixFourType.Passing || c.SixFour == SixFourType.Pedal));

        // 4) Deceptive: V -> vi
        var vi = new[] { Pc(69), Pc(72), Pc(76) }; // A C E
        var seqDec = new[] { V, vi };
        var voicingsDec = new FourPartVoicing?[]
        {
            new FourPartVoicing(83, 79, 74, 67), // V root
            new FourPartVoicing(84, 76, 72, 69), // vi root
        };
        var (_, cadDec) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seqDec, key, voicingsDec);
        Assert.Contains(cadDec, c => c.Type == CadenceType.Deceptive);
        Assert.DoesNotContain(cadDec, c => c.Type != CadenceType.None && (c.SixFour == SixFourType.Passing || c.SixFour == SixFourType.Pedal));
    }
}

