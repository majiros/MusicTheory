using FluentAssertions;
using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.IntegrationTests;

/// <summary>
/// Advanced integration tests for ProgressionAnalyzer covering complex musical scenarios.
/// Focus: Jazz progressions, modal interchange, deceptive cadences, and edge cases.
/// Target: Improve ProgressionAnalyzer coverage from 70.7% to 75%+
/// </summary>
public class ProgressionAdvancedTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    #region Jazz Progressions

    [Fact]
    public void AnalyzeJazzProgression_iii_vi_ii_V_I_ReturnCorrectLabels()
    {
        // Arrange: Extended jazz progression iii-vi-ii-V-I in C major
        var key = new Key(60, true); // C major
        var pcsList = new[]
        {
            new[] { Pc(64), Pc(67), Pc(71) },  // iii (E-G-B)
            new[] { Pc(69), Pc(72), Pc(64) },  // vi (A-C-E)
            new[] { Pc(62), Pc(65), Pc(69) },  // ii (D-F-A)
            new[] { Pc(67), Pc(71), Pc(62) },  // V (G-B-D)
            new[] { Pc(60), Pc(64), Pc(67) }   // I (C-E-G)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().HaveCount(5);
        result.Chords[0].RomanText.Should().Be("iii");
        result.Chords[1].RomanText.Should().Be("vi");
        result.Chords[2].RomanText.Should().Be("ii");
        result.Chords[3].RomanText.Should().Be("V");
        result.Chords[4].RomanText.Should().Be("I");
        
        // Should detect authentic cadence at V→I
        result.Cadences.Should().Contain(c => c.cadence == CadenceType.Authentic && c.indexFrom == 3);
    }

    [Fact]
    public void AnalyzeJazzProgression_ii_V_iii_vi_ii_V_I_ReturnsCorrectLabels()
    {
        // Arrange: Jazz circle progression with double ii-V
        var key = new Key(60, true); // C major
        var pcsList = new[]
        {
            new[] { Pc(62), Pc(65), Pc(69) },  // ii (D-F-A)
            new[] { Pc(67), Pc(71), Pc(62) },  // V (G-B-D)
            new[] { Pc(64), Pc(67), Pc(71) },  // iii (E-G-B)
            new[] { Pc(69), Pc(72), Pc(64) },  // vi (A-C-E)
            new[] { Pc(62), Pc(65), Pc(69) },  // ii (D-F-A)
            new[] { Pc(67), Pc(71), Pc(62) },  // V (G-B-D)
            new[] { Pc(60), Pc(64), Pc(67) }   // I (C-E-G)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().HaveCount(7);
        result.Chords[4].RomanText.Should().Be("ii");
        result.Chords[5].RomanText.Should().Be("V");
        result.Chords[6].RomanText.Should().Be("I");
    }

    [Fact]
    public void AnalyzeJazzProgression_WithSeventhChords_ii7_V7_Imaj7_ReturnsCorrectLabels()
    {
        // Arrange: Jazz ii7-V7-Imaj7 with seventh chords
        var key = new Key(60, true); // C major
        var pcsList = new[]
        {
            new[] { Pc(62), Pc(65), Pc(69), Pc(72) },  // ii7 (D-F-A-C)
            new[] { Pc(67), Pc(71), Pc(62), Pc(65) },  // V7 (G-B-D-F)
            new[] { Pc(60), Pc(64), Pc(67), Pc(71) }   // Imaj7 (C-E-G-B)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().HaveCount(3);
        result.Chords[0].RomanText.Should().Contain("ii");
        result.Chords[1].RomanText.Should().Contain("V");
        result.Chords[2].RomanText.Should().Contain("I");
    }

    #endregion

    #region Modal Interchange

    [Fact]
    public void AnalyzeModalInterchange_I_bIII_bVII_IV_ReturnsCorrectLabels()
    {
        // Arrange: Modal interchange I-bIII-bVII-IV in C major
        var key = new Key(60, true); // C major
        var pcsList = new[]
        {
            new[] { Pc(60), Pc(64), Pc(67) },  // I (C-E-G)
            new[] { Pc(63), Pc(67), Pc(70) },  // bIII (Eb-G-Bb)
            new[] { Pc(70), Pc(62), Pc(65) },  // bVII (Bb-D-F)
            new[] { Pc(65), Pc(69), Pc(72) }   // IV (F-A-C)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().HaveCount(4);
        result.Chords[0].RomanText.Should().Be("I");
        result.Chords[1].RomanText.Should().Contain("bIII");
        result.Chords[2].RomanText.Should().Contain("bVII");
        result.Chords[3].RomanText.Should().Be("IV");
    }

    [Fact]
    public void AnalyzeModalInterchange_I_iv_bVII_bIII_iv_V_I_ReturnsCorrectLabels()
    {
        // Arrange: Complex modal mixture progression
        var key = new Key(60, true); // C major
        var pcsList = new[]
        {
            new[] { Pc(60), Pc(64), Pc(67) },  // I (C-E-G)
            new[] { Pc(65), Pc(68), Pc(72) },  // iv (F-Ab-C)
            new[] { Pc(70), Pc(62), Pc(65) },  // bVII (Bb-D-F)
            new[] { Pc(63), Pc(67), Pc(70) },  // bIII (Eb-G-Bb)
            new[] { Pc(65), Pc(68), Pc(72) },  // iv (F-Ab-C)
            new[] { Pc(67), Pc(71), Pc(62) },  // V (G-B-D)
            new[] { Pc(60), Pc(64), Pc(67) }   // I (C-E-G)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().HaveCount(7);
        result.Chords[5].RomanText.Should().Be("V");
        result.Chords[6].RomanText.Should().Be("I");
        result.Cadences.Should().Contain(c => c.cadence == CadenceType.Authentic);
    }

    #endregion

    #region Deceptive and Plagal Cadences

    [Fact]
    public void AnalyzeDeceptiveCadence_V_vi_DetectsDeceptive()
    {
        // Arrange: Deceptive cadence V-vi in C major
        var key = new Key(60, true); // C major
        var pcsList = new[]
        {
            new[] { Pc(60), Pc(64), Pc(67) },  // I (C-E-G)
            new[] { Pc(67), Pc(71), Pc(62) },  // V (G-B-D)
            new[] { Pc(69), Pc(72), Pc(64) }   // vi (A-C-E)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().HaveCount(3);
        result.Chords[1].RomanText.Should().Be("V");
        result.Chords[2].RomanText.Should().Be("vi");
        
        // Should detect deceptive cadence V→vi
        result.Cadences.Should().Contain(c => c.cadence == CadenceType.Deceptive && c.indexFrom == 1);
    }

    [Fact]
    public void AnalyzePlagalCadence_IV_I_DetectsPlagal()
    {
        // Arrange: Plagal cadence IV-I (Amen cadence) in C major
        var key = new Key(60, true); // C major
        var pcsList = new[]
        {
            new[] { Pc(67), Pc(71), Pc(62) },  // V (G-B-D)
            new[] { Pc(65), Pc(69), Pc(72) },  // IV (F-A-C)
            new[] { Pc(60), Pc(64), Pc(67) }   // I (C-E-G)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().HaveCount(3);
        result.Chords[1].RomanText.Should().Be("IV");
        result.Chords[2].RomanText.Should().Be("I");
        
        // Should detect plagal cadence IV→I
        result.Cadences.Should().Contain(c => c.cadence == CadenceType.Plagal && c.indexFrom == 1);
    }

    [Fact]
    public void AnalyzeMultipleCadences_I_V_vi_IV_V_I_DetectsDeceptiveAndAuthentic()
    {
        // Arrange: Progression with both deceptive and authentic cadences
        var key = new Key(60, true); // C major
        var pcsList = new[]
        {
            new[] { Pc(60), Pc(64), Pc(67) },  // I (C-E-G)
            new[] { Pc(67), Pc(71), Pc(62) },  // V (G-B-D)
            new[] { Pc(69), Pc(72), Pc(64) },  // vi (A-C-E) - deceptive
            new[] { Pc(65), Pc(69), Pc(72) },  // IV (F-A-C)
            new[] { Pc(67), Pc(71), Pc(62) },  // V (G-B-D)
            new[] { Pc(60), Pc(64), Pc(67) }   // I (C-E-G) - authentic
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().HaveCount(6);
        
        // Should detect deceptive cadence at index 1 (V→vi)
        result.Cadences.Should().Contain(c => c.cadence == CadenceType.Deceptive && c.indexFrom == 1);
        
        // Should detect authentic cadence at index 4 (V→I)
        result.Cadences.Should().Contain(c => c.cadence == CadenceType.Authentic && c.indexFrom == 4);
    }

    #endregion

    #region Minor Key Progressions

    [Fact]
    public void AnalyzeMinorProgression_i_iv_V_i_ReturnsCorrectLabels()
    {
        // Arrange: Classic minor progression i-iv-V-i in A minor
        var key = new Key(57, false); // A minor
        var pcsList = new[]
        {
            new[] { Pc(57), Pc(60), Pc(64) },  // i (A-C-E)
            new[] { Pc(62), Pc(65), Pc(69) },  // iv (D-F-A)
            new[] { Pc(64), Pc(68), Pc(71) },  // V (E-G#-B) - harmonic minor
            new[] { Pc(57), Pc(60), Pc(64) }   // i (A-C-E)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().HaveCount(4);
        result.Chords[0].RomanText.Should().Be("i");
        result.Chords[1].RomanText.Should().Be("iv");
        result.Chords[2].RomanText.Should().Be("V");
        result.Chords[3].RomanText.Should().Be("i");
        
        // Should detect authentic cadence in minor
        result.Cadences.Should().Contain(c => c.cadence == CadenceType.Authentic);
    }

    [Fact]
    public void AnalyzeMinorProgression_i_iio_V_i_WithDiminished_ReturnsCorrectLabels()
    {
        // Arrange: Minor progression with ii° (supertonic diminished) in A minor
        var key = new Key(57, false); // A minor
        var pcsList = new[]
        {
            new[] { Pc(57), Pc(60), Pc(64) },  // i (A-C-E)
            new[] { Pc(59), Pc(62), Pc(65) },  // ii° (B-D-F)
            new[] { Pc(64), Pc(68), Pc(71) },  // V (E-G#-B)
            new[] { Pc(57), Pc(60), Pc(64) }   // i (A-C-E)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().HaveCount(4);
        result.Chords[0].RomanText.Should().Be("i");
        result.Chords[1].RomanText.Should().Contain("ii");
        result.Chords[2].RomanText.Should().Be("V");
        result.Chords[3].RomanText.Should().Be("i");
    }

    [Fact]
    public void AnalyzeMinorProgression_i_VI_III_VII_i_WithRaisedLeadingTone_ReturnsCorrectLabels()
    {
        // Arrange: Minor progression i-VI-III-VII-i in A minor with harmonic minor VII
        var key = new Key(57, false); // A minor
        var pcsList = new[]
        {
            new[] { Pc(57), Pc(60), Pc(64) },  // i (A-C-E)
            new[] { Pc(65), Pc(69), Pc(72) },  // VI (F-A-C)
            new[] { Pc(60), Pc(64), Pc(67) },  // III (C-E-G)
            new[] { Pc(68), Pc(71), Pc(62) },  // VII (G#-B-D) - raised leading tone
            new[] { Pc(57), Pc(60), Pc(64) }   // i (A-C-E)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().HaveCount(5);
        result.Chords[0].RomanText.Should().Be("i");
        result.Chords[1].RomanText.Should().Contain("VI");
        result.Chords[2].RomanText.Should().Contain("III");
        result.Chords[4].RomanText.Should().Be("i");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void AnalyzeEmptyProgression_ReturnsEmptyResult()
    {
        // Arrange: Empty progression
        var key = new Key(60, true);
        var pcsList = Array.Empty<int[]>();

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().BeEmpty();
        result.Cadences.Should().BeEmpty();
    }

    [Fact]
    public void AnalyzeSingleChord_ReturnsOneChordNoCadence()
    {
        // Arrange: Single chord (no progression)
        var key = new Key(60, true);
        var pcsList = new[]
        {
            new[] { Pc(60), Pc(64), Pc(67) }  // I (C-E-G)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().ContainSingle();
        result.Chords[0].RomanText.Should().Be("I");
        result.Cadences.Should().BeEmpty(); // No cadence with single chord
    }

    [Fact]
    public void AnalyzeUnanalyzableChord_HandlesGracefully()
    {
        // Arrange: Progression with unanalyzable chord (chromatic cluster)
        var key = new Key(60, true);
        var pcsList = new[]
        {
            new[] { Pc(60), Pc(64), Pc(67) },      // I (C-E-G)
            new[] { Pc(61), Pc(62), Pc(63), Pc(64) }, // Chromatic cluster
            new[] { Pc(67), Pc(71), Pc(62) }       // V (G-B-D)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().HaveCount(3);
        result.Chords[0].RomanText.Should().Be("I");
        // Chords[1] may be null or unanalyzed
        result.Chords[2].RomanText.Should().Be("V");
    }

    [Fact]
    public void AnalyzeWithHarmonyOptions_PreferV7Paren9_UsesCorrectNotation()
    {
        // Arrange: V9 chord with PreferV7Paren9OverV9 option
        var key = new Key(60, true); // C major
        var options = new HarmonyOptions { PreferV7Paren9OverV9 = true };
        var pcsList = new[]
        {
            new[] { Pc(67), Pc(71), Pc(62), Pc(65), Pc(69) }  // V9 (G-B-D-F-A)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key, options);

        // Assert
        result.Chords.Should().ContainSingle();
        result.Chords[0].RomanText.Should().Contain("V");
        // Should use V7(9) notation instead of V9
        result.Chords[0].RomanText.Should().Contain("(9)");
    }

    [Fact]
    public void AnalyzeWithVoicings_UtilizesFourPartVoicingData()
    {
        // Arrange: Progression with explicit voicing data
        var key = new Key(60, true); // C major
        var pcsList = new[]
        {
            new[] { Pc(60), Pc(64), Pc(67) },  // I (C-E-G)
            new[] { Pc(67), Pc(71), Pc(62) }   // V (G-B-D)
        };
        
        var voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(67, 64, 60, 48),  // I: G-E-C-C (soprano-alto-tenor-bass)
            new FourPartVoicing(62, 71, 67, 55)   // V: D-B-G-G (soprano-alto-tenor-bass)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key, voicings);

        // Assert
        result.Chords.Should().HaveCount(2);
        result.Chords[0].RomanText.Should().Be("I");
        result.Chords[1].RomanText.Should().Be("V");
        // Voicing data may affect analysis (e.g., inversion detection)
    }

    #endregion
}
