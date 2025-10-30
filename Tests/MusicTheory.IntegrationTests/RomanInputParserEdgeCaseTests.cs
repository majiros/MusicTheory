using FluentAssertions;
using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.IntegrationTests;

/// <summary>
/// Integration tests for RomanInputParser error handling and edge cases.
/// Targets uncovered areas: null/empty input validation, unsupported tokens, Unicode normalization, secondary variations.
/// </summary>
public class RomanInputParserEdgeCaseTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    #region Error Handling Tests

    [Fact]
    public void Parse_NullInput_ThrowsArgumentNullException()
    {
        // Arrange
        var key = new Key(60, true); // C major
        string? nullRoman = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => RomanInputParser.Parse(nullRoman!, key));
        exception.ParamName.Should().Be("roman", "null input should be rejected with ArgumentNullException");
    }

    [Fact]
    public void Parse_EmptyString_ThrowsArgumentException()
    {
        // Arrange
        var key = new Key(60, true); // C major
        string emptyRoman = "";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => RomanInputParser.Parse(emptyRoman, key));
        exception.Message.Should().Contain("no items", "empty string should be rejected");
    }

    [Fact]
    public void Parse_WhitespaceOnly_ThrowsArgumentException()
    {
        // Arrange
        var key = new Key(60, true); // C major
        string whitespaceRoman = "   ; \t ; \n ";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => RomanInputParser.Parse(whitespaceRoman, key));
        // After splitting by ';', empty/whitespace tokens are removed => no items left
        exception.Message.Should().Contain("no items", "whitespace-only tokens should be rejected");
    }

    [Fact]
    public void Parse_InvalidSecondary_MissingTarget_ThrowsArgumentException()
    {
        // Arrange: secondary notation with slash but no target (e.g., "V/")
        var key = new Key(60, true); // C major
        string invalidSecondary = "V/"; // trailing slash, no target

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => RomanInputParser.Parse(invalidSecondary, key));
        exception.Message.Should().Contain("Invalid secondary token", "secondary with missing target should be rejected");
    }

    [Fact]
    public void Parse_UnsupportedToken_InvalidRomanNumeral_ThrowsArgumentException()
    {
        // Arrange: unsupported token (e.g., "IX" which is beyond VII in diatonic)
        var key = new Key(60, true); // C major
        string unsupportedToken = "IX"; // Roman 9, beyond diatonic scale

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => RomanInputParser.Parse(unsupportedToken, key));
        exception.Message.Should().Contain("Unsupported roman token", "invalid numeral should be rejected");
    }

    #endregion

    #region Unicode and Sanitize Boundary Tests

    [Fact]
    public void Parse_MixedUnicodeAndAscii_bIII_CorrectlyNormalized()
    {
        // Arrange: mixture of Unicode flat (♭), ASCII 'b', and Unicode Roman numeral Ⅲ (U+2162)
        var key = new Key(60, true); // C major
        // Test case: "♭Ⅲ" (Unicode flat + Unicode III) should parse as bIII (Eb-G-Bb)
        string unicodeMixture = "♭Ⅲ"; // U+266D + U+2162

        // Act
        var result = RomanInputParser.Parse(unicodeMixture, key);

        // Assert
        result.Should().HaveCount(1);
        result[0].Pcs.Should().BeEquivalentTo(new[] { Pc(3), Pc(7), Pc(10) }, "♭Ⅲ should parse as bIII (Eb major triad)");
        result[0].BassPcHint.Should().Be(Pc(3), "bass should be on root (Eb)");
    }

    [Fact]
    public void Parse_ComplexUnicodeNormalization_MultipleFormats()
    {
        // Arrange: complex mix of formats in a single sequence
        // "♭Ⅱ6; ⅶø7/Ⅴ; Ⅳ64" => bII6, viiø7/V, IV64
        var key = new Key(60, true); // C major
        string complexUnicode = "♭Ⅱ6; ⅶø7/Ⅴ; Ⅳ64";

        // Act
        var result = RomanInputParser.Parse(complexUnicode, key);

        // Assert
        result.Should().HaveCount(3);
        
        // ♭Ⅱ6 => bII6 (Db-F-Ab, bass=F)
        result[0].Pcs.Should().BeEquivalentTo(new[] { Pc(1), Pc(5), Pc(8) }, "♭Ⅱ6 should parse as bII6");
        result[0].BassPcHint.Should().Be(Pc(5), "bII6 bass should be on F");

        // ⅶø7/Ⅴ => viiø7/V (F#-A-C-E)
        result[1].Pcs.Should().BeEquivalentTo(new[] { Pc(6), Pc(9), Pc(0), Pc(4) }, "ⅶø7/Ⅴ should parse as viiø7/V");
        result[1].BassPcHint.Should().Be(Pc(6), "viiø7/V bass should be on F#");

        // Ⅳ64 => IV64 (F-A-C, bass=C)
        result[2].Pcs.Should().BeEquivalentTo(new[] { Pc(5), Pc(9), Pc(0) }, "Ⅳ64 should parse as IV64");
        result[2].BassPcHint.Should().Be(Pc(0), "IV64 bass should be on C");
    }

    [Fact]
    public void Parse_bII_vs_bIII_Disambiguation()
    {
        // Arrange: test disambiguation when sanitized form could be ambiguous
        // Real case: "bⅢ" (b + Unicode III) should disambiguate to bIII, not bII
        var key = new Key(60, true); // C major
        string bIIIToken = "bⅢ"; // ASCII 'b' + Unicode Ⅲ (U+2162)

        // Act
        var result = RomanInputParser.Parse(bIIIToken, key);

        // Assert
        result.Should().HaveCount(1);
        result[0].Pcs.Should().BeEquivalentTo(new[] { Pc(3), Pc(7), Pc(10) }, 
            "bⅢ should correctly parse as bIII (Eb major), not bII (Db major)");
        result[0].BassPcHint.Should().Be(Pc(3), "bIII bass should be on Eb");
    }

    #endregion

    #region Secondary Variations and Boundary Tests

    [Fact]
    public void Parse_SecondaryToIV_viiDim7_CorrectQuality()
    {
        // Arrange: vii°7/IV => leading-tone diminished seventh to IV (should use MinorSeventh for /IV special case)
        var key = new Key(60, true); // C major
        string secondaryToIV = "vii°7/IV";

        // Act
        var result = RomanInputParser.Parse(secondaryToIV, key);

        // Assert
        result.Should().HaveCount(1);
        // Leading tone to F (IV) is E, /IV special case forces MinorSeventh: E-G-B-D (not E-G-Bb-Db)
        result[0].Pcs.Should().BeEquivalentTo(new[] { Pc(4), Pc(7), Pc(11), Pc(2) }, 
            "vii°7/IV should use MinorSeventh quality (E-G-B-D)");
        result[0].BassPcHint.Should().Be(Pc(4), "bass should be on E (root)");
    }

    [Fact]
    public void Parse_SecondaryInversions_viiDim65_ToV()
    {
        // Arrange: vii°65/V => first inversion of leading-tone diminished seventh to V
        var key = new Key(60, true); // C major
        string secondaryInversion = "vii°65/V";

        // Act
        var result = RomanInputParser.Parse(secondaryInversion, key);

        // Assert
        result.Should().HaveCount(1);
        // vii°7/V: F#-A-C-Eb (diminished seventh to G), 65 => third in bass (A)
        result[0].Pcs.Should().BeEquivalentTo(new[] { Pc(6), Pc(9), Pc(0), Pc(3) }, 
            "vii°65/V should parse as F#-A-C-Eb (diminished seventh)");
        result[0].BassPcHint.Should().Be(Pc(9), "vii°65 bass should be on A (third)");
    }

    [Fact]
    public void Parse_SecondaryTriad_V6_ToII()
    {
        // Arrange: V6/ii => first inversion of dominant triad to ii (D in C major)
        var key = new Key(60, true); // C major
        string secondaryTriad = "V6/ii";

        // Act
        var result = RomanInputParser.Parse(secondaryTriad, key);

        // Assert
        result.Should().HaveCount(1);
        // V of D (ii) is A major: A-C#-E
        result[0].Pcs.Should().BeEquivalentTo(new[] { Pc(9), Pc(1), Pc(4) }, 
            "V6/ii should parse as A major triad (A-C#-E)");
        // V6 figure processing: bass should be A (root=9), not third
        // (figure "6" only applied to non-secondary triads; secondary uses root by default)
        result[0].BassPcHint.Should().Be(Pc(9), "V6/ii bass should be on A (root)");
    }

    [Fact]
    public void Parse_SecondaryToiii_MajorKey()
    {
        // Arrange: V/iii in C major => B major (V of E minor: iii)
        var key = new Key(60, true); // C major
        string secondaryToiii = "V/iii";

        // Act
        var result = RomanInputParser.Parse(secondaryToiii, key);

        // Assert
        result.Should().HaveCount(1);
        // V of E (iii) is B major: B-D#-F#
        result[0].Pcs.Should().BeEquivalentTo(new[] { Pc(11), Pc(3), Pc(6) }, 
            "V/iii should parse as B major triad");
        result[0].BassPcHint.Should().Be(Pc(11), "V/iii bass should be on B (root)");
    }

    #endregion

    #region Figure and Quality Mark Combinations

    [Fact]
    public void Parse_Neapolitan_N64_SecondInversion()
    {
        // Arrange: N64 => Neapolitan in second inversion (fifth in bass)
        var key = new Key(60, true); // C major
        string neapolitanSecondInv = "N64";

        // Act
        var result = RomanInputParser.Parse(neapolitanSecondInv, key);

        // Assert
        result.Should().HaveCount(1);
        // N = bII = Db-F-Ab, 64 => fifth in bass (Ab)
        result[0].Pcs.Should().BeEquivalentTo(new[] { Pc(1), Pc(5), Pc(8) }, 
            "N64 should parse as Neapolitan triad (Db-F-Ab)");
        result[0].BassPcHint.Should().Be(Pc(8), "N64 bass should be on Ab (fifth)");
    }

    [Fact]
    public void Parse_Neapolitan_N7_DominantSeventh()
    {
        // Arrange: N7 => Neapolitan with dominant seventh (bII7: Db-F-Ab-Cb)
        var key = new Key(60, true); // C major
        string neapolitanSeventh = "N7";

        // Act
        var result = RomanInputParser.Parse(neapolitanSeventh, key);

        // Assert
        result.Should().HaveCount(1);
        // N7 = bII7 = Db-F-Ab-Cb (enharmonic Db-F-Ab-B)
        result[0].Pcs.Should().BeEquivalentTo(new[] { Pc(1), Pc(5), Pc(8), Pc(11) }, 
            "N7 should parse as Neapolitan seventh (dominant quality)");
        result[0].BassPcHint.Should().Be(Pc(1), "N7 bass should be on Db (root)");
    }

    [Fact]
    public void Parse_MixtureTriad_bVII6_FirstInversion()
    {
        // Arrange: bVII6 => borrowed bVII major triad, first inversion (third in bass)
        var key = new Key(60, true); // C major
        string mixtureSixth = "bVII6";

        // Act
        var result = RomanInputParser.Parse(mixtureSixth, key);

        // Assert
        result.Should().HaveCount(1);
        // bVII = Bb-D-F, 6 => third in bass (D)
        result[0].Pcs.Should().BeEquivalentTo(new[] { Pc(10), Pc(2), Pc(5) }, 
            "bVII6 should parse as Bb major triad");
        result[0].BassPcHint.Should().Be(Pc(2), "bVII6 bass should be on D (third)");
    }

    [Fact]
    public void Parse_AugmentedSixth_It6_Italian()
    {
        // Arrange: It6 => Italian augmented sixth (b6-1-#4)
        var key = new Key(60, true); // C major
        string italian = "It6";

        // Act
        var result = RomanInputParser.Parse(italian, key);

        // Assert
        result.Should().HaveCount(1);
        // It6 in C: Ab-C-F# (b6-1-#4)
        result[0].Pcs.Should().BeEquivalentTo(new[] { Pc(8), Pc(0), Pc(6) }, 
            "It6 should parse as Italian augmented sixth (Ab-C-F#)");
        result[0].BassPcHint.Should().Be(Pc(8), "It6 bass should be on Ab (b6)");
    }

    [Fact]
    public void Parse_AugmentedSixth_Fr43_French()
    {
        // Arrange: Fr43 => French augmented sixth (b6-1-2-#4)
        var key = new Key(60, true); // C major
        string french = "Fr43";

        // Act
        var result = RomanInputParser.Parse(french, key);

        // Assert
        result.Should().HaveCount(1);
        // Fr43 in C: Ab-C-D-F# (b6-1-2-#4)
        result[0].Pcs.Should().BeEquivalentTo(new[] { Pc(8), Pc(0), Pc(2), Pc(6) }, 
            "Fr43 should parse as French augmented sixth (Ab-C-D-F#)");
        result[0].BassPcHint.Should().Be(Pc(8), "Fr43 bass should be on Ab (b6)");
    }

    [Fact]
    public void Parse_AugmentedSixth_Ger65_German()
    {
        // Arrange: Ger65 => German augmented sixth (b6-1-b3-#4)
        var key = new Key(60, true); // C major
        string german = "Ger65";

        // Act
        var result = RomanInputParser.Parse(german, key);

        // Assert
        result.Should().HaveCount(1);
        // Ger65 in C: Ab-C-Eb-F# (b6-1-b3-#4)
        result[0].Pcs.Should().BeEquivalentTo(new[] { Pc(8), Pc(0), Pc(3), Pc(6) }, 
            "Ger65 should parse as German augmented sixth (Ab-C-Eb-F#)");
        result[0].BassPcHint.Should().Be(Pc(8), "Ger65 bass should be on Ab (b6)");
    }

    #endregion
}
