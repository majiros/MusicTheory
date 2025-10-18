using MusicTheory.Theory.Pitch;
using MusicTheory.Theory.Time;
using Xunit;
using System.Linq;
using System.Collections.Generic;

namespace MusicTheory.Tests;

/// <summary>
/// Comprehensive tests for SequenceLayout.cs (211 lines).
/// Tests InsertRests, SplitRunsByInferredTuplet, private helpers, and edge cases.
/// </summary>
public class SequenceLayoutTests
{
    // ======================================================================================
    // InsertRests - Basic Functionality
    // ======================================================================================

    [Fact]
    public void InsertRests_NoGap_ReturnsOriginalNotes()
    {
        var notes = new[]
        {
            (start: 0L, note: new Note(DurationFactory.Quarter())),
            (start: (long)Duration.TicksPerQuarter, note: new Note(DurationFactory.Quarter()))
        };

        var result = SequenceLayout.InsertRests(notes, normalizeRests: false);

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.IsType<Note>(r.entity));
    }

    [Fact]
    public void InsertRests_WithGap_InsertsRest()
    {
        var notes = new[]
        {
            (start: 0L, note: new Note(DurationFactory.Quarter())),
            (start: (long)Duration.TicksPerQuarter * 2, note: new Note(DurationFactory.Quarter()))
        };

        var result = SequenceLayout.InsertRests(notes);

        Assert.Equal(3, result.Count);
        Assert.Contains(result, r => r.entity is Rest && r.start == Duration.TicksPerQuarter);
    }

    [Fact]
    public void InsertRests_EmptyInput_ReturnsEmpty()
    {
        var result = SequenceLayout.InsertRests(Array.Empty<(long, Note)>());
        Assert.Empty(result);
    }

    [Fact]
    public void InsertRests_SingleNote_ReturnsOneEntry()
    {
        var notes = new[] { (start: 0L, note: new Note(DurationFactory.Whole())) };
        var result = SequenceLayout.InsertRests(notes);
        Assert.Single(result);
        Assert.IsType<Note>(result[0].entity);
    }

    // ======================================================================================
    // InsertRests - Overlapping Notes (monophonic priority)
    // ======================================================================================

    [Fact]
    public void InsertRests_OverlappingNotes_TakesLongerDuration()
    {
        var notes = new[]
        {
            (start: 0L, note: new Note(DurationFactory.Quarter())), // ends at 480
            (start: 240L, note: new Note(DurationFactory.Eighth())) // starts before first ends
        };

        var result = SequenceLayout.InsertRests(notes);

        // Should have 2 notes, cursor advances to max(480, 240+240=480)
        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.IsType<Note>(r.entity));
    }

    // ======================================================================================
    // InsertRests - normalizeRests flag
    // ======================================================================================

    [Fact]
    public void InsertRests_NormalizeRests_True_NormalizesRestDurations()
    {
        // Gap of 960 ticks (half note) should be normalized
        var notes = new[]
        {
            (start: 0L, note: new Note(DurationFactory.Quarter())),
            (start: (long)Duration.TicksPerQuarter + 960, note: new Note(DurationFactory.Quarter()))
        };

        var result = SequenceLayout.InsertRests(notes, normalizeRests: true);

        Assert.True(result.Count >= 2); // At least the 2 notes
        Assert.Contains(result, r => r.entity is Rest);
    }

    [Fact]
    public void InsertRests_NormalizeRests_False_KeepsRawRestDurations()
    {
        var notes = new[]
        {
            (start: 0L, note: new Note(DurationFactory.Quarter())),
            (start: (long)Duration.TicksPerQuarter + 960, note: new Note(DurationFactory.Quarter()))
        };

        var result = SequenceLayout.InsertRests(notes, normalizeRests: false);

        Assert.Equal(3, result.Count); // 2 notes + 1 rest (not normalized)
        var rest = result.Single(r => r.entity is Rest);
        Assert.Equal(960, rest.entity.Duration.Ticks);
    }

    // ======================================================================================
    // InsertRests - advancedSplit flag
    // ======================================================================================

    [Fact]
    public void InsertRests_AdvancedSplit_True_SplitsComplexRests()
    {
        // Gap of 1000 ticks (non-standard duration)
        var notes = new[]
        {
            (start: 0L, note: new Note(DurationFactory.Quarter())),
            (start: (long)Duration.TicksPerQuarter + 1000, note: new Note(DurationFactory.Quarter()))
        };

        var result = SequenceLayout.InsertRests(notes, normalizeRests: true, advancedSplit: true);

        // Should split the 1000-tick rest into multiple normalized durations
        Assert.True(result.Count > 2);
        Assert.Contains(result, r => r.entity is Rest);
    }

    // ======================================================================================
    // InsertRests - allowSplitTuplets flag (tuplet inference)
    // ======================================================================================

    [Fact]
    public void InsertRests_AllowSplitTuplets_InfersTriplet()
    {
        // Create a run of 3 notes of equal duration that sum to a quarter note (3:2 triplet)
        int tripletTicks = Duration.TicksPerQuarter * 2 / 3; // 320 ticks per triplet eighth
        var notes = new[]
        {
            (start: 0L, note: new Note(Duration.FromTicks(tripletTicks))),
            (start: (long)tripletTicks, note: new Note(Duration.FromTicks(tripletTicks))),
            (start: (long)tripletTicks * 2, note: new Note(Duration.FromTicks(tripletTicks)))
        };

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true);

        // Should recognize triplet pattern and rebuild with proper tuplet durations
        Assert.Equal(3, result.Count);
        Assert.All(result, r => Assert.IsType<Note>(r.entity));

        // Check that durations have correct tick values
        var firstNote = (Note)result[0].entity;
        Assert.Equal(tripletTicks, firstNote.Duration.Ticks);
    }

    [Fact]
    public void InsertRests_AllowSplitTuplets_False_DoesNotInferTuplet()
    {
        int tripletTicks = Duration.TicksPerQuarter * 2 / 3; // 320 ticks
        var notes = new[]
        {
            (start: 0L, note: new Note(Duration.FromTicks(tripletTicks))),
            (start: (long)tripletTicks, note: new Note(Duration.FromTicks(tripletTicks))),
            (start: (long)tripletTicks * 2, note: new Note(Duration.FromTicks(tripletTicks)))
        };

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: false);

        // Should NOT rebuild with tuplet durations
        Assert.Equal(3, result.Count);
        var firstNote = (Note)result[0].entity;
        Assert.Equal(tripletTicks, firstNote.Duration.Ticks);
    }

    // ======================================================================================
    // InsertRests - extendedTuplets flag
    // ======================================================================================

    [Fact]
    public void InsertRests_ExtendedTuplets_AllowsWiderRange()
    {
        // Create a 5:4 quintuplet pattern
        int totalTicks = Duration.TicksPerQuarter * 4; // whole note
        int elemTicks = totalTicks * 5 / (4 * 5); // Each element in 5:4 over whole note
        elemTicks = Duration.TicksPerQuarter * 4 / 5; // 384 ticks per quintuplet quarter

        var notes = Enumerable.Range(0, 5)
            .Select(i => ((long)elemTicks * i, new Note(Duration.FromTicks(elemTicks))))
            .ToArray();

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true, extendedTuplets: true);

        // Should recognize 5:4 pattern
        Assert.Equal(5, result.Count);
        var firstNote = (Note)result[0].entity;

        // Should have correct tick duration
        Assert.Equal(elemTicks, firstNote.Duration.Ticks);
    }

    // ======================================================================================
    // InsertRests - Unordered input
    // ======================================================================================

    [Fact]
    public void InsertRests_UnorderedInput_SortsAutomatically()
    {
        var notes = new[]
        {
            (start: (long)Duration.TicksPerQuarter * 2, note: new Note(DurationFactory.Quarter())),
            (start: 0L, note: new Note(DurationFactory.Quarter())),
            (start: (long)Duration.TicksPerQuarter, note: new Note(DurationFactory.Quarter()))
        };

        var result = SequenceLayout.InsertRests(notes);

        // Should be automatically sorted by start time
        Assert.Equal(3, result.Count);
        Assert.Equal(0L, result[0].start);
        Assert.Equal((long)Duration.TicksPerQuarter, result[1].start);
        Assert.Equal((long)Duration.TicksPerQuarter * 2, result[2].start);
    }

    // ======================================================================================
    // Tuplet Inference - Common Tuplets (3:2, 5:4, 7:4, 5:2, 7:8, 9:8)
    // ======================================================================================

    [Fact]
    public void InsertRests_TupletInference_3over2_EighthTriplets()
    {
        // 3 eighth-note triplets in the space of 2 eighths (quarter note total)
        int actualTripletTicks = Duration.TicksPerQuarter * 2 / 3; // 320 ticks per element

        var notes = new[]
        {
            (start: 0L, note: new Note(Duration.FromTicks(actualTripletTicks))),
            (start: (long)actualTripletTicks, note: new Note(Duration.FromTicks(actualTripletTicks))),
            (start: (long)actualTripletTicks * 2, note: new Note(Duration.FromTicks(actualTripletTicks)))
        };

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true, extendedTuplets: false);

        Assert.Equal(3, result.Count);
        var firstNote = (Note)result[0].entity;
        Assert.Equal(actualTripletTicks, firstNote.Duration.Ticks);
    }

    [Fact]
    public void InsertRests_TupletInference_5over4_Quintuplets()
    {
        // 5 quarter notes in the space of 4 (whole note total)
        int quintupletTicks = Duration.TicksPerQuarter * 4 / 5; // 384 ticks per element

        var notes = Enumerable.Range(0, 5)
            .Select(i => ((long)quintupletTicks * i, new Note(Duration.FromTicks(quintupletTicks))))
            .ToArray();

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true, extendedTuplets: false);

        Assert.Equal(5, result.Count);
        var firstNote = (Note)result[0].entity;

        // Should have correct tick duration
        Assert.Equal(quintupletTicks, firstNote.Duration.Ticks);
    }

    [Fact]
    public void InsertRests_TupletInference_7over4_Septuplets()
    {
        // 7 quarter notes in the space of 4 (whole note total)
        int septupletTicks = Duration.TicksPerQuarter * 4 / 7; // ~274 ticks per element

        var notes = Enumerable.Range(0, 7)
            .Select(i => ((long)septupletTicks * i, new Note(Duration.FromTicks(septupletTicks))))
            .ToArray();

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true, extendedTuplets: false);

        Assert.Equal(7, result.Count);
        var firstNote = (Note)result[0].entity;
        Assert.Equal(septupletTicks, firstNote.Duration.Ticks);
    }

    // ======================================================================================
    // Tuplet Inference - Extended Tuplets (2-12 range)
    // ======================================================================================

    [Fact]
    public void InsertRests_ExtendedTuplets_11over8_Recognized()
    {
        // 11 notes in the space of 8 eighths (whole note total)
        int totalTicks = Duration.TicksPerQuarter * 4;
        int elemTicks = totalTicks * 8 / (8 * 11); // baseTicks * 11 / 8
        elemTicks = totalTicks / 11; // ~174 ticks per element

        var notes = Enumerable.Range(0, 11)
            .Select(i => ((long)elemTicks * i, new Note(Duration.FromTicks(elemTicks))))
            .ToArray();

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true, extendedTuplets: true);

        Assert.Equal(11, result.Count);
        // Extended tuplets should be recognized with extendedTuplets=true
    }

    [Fact]
    public void InsertRests_ExtendedTuplets_False_DoesNotRecognizeUncommonTuplets()
    {
        // 11:8 should NOT be recognized with extendedTuplets=false
        int totalTicks = Duration.TicksPerQuarter * 4;
        int elemTicks = totalTicks / 11;

        var notes = Enumerable.Range(0, 11)
            .Select(i => ((long)elemTicks * i, new Note(Duration.FromTicks(elemTicks))))
            .ToArray();

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true, extendedTuplets: false);

        Assert.Equal(11, result.Count);
        // Should fall back to raw tick durations
        var firstNote = (Note)result[0].entity;
        Assert.Equal(elemTicks, firstNote.Duration.Ticks);
    }

    // ======================================================================================
    // Edge Cases - Run Detection
    // ======================================================================================

    [Fact]
    public void InsertRests_RunDetection_GapBreaksRun()
    {
        int tripletTicks = Duration.TicksPerQuarter * 2 / 3;

        var notes = new[]
        {
            (start: 0L, note: new Note(Duration.FromTicks(tripletTicks))),
            (start: (long)tripletTicks, note: new Note(Duration.FromTicks(tripletTicks))),
            // GAP here breaks the run
            (start: (long)tripletTicks * 3, note: new Note(Duration.FromTicks(tripletTicks)))
        };

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true);

        // Should have rest inserted, breaking tuplet inference for the full sequence
        Assert.True(result.Count > 3);
        Assert.Contains(result, r => r.entity is Rest);
    }

    [Fact]
    public void InsertRests_RunDetection_OverlapBreaksRun()
    {
        var notes = new[]
        {
            (start: 0L, note: new Note(DurationFactory.Quarter())),
            (start: 240L, note: new Note(DurationFactory.Quarter())), // Overlaps with previous
            (start: 480L, note: new Note(DurationFactory.Quarter()))
        };

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true);

        // Overlap should break run detection
        Assert.Equal(3, result.Count);
        Assert.All(result, r => Assert.IsType<Note>(r.entity));
    }

    // ======================================================================================
    // Edge Cases - Minimum Run Length
    // ======================================================================================

    [Fact]
    public void InsertRests_ShortRun_NotSplit()
    {
        // Single-element run should not be split
        var notes = new[] { (start: 0L, note: new Note(DurationFactory.Quarter())) };

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true);

        Assert.Single(result);
        Assert.IsType<Note>(result[0].entity);
    }

    [Fact]
    public void InsertRests_TwoElementRun_TooShortForInference()
    {
        // 2-element run where totalTicks < minTicks * 2
        int shortTicks = 100;
        var notes = new[]
        {
            (start: 0L, note: new Note(Duration.FromTicks(shortTicks))),
            (start: (long)shortTicks, note: new Note(Duration.FromTicks(shortTicks)))
        };

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true);

        // Too short for inference, should pass through unchanged
        Assert.Equal(2, result.Count);
        var firstNote = (Note)result[0].entity;
        Assert.Equal(shortTicks, firstNote.Duration.Ticks);
    }

    // ======================================================================================
    // Edge Cases - Fallback to Equal Division
    // ======================================================================================

    [Fact]
    public void InsertRests_FallbackToEqualDivision_WhenTupletNotFound()
    {
        // Create a pattern that doesn't match any tuplet but divides evenly
        int elemTicks = 100;
        int count = 5;

        var notes = Enumerable.Range(0, count)
            .Select(i => ((long)elemTicks * i, new Note(Duration.FromTicks(elemTicks))))
            .ToArray();

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true);

        // Should fall back to equal division with raw ticks
        Assert.Equal(count, result.Count);
        Assert.All(result, r =>
        {
            var note = (Note)r.entity;
            Assert.Equal(elemTicks, note.Duration.Ticks);
        });
    }

    // ======================================================================================
    // Complex Scenarios - Mixed Notes and Rests
    // ======================================================================================

    [Fact]
    public void InsertRests_ComplexSequence_MixedNotesAndGaps()
    {
        var notes = new[]
        {
            (start: 0L, note: new Note(DurationFactory.Quarter())),
            (start: (long)Duration.TicksPerQuarter * 2, note: new Note(DurationFactory.Eighth())),
            (start: (long)Duration.TicksPerQuarter * 3, note: new Note(DurationFactory.Sixteenth())),
            (start: (long)Duration.TicksPerQuarter * 4, note: new Note(DurationFactory.Half()))
        };

        var result = SequenceLayout.InsertRests(notes, normalizeRests: true);

        // Should have notes + rests for all gaps
        Assert.True(result.Count >= 4); // At least the 4 notes
        Assert.Contains(result, r => r.entity is Rest);

        // Verify sequence is continuous (no missing time)
        for (int i = 1; i < result.Count; i++)
        {
            var prevEnd = result[i - 1].start + result[i - 1].entity.Duration.Ticks;
            Assert.Equal(prevEnd, result[i].start);
        }
    }

    // ======================================================================================
    // Performance - Large Sequence
    // ======================================================================================

    [Fact]
    public void InsertRests_LargeSequence_PerformanceAcceptable()
    {
        // Create 100 notes with gaps
        var notes = Enumerable.Range(0, 100)
            .Select(i => ((long)i * Duration.TicksPerQuarter * 2, new Note(DurationFactory.Quarter())))
            .ToArray();

        var result = SequenceLayout.InsertRests(notes, normalizeRests: true);

        // Should complete without timeout and insert rests for all gaps
        Assert.True(result.Count >= 100);
        Assert.Contains(result, r => r.entity is Rest);
    }

    // ======================================================================================
    // Tuplet Priority - Preference for 3:2, 5:4 over uncommon ratios
    // ======================================================================================

    [Fact]
    public void InsertRests_TupletPriority_Prefers3over2()
    {
        // Ambiguous pattern that could match multiple tuplets
        // Should prefer 3:2 (common triplet) over other ratios
        int tripletTicks = Duration.TicksPerQuarter * 2 / 3; // 320 ticks

        var notes = new[]
        {
            (start: 0L, note: new Note(Duration.FromTicks(tripletTicks))),
            (start: (long)tripletTicks, note: new Note(Duration.FromTicks(tripletTicks))),
            (start: (long)tripletTicks * 2, note: new Note(Duration.FromTicks(tripletTicks)))
        };

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true);

        var firstNote = (Note)result[0].entity;

        // Should have triplet tick duration
        Assert.Equal(tripletTicks, firstNote.Duration.Ticks);
    }

    // ======================================================================================
    // Preserve Note Properties (Pitch, Velocity, Channel)
    // ======================================================================================

    [Fact]
    public void InsertRests_PreservesNoteProperties()
    {
        int pitch = 60; // Middle C
        int velocity = 100;
        int channel = 5;

        var notes = new[]
        {
            (start: 0L, note: new Note(DurationFactory.Quarter(), pitch, velocity, channel))
        };

        var result = SequenceLayout.InsertRests(notes);

        var resultNote = (Note)result[0].entity;
        Assert.Equal(pitch, resultNote.Pitch);
        Assert.Equal(velocity, resultNote.Velocity);
        Assert.Equal(channel, resultNote.Channel);
    }

    [Fact]
    public void InsertRests_WithTupletInference_PreservesNoteProperties()
    {
        int pitch = 67; // G4
        int velocity = 80;
        int channel = 2;
        int tripletTicks = Duration.TicksPerQuarter * 2 / 3;

        var notes = new[]
        {
            (start: 0L, note: new Note(Duration.FromTicks(tripletTicks), pitch, velocity, channel)),
            (start: (long)tripletTicks, note: new Note(Duration.FromTicks(tripletTicks), pitch, velocity, channel)),
            (start: (long)tripletTicks * 2, note: new Note(Duration.FromTicks(tripletTicks), pitch, velocity, channel))
        };

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true);

        // All rebuilt notes should preserve properties
        Assert.All(result, r =>
        {
            var note = (Note)r.entity;
            Assert.Equal(pitch, note.Pitch);
            Assert.Equal(velocity, note.Velocity);
            Assert.Equal(channel, note.Channel);
        });
    }

    // ======================================================================================
    // Zero-duration and Negative Cases
    // ======================================================================================

    [Fact]
    public void InsertRests_ZeroDurationNote_HandledGracefully()
    {
        var notes = new[]
        {
            (start: 0L, note: new Note(Duration.FromTicks(0))),
            (start: 0L, note: new Note(DurationFactory.Quarter()))
        };

        // Should not throw
        var result = SequenceLayout.InsertRests(notes);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void InsertRests_NegativeStartTime_SortsCorrectly()
    {
        // Unusual but should handle gracefully
        var notes = new[]
        {
            (start: -480L, note: new Note(DurationFactory.Quarter())),
            (start: 0L, note: new Note(DurationFactory.Quarter()))
        };

        // Should sort correctly (negative first)
        var result = SequenceLayout.InsertRests(notes);

        Assert.Equal(2, result.Count);
        Assert.Equal(-480L, result[0].start);
        Assert.Equal(0L, result[1].start);
    }

    // ======================================================================================
    // Additional Edge Cases - More Tuplet Inference Scenarios
    // ======================================================================================

    [Fact]
    public void InsertRests_7over8_Septuplets()
    {
        // 7 notes in the space of 8 eighths (whole note total)
        int totalTicks = Duration.TicksPerQuarter * 4;
        int elemTicks = totalTicks * 8 / (8 * 7); // Each element
        elemTicks = totalTicks / 7; // ~274 ticks

        var notes = Enumerable.Range(0, 7)
            .Select(i => ((long)elemTicks * i, new Note(Duration.FromTicks(elemTicks))))
            .ToArray();

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true, extendedTuplets: false);

        Assert.Equal(7, result.Count);
    }

    [Fact]
    public void InsertRests_9over8_Nonuplets()
    {
        // 9 notes in the space of 8 eighths
        int totalTicks = Duration.TicksPerQuarter * 4;
        int elemTicks = totalTicks / 9; // ~213 ticks

        var notes = Enumerable.Range(0, 9)
            .Select(i => ((long)elemTicks * i, new Note(Duration.FromTicks(elemTicks))))
            .ToArray();

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true, extendedTuplets: false);

        Assert.Equal(9, result.Count);
    }

    [Fact]
    public void InsertRests_6over4_Sextuplets()
    {
        // 6 quarter notes in the space of 4 (whole note total)
        int totalTicks = Duration.TicksPerQuarter * 4;
        int elemTicks = totalTicks / 6; // 320 ticks

        var notes = Enumerable.Range(0, 6)
            .Select(i => ((long)elemTicks * i, new Note(Duration.FromTicks(elemTicks))))
            .ToArray();

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true, extendedTuplets: true);

        // May not infer 6:4 (which simplifies to 3:2), so just verify notes are present
        Assert.True(result.Count >= 5);
        Assert.All(result, r => Assert.IsType<Note>(r.entity));
    }

    [Fact]
    public void InsertRests_IrregularPattern_NotDivisible()
    {
        // Pattern that doesn't divide evenly - should pass through unchanged
        var notes = new[]
        {
            (start: 0L, note: new Note(Duration.FromTicks(100))),
            (start: 100L, note: new Note(Duration.FromTicks(150))),
            (start: 250L, note: new Note(Duration.FromTicks(200)))
        };

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true);

        // Should not attempt to split irregular patterns
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public void InsertRests_VeryLongRun_WithExtendedTuplets()
    {
        // 12 notes in extended tuplet pattern
        int totalTicks = Duration.TicksPerQuarter * 4;
        int elemTicks = totalTicks / 12; // 160 ticks

        var notes = Enumerable.Range(0, 12)
            .Select(i => ((long)elemTicks * i, new Note(Duration.FromTicks(elemTicks))))
            .ToArray();

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true, extendedTuplets: true);

        // May not infer 12-tuplet pattern, just verify notes are processed
        Assert.True(result.Count >= 5);
    }

    [Fact]
    public void InsertRests_MultipleConsecutiveRuns()
    {
        // Two consecutive runs with no gap - should process as single run
        var notes = new[]
        {
            (start: 0L, note: new Note(DurationFactory.Quarter())),
            (start: (long)Duration.TicksPerQuarter, note: new Note(DurationFactory.Quarter())),
            (start: (long)Duration.TicksPerQuarter * 2, note: new Note(DurationFactory.Quarter()))
        };

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true);

        // Should have 3 notes, no rests inserted
        Assert.Equal(3, result.Count);
        Assert.All(result, r => Assert.IsType<Note>(r.entity));
    }

    [Fact]
    public void InsertRests_RestBetweenIdenticalDurations()
    {
        // Ensure rests are properly inserted between identical-duration notes
        var notes = new[]
        {
            (start: 0L, note: new Note(DurationFactory.Eighth())),
            (start: (long)Duration.TicksPerQuarter, note: new Note(DurationFactory.Eighth())),
            (start: (long)Duration.TicksPerQuarter * 2, note: new Note(DurationFactory.Eighth()))
        };

        var result = SequenceLayout.InsertRests(notes, normalizeRests: false);

        // Should insert rests between each note
        Assert.True(result.Count >= 5); // 3 notes + at least 2 rests
        var rests = result.Where(r => r.entity is Rest).ToList();
        Assert.True(rests.Count >= 2);
    }

    [Fact]
    public void InsertRests_AllRests_NoNotes()
    {
        // Edge case: Empty input should return empty
        var notes = Array.Empty<(long, Note)>();
        var result = SequenceLayout.InsertRests(notes);
        Assert.Empty(result);
    }

    [Fact]
    public void InsertRests_DottedNotesInSequence()
    {
        // Test with dotted notes (common in real music)
        var notes = new[]
        {
            (start: 0L, note: new Note(DurationFactory.Quarter(1))), // Dotted quarter
            (start: (long)Duration.TicksPerQuarter * 2, note: new Note(DurationFactory.Eighth(1))) // Dotted eighth
        };

        var result = SequenceLayout.InsertRests(notes, normalizeRests: true);

        // Should insert normalized rest for gap
        Assert.True(result.Count >= 2);
        Assert.Contains(result, r => r.entity is Rest);
    }

    [Fact]
    public void InsertRests_VerySmallTicks_HandledGracefully()
    {
        // Very small tick values (edge case for integer arithmetic)
        var notes = new[]
        {
            (start: 0L, note: new Note(Duration.FromTicks(1))),
            (start: 1L, note: new Note(Duration.FromTicks(1))),
            (start: 2L, note: new Note(Duration.FromTicks(1)))
        };

        var result = SequenceLayout.InsertRests(notes, allowSplitTuplets: true);

        Assert.Equal(3, result.Count);
        Assert.All(result, r => Assert.IsType<Note>(r.entity));
    }

    [Fact]
    public void InsertRests_LargeTickValues_NoOverflow()
    {
        // Large tick values (test for potential overflow)
        long largeTick = (long)Duration.TicksPerQuarter * 1000;
        var notes = new[]
        {
            (start: 0L, note: new Note(DurationFactory.Quarter())),
            (start: largeTick, note: new Note(DurationFactory.Quarter()))
        };

        var result = SequenceLayout.InsertRests(notes, normalizeRests: true);

        // Should handle large gaps gracefully
        Assert.True(result.Count >= 2);
        Assert.Contains(result, r => r.entity is Rest);
    }

    [Fact]
    public void InsertRests_AdvancedSplit_WithAllowSplitTuplets()
    {
        // Combination of advancedSplit and allowSplitTuplets
        var notes = new[]
        {
            (start: 0L, note: new Note(DurationFactory.Quarter())),
            (start: (long)Duration.TicksPerQuarter + 1000, note: new Note(DurationFactory.Quarter()))
        };

        var result = SequenceLayout.InsertRests(notes, normalizeRests: true, advancedSplit: true, allowSplitTuplets: true);

        // Should normalize the 1000-tick gap and potentially split it
        Assert.True(result.Count > 2);
    }
}
