using MusicTheory.Theory.Analysis;
using Xunit;

namespace MusicTheory.Tests;

public class AnalysisUtilsTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(12, 0)]
    [InlineData(-1, 11)]
    [InlineData(13, 1)]
    [InlineData(-13, 11)]
    public void Mod12_CorrectlyNormalizes(int input, int expected)
    {
        Assert.Equal(expected, AnalysisUtils.Mod12(input));
    }

    [Fact]
    public void NormalizeToRoot_RotatesAndSorts()
    {
        var pcs = new[] { 7, 11, 2, 5 }; // G7 chord: G B D F
        var normalized = AnalysisUtils.NormalizeToRoot(pcs, 7); // Root = G (7)
        Assert.Equal(new[] { 0, 4, 7, 10 }, normalized); // Relative to G: G(0) B(4) D(7) F(10)
    }

    [Fact]
    public void NormalizeToRoot_RemovesDuplicates()
    {
        var pcs = new[] { 0, 4, 7, 4, 0 };
        var normalized = AnalysisUtils.NormalizeToRoot(pcs, 0);
        Assert.Equal(new[] { 0, 4, 7 }, normalized);
    }

    [Fact]
    public void NormalizeToRoot_HandlesNegativeRoot()
    {
        var pcs = new[] { 0, 4, 7 };
        var normalized = AnalysisUtils.NormalizeToRoot(pcs, -1); // Root = B (11)
        // 0-11=1, 4-11=5, 7-11=8 (mod 12)
        Assert.Equal(new[] { 1, 5, 8 }, normalized);
    }

    [Fact]
    public void Signature_CreatesHyphenatedString()
    {
        var pcs = new[] { 7, 11, 2, 5 };
        var signature = AnalysisUtils.Signature(pcs);
        Assert.Equal("2-5-7-11", signature);
    }

    [Fact]
    public void Signature_RemovesDuplicates()
    {
        var pcs = new[] { 0, 4, 7, 4, 0 };
        var signature = AnalysisUtils.Signature(pcs);
        Assert.Equal("0-4-7", signature);
    }

    [Fact]
    public void Signature_NormalizesNegativeValues()
    {
        var pcs = new[] { -1, 4, 13 };
        var signature = AnalysisUtils.Signature(pcs);
        Assert.Equal("1-4-11", signature); // -1 -> 11, 13 -> 1
    }

    [Fact]
    public void Signature_EmptyInput_ReturnsEmptyString()
    {
        var pcs = Array.Empty<int>();
        var signature = AnalysisUtils.Signature(pcs);
        Assert.Equal("", signature);
    }

    [Fact]
    public void Signature_SingleValue_ReturnsValue()
    {
        var pcs = new[] { 5 };
        var signature = AnalysisUtils.Signature(pcs);
        Assert.Equal("5", signature);
    }

    [Fact]
    public void NormalizeToRoot_EmptyInput_ReturnsEmpty()
    {
        var pcs = Array.Empty<int>();
        var normalized = AnalysisUtils.NormalizeToRoot(pcs, 0);
        Assert.Empty(normalized);
    }
}
