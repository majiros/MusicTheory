namespace MusicTheory.Tests;

public class ChordNameParseTests
{
    [Theory]
    [InlineData("Cmaj7","C","maj7")]
    [InlineData("Am7","A","m7")]
    [InlineData("G7alt","G","7alt")]
    public void ParseChordName(string text, string expectedRoot, string expectedSymbol)
    {
        var parsed = MusicTheory.Theory.Chord.ChordFormulas.ParseChordName(text);
        Assert.NotNull(parsed);
        Assert.Equal(expectedRoot, parsed!.Root);
        Assert.Equal(expectedSymbol, parsed.Formula.Symbol);
    }
}
