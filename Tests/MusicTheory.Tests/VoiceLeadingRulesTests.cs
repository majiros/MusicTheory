using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class VoiceLeadingRulesTests
{
    [Fact]
    public void ParallelPerfects_AreDetected()
    {
        var a = new FourPartVoicing(S: 60, A: 53, T: 48, B: 36); // S-A = 7 (P5)
        var b = new FourPartVoicing(S: 62, A: 55, T: 50, B: 38); // S-A remains 7, both move up => parallel P5
        Assert.True(VoiceLeadingRules.HasParallelPerfects(a, b));
    }

    [Fact]
    public void Overlap_IsDetected()
    {
        var prev = new FourPartVoicing(S: 70, A: 60, T: 50, B: 40);
        var next = new FourPartVoicing(S: 59, A: 61, T: 49, B: 39); // S < prev.A triggers overlap
        Assert.True(VoiceLeadingRules.HasOverlap(prev, next));
    }

    [Fact]
    public void Spacing_And_Range_Violations()
    {
        var ok = new FourPartVoicing(S: 72, A: 67, T: 60, B: 48); // within spacing and range
        Assert.False(VoiceLeadingRules.HasSpacingViolations(ok));
        Assert.False(VoiceLeadingRules.HasRangeViolation(ok));

        var spacingBad = new FourPartVoicing(S: 80, A: 60, T: 47, B: 37); // S-A=20>12 and A-T=13>12
        Assert.True(VoiceLeadingRules.HasSpacingViolations(spacingBad));

        var rangeBad = new FourPartVoicing(S: 90, A: 80, T: 70, B: 30); // Bass below warn range
        Assert.True(VoiceLeadingRules.HasRangeViolation(rangeBad));
    }
}
