using FluentAssertions;
using MusicTheory.Theory.Harmony;
using MusicTheory.Theory.Scale;
using Xunit;

namespace MusicTheory.IntegrationTests;

/// <summary>
/// Integration tests for end-to-end harmony analysis workflows.
/// Tests validate complete progression analysis from pitch class input to labeled output.
/// </summary>
public class ProgressionScenarioTests
{
    #region Basic Diatonic Progressions

    [Fact]
    public void AnalyzeDiatonicProgression_I_IV_V_I_ReturnsCorrectLabels()
    {
        // Arrange: Classic I-IV-V-I progression in C major
        var key = Key.CMajor;
        var pcsList = new[]
        {
            new[] { 0, 4, 7 },  // I (C-E-G)
            new[] { 5, 9, 0 },  // IV (F-A-C)
            new[] { 7, 11, 2 }, // V (G-B-D)
            new[] { 0, 4, 7 }   // I (C-E-G)
        };

        // Act
        var result = ProgressionAnalyzer.Analyze(pcsList, key);

        // Assert
        result.Chords.Should().HaveCount(4);
        result.Chords[0].RomanText.Should().Be("I");
        result.Chords[1].RomanText.Should().Be("IV");
        result.Chords[2].RomanText.Should().Be("V");
        result.Chords[3].RomanText.Should().Be("I");
    }

    [Fact]
    public void AnalyzeDiatonicProgression_ii_V_I_ReturnsCorrectLabels()
    {
        // Arrange: Jazz cadence ii-V-I in C major
        var key = Key.CMajor;
        var analyzer = new HarmonyAnalyzer();
        var chords = new[]
        {
            new PitchClassSet(new[] { 2, 5, 9 }),  // ii (D-F-A)
            new PitchClassSet(new[] { 7, 11, 2 }), // V (G-B-D)
            new PitchClassSet(new[] { 0, 4, 7 })   // I (C-E-G)
        };

        // Act
        var results = analyzer.Analyze(chords, key);

        // Assert
        results.Should().HaveCount(3);
        results[0].PrimaryLabel.Should().Be("ii");
        results[1].PrimaryLabel.Should().Be("V");
        results[2].PrimaryLabel.Should().Be("I");
    }

    [Fact]
    public void AnalyzeDiatonicProgression_I_vi_IV_V_ReturnsCorrectLabels()
    {
        // Arrange: Pop progression I-vi-IV-V in C major
        var key = Key.CMajor;
        var analyzer = new HarmonyAnalyzer();
        var chords = new[]
        {
            new PitchClassSet(new[] { 0, 4, 7 }),  // I (C-E-G)
            new PitchClassSet(new[] { 9, 0, 4 }),  // vi (A-C-E)
            new PitchClassSet(new[] { 5, 9, 0 }),  // IV (F-A-C)
            new PitchClassSet(new[] { 7, 11, 2 })  // V (G-B-D)
        };

        // Act
        var results = analyzer.Analyze(chords, key);

        // Assert
        results.Should().HaveCount(4);
        results[0].PrimaryLabel.Should().Be("I");
        results[1].PrimaryLabel.Should().Be("vi");
        results[2].PrimaryLabel.Should().Be("IV");
        results[3].PrimaryLabel.Should().Be("V");
    }

    #endregion

    #region Secondary Dominant Chains

    [Fact]
    public void AnalyzeSecondaryDominantChain_VofV_V_I_ReturnsCorrectLabels()
    {
        // Arrange: V7/V-V-I tonicization in C major
        var key = Key.CMajor;
        var analyzer = new HarmonyAnalyzer();
        var chords = new[]
        {
            new PitchClassSet(new[] { 2, 6, 9, 0 }), // V7/V (D-F#-A-C)
            new PitchClassSet(new[] { 7, 11, 2 }),   // V (G-B-D)
            new PitchClassSet(new[] { 0, 4, 7 })     // I (C-E-G)
        };

        // Act
        var results = analyzer.Analyze(chords, key);

        // Assert
        results.Should().HaveCount(3);
        results[0].PrimaryLabel.Should().Contain("V7/V");
        results[1].PrimaryLabel.Should().Be("V");
        results[2].PrimaryLabel.Should().Be("I");
    }

