using System;
using System.Linq;
using MusicTheory.Theory.Time;

namespace MusicTheory.Tests;

/// <summary>
/// Phase 11: Detailed tests for Quantize class to improve coverage (82.2% → 85%+).
/// Tests edge cases, boundary conditions, and flag combinations for quantization utilities.
/// </summary>
public class QuantizeDetailedTests
{
    #region ToGrid Tests

    [Fact]
    public void ToGrid_RoundsToNearestGrid()
    {
        int gridTicks = 120; // 32nd note
        long tick = 250; // Between 240 and 360

        var result = Quantize.ToGrid(tick, gridTicks);

        // 250 is closer to 240 than 360
        Assert.Equal(240, result);
    }

    [Fact]
    public void ToGrid_RoundsUp_WhenAtMidpoint()
    {
        int gridTicks = 100;
        long tick = 250; // Exactly halfway between 200 and 300

        var result = Quantize.ToGrid(tick, gridTicks);

        // Should round up at midpoint
        Assert.Equal(300, result);
    }

    [Fact]
    public void ToGrid_WithZeroGridTicks_ReturnsOriginalTick()
    {
        long tick = 500;

        var result = Quantize.ToGrid(tick, 0);

        Assert.Equal(tick, result);
    }

    [Fact]
    public void ToGrid_WithNegativeGridTicks_ReturnsOriginalTick()
    {
        long tick = 500;

        var result = Quantize.ToGrid(tick, -120);

        Assert.Equal(tick, result);
    }

    [Fact]
    public void ToGrid_ExactlyOnGrid_RemainsSame()
    {
        int gridTicks = Duration.TicksPerQuarter;
        long tick = Duration.TicksPerQuarter * 3;

        var result = Quantize.ToGrid(tick, gridTicks);

        Assert.Equal(tick, result);
    }

    [Fact]
    public void ToGrid_VerySmallTick_QuantizesToZeroOrGrid()
    {
        int gridTicks = 120;
        long tick = 10; // Much smaller than half grid

        var result = Quantize.ToGrid(tick, gridTicks);

        Assert.Equal(0, result);
    }

    [Fact]
    public void ToGrid_VeryLargeTick_QuantizesCorrectly()
    {
        int gridTicks = Duration.TicksPerQuarter;
        long tick = 1_000_000; // Very large tick value

        var result = Quantize.ToGrid(tick, gridTicks);

        // Should be multiple of gridTicks
        Assert.Equal(0, result % gridTicks);
        // Should be close to original
        Assert.InRange(result, tick - gridTicks / 2, tick + gridTicks / 2);
    }

    #endregion

    #region ApplySwing Tests

    [Fact]
    public void ApplySwing_WithZeroRatio_ReturnsOriginalTick()
    {
        long tick = 480;
        int subdivisionTicks = 240;

        var result = Quantize.ApplySwing(tick, subdivisionTicks, 0);

        Assert.Equal(tick, result);
    }

    [Fact]
    public void ApplySwing_WithNegativeRatio_ReturnsOriginalTick()
    {
        long tick = 480;
        int subdivisionTicks = 240;

        var result = Quantize.ApplySwing(tick, subdivisionTicks, -0.2);

        Assert.Equal(tick, result);
    }

    [Fact]
    public void ApplySwing_FirstSubdivision_RemainsUnchanged()
    {
        long tick = 0; // First subdivision
        int subdivisionTicks = 240;
        double ratio = 0.5;

        var result = Quantize.ApplySwing(tick, subdivisionTicks, ratio);

        Assert.Equal(tick, result);
    }

    [Fact]
    public void ApplySwing_SecondSubdivision_DelaysBy_RatioTimes_Subdivision()
    {
        long tick = 240; // Second subdivision (first is 0, second is 240)
        int subdivisionTicks = 240;
        double ratio = 0.5;

        var result = Quantize.ApplySwing(tick, subdivisionTicks, ratio);

        // Delay = subdivisionTicks * ratio = 240 * 0.5 = 120
        Assert.Equal(360, result); // 240 + 120
    }

