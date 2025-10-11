namespace MusicTheory.Tests;

using MusicTheory.Theory.Interval;
using MusicTheory.Theory.Pitch;
using MusicTheory.Theory.Analysis;
using Xunit;

public class SmallUtilityStructsTests
{
    // ========================================
    // SemitoneInterval Tests (25% → 100%)
    // ========================================
    
    [Fact]
    public void SemitoneInterval_Constructor_StoresSemitones()
    {
        var si = new SemitoneInterval(7);
        Assert.Equal(7, si.Semitones);
    }
    
    [Fact]
    public void SemitoneInterval_ImplicitFromInt_CreatesInstance()
    {
        SemitoneInterval si = 12;
        Assert.Equal(12, si.Semitones);
    }
    
    [Fact]
    public void SemitoneInterval_ImplicitToInt_ReturnsValue()
    {
        var si = new SemitoneInterval(5);
        int value = si;
        Assert.Equal(5, value);
    }
    
    [Fact]
    public void SemitoneInterval_ToString_FormatsWithSuffix()
    {
        var si = new SemitoneInterval(11);
        Assert.Equal("11st", si.ToString());
    }
    
    [Fact]
    public void SemitoneInterval_ZeroValue_HandlesCorrectly()
    {
        SemitoneInterval si = 0;
        Assert.Equal(0, si.Semitones);
        Assert.Equal("0st", si.ToString());
    }
    
    [Fact]
    public void SemitoneInterval_NegativeValue_HandlesCorrectly()
    {
        var si = new SemitoneInterval(-3);
        Assert.Equal(-3, si.Semitones);
        Assert.Equal("-3st", si.ToString());
    }

    // ========================================
    // Pitch Tests (50% → 100%)
    // ========================================
    
    [Fact]
    public void Pitch_Constructor_StoresSpellingAndOctave()
    {
        var spelling = new SpelledPitch(Letter.C, new Accidental(0));
        var pitch = new Pitch(spelling, 4);
        Assert.Equal(spelling, pitch.Spelling);
        Assert.Equal(4, pitch.Octave);
    }
    
    [Fact]
    public void Pitch_ToString_FormatsSpellingWithOctave()
    {
        var spelling = new SpelledPitch(Letter.D, new Accidental(1)); // D#
        var pitch = new Pitch(spelling, 5);
        Assert.Contains("D", pitch.ToString());
        Assert.Contains("5", pitch.ToString());
    }
    
    [Fact]
    public void Pitch_NegativeOctave_HandlesCorrectly()
    {
        var spelling = new SpelledPitch(Letter.A, new Accidental(0));
        var pitch = new Pitch(spelling, -1);
        Assert.Equal(-1, pitch.Octave);
    }
    
    [Fact]
    public void Pitch_EqualityComparison_WorksCorrectly()
    {
        var spelling1 = new SpelledPitch(Letter.E, new Accidental(-1)); // Eb
        var pitch1 = new Pitch(spelling1, 3);
        var pitch2 = new Pitch(spelling1, 3);
        var pitch3 = new Pitch(spelling1, 4);
        
        Assert.Equal(pitch1, pitch2);
        Assert.NotEqual(pitch1, pitch3);
    }

    // ========================================
    // PitchClass Tests (60% → 100%)
    // ========================================
    
    [Fact]
    public void PitchClass_Constructor_StoresValue()
    {
        var pc = new PitchClass(7);
        Assert.Equal(7, pc.Value);
        Assert.Equal(7, pc.Pc);
    }
    
    [Fact]
    public void PitchClass_FromMidi_ModulosTwelve()
    {
        var pc = PitchClass.FromMidi(64); // E4 → E (pc 4)
        Assert.Equal(4, pc.Pc);
    }
    
    [Fact]
    public void PitchClass_FromMidi_HandlesNegative()
    {
        var pc = PitchClass.FromMidi(-5); // wraps to 7
        Assert.Equal(7, pc.Pc);
    }
    
    [Fact]
    public void PitchClass_Pc_NormalizesModulo12()
    {
        var pc1 = new PitchClass(15); // wraps to 3
        var pc2 = new PitchClass(-9); // wraps to 3
        Assert.Equal(3, pc1.Pc);
        Assert.Equal(3, pc2.Pc);
    }
    
    [Fact]
    public void PitchClass_AddOperator_TransposesCorrectly()
    {
        var pc = new PitchClass(5);
        var transposed = pc + 7; // 5+7=12 → wraps to 0
        Assert.Equal(0, transposed.Pc);
    }
    
    [Fact]
    public void PitchClass_ToString_FormatsAsTwoDigits()
    {
        var pc1 = new PitchClass(5);
        var pc2 = new PitchClass(11);
        Assert.Equal("05", pc1.ToString());
        Assert.Equal("11", pc2.ToString());
    }

    // ========================================
    // AlternativeScaleRankInfo Tests (50% → 100%)
    // ========================================
    
    [Fact]
    public void AlternativeScaleRankInfo_Constructor_StoresAllFields()
    {
        var info = new AlternativeScaleRankInfo(
            ScaleName: "Dorian",
            SharedRatio: 0.85,
            TensionPresence: 0.70,
            SharedCount: 6,
            CoveredTensionCount: 3,
            TotalTensionCount: 4,
            ExactCoverBonus: 0.1,
            Priority: 1.0
        );
        
        Assert.Equal("Dorian", info.ScaleName);
        Assert.Equal(0.85, info.SharedRatio);
        Assert.Equal(0.70, info.TensionPresence);
        Assert.Equal(6, info.SharedCount);
        Assert.Equal(3, info.CoveredTensionCount);
        Assert.Equal(4, info.TotalTensionCount);
        Assert.Equal(0.1, info.ExactCoverBonus);
        Assert.Equal(1.0, info.Priority);
    }
    
    [Fact]
    public void AlternativeScaleRankInfo_Equality_ComparesAllFields()
    {
        var info1 = new AlternativeScaleRankInfo("Mixolydian", 0.9, 0.8, 7, 4, 5, 0.2, 1.5);
        var info2 = new AlternativeScaleRankInfo("Mixolydian", 0.9, 0.8, 7, 4, 5, 0.2, 1.5);
        var info3 = new AlternativeScaleRankInfo("Lydian", 0.9, 0.8, 7, 4, 5, 0.2, 1.5);
        
        Assert.Equal(info1, info2);
        Assert.NotEqual(info1, info3);
    }
    
    [Fact]
    public void AlternativeScaleRankInfo_GetHashCode_ConsistentWithEquals()
    {
        var info1 = new AlternativeScaleRankInfo("Altered", 0.75, 0.65, 5, 2, 3, 0.15, 0.95);
        var info2 = new AlternativeScaleRankInfo("Altered", 0.75, 0.65, 5, 2, 3, 0.15, 0.95);
        
        Assert.Equal(info1.GetHashCode(), info2.GetHashCode());
    }
    
    [Fact]
    public void AlternativeScaleRankInfo_ToString_ContainsScaleName()
    {
        var info = new AlternativeScaleRankInfo("Phrygian", 0.80, 0.60, 6, 3, 4, 0.05, 0.90);
        var str = info.ToString();
        Assert.Contains("Phrygian", str);
    }
}
