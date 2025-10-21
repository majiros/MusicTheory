# Integration Testing Strategy for v1.1.0

**Document Version:** 1.0.0  
**Created:** 2025-10-21  
**Target Release:** v1.1.0 (Q1 2026)  
**Current Coverage:** 84.8% → **Target:** 85.5%+ (+0.5-1.0%)

---

## 🎯 Executive Summary

### Objective
Design and implement integration tests to validate end-to-end harmony analysis workflows, targeting 85.5%+ coverage (current: 84.8%).

### Key Insight from LESSONS_LEARNED.md
> **Phase 19-20 taught us:** Unit tests have a natural ceiling at 84.8%. Integration tests are the next step to reach 85%+ while adding real value.

### Success Criteria
- ✅ Coverage increase: +0.5-1.0% (realistic, not +5%)
- ✅ Test quality: Validate actual user workflows
- ✅ Maintainability: Clear, documented test scenarios
- ✅ CI/CD integration: Automated execution with quality gates

---

## 📊 Coverage Analysis

### Current State (v1.0.0)
```
Line Coverage:    84.8% (3,119 / 3,674 lines)
Branch Coverage:  74.8% (2,515 / 3,359 branches)
Method Coverage:  92.4% (536 / 580 methods)
Total Tests:      915 passing, 2 skipped
```

### Coverage Gaps Identified
Based on COVERAGE_ACHIEVEMENT.md Phase 1-18 analysis:

1. **HarmonyAnalyzer Integration Paths** (~40-60 lines)
   - Multi-chord progression scenarios
   - Modulation detection workflows
   - Borrowed chord + augmented sixth sequences

2. **ProgressionAnalyzer Complex Flows** (~30-50 lines)
   - Key estimation with ambiguous progressions
   - Segment boundary detection edge cases
   - Preset parameter interaction

3. **Cross-Component Integration** (~20-40 lines)
   - RomanNumeralParser → HarmonyAnalyzer → CadenceAnalyzer pipeline
   - PitchClassSet → ChordAnalyzer → HarmonyAnalyzer workflow
   - Duration → TimeSignature → BarBeatTick conversion chains

**Estimated Total Gap:** 90-150 lines  
**Realistic Coverage Gain:** 0.5-1.0% (90-150 / 3,674 = 2.4-4.1% theoretical, expect ~25-40% effectiveness)

---

## 🏗️ Integration Test Categories

### Category 1: Progression Analysis Scenarios
**Goal:** Test full harmony analysis workflows from input to labeled output

**Test Cases:**
1. **Basic Diatonic Progressions**
   - I-IV-V-I (C major)
   - ii-V-I (jazz cadence)
   - I-vi-IV-V (pop progression)
   
2. **Secondary Dominant Chains**
   - V/V-V-I (tonicization)
   - vii°7/V-V-I (leading-tone approach)
   - V/ii-ii-V-I (extended secondary)

3. **Borrowed Chord Progressions**
   - I-bVI-bVII-I (modal mixture)
   - iv-V-I (minor subdominant)
   - bII-V-I (Neapolitan resolution)

4. **Augmented Sixth Resolutions**
   - It6-V-I (Italian sixth)
   - Fr43-V-I (French augmented sixth)
   - Ger65-V-I (German augmented sixth)
   - Ger65 vs bVI7 disambiguation

**Expected Coverage Gain:** +0.2-0.4%

### Category 2: Modulation Detection Workflows
**Goal:** Validate key estimation and segment detection

**Test Cases:**
1. **Common Modulations**
   - C major → G major (dominant modulation)
   - C major → F major (subdominant modulation)
   - C major → A minor (relative minor)

2. **Pivot Chord Modulations**
   - I-IV-[pivot]-ii-V-I (C→G via shared IV/I)
   - Detection of segment boundaries
   - Confidence score validation

3. **Direct Modulations**
   - Abrupt key changes without pivot
   - Minimum segment length enforcement
   - Preset parameter impact (stable vs permissive)

**Expected Coverage Gain:** +0.1-0.2%