    [Fact]
    public void ApplySwing_ThirdSubdivision_RemainsUnchanged()
    {
        long tick = 480; // Third subdivision (pair boundary)
        int subdivisionTicks = 240;
        double ratio = 0.5;

        var result = Quantize.ApplySwing(tick, subdivisionTicks, ratio);

        // Third subdivision is first of next pair, not delayed
        Assert.Equal(tick, result);
    }

    [Fact]
    public void ApplySwing_FourthSubdivision_DelaysAgain()
    {
        long tick = 720; // Fourth subdivision (second of second pair)
        int subdivisionTicks = 240;
        double ratio = 0.5;

        var result = Quantize.ApplySwing(tick, subdivisionTicks, ratio);

        // Delay = 120
        Assert.Equal(840, result); // 720 + 120
    }

    [Fact]
    public void ApplySwing_FullRatio_DelaysBy_FullSubdivision()
    {
        long tick = 240;
        int subdivisionTicks = 240;
        double ratio = 1.0; // Full triplet feel

        var result = Quantize.ApplySwing(tick, subdivisionTicks, ratio);

        // Delay = 240 * 1.0 = 240
        Assert.Equal(480, result); // Becomes triplet timing
    }

    [Fact]
    public void ApplySwing_SmallRatio_DelaysSlightly()
    {
        long tick = 240;
        int subdivisionTicks = 240;
        double ratio = 0.1;

        var result = Quantize.ApplySwing(tick, subdivisionTicks, ratio);

        // Delay = 240 * 0.1 = 24
        Assert.Equal(264, result);
    }

    #endregion

    #region ToGridStrongBeat Tests

    [Fact]
    public void ToGridStrongBeat_WithZeroGridTicks_ReturnsToGrid()
    {
        long tick = 500;
        int ticksPerBar = 1920;
        var strongBeats = new[] { 0, 960 };

        var result = Quantize.ToGridStrongBeat(tick, 0, ticksPerBar, strongBeats, 0.5);

        Assert.Equal(tick, result); // ToGrid returns original when gridTicks=0
    }

    [Fact]
    public void ToGridStrongBeat_WithEmptyStrongBeats_ReturnsToGrid()
    {
        long tick = 500;
        int gridTicks = 120;
        int ticksPerBar = 1920;

        var result = Quantize.ToGridStrongBeat(tick, gridTicks, ticksPerBar, Array.Empty<int>(), 0.5);

        // Should fall back to regular ToGrid
        Assert.Equal(480, result); // 500 rounds to 480
    }

    [Fact]
    public void ToGridStrongBeat_WithZeroBias_PreferRounded()
    {
        int gridTicks = 120;
        int ticksPerBar = 1920;
        var strongBeats = new[] { 0, 960 };
        long tick = 485; // Rounds to 480, strongBeat at 0 or 960

        var result = Quantize.ToGridStrongBeat(tick, gridTicks, ticksPerBar, strongBeats, 0);

        // bias=0 means distStrong * 1.0 < distRounded must be true
        // distRounded = |480 - 485| = 5
        // nearestStrong = 0 (closer than 960), distStrong = 485
        // 485 * 1.0 = 485 NOT < 5, so returns rounded
        Assert.Equal(480, result);
    }

    [Fact]
    public void ToGridStrongBeat_WithHighBias_PreferStrongBeat()
    {
        int gridTicks = 120;
        int ticksPerBar = 1920;
        var strongBeats = new[] { 0, 960 };
        long tick = 50; // Rounds to 0, strongBeat at 0

        var result = Quantize.ToGridStrongBeat(tick, gridTicks, ticksPerBar, strongBeats, 0.9);

        // distRounded = |0 - 50| = 50
        // nearestStrong = 0, distStrong = 50
        // 50 * (1 - 0.9) = 5 < 50, so returns strongBeat
        Assert.Equal(0, result);
    }

