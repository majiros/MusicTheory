namespace MusicTheory.Tests;

using MusicTheory.Theory.Interval;
using MusicTheory.Theory.Time;
using MusicTheory.Theory.Chord;
using System.Linq;
using Xunit;

public class MediumCoverageStructsTests
{
    // ========================================
    // FunctionalInterval DisplayName Tests (52.9% → 100%)
    // ========================================
    
    [Fact]
    public void FunctionalInterval_DisplayName_MinorSecond()
    {
        var interval = new FunctionalInterval(IntervalType.MinorSecond);
        Assert.Equal("♭9", interval.DisplayName);
    }
    
    [Fact]
    public void FunctionalInterval_DisplayName_MajorSecond()
    {
        var interval = new FunctionalInterval(IntervalType.MajorSecond);
        Assert.Equal("9", interval.DisplayName);
    }
    
    [Fact]
    public void FunctionalInterval_DisplayName_AugmentedNinth()
    {
        var interval = new FunctionalInterval(IntervalType.AugmentedNinth);
        Assert.Equal("♯9", interval.DisplayName);
    }
    
    [Fact]
    public void FunctionalInterval_DisplayName_PerfectEleventh()
    {
        var interval = new FunctionalInterval(IntervalType.PerfectEleventh);
        Assert.Equal("11", interval.DisplayName);
    }
    
    [Fact]
    public void FunctionalInterval_DisplayName_AugmentedEleventh()
    {
        var interval = new FunctionalInterval(IntervalType.AugmentedEleventh);
        Assert.Equal("♯11", interval.DisplayName);
    }
    
    [Fact]
    public void FunctionalInterval_DisplayName_MinorThirteenth()
    {
        var interval = new FunctionalInterval(IntervalType.MinorThirteenth);
        Assert.Equal("♭13", interval.DisplayName);
    }
    
    [Fact]
    public void FunctionalInterval_DisplayName_MajorThirteenth()
    {
        var interval = new FunctionalInterval(IntervalType.MajorThirteenth);
        Assert.Equal("13", interval.DisplayName);
    }
    
    [Fact]
    public void FunctionalInterval_DisplayName_DefaultCase_ReturnsTypeName()
    {
        var interval = new FunctionalInterval(IntervalType.PerfectFifth);
        Assert.Equal("PerfectFifth", interval.DisplayName);
    }
    
    [Fact]
    public void FunctionalInterval_WithFunction_StoresFunction()
    {
        var interval = new FunctionalInterval(IntervalType.MajorSecond, TensionFunction.Modal);
        Assert.Equal(TensionFunction.Modal, interval.Function);
        Assert.Equal("9", interval.DisplayName);
    }

    // ========================================
    // Tempo Tests (62.5% → 100%)
    // ========================================
    
    [Fact]
    public void Tempo_Constructor_StoresMicrosecondsPerQuarter()
    {
        var tempo = new Tempo(500000); // 120 BPM
        Assert.Equal(500000, tempo.MicrosecondsPerQuarter);
    }
    
    [Fact]
    public void Tempo_Bpm_CalculatesCorrectly()
    {
        var tempo = new Tempo(500000); // 120 BPM
        Assert.Equal(120.0, tempo.Bpm, precision: 2);
    }
    
    [Fact]
    public void Tempo_FromBpm_CreatesCorrectTempo()
    {
        var tempo = Tempo.FromBpm(120);
        Assert.Equal(500000, tempo.MicrosecondsPerQuarter);
        Assert.Equal(120.0, tempo.Bpm, precision: 2);
    }
    
    [Fact]
    public void Tempo_Equals_ComparesCorrectly()
    {
        var tempo1 = new Tempo(500000);
        var tempo2 = new Tempo(500000);
        var tempo3 = new Tempo(600000);
        
        Assert.True(tempo1.Equals(tempo2));
        Assert.False(tempo1.Equals(tempo3));
    }
    
    [Fact]
    public void Tempo_GetHashCode_ConsistentWithEquals()
    {
        var tempo1 = new Tempo(500000);
        var tempo2 = new Tempo(500000);
        
        Assert.Equal(tempo1.GetHashCode(), tempo2.GetHashCode());
    }
    
    [Fact]
    public void Tempo_ToString_FormatsBpm()
    {
        var tempo = new Tempo(500000);
        var str = tempo.ToString();
        Assert.Contains("120", str);
        Assert.Contains("BPM", str);
    }
    
