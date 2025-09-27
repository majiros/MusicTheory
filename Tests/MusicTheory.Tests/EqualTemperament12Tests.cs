using MusicTheory;
using Xunit;

namespace MusicTheory.Tests;

public class EqualTemperament12Tests
{
    [Theory]
    [InlineData(69, 440.0)] // A4
    [InlineData(60, 261.6255653005986)] // C4 ≈ 261.626Hz
    [InlineData(57, 220.0)] // A3
    public void ToFrequency_Computes_12TET_Frequency(int midi, double expected)
    {
        var et = new EqualTemperament12();
        var f = et.ToFrequency(midi);
        Assert.InRange(f, expected * 0.999, expected * 1.001);
    }

    [Fact]
    public void A4Hz_Custom_Base_Is_Respected()
    {
        var et = new EqualTemperament12(442.0);
        Assert.InRange(et.ToFrequency(69), 441.55, 442.45);
    }
}
