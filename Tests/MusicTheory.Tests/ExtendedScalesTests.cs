using Xunit;
using MusicTheory.Theory.Scale;
using System.Linq;

namespace MusicTheory.Tests;

/// <summary>
/// Tests for ExtendedScales to improve coverage.
/// </summary>
public class ExtendedScalesTests
{
    [Fact]
    public void All_ContainsAllChurchModes()
    {
        var all = ExtendedScales.All.ToList();

        Assert.Contains(all, s => s.Name == "Ionian");
        Assert.Contains(all, s => s.Name == "Dorian");
        Assert.Contains(all, s => s.Name == "Phrygian");
        Assert.Contains(all, s => s.Name == "Lydian");
        Assert.Contains(all, s => s.Name == "Mixolydian");
        Assert.Contains(all, s => s.Name == "Aeolian");
        Assert.Contains(all, s => s.Name == "Locrian");
    }

    [Fact]
    public void All_ContainsJazzScales()
    {
        var all = ExtendedScales.All.ToList();

        Assert.Contains(all, s => s.Name == "Lydian Dominant");
        Assert.Contains(all, s => s.Name == "Altered");
        Assert.Contains(all, s => s.Name == "Bebop Major");
        Assert.Contains(all, s => s.Name == "Bebop Dominant");
    }

    [Fact]
    public void All_ContainsSymmetricScales()
    {
        var all = ExtendedScales.All.ToList();

        Assert.Contains(all, s => s.Name == "Whole Tone");
        Assert.Contains(all, s => s.Name == "Diminished (H-W)");
        Assert.Contains(all, s => s.Name == "Diminished (W-H)");
    }

    [Fact]
    public void All_Returns14Scales()
    {
        var all = ExtendedScales.All.ToList();

        // 7 church modes + 4 bebop/altered + 3 symmetric = 14
        Assert.Equal(14, all.Count);
    }

    [Fact]
    public void Ionian_HasSevenNotes()
    {
        var ionian = ExtendedScales.Ionian;

        Assert.Equal("Ionian", ionian.Name);
        Assert.Equal(7, ionian.GetSemitoneSet().Count);
        Assert.Equal(new[] { 0, 2, 4, 5, 7, 9, 11 }, ionian.GetSemitoneSet());
    }

    [Fact]
    public void Dorian_HasSevenNotes()
    {
        var dorian = ExtendedScales.Dorian;

        Assert.Equal("Dorian", dorian.Name);
        Assert.Equal(new[] { 0, 2, 3, 5, 7, 9, 10 }, dorian.GetSemitoneSet());
    }

    [Fact]
    public void Phrygian_HasSevenNotes()
    {
        var phrygian = ExtendedScales.Phrygian;

        Assert.Equal("Phrygian", phrygian.Name);
        Assert.Equal(new[] { 0, 1, 3, 5, 7, 8, 10 }, phrygian.GetSemitoneSet());
    }

    [Fact]
    public void Lydian_HasSevenNotes_Sharp4()
    {
        var lydian = ExtendedScales.Lydian;

        Assert.Equal("Lydian", lydian.Name);
        Assert.Equal(new[] { 0, 2, 4, 6, 7, 9, 11 }, lydian.GetSemitoneSet());
        Assert.Contains(6, lydian.GetSemitoneSet()); // #4
    }

    [Fact]
    public void Mixolydian_HasSevenNotes_Flat7()
    {
        var mixolydian = ExtendedScales.Mixolydian;

        Assert.Equal("Mixolydian", mixolydian.Name);
        Assert.Equal(new[] { 0, 2, 4, 5, 7, 9, 10 }, mixolydian.GetSemitoneSet());
        Assert.Contains(10, mixolydian.GetSemitoneSet()); // b7
    }

    [Fact]
    public void Aeolian_HasSevenNotes()
    {
        var aeolian = ExtendedScales.Aeolian;

        Assert.Equal("Aeolian", aeolian.Name);
        Assert.Equal(new[] { 0, 2, 3, 5, 7, 8, 10 }, aeolian.GetSemitoneSet());
    }

    [Fact]
    public void Locrian_HasSevenNotes_Flat2_Flat5()
    {
        var locrian = ExtendedScales.Locrian;

        Assert.Equal("Locrian", locrian.Name);
        Assert.Equal(new[] { 0, 1, 3, 5, 6, 8, 10 }, locrian.GetSemitoneSet());
        Assert.Contains(1, locrian.GetSemitoneSet()); // b2
        Assert.Contains(6, locrian.GetSemitoneSet()); // b5
    }

    [Fact]
    public void LydianDominant_HasSevenNotes_Sharp4_Flat7()
    {
        var lydianDom = ExtendedScales.LydianDominant;

        Assert.Equal("Lydian Dominant", lydianDom.Name);
        Assert.Equal(new[] { 0, 2, 4, 6, 7, 9, 10 }, lydianDom.GetSemitoneSet());
        Assert.Contains(6, lydianDom.GetSemitoneSet()); // #4 (#11)
        Assert.Contains(10, lydianDom.GetSemitoneSet()); // b7
    }

    [Fact]
    public void Altered_HasSevenNotes()
    {
        var altered = ExtendedScales.Altered;

        Assert.Equal("Altered", altered.Name);
        Assert.Equal(new[] { 0, 1, 3, 4, 6, 8, 10 }, altered.GetSemitoneSet());
        Assert.Equal(7, altered.GetSemitoneSet().Count);
    }

    [Fact]
    public void BebopMajor_HasEightNotes()
    {
        var bebopMajor = ExtendedScales.BebopMajor;

        Assert.Equal("Bebop Major", bebopMajor.Name);
        Assert.Equal(new[] { 0, 2, 4, 5, 7, 8, 9, 11 }, bebopMajor.GetSemitoneSet());
        Assert.Equal(8, bebopMajor.GetSemitoneSet().Count);
    }