### Category 3: Six-Four Classification Edge Cases
**Goal:** Test complex 6-4 voicing scenarios

**Test Cases:**
1. **Passing Six-Four Variations**
   - Ascending bass: IV-IV64-IV6
   - Descending bass: IV-IV64-IV (already tested in unit tests)
   - Adjacent voicing ambiguity

2. **Pedal Six-Four Variations**
   - Root returns: IV-IV64-IV
   - Root sustains: V-I64-V (pedal tone)

3. **Cadential Six-Four Options**
   - I64-V-I (standard)
   - I64-V-I with --cad64Dominant flag
   - Non-cadential suppression edge cases

**Expected Coverage Gain:** +0.05-0.1%

### Category 4: CLI Integration Tests
**Goal:** Validate end-to-end CLI workflows with JSON output

**Test Cases:**
1. **Roman Numeral Input Pipeline**
   - `--roman "I; V; I" --json` → Valid JSON schema
   - `--roman "bVI7; V; I" --warnings` → Mixture-7th warnings present
   - `--roman "bII; V; I" --enforceN6` → Neapolitan forced to 6th

2. **Pitch Class Set Input Pipeline**
   - `--pcs "0,4,7; 7,11,2; 0,4,7" --segments --json` → Modulation detection
   - `--key C --pcs "5,8,0,3"` → iv7 borrowed chord detection

3. **Preset Application**
   - `--preset stable` → Conservative parameters applied
   - `--preset permissive` → Sensitive parameters applied
   - `--preset pedagogical` → N6 enforcement active

**Expected Coverage Gain:** +0.1-0.2%

### Category 5: Error Recovery and Resilience
**Goal:** Test graceful degradation and error handling

**Test Cases:**
1. **Invalid Input Handling**
   - Malformed Roman numerals → Clear error messages
   - Out-of-range pitch classes → Validation errors
   - Conflicting options → Option precedence

2. **Ambiguous Analysis Results**
   - Tied confidence scores → Consistent disambiguation
   - Multiple valid interpretations → Stable selection
   - Empty/single-chord inputs → No crash

3. **Edge Case Inputs**
   - Very long progressions (100+ chords)
   - Atonal pitch class sets
   - All 12 pitch classes (chromatic)

**Expected Coverage Gain:** +0.05-0.1%

---

## 🛠️ Implementation Plan

### Phase 1: Project Setup (Week 1)
**Tasks:**
- [ ] Create `Tests/MusicTheory.IntegrationTests` project
- [ ] Add xUnit + FluentAssertions dependencies
- [ ] Configure test project reference to main MusicTheory project
- [ ] Add integration tests to `test.yml` CI workflow
- [ ] Create `IntegrationTestBase.cs` helper class

**Deliverables:**
- Working integration test project structure
- CI/CD execution confirmed

### Phase 2: Progression Scenario Tests (Week 2-3)
**Tasks:**
- [ ] Implement Category 1 tests (Basic/Secondary/Borrowed/Aug6)
- [ ] Create `ProgressionScenarioTests.cs`
- [ ] Validate JSON output schema compliance
- [ ] Measure coverage gain

**Deliverables:**
- 15-20 progression scenario tests
- +0.2-0.4% coverage increase

### Phase 3: Modulation Detection Tests (Week 4)
**Tasks:**
- [ ] Implement Category 2 tests (Modulations/Pivots/Direct)
- [ ] Create `ModulationDetectionTests.cs`
- [ ] Test preset parameter variations
- [ ] Validate segment boundary detection

**Deliverables:**
- 10-15 modulation tests
- +0.1-0.2% coverage increase

### Phase 4: CLI Integration Tests (Week 5)
**Tasks:**
- [ ] Implement Category 4 tests (Roman/PCS/Presets)
- [ ] Create `CliIntegrationTests.cs`
- [ ] Use ProcessRunner to execute CLI
- [ ] Parse and validate JSON output

**Deliverables:**
- 8-12 CLI integration tests
- +0.1-0.2% coverage increase

