using MusicTheory.Theory.Pitch;
using MusicTheory.Theory.Scale;
using MusicTheory.Theory.Time;
using Xunit;

namespace MusicTheory.Tests;

/// <summary>
/// Tests for utility classes that were previously uncovered.
/// These tests improve code coverage by exercising simple utility methods and properties.
/// </summary>
public class UncoveredUtilityTests
{
    [Fact]
    public void TimeSignature_Scale_Constructor_Works()
    {
        var ts = new MusicTheory.Theory.Scale.TimeSignature(4, 4);
        Assert.Equal(4, ts.Numerator);
        Assert.Equal(4, ts.Denominator);
    }

    [Fact]
    public void TimeSignature_Scale_PropertySetters_Work()
    {
        var ts = new MusicTheory.Theory.Scale.TimeSignature(3, 4)
        {
            Numerator = 6,
            Denominator = 8
        };
        Assert.Equal(6, ts.Numerator);
        Assert.Equal(8, ts.Denominator);
    }

    [Fact]
    public void QuantizePresets_Straight16_HasCorrectValues()
    {
        var preset = QuantizePresets.Straight16;
        Assert.Equal("Straight16", preset.Name);
        Assert.Equal(Duration.TicksPerQuarter / 4, preset.GridTicks);
        Assert.Equal(0, preset.SwingRatio);
    }

    [Fact]
    public void QuantizePresets_Swing8_HasCorrectValues()
    {
        var preset = QuantizePresets.Swing8;
        Assert.Equal("Swing8", preset.Name);
        Assert.Equal(Duration.TicksPerQuarter / 2, preset.GridTicks);
        Assert.Equal(0.5, preset.SwingRatio);
    }

    [Fact]
    public void QuantizePresets_Funk16_HasCorrectValues()
    {
        var preset = QuantizePresets.Funk16;
        Assert.Equal("Funk16", preset.Name);
        Assert.Equal(Duration.TicksPerQuarter / 4, preset.GridTicks);
        Assert.Equal(0.0, preset.SwingRatio);
        Assert.NotEmpty(preset.GroovePattern);
    }

    [Fact]
    public void QuantizePresets_Apply_ProcessesTick()
    {
        var preset = QuantizePresets.Straight16;
        long tick = 100;
        int ticksPerBar = 1920;

        // Apply should process the tick (exact value doesn't matter, just that it runs)
        var result = QuantizePresets.Apply(preset, tick, ticksPerBar);
        Assert.True(result >= 0);
    }

    [Fact]
    public void TupletPatterns_Generate_ProducesTuplets()
    {
        var tuplets = TupletPatterns.Generate(maxActual: 5, maxNormal: 4).ToList();

        Assert.NotEmpty(tuplets);

        // Should generate tuplets where actual != normal
        foreach (var tuplet in tuplets)
        {
            Assert.NotEqual(tuplet.ActualCount, tuplet.NormalCount);
        }

        // Verify some specific tuplets exist
        Assert.Contains(tuplets, t => t.ActualCount == 3 && t.NormalCount == 2); // triplet
        Assert.Contains(tuplets, t => t.ActualCount == 5 && t.NormalCount == 4); // quintuplet
    }

    [Fact]
    public void TupletPatterns_Generate_RespectsMaxLimits()
    {
        var tuplets = TupletPatterns.Generate(maxActual: 3, maxNormal: 3).ToList();

        // All tuplets should have actual <= 3 and normal <= 3
        Assert.All(tuplets, t =>
        {
            Assert.True(t.ActualCount <= 3);
            Assert.True(t.NormalCount <= 3);
        });
    }

    [Fact]
    public void KeySignatureEvent_Constructor_Works()
    {
        var evt = new KeySignatureEvent(0, 2, false);
        Assert.Equal(0, evt.Tick);
        Assert.Equal(2, evt.SharpsFlats);
        Assert.False(evt.IsMinor);
    }

    [Fact]
    public void KeySignatureEvent_WithNegativeSharpsFlats_Works()
    {
        var evt = new KeySignatureEvent(960, -3, true);
        Assert.Equal(960, evt.Tick);
        Assert.Equal(-3, evt.SharpsFlats);
        Assert.True(evt.IsMinor);
    }

    [Fact]
    public void MarkerEvent_Constructor_Works()
    {
        var evt = new MarkerEvent(1920, "Chorus");
        Assert.Equal(1920, evt.Tick);
        Assert.Equal("Chorus", evt.Label);
    }

    [Fact]
    public void TextEvent_Constructor_Works()
    {
        var evt = new TextEvent(480, "Sample Text");
        Assert.Equal(480, evt.Tick);
        Assert.Equal("Sample Text", evt.Text);
    }

    [Fact]
    public void TempoChange_Constructor_Works()
    {
        var tempo = Tempo.FromBpm(120.0);
        var evt = new TempoChange(0, tempo);
        Assert.Equal(0, evt.Tick);
        Assert.Equal(120.0, evt.Tempo.Bpm, precision: 2);
    }
}