    [Fact]
    public void ToGridStrongBeat_NearSecondStrongBeat_SnapsToIt()
    {
        int gridTicks = 120;
        int ticksPerBar = 1920;
        var strongBeats = new[] { 0, 960 };
        long tick = 970; // Very close to 960

        var result = Quantize.ToGridStrongBeat(tick, gridTicks, ticksPerBar, strongBeats, 0.5);

        // Rounds to 960, strongBeat is 960
        Assert.Equal(960, result);
    }

    [Fact]
    public void ToGridStrongBeat_BetweenStrongBeats_RoundsToGrid()
    {
        int gridTicks = 120;
        int ticksPerBar = 1920;
        var strongBeats = new[] { 0, 960 };
        long tick = 500; // Between strongBeats, rounds to 480

        var result = Quantize.ToGridStrongBeat(tick, gridTicks, ticksPerBar, strongBeats, 0.25);

        // distRounded = |480 - 500| = 20
        // nearestStrong = 0 or 960, let's say 0 (500 % 1920 = 500, closer to 0 than 960)
        // nearestStrong = 0, distStrong = 500
        // 500 * (1 - 0.25) = 375 NOT < 20, returns rounded
        Assert.Equal(480, result);
    }

    [Fact]
    public void ToGridStrongBeat_AcrossBarBoundary_HandlesCorrectly()
    {
        int gridTicks = 120;
        int ticksPerBar = 1920;
        var strongBeats = new[] { 0, 960 };
        long tick = 1920 + 50; // Start of second bar + 50

        var result = Quantize.ToGridStrongBeat(tick, gridTicks, ticksPerBar, strongBeats, 0.8);

        // barIndex = 1, posInBar = 50
        // nearestStrong = 0, strongTick = 1920 + 0 = 1920
        // rounded = ToGrid(1970, 120) = 1920
        // distRounded = |1920 - 1970| = 50
        // distStrong = |1920 - 1970| = 50
        // 50 * (1 - 0.8) = 10 < 50, returns strongTick
        Assert.Equal(1920, result);
    }

    [Fact]
    public void ToGridStrongBeat_MultipleStrongBeats_ChoosesNearest()
    {
        int gridTicks = 120;
        int ticksPerBar = 1920;
        var strongBeats = new[] { 0, 480, 960, 1440 }; // 4 strong beats
        long tick = 500; // Closer to 480 than 960

        var result = Quantize.ToGridStrongBeat(tick, gridTicks, ticksPerBar, strongBeats, 0.5);

        // Rounds to 480, strongBeat nearest = 480
        Assert.Equal(480, result);
    }

    [Fact]
    public void ToGridStrongBeat_NegativeTick_HandlesCorrectly()
    {
        int gridTicks = 120;
        int ticksPerBar = 1920;
        var strongBeats = new[] { 0, 960 };
        long tick = -50; // Negative tick

        var result = Quantize.ToGridStrongBeat(tick, gridTicks, ticksPerBar, strongBeats, 0.5);

        // Should handle negative tick gracefully
        // ToGrid(-50, 120) may return -120 or 0 depending on implementation
        // This test verifies no exception
        Assert.True(result <= 0);
    }

    #endregion

    #region ApplyGroove Tests

    [Fact]
    public void ApplyGroove_WithZeroGridTicks_ReturnsOriginalTick()
    {
        long tick = 500;
        var pattern = new[] { 0, 10, -10 };

        var result = Quantize.ApplyGroove(tick, 0, pattern);

        Assert.Equal(tick, result);
    }

    [Fact]
    public void ApplyGroove_WithEmptyPattern_ReturnsOriginalTick()
    {
        long tick = 500;
        int gridTicks = 120;

        var result = Quantize.ApplyGroove(tick, gridTicks, Array.Empty<int>());

        Assert.Equal(tick, result);
    }

    [Fact]
    public void ApplyGroove_FirstPattern_AppliesFirstOffset()
    {
        long tick = 0;
        int gridTicks = 120;
        var pattern = new[] { 5, 10, -5 };

        var result = Quantize.ApplyGroove(tick, gridTicks, pattern);

        // baseGrid = 0, index = (0 / 120) % 3 = 0
        Assert.Equal(5, result); // 0 + pattern[0]
    }

