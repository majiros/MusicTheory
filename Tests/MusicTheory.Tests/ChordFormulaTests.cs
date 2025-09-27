namespace MusicTheory.Tests;

public class ChordFormulaTests
{
    [Fact]
    public void Dominant7_CoreIntervals()
    {
        var dom7 = MusicTheory.Theory.Chord.ChordFormulas.All.First(f => f.Symbol == "7");
        var semis = dom7.CoreIntervals.Select(i => i.Semitones).OrderBy(x=>x).ToArray();
        Assert.Equal(new[]{4,7,10}, semis); // 3rd,5th,b7
    }

    [Fact]
    public void AlteredDominant_Tensions()
    {
        var alt = MusicTheory.Theory.Chord.ChordFormulas.All.First(f => f.Symbol == "7alt");
        var tens = alt.Tensions.Select(i => i.Semitones % 12).OrderBy(x=>x).ToArray();
        // b9(1), #9(3), #11(6), b13(8)
        Assert.Equal(new[]{1,3,6,8}, tens);
    }
}
