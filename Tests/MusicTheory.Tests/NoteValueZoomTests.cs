using System.Linq;
using MusicTheory.Theory.Time;
using Xunit;

namespace MusicTheory.Tests;

public class NoteValueZoomTests
{
    [Fact]
    public void Entries_Are_Sorted_And_Cover_Requested_List()
    {
        var labels = NoteValueZoom.Entries.Select(e=>e.Label).ToArray();
        Assert.Contains("32分音符[*0.125]", labels);
        Assert.Contains("16分三連符[*0.165]", labels);
        Assert.Contains("8分三連符[*0.33]", labels);
        Assert.Contains("2分三連符[*1.32]", labels);
        Assert.Equal(NoteValueZoom.Entries.OrderBy(e=>e.Ticks).Select(e=>e.Label), labels);
    }

    [Fact]
    public void Zoom_Out_Then_In_Stays_Near_List()
    {
        var start = DurationFactory.Quarter(); // 4分[*1]
        var smaller = NoteValueZoom.ZoomOut(start); // 付点8分 or 4分三連符 付近 → 8分三連符[*0.33] よりは大、8分[*0.5]よりは小
        var larger = NoteValueZoom.ZoomIn(start);
        Assert.True(smaller.Ticks < start.Ticks);
        Assert.True(larger.Ticks > start.Ticks);
    }

    [Fact]
    public void Zoom_Clamps_At_Bounds()
    {
        var min = NoteValueZoom.Entries.First().Create();
        var max = NoteValueZoom.Entries.Last().Create();
        // さらに縮小/拡大しても端で止まる
        Assert.Equal(min.Ticks, NoteValueZoom.ZoomOut(min).Ticks);
        Assert.Equal(max.Ticks, NoteValueZoom.ZoomIn(max).Ticks);
    }
}
