using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class CadenceStrictPacSopranoTests
{
    private static readonly Key CMaj = new Key(60, true);

    [Fact]
    public void SopranoRequirement_Demotes_When_NoVoicings()
    {
        var seq = new[]
        {
            new[] { 7,11,2 }, // V
            new[] { 0,4,7 },  // I
        };
        var opts = new HarmonyOptions { StrictPacRequireSopranoTonic = true };
    var (res, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, CMaj, opts);
        Assert.Single(cads);
        Assert.Equal(CadenceType.Authentic, cads[0].Type);
        Assert.False(cads[0].IsPerfectAuthentic); // voicingなし -> PAC不可
    }

    [Fact]
    public void SopranoRequirement_Keeps_PAC_With_Voicings()
    {
        var seq = new[]
        {
            new[] { 7,11,2 }, // V
            new[] { 0,4,7 },  // I
        };
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(79, 71, 67, 55), // S=G (not tonic)
            new FourPartVoicing(84, 72, 67, 60), // S=C tonic
        };
        var opts = new HarmonyOptions { StrictPacRequireSopranoTonic = true };
        var (res, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, CMaj, opts, voicings);
        Assert.Single(cads);
        Assert.True(cads[0].IsPerfectAuthentic);
    }

    [Fact]
    public void LeadingToneResolution_Demotes_NoStepMotion()
    {
        var seq = new[] { new[] { 7,11,2 }, new[] { 0,4,7 } }; // V -> I
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(83, 71, 67, 55), // S=B (leading tone)
            new FourPartVoicing(83, 72, 67, 60), // S=B (no resolution)
        };
        var opts = new HarmonyOptions { StrictPacRequireSopranoTonic = true, StrictPacRequireSopranoLeadingToneResolution = true };
        var (res, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, CMaj, opts, voicings);
        Assert.Single(cads);
        Assert.False(cads[0].IsPerfectAuthentic); // no step motion
    }

    [Fact]
    public void LeadingToneResolution_Allows_StepMotion()
    {
        var seq = new[] { new[] { 7,11,2 }, new[] { 0,4,7 } }; // V -> I
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(83, 71, 67, 55), // S=B
            new FourPartVoicing(84, 72, 67, 60), // S=C (resolution)
        };
        var opts = new HarmonyOptions { StrictPacRequireSopranoTonic = true, StrictPacRequireSopranoLeadingToneResolution = true };
        var (res, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, CMaj, opts, voicings);
        Assert.Single(cads);
        Assert.True(cads[0].IsPerfectAuthentic);
    }

    [Fact]
    public void SopranoRequirement_Demotes_When_FinalSopranoNotTonic()
    {
        var seq = new[] { new[] { 7,11,2 }, new[] { 0,4,7 } }; // V -> I
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(79, 71, 67, 55), // S=G
            new FourPartVoicing(76, 64, 67, 60), // S=E (not tonic)
        };
        var opts = new HarmonyOptions { StrictPacRequireSopranoTonic = true };
        var (res, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, CMaj, opts, voicings);
        Assert.Single(cads);
        Assert.Equal(CadenceType.Authentic, cads[0].Type);
        Assert.False(cads[0].IsPerfectAuthentic); // final soprano not tonic
    }

    [Fact]
    public void Default_Allows_PAC_Without_LeadingTone_Resolution()
    {
        var seq = new[] { new[] { 7,11,2 }, new[] { 0,4,7 } }; // V -> I
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(83, 71, 67, 55), // S=B (leading tone)
            new FourPartVoicing(83, 72, 67, 60), // S=B (no resolution, functions as added 7th in voicing)
        };
        // No strict options -> still treated as PAC (default lenient behavior)
        var (res, cads) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, CMaj, new HarmonyOptions(), voicings);
        Assert.Single(cads);
        Assert.True(cads[0].IsPerfectAuthentic);
    }
}