    [Fact]
    public void ApplyGroove_SecondPattern_AppliesSecondOffset()
    {
        long tick = 120;
        int gridTicks = 120;
        var pattern = new[] { 5, 10, -5 };

        var result = Quantize.ApplyGroove(tick, gridTicks, pattern);

        // baseGrid = 120, index = (120 / 120) % 3 = 1
        Assert.Equal(130, result); // 120 + pattern[1]
    }

    [Fact]
    public void ApplyGroove_ThirdPattern_AppliesThirdOffset()
    {
        long tick = 240;
        int gridTicks = 120;
        var pattern = new[] { 5, 10, -5 };

        var result = Quantize.ApplyGroove(tick, gridTicks, pattern);

        // baseGrid = 240, index = (240 / 120) % 3 = 2
        Assert.Equal(235, result); // 240 + pattern[2] = 240 - 5
    }

    [Fact]
    public void ApplyGroove_PatternWrapsAround_CyclesCorrectly()
    {
        long tick = 360; // Fourth position
        int gridTicks = 120;
        var pattern = new[] { 5, 10, -5 };

        var result = Quantize.ApplyGroove(tick, gridTicks, pattern);

        // baseGrid = 360, index = (360 / 120) % 3 = 0 (wraps)
        Assert.Equal(365, result); // 360 + pattern[0]
    }

    [Fact]
    public void ApplyGroove_NegativeOffset_DecreasesTime()
    {
        long tick = 240;
        int gridTicks = 120;
        var pattern = new[] { 0, 0, -20 };

        var result = Quantize.ApplyGroove(tick, gridTicks, pattern);

        // baseGrid = 240, index = 2
        Assert.Equal(220, result); // 240 - 20
    }

    [Fact]
    public void ApplyGroove_PositiveOffset_IncreasesTime()
    {
        long tick = 120;
        int gridTicks = 120;
        var pattern = new[] { 0, 15 };

        var result = Quantize.ApplyGroove(tick, gridTicks, pattern);

        // baseGrid = 120, index = 1 % 2 = 1
        Assert.Equal(135, result); // 120 + 15
    }

    [Fact]
    public void ApplyGroove_QuantizesFirst_ThenAppliesOffset()
    {
        long tick = 250; // Not on grid
        int gridTicks = 120;
        var pattern = new[] { 10 };

        var result = Quantize.ApplyGroove(tick, gridTicks, pattern);

        // baseGrid = ToGrid(250, 120) = 240
        // index = (240 / 120) % 1 = 0
        Assert.Equal(250, result); // 240 + 10
    }

    [Fact]
    public void ApplyGroove_VeryLargePattern_HandlesIndexCorrectly()
    {
        long tick = 2400; // 20th grid position
        int gridTicks = 120;
        var pattern = new[] { 1, 2, 3, 4, 5, 6, 7 }; // 7-element pattern

        var result = Quantize.ApplyGroove(tick, gridTicks, pattern);

        // baseGrid = 2400, index = (2400 / 120) % 7 = 20 % 7 = 6
        Assert.Equal(2407, result); // 2400 + pattern[6] = 2400 + 7
    }

    [Fact]
    public void ApplyGroove_ZeroOffset_LeavesGridUnchanged()
    {
        long tick = 240;
        int gridTicks = 120;
        var pattern = new[] { 0, 0, 0 };

        var result = Quantize.ApplyGroove(tick, gridTicks, pattern);

        Assert.Equal(240, result);
    }

    #endregion

    #region Combined Scenarios Tests

    [Fact]
    public void CombinedQuantization_GridThenSwing_ProducesExpectedResult()
    {
        long tick = 485;
        int gridTicks = 120;
        int subdivisionTicks = 240;
        double swingRatio = 0.5;

        // Step 1: Quantize to grid
        var gridded = Quantize.ToGrid(tick, gridTicks);
        // Step 2: Apply swing
        var swung = Quantize.ApplySwing(gridded, subdivisionTicks, swingRatio);

        // gridded = 480, swing applies to second subdivision
        // 480 % (240*2) = 0, not second subdivision, no delay
        Assert.Equal(480, swung);
    }

