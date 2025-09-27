using System.Linq;
using MusicTheory.Theory.Time;
using Xunit;

namespace MusicTheory.Tests;

public class NoteValueZoomSelectionTests
{
    [Fact]
    public void SnapAllToIndex_MaintainStarts_Works()
    {
        var notes = new[]{
            (0L, new Note(DurationFactory.Eighth())),
            (240L, new Note(DurationFactory.Sixteenth())),
            (360L, new Note(DurationFactory.Sixteenth()))
        };
        int idx = NoteValueZoom.FindNearestIndex(DurationFactory.Quarter());
        var snapped = NoteValueZoomSelection.SnapAllToIndexMaintainStarts(notes, idx).ToList();
        Assert.All(snapped, x => Assert.Equal(DurationFactory.Quarter().Ticks, x.note.Duration.Ticks));
        Assert.Equal(new long[]{0,240,360}, snapped.Select(x=>x.start).ToArray());
    }

    [Fact]
    public void SnapAllToIndex_PreserveTotalSpan_Works()
    {
        var notes = new[]{
            (0L, new Note(DurationFactory.Eighth())),
            (240L, new Note(DurationFactory.Sixteenth())),
            (360L, new Note(DurationFactory.Sixteenth()))
        };
        long spanEnd = notes.Last().Item1 + notes.Last().Item2.Duration.Ticks; // 360+120=480
        int idx = NoteValueZoom.FindNearestIndex(DurationFactory.Sixteenth());
        var snapped = NoteValueZoomSelection.SnapAllToIndexPreserveTotalSpan(notes, idx).ToList();
        long newEnd = snapped.Last().start + snapped.Last().note.Duration.Ticks;
        Assert.Equal(spanEnd, newEnd);
    }
}