    [Fact]
    public void AnalyzeSecondaryLeadingTone_viio7ofV_V_I_ReturnsCorrectLabels()
    {
        // Arrange: vii°7/V-V-I leading-tone approach in C major
        var key = Key.CMajor;
        var analyzer = new HarmonyAnalyzer();
        var chords = new[]
        {
            new PitchClassSet(new[] { 6, 9, 0, 3 }), // vii°7/V (F#-A-C-Eb)
            new PitchClassSet(new[] { 7, 11, 2 }),   // V (G-B-D)
            new PitchClassSet(new[] { 0, 4, 7 })     // I (C-E-G)
        };

        // Act
        var results = analyzer.Analyze(chords, key);

        // Assert
        results.Should().HaveCount(3);
        results[0].PrimaryLabel.Should().Contain("vii");
        results[0].PrimaryLabel.Should().Contain("/V");
        results[1].PrimaryLabel.Should().Be("V");
        results[2].PrimaryLabel.Should().Be("I");
    }

    [Fact]
    public void AnalyzeExtendedSecondary_VofII_ii_V_I_ReturnsCorrectLabels()
    {
        // Arrange: V/ii-ii-V-I extended secondary in C major
        var key = Key.CMajor;
        var analyzer = new HarmonyAnalyzer();
        var chords = new[]
        {
            new PitchClassSet(new[] { 9, 1, 4 }),  // V/ii (A-C#-E)
            new PitchClassSet(new[] { 2, 5, 9 }),  // ii (D-F-A)
            new PitchClassSet(new[] { 7, 11, 2 }), // V (G-B-D)
            new PitchClassSet(new[] { 0, 4, 7 })   // I (C-E-G)
        };

        // Act
        var results = analyzer.Analyze(chords, key);

        // Assert
        results.Should().HaveCount(4);
        results[0].PrimaryLabel.Should().Contain("V/ii");
        results[1].PrimaryLabel.Should().Be("ii");
        results[2].PrimaryLabel.Should().Be("V");
        results[3].PrimaryLabel.Should().Be("I");
    }

    #endregion

    #region Borrowed Chord Progressions

    [Fact]
    public void AnalyzeBorrowedChordProgression_I_bVI_bVII_I_ReturnsCorrectLabels()
    {
        // Arrange: Modal mixture I-bVI-bVII-I in C major
        var key = Key.CMajor;
        var analyzer = new HarmonyAnalyzer();
        var chords = new[]
        {
            new PitchClassSet(new[] { 0, 4, 7 }),  // I (C-E-G)
            new PitchClassSet(new[] { 8, 0, 3 }),  // bVI (Ab-C-Eb)
            new PitchClassSet(new[] { 10, 2, 5 }), // bVII (Bb-D-F)
            new PitchClassSet(new[] { 0, 4, 7 })   // I (C-E-G)
        };

        // Act
        var results = analyzer.Analyze(chords, key);

        // Assert
        results.Should().HaveCount(4);
        results[0].PrimaryLabel.Should().Be("I");
        results[1].PrimaryLabel.Should().Contain("bVI");
        results[2].PrimaryLabel.Should().Contain("bVII");
        results[3].PrimaryLabel.Should().Be("I");
    }

    [Fact]
    public void AnalyzeBorrowedSubdominant_iv_V_I_ReturnsCorrectLabels()
    {
        // Arrange: Minor subdominant iv-V-I in C major
        var key = Key.CMajor;
        var analyzer = new HarmonyAnalyzer();
        var chords = new[]
        {
            new PitchClassSet(new[] { 5, 8, 0 }),  // iv (F-Ab-C)
            new PitchClassSet(new[] { 7, 11, 2 }), // V (G-B-D)
            new PitchClassSet(new[] { 0, 4, 7 })   // I (C-E-G)
        };

        // Act
        var results = analyzer.Analyze(chords, key);

        // Assert
        results.Should().HaveCount(3);
        
        // iv in major is borrowed from parallel minor
        // HarmonyAnalyzer may label it as iv or IVm depending on implementation
        var firstLabel = results[0].PrimaryLabel;
        (firstLabel.Should().Be("iv").Or.Be("IVm"));
        
        results[1].PrimaryLabel.Should().Be("V");
        results[2].PrimaryLabel.Should().Be("I");
    }

    [Fact]
    public void AnalyzeNeapolitanResolution_bII_V_I_ReturnsCorrectLabels()
    {
        // Arrange: Neapolitan bII-V-I in C major
        var key = Key.CMajor;
        var analyzer = new HarmonyAnalyzer();
        var chords = new[]
        {
            new PitchClassSet(new[] { 1, 5, 8 }),  // bII (Db-F-Ab)
            new PitchClassSet(new[] { 7, 11, 2 }), // V (G-B-D)
            new PitchClassSet(new[] { 0, 4, 7 })   // I (C-E-G)
        };

        // Act
        var results = analyzer.Analyze(chords, key);

        // Assert
        results.Should().HaveCount(3);
        results[0].PrimaryLabel.Should().Contain("bII");
        results[1].PrimaryLabel.Should().Be("V");
        results[2].PrimaryLabel.Should().Be("I");
    }