    [Fact]
    public void CombinedQuantization_GridThenGroove_ProducesExpectedResult()
    {
        long tick = 485;
        int gridTicks = 120;
        var pattern = new[] { 5, -5 };

        // Step 1: Quantize to grid
        var gridded = Quantize.ToGrid(tick, gridTicks);
        // Step 2: Apply groove
        var grooved = Quantize.ApplyGroove(gridded, gridTicks, pattern);

        // gridded = 480, index = (480 / 120) % 2 = 0
        Assert.Equal(485, grooved); // 480 + 5
    }

    [Fact]
    public void CombinedQuantization_StrongBeatThenGroove_ProducesExpectedResult()
    {
        long tick = 50;
        int gridTicks = 120;
        int ticksPerBar = 1920;
        var strongBeats = new[] { 0, 960 };
        var pattern = new[] { 10 };

        // Step 1: Strong beat quantize
        var strong = Quantize.ToGridStrongBeat(tick, gridTicks, ticksPerBar, strongBeats, 0.8);
        // Step 2: Apply groove
        var grooved = Quantize.ApplyGroove(strong, gridTicks, pattern);

        // strong likely = 0 (with high bias)
        // grooved = 0 + 10 = 10
        Assert.True(grooved >= 0);
        Assert.True(grooved <= 120);
    }

    #endregion

    #region Edge Cases and Boundary Conditions

    [Fact]
    public void EdgeCase_VeryLargeTickValues_NoOverflow()
    {
        long tick = long.MaxValue / 2;
        int gridTicks = Duration.TicksPerQuarter;

        // Should not overflow
        var result = Quantize.ToGrid(tick, gridTicks);

        Assert.True(result > 0);
    }

    [Fact]
    public void EdgeCase_NegativeGridTicks_HandlesGracefully()
    {
        long tick = 500;

        var result1 = Quantize.ToGrid(tick, -100);
        var result2 = Quantize.ApplyGroove(tick, -100, new[] { 10 });

        Assert.Equal(tick, result1);
        Assert.Equal(tick, result2);
    }

    [Fact]
    public void EdgeCase_SingleElementPattern_AlwaysAppliesSameOffset()
    {
        int gridTicks = 120;
        var pattern = new[] { 15 };

        var result1 = Quantize.ApplyGroove(0, gridTicks, pattern);
        var result2 = Quantize.ApplyGroove(120, gridTicks, pattern);
        var result3 = Quantize.ApplyGroove(240, gridTicks, pattern);

        Assert.Equal(15, result1);
        Assert.Equal(135, result2);
        Assert.Equal(255, result3);
    }

    [Fact]
    public void EdgeCase_StrongBeatAtBarBoundary_SnapsCorrectly()
    {
        int gridTicks = 120;
        int ticksPerBar = 1920;
        var strongBeats = new[] { 0 }; // Only first beat is strong
        long tick = 1919; // Just before second bar

        var result = Quantize.ToGridStrongBeat(tick, gridTicks, ticksPerBar, strongBeats, 0.5);

        // Should snap to nearest grid or strong beat
        Assert.True(result % gridTicks == 0);
    }

    [Fact]
    public void EdgeCase_SwingWithVerySmallSubdivision_HandlesCorrectly()
    {
        long tick = 10;
        int subdivisionTicks = 5;
        double ratio = 0.5;

        var result = Quantize.ApplySwing(tick, subdivisionTicks, ratio);

        // tick=10, pair=10, inPair=0 (first subdivision)
        Assert.Equal(10, result); // No delay
    }

    [Fact]
    public void EdgeCase_SwingWithSecondVerySmallSubdivision_DelaysCorrectly()
    {
        long tick = 5;
        int subdivisionTicks = 5;
        double ratio = 0.5;

        var result = Quantize.ApplySwing(tick, subdivisionTicks, ratio);

        // tick=5, pair=10, inPair=5 (second subdivision)
        // delay = 5 * 0.5 = 2 (rounded down)
        Assert.Equal(7, result); // 5 + 2
    }

    #endregion
}
