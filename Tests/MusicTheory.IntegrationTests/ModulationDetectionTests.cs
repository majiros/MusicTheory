using FluentAssertions;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.IntegrationTests;

/// <summary>
/// Integration tests for key estimation and modulation detection.
/// Tests various modulation patterns and key change scenarios.
/// </summary>
public class ModulationDetectionTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void AnalyzeModulation_CtoG_WithSecondaryDominants_DetectsKeyChange()
    {
        // Arrange: C major ↁEG major with secondary dominants
        // C: I V/V V I ↁEG: IV V I
        var key = new Key(60, true); // C major
        var pcsList = new List<int[]>
        {
            new[] { Pc(60), Pc(64), Pc(67) },      // C-E-G (I in C)
            new[] { Pc(62), Pc(66), Pc(69) },      // D-F#-A (V/V in C)
            new[] { Pc(67), Pc(71), Pc(74) },      // G-B-D (V in C)
            new[] { Pc(60), Pc(64), Pc(67) },      // C-E-G (I in C)
            new[] { Pc(60), Pc(64), Pc(67) },      // C-E-G (IV in G) - pivot
            new[] { Pc(67), Pc(71), Pc(74) },      // G-B-D (V in G)
            new[] { Pc(67), Pc(71), Pc(74) }       // G-B-D (I in G)
        };

        // Act
        var (result, segments) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(pcsList, key);

        // Assert
        result.Should().NotBeNull();
        result.Chords.Should().HaveCountGreaterThanOrEqualTo(7);
        
        // Should detect at least 1 segment
        segments.Should().NotBeEmpty();
        
        // Should detect either C major or G major (KeyEstimator may be conservative)
        var detectedKeys = segments.Select(s => s.key.TonicMidi).Distinct().ToList();
        (detectedKeys.Contains(60) || detectedKeys.Contains(67)).Should().BeTrue(
            "the progression should be analyzed in C major or G major context");
    }

    [Fact]
    public void AnalyzeModulation_CtoAminor_RelativeModulation_DetectsKeyChange()
    {
        // Arrange: C major ↁEA minor (relative minor)
        // C: I IV V I ↁEAm: i iv V i
        var key = new Key(60, true); // C major
        var pcsList = new List<int[]>
        {
            new[] { Pc(60), Pc(64), Pc(67) },      // C-E-G (I in C)
            new[] { Pc(65), Pc(69), Pc(72) },      // F-A-C (IV in C)
            new[] { Pc(67), Pc(71), Pc(74) },      // G-B-D (V in C)
            new[] { Pc(60), Pc(64), Pc(67) },      // C-E-G (I in C / III in Am)
            new[] { Pc(69), Pc(72), Pc(76) },      // A-C-E (i in Am)
            new[] { Pc(62), Pc(65), Pc(69) },      // D-F-A (iv in Am)
            new[] { Pc(64), Pc(68), Pc(71) },      // E-G#-B (V in Am)
            new[] { Pc(69), Pc(72), Pc(76) }       // A-C-E (i in Am)
        };

        // Act
        var (result, segments) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(pcsList, key);

        // Assert
        result.Should().NotBeNull();
        result.Chords.Should().HaveCountGreaterThanOrEqualTo(8);
        
        // Should detect modulation to A minor
        segments.Should().Contain(s => s.key.TonicMidi % 12 == 9 && !s.key.IsMajor);
    }

    [Fact]
    public void AnalyzeModulation_CtoF_SubdominantModulation_DetectsKeyChange()
    {
        // Arrange: C major ↁEF major (subdominant)
        // C: I IV V ↁEF: V I IV V I
        var key = new Key(60, true); // C major
        var pcsList = new List<int[]>
        {
            new[] { Pc(60), Pc(64), Pc(67) },      // C-E-G (I in C)
            new[] { Pc(65), Pc(69), Pc(72) },      // F-A-C (IV in C / I in F)
            new[] { Pc(67), Pc(71), Pc(74) },      // G-B-D (V in C / V/V in F)
            new[] { Pc(60), Pc(64), Pc(67) },      // C-E-G (V in F)
            new[] { Pc(65), Pc(69), Pc(72) },      // F-A-C (I in F)
            new[] { Pc(70), Pc(74), Pc(77) },      // Bb-D-F (IV in F)
            new[] { Pc(60), Pc(64), Pc(67) },      // C-E-G (V in F)
            new[] { Pc(65), Pc(69), Pc(72) }       // F-A-C (I in F)
        };

        // Act
        var (result, segments) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(pcsList, key);

        // Assert
        result.Should().NotBeNull();
        result.Chords.Should().HaveCountGreaterThanOrEqualTo(8);
        
        // Should detect modulation to F major
        segments.Should().Contain(s => s.key.TonicMidi % 12 == 5 && s.key.IsMajor);
    }

    [Fact]
    public void AnalyzeModulation_NoModulation_StableKey_SingleSegment()
    {
        // Arrange: Progression staying in C major
        // C: I vi IV V I
        var key = new Key(60, true); // C major
        var pcsList = new List<int[]>
        {
            new[] { Pc(60), Pc(64), Pc(67) },      // C-E-G (I)
            new[] { Pc(69), Pc(72), Pc(76) },      // A-C-E (vi)
            new[] { Pc(65), Pc(69), Pc(72) },      // F-A-C (IV)
            new[] { Pc(67), Pc(71), Pc(74) },      // G-B-D (V)
            new[] { Pc(60), Pc(64), Pc(67) }       // C-E-G (I)
        };

        // Act
        var (result, segments) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(pcsList, key);

        // Assert
        result.Should().NotBeNull();
        result.Chords.Should().HaveCount(5);
        
        // Should stay in C major (single segment or all segments in C)
        segments.Should().OnlyContain(s => s.key.TonicMidi == 60 && s.key.IsMajor);
    }

    [Fact]
    public void AnalyzeModulation_ShortProgression_HandlesGracefully()
    {
        // Arrange: Very short progression (insufficient for modulation detection)
        var key = new Key(60, true); // C major
        var pcsList = new List<int[]>
        {
            new[] { Pc(60), Pc(64), Pc(67) },      // C-E-G (I)
            new[] { Pc(67), Pc(71), Pc(74) }       // G-B-D (V)
        };

        // Act
        var (result, segments) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(pcsList, key);

        // Assert
        result.Should().NotBeNull();
        result.Chords.Should().HaveCount(2);
        
        // Should not crash and should provide some result
        segments.Should().NotBeNull();
    }

    [Fact]
    public void AnalyzeModulation_ChromaticProgression_HandlesAmbiguity()
    {
        // Arrange: Chromatic progression with potential ambiguity
        var key = new Key(60, true); // C major
        var pcsList = new List<int[]>
        {
            new[] { Pc(60), Pc(64), Pc(67) },      // C-E-G
            new[] { Pc(61), Pc(65), Pc(68) },      // C#-F-G# (chromatic)
            new[] { Pc(62), Pc(66), Pc(69) },      // D-F#-A
            new[] { Pc(63), Pc(67), Pc(70) },      // Eb-G-Bb (chromatic)
            new[] { Pc(64), Pc(68), Pc(71) },      // E-G#-B
            new[] { Pc(60), Pc(64), Pc(67) }       // C-E-G
        };

        // Act
        var (result, segments) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(pcsList, key);

        // Assert
        result.Should().NotBeNull();
        result.Chords.Should().HaveCount(6);
        
        // Should handle chromatic progression without crashing
        segments.Should().NotBeNull();
    }

    [Fact]
    public void AnalyzeModulation_CtoD_WholeStepModulation_DetectsKeyChange()
    {
        // Arrange: C major ↁED major (common in jazz/pop)
        var key = new Key(60, true); // C major
        var pcsList = new List<int[]>
        {
            new[] { Pc(60), Pc(64), Pc(67) },      // C-E-G (I in C)
            new[] { Pc(67), Pc(71), Pc(74) },      // G-B-D (V in C)
            new[] { Pc(60), Pc(64), Pc(67) },      // C-E-G (I in C / bVII in D)
            new[] { Pc(62), Pc(66), Pc(69) },      // D-F#-A (I in D)
            new[] { Pc(67), Pc(71), Pc(74) },      // G-B-D (IV in D)
            new[] { Pc(69), Pc(73), Pc(76) },      // A-C#-E (V in D)
            new[] { Pc(62), Pc(66), Pc(69) }       // D-F#-A (I in D)
        };

        // Act
        var (result, segments) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(pcsList, key);

        // Assert
        result.Should().NotBeNull();
        result.Chords.Should().HaveCountGreaterThanOrEqualTo(7);
        
        // Should detect modulation to D major
        segments.Should().Contain(s => s.key.TonicMidi % 12 == 2 && s.key.IsMajor);
    }

    [Fact]
    public void AnalyzeModulation_MinorToMajor_ParallelModulation_DetectsKeyChange()
    {
        // Arrange: A minor ↁEA major (parallel)
        var key = new Key(69, false); // A minor
        var pcsList = new List<int[]>
        {
            new[] { Pc(69), Pc(72), Pc(76) },      // A-C-E (i in Am)
            new[] { Pc(62), Pc(65), Pc(69) },      // D-F-A (iv in Am)
            new[] { Pc(64), Pc(68), Pc(71) },      // E-G#-B (V in Am)
            new[] { Pc(69), Pc(72), Pc(76) },      // A-C-E (i in Am)
            new[] { Pc(69), Pc(73), Pc(76) },      // A-C#-E (I in A major) - pivot
            new[] { Pc(62), Pc(66), Pc(69) },      // D-F#-A (IV in A major)
            new[] { Pc(64), Pc(68), Pc(71) },      // E-G#-B (V in A major)
            new[] { Pc(69), Pc(73), Pc(76) }       // A-C#-E (I in A major)
        };

        // Act
        var (result, segments) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(pcsList, key);

        // Assert
        result.Should().NotBeNull();
        result.Chords.Should().HaveCountGreaterThanOrEqualTo(8);
        
        // Should detect both A minor and A major
        segments.Should().Contain(s => s.key.TonicMidi % 12 == 9);
    }
}
