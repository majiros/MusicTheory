using System.Linq;
using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class RomanInputParserTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Fact]
    public void Parses_Triad_Inversions()
    {
        var key = new Key(60, true); // C major
        var res = RomanInputParser.Parse("V6; I64", key);
        Assert.Equal(2, res.Length);

        // V6 => G-B-D, bass=B(11)
        AssertEx.SetEquals(new[] { Pc(7), Pc(11), Pc(2) }, res[0].Pcs);
        Assert.Equal(Pc(11), res[0].BassPcHint);

        // I64 => C-E-G, bass=G(7)
        AssertEx.SetEquals(new[] { Pc(0), Pc(4), Pc(7) }, res[1].Pcs);
        Assert.Equal(Pc(7), res[1].BassPcHint);
    }

    [Fact]
    public void Parses_Roman_With_Invisible_Chars_And_Flat_Sign()
    {
        var key = new Key(60, true); // C major
        // 含意:
        // - '♭' を 'b' として扱う
        // - N の直後にゼロ幅スペースがあっても無視
        // - ASCII '0' は ø、'o' は ° として扱う
        var res = RomanInputParser.Parse("♭II6; N\u200B6; vii0/V; viio7/V", key);
        Assert.Equal(4, res.Length);

        // ♭II6 => bII6 と同義: Db F Ab / bass=F
        AssertEx.SetEquals(new[] { Pc(1), Pc(5), Pc(8) }, res[0].Pcs);
        Assert.Equal(Pc(5), res[0].BassPcHint);

        // N(ゼロ幅スペース)6 => bII6 と同義
        AssertEx.SetEquals(new[] { Pc(1), Pc(5), Pc(8) }, res[1].Pcs);
        Assert.Equal(Pc(5), res[1].BassPcHint);

        // vii0/V => 半減七: F# A C E
        AssertEx.SetEquals(new[] { Pc(6), Pc(9), Pc(0), Pc(4) }, res[2].Pcs);
        Assert.Equal(Pc(6), res[2].BassPcHint);

        // viio7/V => 減七: F# A C Eb
        AssertEx.SetEquals(new[] { Pc(6), Pc(9), Pc(0), Pc(3) }, res[3].Pcs);
        Assert.Equal(Pc(6), res[3].BassPcHint);
    }

    [Fact]
    public void Parses_Mixture_Chords_and_Neapolitan_Six()
    {
        var key = new Key(60, true); // C major
    var roman = "bII; bIII; bVI; bVII; bII6; N6";
    var res = RomanInputParser.Parse(roman, key);
    Assert.Equal(6, res.Length);

        // bII => Db F Ab
    AssertEx.SetEquals(new[] { Pc(1), Pc(5), Pc(8) }, res[0].Pcs, roman);
        Assert.Equal(Pc(1), res[0].BassPcHint);

        // bIII => Eb G Bb
    AssertEx.SetEquals(new[] { Pc(3), Pc(7), Pc(10) }, res[1].Pcs, roman);
        Assert.Equal(Pc(3), res[1].BassPcHint);

        // bVI => Ab C Eb
    AssertEx.SetEquals(new[] { Pc(8), Pc(0), Pc(3) }, res[2].Pcs, roman);
        Assert.Equal(Pc(8), res[2].BassPcHint);

        // bVII => Bb D F
    AssertEx.SetEquals(new[] { Pc(10), Pc(2), Pc(5) }, res[3].Pcs, roman);
        Assert.Equal(Pc(10), res[3].BassPcHint);

    // bII6 => first inversion: 3rd in bass (F)
    AssertEx.SetEquals(new[] { Pc(1), Pc(5), Pc(8) }, res[4].Pcs, roman);
        Assert.Equal(Pc(5), res[4].BassPcHint);

    // N6 => same as bII6
    AssertEx.SetEquals(new[] { Pc(1), Pc(5), Pc(8) }, res[5].Pcs, roman);
    Assert.Equal(Pc(5), res[5].BassPcHint);
    }

    [Fact]
    public void Parses_Neapolitan_N_and_N64()
    {
        var key = new Key(60, true); // C major
        var res = RomanInputParser.Parse("N; N64", key);
        Assert.Equal(2, res.Length);

        // N (root) => Db F Ab, bass=Db
        AssertEx.SetEquals(new[] { Pc(1), Pc(5), Pc(8) }, res[0].Pcs);
        Assert.Equal(Pc(1), res[0].BassPcHint);

        // N64 => second inversion: Ab in bass
        AssertEx.SetEquals(new[] { Pc(1), Pc(5), Pc(8) }, res[1].Pcs);
        Assert.Equal(Pc(8), res[1].BassPcHint);
    }

    [Fact]
    public void Parses_MinorKey_ii_and_ii7_Qualities()
    {
        var aMinor = new Key(57, false); // A minor
        var res = RomanInputParser.Parse("ii; ii7", aMinor);
        Assert.Equal(2, res.Length);

        // ii in minor => diminished triad: B D F
        AssertEx.SetEquals(new[] { Pc(11), Pc(2), Pc(5) }, res[0].Pcs);
        // ii7 in minor => half-diminished seventh: B D F A
        AssertEx.SetEquals(new[] { Pc(11), Pc(2), Pc(5), Pc(9) }, res[1].Pcs);
    }

    [Fact]
    public void Parses_Secondary_LeadingTone_HalfDiminished()
    {
        var key = new Key(60, true); // C major
        var res = RomanInputParser.Parse("viiø7/V", key);
        Assert.Single(res);
        // viiø7/V = F# A C E
        AssertEx.SetEquals(new[] { Pc(6), Pc(9), Pc(0), Pc(4) }, res[0].Pcs);
        Assert.Equal(Pc(6), res[0].BassPcHint);
    }

    [Fact]
    public void Parses_Dominant_Seventh_Inversions()
    {
        var key = new Key(60, true); // C major
        var res = RomanInputParser.Parse("V7; V65; V43; V42", key);
        Assert.Equal(4, res.Length);
        var pcs = new[] { Pc(7), Pc(11), Pc(2), Pc(5) }; // G B D F

        AssertEx.SetEquals(pcs, res[0].Pcs); Assert.Equal(Pc(7), res[0].BassPcHint);  // root
        AssertEx.SetEquals(pcs, res[1].Pcs); Assert.Equal(Pc(11), res[1].BassPcHint); // 3rd
        AssertEx.SetEquals(pcs, res[2].Pcs); Assert.Equal(Pc(2), res[2].BassPcHint);  // 5th
        AssertEx.SetEquals(pcs, res[3].Pcs); Assert.Equal(Pc(5), res[3].BassPcHint);  // 7th
    }

    [Fact]
    public void Parses_Mixture_Sevenths_From_Roman_Input()
    {
        var key = new Key(60, true); // C major
        var res = RomanInputParser.Parse("bVI7; bVII7; bII7; N7", key);
        Assert.Equal(4, res.Length);

        // bVI7 => Ab C Eb Gb
        AssertEx.SetEquals(new[] { Pc(8), Pc(0), Pc(3), Pc(6) }, res[0].Pcs);
        Assert.Equal(Pc(8), res[0].BassPcHint);

        // bVII7 => Bb D F Ab
        AssertEx.SetEquals(new[] { Pc(10), Pc(2), Pc(5), Pc(8) }, res[1].Pcs);
        Assert.Equal(Pc(10), res[1].BassPcHint);

        // bII7 => Db F Ab Cb(B)
        AssertEx.SetEquals(new[] { Pc(1), Pc(5), Pc(8), Pc(11) }, res[2].Pcs);
        Assert.Equal(Pc(1), res[2].BassPcHint);

        // N7 == bII7
        AssertEx.SetEquals(new[] { Pc(1), Pc(5), Pc(8), Pc(11) }, res[3].Pcs);
        Assert.Equal(Pc(1), res[3].BassPcHint);
    }

    [Fact]
    public void Parses_Mixture_Seventh_Inversions_From_Roman_Input()
    {
        var key = new Key(60, true); // C major
        var res = RomanInputParser.Parse("bVI7; bVI65; bVI43; bVI42", key);
        Assert.Equal(4, res.Length);
        var pcs = new[] { Pc(8), Pc(0), Pc(3), Pc(6) }; // Ab C Eb Gb

        AssertEx.SetEquals(pcs, res[0].Pcs); Assert.Equal(Pc(8), res[0].BassPcHint);  // root
        AssertEx.SetEquals(pcs, res[1].Pcs); Assert.Equal(Pc(0), res[1].BassPcHint);  // 3rd
        AssertEx.SetEquals(pcs, res[2].Pcs); Assert.Equal(Pc(3), res[2].BassPcHint);  // 5th
        AssertEx.SetEquals(pcs, res[3].Pcs); Assert.Equal(Pc(6), res[3].BassPcHint);  // 7th
    }

    [Fact]
    public void Parses_Secondary_Dominant_And_LeadingTone()
    {
        var key = new Key(60, true); // C major
    var roman = "V/ii; vii°7/V; vii0/V; viio7/V";
    var res = RomanInputParser.Parse(roman, key);
    Assert.Equal(4, res.Length);

        // V/ii = A C# E
    AssertEx.SetEquals(new[] { Pc(9), Pc(1), Pc(4) }, res[0].Pcs, roman);
        Assert.Equal(Pc(9), res[0].BassPcHint);

    // vii°7/V = F# A C Eb
    AssertEx.SetEquals(new[] { Pc(6), Pc(9), Pc(0), Pc(3) }, res[1].Pcs, roman);
        Assert.Equal(Pc(6), res[1].BassPcHint);

    // vii0/V (ASCII zero for half-diminished) = F# A C E
    AssertEx.SetEquals(new[] { Pc(6), Pc(9), Pc(0), Pc(4) }, res[2].Pcs, roman);
    Assert.Equal(Pc(6), res[2].BassPcHint);

    // viio7/V (ASCII 'o' for diminished) = F# A C Eb
    AssertEx.SetEquals(new[] { Pc(6), Pc(9), Pc(0), Pc(3) }, res[3].Pcs, roman);
    Assert.Equal(Pc(6), res[3].BassPcHint);
    }

    [Fact]
    public void Parses_Augmented_Sixth_Tokens()
    {
        var key = new Key(60, true); // C major
        var res = RomanInputParser.Parse("It6; Fr43; Ger65", key);
        Assert.Equal(3, res.Length);

        AssertEx.SetEquals(new[] { Pc(8), Pc(0), Pc(6) }, res[0].Pcs);  // Ab C F#
        Assert.Equal(Pc(8), res[0].BassPcHint);

        AssertEx.SetEquals(new[] { Pc(8), Pc(0), Pc(2), Pc(6) }, res[1].Pcs); // Ab C D F#
        Assert.Equal(Pc(8), res[1].BassPcHint);

        AssertEx.SetEquals(new[] { Pc(8), Pc(0), Pc(3), Pc(6) }, res[2].Pcs); // Ab C Eb F#
        Assert.Equal(Pc(8), res[2].BassPcHint);
    }

    [Fact]
    public void Parses_Unicode_Roman_For_bIII()
    {
        var key = new Key(60, true); // C major
        // b\u2162 = bⅢ （U+2162）
        var res = RomanInputParser.Parse("b\u2162", key);
        Assert.Single(res);
        // 期待: bIII = Eb G Bb
        AssertEx.SetEquals(new[] { Pc(3), Pc(7), Pc(10) }, res[0].Pcs);
        Assert.Equal(Pc(3), res[0].BassPcHint);
    }

    [Fact]
    public void Parses_Secondary_LeadingTone_Diminished_NoFigure()
    {
        var key = new Key(60, true); // C major
        // vii°/V と viio/V（ASCII 'o'）は明示的な 7 が無くても減七を含むとみなす
        var res = RomanInputParser.Parse("vii°/V; viio/V", key);
        Assert.Equal(2, res.Length);

        // 期待: 減七 F# A C Eb
        var pcsDim7 = new[] { Pc(6), Pc(9), Pc(0), Pc(3) };
        AssertEx.SetEquals(pcsDim7, res[0].Pcs);
        Assert.Equal(Pc(6), res[0].BassPcHint);
        AssertEx.SetEquals(pcsDim7, res[1].Pcs);
        Assert.Equal(Pc(6), res[1].BassPcHint);
    }

    [Fact]
    public void Parses_Secondary_LeadingTone_To_Other_Targets()
    {
        var key = new Key(60, true); // C major
        var res = RomanInputParser.Parse("vii°/iii; vii°7/iii; viiø/vi; viiø7/vi", key);
        Assert.Equal(4, res.Length);

        // /iii: target E(4) → LT root = D#(3) → fully diminished 7th: 3,6,9,0
        var pcsDim7_iii = new[] { Pc(3), Pc(6), Pc(9), Pc(0) };
        AssertEx.SetEquals(pcsDim7_iii, res[0].Pcs);
        Assert.Equal(Pc(3), res[0].BassPcHint);
        AssertEx.SetEquals(pcsDim7_iii, res[1].Pcs);
        Assert.Equal(Pc(3), res[1].BassPcHint);

        // /vi: target A(9) → LT root = G#(8) → half-diminished 7th: 8,11,2,6
        var pcsHalfDim7_vi = new[] { Pc(8), Pc(11), Pc(2), Pc(6) };
        AssertEx.SetEquals(pcsHalfDim7_vi, res[2].Pcs);
        Assert.Equal(Pc(8), res[2].BassPcHint);
        AssertEx.SetEquals(pcsHalfDim7_vi, res[3].Pcs);
        Assert.Equal(Pc(8), res[3].BassPcHint);
    }

    [Fact]
    public void Parses_Secondary_LeadingTone_To_ii_and_iv()
    {
        var key = new Key(60, true); // C major
        var res = RomanInputParser.Parse("vii°/ii; vii°7/ii; viiø/iv; viiø7/iv", key);
        Assert.Equal(4, res.Length);

        // /ii: LT root = C#(1)
        var pcsDim7_ii = new[] { Pc(1), Pc(4), Pc(7), Pc(10) }; // C#, E, G, Bb
        AssertEx.SetEquals(pcsDim7_ii, res[0].Pcs);
        Assert.Equal(Pc(1), res[0].BassPcHint);
        AssertEx.SetEquals(pcsDim7_ii, res[1].Pcs);
        Assert.Equal(Pc(1), res[1].BassPcHint);

        // /iv: LT root = E(4)
        var pcsHalfDim7_iv = new[] { Pc(4), Pc(7), Pc(11), Pc(2) }; // E, G, B, D
        AssertEx.SetEquals(pcsHalfDim7_iv, res[2].Pcs);
        Assert.Equal(Pc(4), res[2].BassPcHint);
        AssertEx.SetEquals(pcsHalfDim7_iv, res[3].Pcs);
        Assert.Equal(Pc(4), res[3].BassPcHint);
    }

    [Fact]
    public void Parses_Secondary_LeadingTone_Seventh_Inversion_BassHints()
    {
        var key = new Key(60, true); // C major
        // Target = V (G). Leading tone root = F# (6). Fully diminished seventh: F# A C Eb
        var res = RomanInputParser.Parse("vii°7/V; vii°65/V; vii°43/V; vii°42/V", key);
        Assert.Equal(4, res.Length);

        var pcs = new[] { Pc(6), Pc(9), Pc(0), Pc(3) }; // F#, A, C, Eb
        // 7: root position -> bass = F#(6)
        AssertEx.SetEquals(pcs, res[0].Pcs); Assert.Equal(Pc(6), res[0].BassPcHint);
        // 65: first inversion -> bass = 3rd = A(9)
        AssertEx.SetEquals(pcs, res[1].Pcs); Assert.Equal(Pc(9), res[1].BassPcHint);
        // 43: second inversion -> bass = 5th = C(0) [diminished fifth handled]
        AssertEx.SetEquals(pcs, res[2].Pcs); Assert.Equal(Pc(0), res[2].BassPcHint);
        // 42: third inversion -> bass = 7th = Eb(3)
        AssertEx.SetEquals(pcs, res[3].Pcs); Assert.Equal(Pc(3), res[3].BassPcHint);
    }
}

internal static class AssertEx
{
    public static void SetEquals(int[] expected, int[] actual, string? context = null)
    {
        var e = expected.OrderBy(x => x).ToArray();
        var a = actual.OrderBy(x => x).ToArray();
        if (!e.SequenceEqual(a))
        {
            var msg = $"SetEquals failed.\nExpected: [{string.Join(", ", e)}]\nActual:   [{string.Join(", ", a)}]";
            if (!string.IsNullOrEmpty(context))
            {
                msg += $"\nInput: '{context}'\nCodepoints: {DumpCodepoints(context)}";
            }
            throw new Xunit.Sdk.XunitException(msg);
        }
    }

    private static string DumpCodepoints(string s)
    {
        var parts = s.Select(ch => $"U+{((int)ch):X4}('{(char.IsControl(ch) ? ' ' : ch)}')");
        return string.Join(" ", parts);
    }
}
