using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class CadenceStrictPacOptionTests
{
    private static readonly Key CMaj = new Key(60, true);

    [Fact]
    public void Default_Allows_V7_Imaj7_As_PAC()
    {
        // V7 -> Imaj7 should be PAC under default (non-strict) rules
        var pcsSeq = new[]
        {
            new[] { 67, 71, 74, 60 }, // V7 (G B D F) -> actually F=65; ensure pitch classes G B D F
            new[] { 60, 64, 67, 71 }, // Imaj7 (C E G B)
        };
        // fix pitch classes precisely
        pcsSeq[0] = new[] { 7, 11, 2, 5 }; // G B D F
        pcsSeq[1] = new[] { 0, 4, 7, 11 }; // C E G B
    var (res, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(pcsSeq, CMaj);
        Assert.Single(cads);
        Assert.True(cads[0].IsPerfectAuthentic);
    }

    [Fact]
    public void StrictPacPlainTriadsOnly_Demotes_V7_Imaj7()
    {
        var pcsSeq = new[]
        {
            new[] { 7, 11, 2, 5 }, // V7
            new[] { 0, 4, 7, 11 }, // Imaj7
        };
        var opts = new HarmonyOptions { StrictPacPlainTriadsOnly = true };
    var (res, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(pcsSeq, CMaj, opts);
        Assert.Single(cads);
        // Authentic だが PAC ではない → IsPerfectAuthentic=false
        Assert.Equal(CadenceType.Authentic, cads[0].Type);
        Assert.False(cads[0].IsPerfectAuthentic);
    }

    [Fact]
    public void StrictPacNoExtensions_Demotes_V9_I()
    {
        var pcsSeq = new[]
        {
            new[] { 7, 11, 2, 5, 9 }, // V9 (G B D F A)
            new[] { 0, 4, 7 },        // I (triad)
        };
        var opts = new HarmonyOptions { StrictPacDisallowDominantExtensions = true };
    var (res, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(pcsSeq, CMaj, opts);
        Assert.Single(cads);
        Assert.Equal(CadenceType.Authentic, cads[0].Type);
        Assert.False(cads[0].IsPerfectAuthentic);
    }

    [Fact]
    public void StrictPacPreset_Requires_Plain_V_I()
    {
        var pcsSeq = new[]
        {
            new[] { 7, 11, 2 }, // V (triad)
            new[] { 0, 4, 7 },  // I (triad)
        };
        // Provide voicings so that soprano on the goal chord is tonic (C) enabling PAC under StrictPacRequireSopranoTonic
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(S:79, A:71, T:67, B:55), // V: soprano = G (not required to be LT)
            new FourPartVoicing(S:84, A:76, T:72, B:60), // I: soprano = C (tonic)
        };
        var (res, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(pcsSeq, CMaj, HarmonyOptions.StrictPac, voicings);
        Assert.Single(cads);
        Assert.Equal(CadenceType.Authentic, cads[0].Type);
        Assert.True(cads[0].IsPerfectAuthentic); // plain V->I remains PAC
    }
}