    [Fact]
    public void Tempo_InvalidConstructor_ThrowsException()
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() => new Tempo(0));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => new Tempo(-1000));
    }

    // ========================================
    // ChordFormula Tests (64.7% → 100%)
    // ========================================
    
    [Fact]
    public void ChordFormula_GetDisplayName_NoTensions_ReturnsSymbolOnly()
    {
        var formula = ChordFormula.Maj7;
        var displayName = formula.GetDisplayName();
        Assert.Equal("maj7", displayName);
    }
    
    [Fact]
    public void ChordFormula_GetDisplayName_WithTensions_IncludesTensionPart()
    {
        // Find a formula with tensions (e.g., 9, 11, 13 chords)
        var formulas = ChordFormulas.All.Where(f => f.Tensions.Length > 0).ToList();
        Assert.NotEmpty(formulas);
        
        var formula = formulas.First();
        var displayName = formula.GetDisplayName();
        
        // Should contain symbol and parenthesized tension part
        Assert.Contains(formula.Symbol, displayName);
        Assert.Contains("(", displayName);
        Assert.Contains(")", displayName);
    }
    
    [Fact]
    public void ChordFormula_Properties_StoreCorrectly()
    {
        var formula = ChordFormula.Dom7;
        Assert.Equal("Dom7", formula.Name);
        Assert.Equal("7", formula.Symbol);
        Assert.NotEmpty(formula.CoreIntervals);
        Assert.Empty(formula.Tensions); // Dom7 has no tensions by default
    }
    
    [Fact]
    public void ChordFormula_Aliases_StoresCorrectly()
    {
        var formula = ChordFormula.Maj7;
        Assert.Contains("M7", formula.Aliases);
        Assert.Contains("Δ7", formula.Aliases);
    }

    // ========================================
    // NoteValueZoomSelection Tests (56.8% → 100%)
    // ========================================
    
    [Fact]
    public void NoteValueZoomSelection_ChooseTargetIndexByMedian_EmptyInput_ReturnsQuarterIndex()
    {
        var notes = System.Array.Empty<Note>();
        int idx = NoteValueZoomSelection.ChooseTargetIndexByMedian(notes);
        
        // Should return index of quarter note (default)
        var quarterIdx = NoteValueZoom.FindNearestIndex(DurationFactory.Quarter());
        Assert.Equal(quarterIdx, idx);
    }
    
    [Fact]
    public void NoteValueZoomSelection_ChooseTargetIndexByMedian_SingleNote_ReturnsItsIndex()
    {
        var notes = new[] { new Note(DurationFactory.Eighth()) };
        int idx = NoteValueZoomSelection.ChooseTargetIndexByMedian(notes);
        
        var expectedIdx = NoteValueZoom.FindNearestIndex(DurationFactory.Eighth());
        Assert.Equal(expectedIdx, idx);
    }
    
    [Fact]
    public void NoteValueZoomSelection_ChooseTargetIndexByMedian_MultipleNotes_ReturnsMedian()
    {
        var notes = new[]
        {
            new Note(DurationFactory.Sixteenth()),
            new Note(DurationFactory.Eighth()),
            new Note(DurationFactory.Quarter())
        };
        int idx = NoteValueZoomSelection.ChooseTargetIndexByMedian(notes);
        
        // Median of three sorted indices should be the middle one (Eighth)
        var expectedIdx = NoteValueZoom.FindNearestIndex(DurationFactory.Eighth());
        Assert.Equal(expectedIdx, idx);
    }
    
    [Fact]
    public void NoteValueZoomSelection_ZoomSelection_NonPreservingSpan_ZoomsAllNotes()
    {
        var sel = new[]
        {
            (0L, new Note(DurationFactory.Eighth())),
            (240L, new Note(DurationFactory.Sixteenth()))
        };
        
        var zoomed = NoteValueZoomSelection.ZoomSelection(sel, delta: +1, preserveTotalSpan: false).ToList();
        
        Assert.Equal(2, zoomed.Count);
        // Each note should be zoomed independently
        Assert.All(zoomed, x => Assert.True(x.note.Duration.Ticks > 0));
    }
    
    [Fact]
    public void NoteValueZoomSelection_ZoomSelection_PreservingSpan_MaintainsTotalLength()
    {
        var sel = new[]
        {
            (0L, new Note(DurationFactory.Eighth())), // 240 ticks
            (240L, new Note(DurationFactory.Eighth())) // 240 ticks
        }.OrderBy(x => x.Item1).ToArray();
        
        long originalSpan = sel.Last().Item1 + sel.Last().Item2.Duration.Ticks - sel.First().Item1;
        
        var zoomed = NoteValueZoomSelection.ZoomSelection(sel, delta: +1, preserveTotalSpan: true).ToList();
        
        long newSpan = zoomed.Last().start + zoomed.Last().note.Duration.Ticks - zoomed.First().start;
        
        // Total span should be preserved
        Assert.Equal(originalSpan, newSpan);
    }
    
    [Fact]
    public void NoteValueZoomSelection_SnapAllToIndexPreserveTotalSpan_SingleNote_Works()
    {
        var notes = new[]
        {
            (0L, new Note(DurationFactory.Eighth()))
        };
        
        int targetIdx = NoteValueZoom.FindNearestIndex(DurationFactory.Quarter());
        var snapped = NoteValueZoomSelection.SnapAllToIndexPreserveTotalSpan(notes, targetIdx).ToList();
        
        Assert.Single(snapped);
        Assert.True(snapped[0].note.Duration.Ticks >= 1);
    }

    // ========================================
    // IntervalName Additional Coverage Tests (61.5% → 100%)
    // ========================================
    
    [Fact]
    public void IntervalName_Equality_WorksCorrectly()
    {
        var name1 = new IntervalName(IntervalQuality.Perfect, 5);
        var name2 = new IntervalName(IntervalQuality.Perfect, 5);
        var name3 = new IntervalName(IntervalQuality.Major, 6);
        
        Assert.Equal(name1, name2);
        Assert.NotEqual(name1, name3);
    }
    
    [Fact]
    public void IntervalName_GetHashCode_ConsistentWithEquals()
    {
        var name1 = new IntervalName(IntervalQuality.Minor, 3);
        var name2 = new IntervalName(IntervalQuality.Minor, 3);
        
        Assert.Equal(name1.GetHashCode(), name2.GetHashCode());
    }
}