    [Fact]
    public void BebopDominant_HasEightNotes()
    {
        var bebopDom = ExtendedScales.BebopDominant;

        Assert.Equal("Bebop Dominant", bebopDom.Name);
        Assert.Equal(new[] { 0, 2, 4, 5, 7, 9, 10, 11 }, bebopDom.GetSemitoneSet());
        Assert.Equal(8, bebopDom.GetSemitoneSet().Count);
    }

    [Fact]
    public void WholeTone_HasSixNotes()
    {
        var wholeTone = ExtendedScales.WholeTone;

        Assert.Equal("Whole Tone", wholeTone.Name);
        Assert.Equal(new[] { 0, 2, 4, 6, 8, 10 }, wholeTone.GetSemitoneSet());
        Assert.Equal(6, wholeTone.GetSemitoneSet().Count);
    }

    [Fact]
    public void WholeTone_AllIntervalsAreWholeTones()
    {
        var wholeTone = ExtendedScales.WholeTone;
        var semitones = wholeTone.GetSemitoneSet().OrderBy(x => x).ToList();

        // Check all adjacent intervals are 2 semitones (whole tone)
        for (int i = 0; i < semitones.Count - 1; i++)
        {
            Assert.Equal(2, semitones[i + 1] - semitones[i]);
        }
    }

    [Fact]
    public void DiminishedHalfWhole_HasEightNotes()
    {
        var dimHW = ExtendedScales.DiminishedHalfWhole;

        Assert.Equal("Diminished (H-W)", dimHW.Name);
        Assert.Equal(new[] { 0, 1, 3, 4, 6, 7, 9, 10 }, dimHW.GetSemitoneSet());
        Assert.Equal(8, dimHW.GetSemitoneSet().Count);
    }

    [Fact]
    public void DiminishedHalfWhole_AlternatesHalfWhole()
    {
        var dimHW = ExtendedScales.DiminishedHalfWhole;
        var semitones = dimHW.GetSemitoneSet().OrderBy(x => x).ToList();

        // Pattern: H-W-H-W-H-W-H-W (1-2-1-2-1-2-1-2)
        Assert.Equal(1, semitones[1] - semitones[0]); // H
        Assert.Equal(2, semitones[2] - semitones[1]); // W
        Assert.Equal(1, semitones[3] - semitones[2]); // H
        Assert.Equal(2, semitones[4] - semitones[3]); // W
    }

    [Fact]
    public void DiminishedWholeHalf_HasEightNotes()
    {
        var dimWH = ExtendedScales.DiminishedWholeHalf;

        Assert.Equal("Diminished (W-H)", dimWH.Name);
        Assert.Equal(new[] { 0, 2, 3, 5, 6, 8, 9, 11 }, dimWH.GetSemitoneSet());
        Assert.Equal(8, dimWH.GetSemitoneSet().Count);
    }

    [Fact]
    public void DiminishedWholeHalf_AlternatesWholeHalf()
    {
        var dimWH = ExtendedScales.DiminishedWholeHalf;
        var semitones = dimWH.GetSemitoneSet().OrderBy(x => x).ToList();

        // Pattern: W-H-W-H-W-H-W-H (2-1-2-1-2-1-2-1)
        Assert.Equal(2, semitones[1] - semitones[0]); // W
        Assert.Equal(1, semitones[2] - semitones[1]); // H
        Assert.Equal(2, semitones[3] - semitones[2]); // W
        Assert.Equal(1, semitones[4] - semitones[3]); // H
    }

    [Fact]
    public void All_EnumerationProducesDistinctScales()
    {
        var all = ExtendedScales.All.ToList();
        var names = all.Select(s => s.Name).ToList();

        // All names should be distinct
        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Fact]
    public void All_CanBeEnumeratedMultipleTimes()
    {
        var first = ExtendedScales.All.ToList();
        var second = ExtendedScales.All.ToList();

        Assert.Equal(first.Count, second.Count);

        // Names should match
        var firstNames = first.Select(s => s.Name).ToList();
        var secondNames = second.Select(s => s.Name).ToList();
        Assert.Equal(firstNames, secondNames);
    }

    [Fact]
    public void ModalScale_ContainsSemitone_Works()
    {
        var ionian = ExtendedScales.Ionian;

        Assert.True(ionian.ContainsSemitone(0)); // Root
        Assert.True(ionian.ContainsSemitone(4)); // Major 3rd
        Assert.True(ionian.ContainsSemitone(7)); // Perfect 5th
        Assert.False(ionian.ContainsSemitone(1)); // b9
        Assert.False(ionian.ContainsSemitone(6)); // #11
    }

    [Fact]
    public void ModalScale_ContainsSemitone_NormalizesNegative()
    {
        var ionian = ExtendedScales.Ionian;

        // -1 mod 12 = 11
        Assert.True(ionian.ContainsSemitone(-1)); // Should map to 11
        // -2 mod 12 = 10
        Assert.False(ionian.ContainsSemitone(-2)); // Should map to 10 (not in Ionian)
    }

    [Fact]
    public void ModalScale_ContainsSemitone_NormalizesOver12()
    {
        var ionian = ExtendedScales.Ionian;

        // 12 mod 12 = 0
        Assert.True(ionian.ContainsSemitone(12)); // Root
        // 16 mod 12 = 4
        Assert.True(ionian.ContainsSemitone(16)); // Major 3rd
        // 13 mod 12 = 1
        Assert.False(ionian.ContainsSemitone(13)); // b9
    }
}