### Phase 5: Polish & Documentation (Week 6)
**Tasks:**
- [ ] Implement Category 3 & 5 tests (remaining gaps)
- [ ] Review coverage reports
- [ ] Update INTEGRATION_TESTING.md documentation
- [ ] Update CI/CD coverage gate (75% → 76%)

**Deliverables:**
- Complete integration test suite (40-60 tests)
- **Target: 85.5%+ coverage achieved**
- Comprehensive documentation

---

## 📝 Test Structure Example

### Sample Integration Test: Diatonic Progression

```csharp
namespace MusicTheory.IntegrationTests;

public class ProgressionScenarioTests
{
    [Fact]
    public void AnalyzeDiatonicProgression_IIVVIPi_ReturnsCorrectLabels()
    {
        // Arrange
        var key = Key.CMajor;
        var chords = new[]
        {
            new PitchClassSet(new[] { 0, 4, 7 }),  // I (C-E-G)
            new PitchClassSet(new[] { 5, 9, 0 }),  // IV (F-A-C)
            new PitchClassSet(new[] { 7, 11, 2 }), // V (G-B-D)
            new PitchClassSet(new[] { 0, 4, 7 })   // I (C-E-G)
        };
        
        var analyzer = new HarmonyAnalyzer();
        
        // Act
        var results = analyzer.Analyze(chords, key);
        
        // Assert
        results.Should().HaveCount(4);
        results[0].PrimaryLabel.Should().Be("I");
        results[1].PrimaryLabel.Should().Be("IV");
        results[2].PrimaryLabel.Should().Be("V");
        results[3].PrimaryLabel.Should().Be("I");
        
        // Verify cadence detection
        var cadenceAnalyzer = new CadenceAnalyzer();
        var cadence = cadenceAnalyzer.AnalyzeProgression(results);
        cadence.Type.Should().Be(CadenceType.Perfect);
    }
    
    [Fact]
    public void AnalyzeSecondaryDominantChain_VofVVI_ReturnsCorrectLabels()
    {
        // Arrange
        var key = Key.CMajor;
        var chords = new[]
        {
            new PitchClassSet(new[] { 2, 6, 9, 0 }), // V7/V (D7)
            new PitchClassSet(new[] { 7, 11, 2 }),   // V (G)
            new PitchClassSet(new[] { 0, 4, 7 })     // I (C)
        };
        
        var analyzer = new HarmonyAnalyzer();
        
        // Act
        var results = analyzer.Analyze(chords, key);
        
        // Assert
        results[0].PrimaryLabel.Should().Contain("V7/V");
        results[1].PrimaryLabel.Should().Be("V");
        results[2].PrimaryLabel.Should().Be("I");
    }
}
```

### Sample CLI Integration Test

```csharp
public class CliIntegrationTests
{
    [Fact]
    public async Task CliRomanInput_WithJsonFlag_ReturnsValidJsonSchema()
    {
        // Arrange
        var cliPath = Path.Combine(TestContext.BinDirectory, "MusicTheory.Cli.exe");
        var args = "--roman \"I; V; I\" --json";
        
        // Act
        var result = await ProcessRunner.RunAsync(cliPath, args);
        
        // Assert
        result.ExitCode.Should().Be(0);
        
        var json = JsonDocument.Parse(result.StandardOutput);
        json.RootElement.GetProperty("chords").Should().NotBeNull();
        json.RootElement.GetProperty("chords").GetArrayLength().Should().Be(3);
        
        var firstChord = json.RootElement.GetProperty("chords")[0];
        firstChord.GetProperty("label").GetString().Should().Be("I");
        firstChord.GetProperty("pitchClasses").Should().NotBeNull();
    }
    
    [Fact]
    public async Task CliMixtureSeventh_WithWarnings_IncludesWarningInJson()
    {
        // Arrange
        var cliPath = Path.Combine(TestContext.BinDirectory, "MusicTheory.Cli.exe");
        var args = "--roman \"bVI7; V; I\" --json";
        
        // Act
        var result = await ProcessRunner.RunAsync(cliPath, args);
        
        // Assert
        result.ExitCode.Should().Be(0);
        
        var json = JsonDocument.Parse(result.StandardOutput);
        var firstChord = json.RootElement.GetProperty("chords")[0];
        
        firstChord.GetProperty("warnings").GetArrayLength().Should().BeGreaterThan(0);
        firstChord.GetProperty("warnings")[0].GetString()
            .Should().Contain("borrowed seventh chord");
    }
}
```