    #endregion

    #region Augmented Sixth Resolutions

    [Fact]
    public void AnalyzeItalianAugmentedSixth_It6_V_I_ReturnsCorrectLabels()
    {
        // Arrange: Italian sixth It6-V-I in C major
        var key = Key.CMajor;
        var analyzer = new HarmonyAnalyzer();
        var chords = new[]
        {
            new PitchClassSet(new[] { 8, 0, 6 }),  // It6 (Ab-C-F#)
            new PitchClassSet(new[] { 7, 11, 2 }), // V (G-B-D)
            new PitchClassSet(new[] { 0, 4, 7 })   // I (C-E-G)
        };

        // Act
        var results = analyzer.Analyze(chords, key);

        // Assert
        results.Should().HaveCount(3);
        results[0].PrimaryLabel.Should().Contain("It");
        results[1].PrimaryLabel.Should().Be("V");
        results[2].PrimaryLabel.Should().Be("I");
    }

    [Fact]
    public void AnalyzeFrenchAugmentedSixth_Fr43_V_I_ReturnsCorrectLabels()
    {
        // Arrange: French augmented sixth Fr43-V-I in C major
        var key = Key.CMajor;
        var analyzer = new HarmonyAnalyzer();
        var chords = new[]
        {
            new PitchClassSet(new[] { 8, 0, 2, 6 }), // Fr43 (Ab-C-D-F#)
            new PitchClassSet(new[] { 7, 11, 2 }),   // V (G-B-D)
            new PitchClassSet(new[] { 0, 4, 7 })     // I (C-E-G)
        };

        // Act
        var results = analyzer.Analyze(chords, key);

        // Assert
        results.Should().HaveCount(3);
        results[0].PrimaryLabel.Should().Contain("Fr");
        results[1].PrimaryLabel.Should().Be("V");
        results[2].PrimaryLabel.Should().Be("I");
    }

    [Fact]
    public void AnalyzeGermanAugmentedSixth_Ger65_V_I_ReturnsCorrectLabels()
    {
        // Arrange: German augmented sixth Ger65-V-I in C major
        var key = Key.CMajor;
        var analyzer = new HarmonyAnalyzer();
        var chords = new[]
        {
            new PitchClassSet(new[] { 8, 0, 3, 6 }), // Ger65 (Ab-C-Eb-F#)
            new PitchClassSet(new[] { 7, 11, 2 }),   // V (G-B-D)
            new PitchClassSet(new[] { 0, 4, 7 })     // I (C-E-G)
        };

        // Act
        var results = analyzer.Analyze(chords, key);

        // Assert
        results.Should().HaveCount(3);
        results[0].PrimaryLabel.Should().Contain("Ger");
        results[1].PrimaryLabel.Should().Be("V");
        results[2].PrimaryLabel.Should().Be("I");
    }

    [Fact]
    public void AnalyzeGermanVsBorrowedSeventh_Ger65_bVI7_V_I_DistinguishesCorrectly()
    {
        // Arrange: Ger65 vs bVI7 disambiguation in C major
        // This tests the analyzer's ability to distinguish between:
        // - Ger65 (Ab-C-Eb-F#): Augmented sixth chord
        // - bVI7 (Ab-C-Eb-Gb): Borrowed seventh chord
        var key = Key.CMajor;
        var analyzer = new HarmonyAnalyzer();
        var chords = new[]
        {
            new PitchClassSet(new[] { 8, 0, 3, 6 }),  // Ger65 (Ab-C-Eb-F#)
            new PitchClassSet(new[] { 8, 0, 3, 10 }), // bVI7 (Ab-C-Eb-Gb)
            new PitchClassSet(new[] { 7, 11, 2 }),    // V (G-B-D)
            new PitchClassSet(new[] { 0, 4, 7 })      // I (C-E-G)
        };

        // Act
        var results = analyzer.Analyze(chords, key);

        // Assert
        results.Should().HaveCount(4);
        
        // First chord should be Ger65 (augmented sixth)
        results[0].PrimaryLabel.Should().Contain("Ger");
        
        // Second chord should be bVI7 (borrowed seventh)
        results[1].PrimaryLabel.Should().Contain("bVI7");
        
        results[2].PrimaryLabel.Should().Be("V");
        results[3].PrimaryLabel.Should().Be("I");
    }

    #endregion
}