---

## 🎯 Realistic Expectations

### Coverage Gain Projection

Based on LESSONS_LEARNED.md Phase 19-20 analysis:

| Category | Tests | Line Gap | Expected Gain | Confidence |
|----------|-------|----------|---------------|------------|
| Progression Scenarios | 15-20 | 40-60 | +0.2-0.4% | High |
| Modulation Detection | 10-15 | 30-50 | +0.1-0.2% | Medium |
| CLI Integration | 8-12 | 20-30 | +0.1-0.2% | High |
| Six-Four Edge Cases | 5-8 | 10-20 | +0.05-0.1% | Low |
| Error Recovery | 5-8 | 10-20 | +0.05-0.1% | Low |
| **Total** | **40-60** | **110-180** | **+0.5-1.0%** | **Medium** |

**Target:** 85.5% (current 84.8% + 0.7%)  
**Stretch:** 85.8% (current 84.8% + 1.0%)

### Why Not Target 90%+?

From LESSONS_LEARNED.md:

> **Phase 19-20 Insight:** The remaining 15.2% gap includes:
> - Private helper methods (not directly testable)
> - Complex analyzer internals (require integration tests)
> - Error paths already covered indirectly
> - Parser edge cases already validated

**Realistic ceiling:** 85.5-86.0% (unit + integration combined)  
**Unrealistic target:** 90%+ (would require intrusive testing patterns)

---

## 📈 Success Metrics

### Quantitative Metrics
- ✅ Coverage: 85.0%+ line coverage (target: 85.5%)
- ✅ Tests: 40-60 new integration tests (total: 955-975)
- ✅ CI/CD: All tests passing in automated builds
- ✅ Performance: Integration tests complete in <30s

### Qualitative Metrics
- ✅ **Real-world value:** Tests validate actual user workflows
- ✅ **Maintainability:** Clear test names and documentation
- ✅ **Regression protection:** Catch breaking changes early
- ✅ **Developer confidence:** Integration tests provide end-to-end validation

### Documentation Deliverables
- ✅ INTEGRATION_TESTING.md: Best practices and patterns
- ✅ Test code comments: Explain complex scenarios
- ✅ README.md update: Mention integration test coverage
- ✅ CHANGELOG.md: Document v1.1.0 integration test addition

---

## 🚀 Next Steps

### Immediate Actions (This Session)
1. ✅ Create INTEGRATION_TESTING_STRATEGY.md (this document)
2. ⏭️ Create `Tests/MusicTheory.IntegrationTests` project structure
3. ⏭️ Implement first 5 progression scenario tests
4. ⏭️ Measure initial coverage gain

### Week 1 Goals
- [ ] Integration test project fully configured
- [ ] CI/CD integration complete
- [ ] 10-15 tests implemented
- [ ] +0.2-0.3% coverage gain

### v1.1.0 Release Criteria
- [ ] 85.5%+ coverage achieved
- [ ] 40-60 integration tests passing
- [ ] INTEGRATION_TESTING.md complete
- [ ] CI/CD coverage gate updated to 76%
- [ ] All documentation updated

---

## 📚 References

- **LESSONS_LEARNED.md:** Phase 19-20 plateau analysis, realistic expectations
- **COVERAGE_ACHIEVEMENT.md:** Phase 1-18 detailed progress, gap analysis
- **ROADMAP.md:** v1.1.0 timeline and milestones
- **test.yml:** CI/CD workflow configuration

---

**Document Status:** ✅ Complete  
**Next Document:** INTEGRATION_TESTING.md (implementation guide)  
**Implementation Start:** 2025-10-21
