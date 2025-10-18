# Developer Quick Start Guide

**MusicTheory Library - Getting Started in 5 Minutes**

This guide helps you quickly integrate and use the MusicTheory library for harmony analysis, chord progression detection, and key estimation.

---

## 📋 Table of Contents

- [Installation](#installation)
- [Quick Examples](#quick-examples)
  - [Basic Chord Analysis](#basic-chord-analysis)
  - [Roman Numeral Analysis](#roman-numeral-analysis)
  - [Key Estimation](#key-estimation)
  - [Chord Progression Analysis](#chord-progression-analysis)
- [CLI Usage](#cli-usage)
  - [Analyzing Chord Progressions](#analyzing-chord-progressions)
  - [JSON Output](#json-output)
  - [Key Modulation Detection](#key-modulation-detection)
- [Common Patterns](#common-patterns)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Installation

### From Source

```bash
git clone https://github.com/majiros/MusicTheory.git
cd MusicTheory
dotnet build -c Release
```

### Running Tests

```bash
dotnet test -c Release
```

**Coverage:** 84.8% (915 tests) with 75% quality gate enforced.

---

## Quick Examples

### Basic Chord Analysis

```csharp
using MusicTheory.Theory.Chord;

// Create a C major chord
var chord = new Chord(
    new[] { Pitch.C4, Pitch.E4, Pitch.G4 }
);

Console.WriteLine(chord.Name);  // "C"
Console.WriteLine(chord.Root);  // "C"
```

### Roman Numeral Analysis

```csharp
using MusicTheory.Theory.Harmony;
using MusicTheory.Theory.Scale;

// Analyze I-IV-V-I progression in C major
var key = Key.CMajor;
var analyzer = new HarmonyAnalyzer();

// Create chords
var chords = new[] {
    new Chord(new[] { Pitch.C4, Pitch.E4, Pitch.G4 }),  // I
    new Chord(new[] { Pitch.F4, Pitch.A4, Pitch.C5 }),  // IV
    new Chord(new[] { Pitch.G4, Pitch.B4, Pitch.D5 }),  // V
    new Chord(new[] { Pitch.C4, Pitch.E4, Pitch.G4 })   // I
};

var analysis = analyzer.Analyze(chords, key);

foreach (var result in analysis) {
    Console.WriteLine($"{result.Roman} - {result.Function}");
}
// Output:
// I - Tonic
// IV - Subdominant
// V - Dominant
// I - Tonic
```

### Key Estimation

```csharp
using MusicTheory.Theory.Analysis;

// Estimate key from pitch classes
var pitchClasses = new[] { 0, 2, 4, 5, 7, 9, 11 };  // C major scale
var estimator = new KeyEstimator();

var results = estimator.EstimatePerChord(
    new[] { new[] { 0, 4, 7 } },  // C major chord
    windowSize: 2
);

Console.WriteLine(results[0].Key);        // "C major"
Console.WriteLine(results[0].Confidence); // High confidence value
```

### Chord Progression Analysis

```csharp
using MusicTheory.Theory.Harmony;

// Analyze ii-V-I progression (jazz standard)
var progression = new[] {
    "ii7",  // Dm7
    "V7",   // G7
    "I"     // Cmaj7
};

var analyzer = new ProgressionAnalyzer();
var result = analyzer.Analyze(progression, Key.CMajor);

Console.WriteLine(result.CadenceType);  // "Authentic"
Console.WriteLine(result.Strength);     // "Strong"
```

---

## CLI Usage

The CLI tool provides powerful command-line analysis capabilities.

### Analyzing Chord Progressions

**Example 1: Simple I-V-I progression**

```bash
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj \
  -c Release \
  --roman "I; V; I" \
  --trace
```

**Output:**
```
Chord 0: I (C-E-G) → Function: Tonic
Chord 1: V (G-B-D) → Function: Dominant
Chord 2: I (C-E-G) → Function: Tonic
Cadence: Authentic Cadence (PAC)
```

**Example 2: Jazz ii-V-I with seventh chords**

```bash
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj \
  -c Release \
  --roman "ii7; V7; Imaj7" \
  --trace
```

**Output:**
```
Chord 0: ii7 (Dm7) → Function: Subdominant
Chord 1: V7 (G7) → Function: Dominant
Chord 2: Imaj7 (Cmaj7) → Function: Tonic
Cadence: Authentic Cadence (PAC)
```

### JSON Output

Get structured JSON output for programmatic use:

```bash
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj \
  -c Release \
  --roman "I; IV; V; I" \
  --json
```

**Output:**
```json
{
  "key": "C",
  "mode": "major",
  "chords": [
    {
      "index": 0,
      "roman": "I",
      "function": "Tonic",
      "pitchClasses": [0, 4, 7],
      "warnings": [],
      "errors": []
    },
    {
      "index": 1,
      "roman": "IV",
      "function": "Subdominant",
      "pitchClasses": [5, 9, 0],
      "warnings": [],
      "errors": []
    },
    {
      "index": 2,
      "roman": "V",
      "function": "Dominant",
      "pitchClasses": [7, 11, 2],
      "warnings": [],
      "errors": []
    },
    {
      "index": 3,
      "roman": "I",
      "function": "Tonic",
      "pitchClasses": [0, 4, 7],
      "warnings": [],
      "errors": []
    }
  ],
  "cadences": [
    {
      "position": 3,
      "type": "Authentic",
      "subtype": "PAC"
    }
  ]
}
```

### Key Modulation Detection

Detect key changes in chord progressions:

```bash
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj \
  -c Release \
  --pcs "0,4,7; 7,11,2; 0,4,7; 2,6,9,0; 7,11,2; 4,7,11; 9,0,4; 2,6,9; 7,11,2" \
  --segments \
  --trace \
  --preset permissive
```

**Output:**
```
Segment 0 (Chords 0-2): Key = C major
  Chord 0: I (C-E-G)
  Chord 1: V (G-B-D)
  Chord 2: I (C-E-G)

Modulation detected at chord 3!

Segment 1 (Chords 3-8): Key = G major
  Chord 3: I (G-B-D) - new tonic
  Chord 4: V (D-F#-A)
  ...
```

---

## Common Patterns

### Pattern 1: Borrowed Chords (Mixture)

```bash
# bVI7 and bII7 in C major
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj \
  -c Release \
  --roman "bVI7; bII7; V; I" \
  --trace
```

**Warnings:** The CLI will flag mixture seventh chords (bVI7, bII7, bVII7, iv7) with advisory warnings for pedagogical clarity.

### Pattern 2: Secondary Dominants

```bash
# V/V → V → I (tonicization)
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj \
  -c Release \
  --roman "V/V; V; I" \
  --trace
```

### Pattern 3: Neapolitan Sixth

```bash
# N6 → V → I (Neapolitan resolution)
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj \
  -c Release \
  --roman "N6; V; I" \
  --trace
```

**Tip:** Use `--enforceN6` to force bII → bII6 (first inversion) for pedagogical clarity.

### Pattern 4: Augmented Sixth Chords

```bash
# Italian, French, German augmented sixth chords
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj \
  -c Release \
  --roman "It6; Fr43; Ger65; V; I"
```

**Disambiguation:** Use `--preferMixture7` to prefer mixture interpretation (bVI7) over Ger65 when ambiguous.

---

## Best Practices

### 1. Key Estimation

**✅ Do:**
- Use `--segments` with `--preset stable` for reliable modulation detection
- Provide at least 4-6 chords for accurate key estimation
- Use `--window 2` for balanced context awareness

**❌ Don't:**
- Use `--preset sensitive` for short progressions (too unstable)
- Estimate key from single isolated chords
- Ignore low confidence values (<0.5)

### 2. JSON Parsing

**✅ Do:**
```csharp
// Parse CLI JSON output
var json = JsonSerializer.Deserialize<AnalysisResult>(output);
foreach (var chord in json.Chords) {
    if (chord.Warnings.Any()) {
        Console.WriteLine($"Warning on {chord.Roman}: {chord.Warnings[0]}");
    }
}
```

**❌ Don't:**
- Parse `--trace` output (human-readable, not stable)
- Rely on order of warnings array (use warning type field)

### 3. Cadence Analysis

**✅ Do:**
- Use `--cad64Dominant` to interpret I64 as V suspension
- Check both `type` and `subtype` for complete cadence info
- Consider context (phrase endings, harmonic rhythm)

**❌ Don't:**
- Expect cadence detection on every V→I (needs sufficient context)
- Use cadence info alone for form analysis

### 4. Voicing and Inversions

**✅ Do:**
```bash
# Specify voicing explicitly for inversion detection
--pcs "4,7,0"  # I6 (first inversion)
--pcs "7,0,4"  # I64 (second inversion)
```

**❌ Don't:**
- Omit voicing when inversion matters
- Assume root position without verification

---

## Troubleshooting

### Issue 1: "Key estimation returns low confidence"

**Cause:** Insufficient context or ambiguous progression.

**Solution:**
```bash
# Add more chords for context
--pcs "0,4,7; 7,11,2; 0,4,7; 5,9,0; 7,11,2; 0,4,7"  # Better

# Or use wider window
--window 3
```

### Issue 2: "Modulation not detected"

**Cause:** Default preset is too conservative.

**Solution:**
```bash
# Try permissive preset
--preset permissive

# Or adjust thresholds manually
--minSwitch 1 --switchMargin 1
```

### Issue 3: "JSON schema validation fails"

**Cause:** Using old schema or custom modifications.

**Solution:**
```bash
# Get latest schema
dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj \
  -c Release \
  --schema

# Validate against schema:
# 1. Save schema to file
# 2. Use JSON schema validator library
```

### Issue 4: "Coverage below 75% gate"

**Cause:** New code added without tests.

**Solution:**
```bash
# Run coverage locally
dotnet test -c Release --collect "XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:./coverage-report

# Add tests to reach 75%+ before PR
# See LESSONS_LEARNED.md for testing strategy
```

---

## Advanced Topics

### Custom Analysis Options

```csharp
var options = new HarmonyOptions {
    MajorSeventhInversionNotation = true,  // IVmaj65 vs IV65
    V9Notation = false,                     // V7(9) vs V9
    Hide64 = true,                          // Hide passing/pedal 64
    Cadential64AsDominant = true,           // I64-V as V
    EnforceNeapolitanSixth = true,          // Force bII → bII6
    PreferMixture7 = false,                 // Ger65 vs bVI7
    StrictPAC = false                       // Soprano must be do
};

var analyzer = new HarmonyAnalyzer(options);
```

### Performance Optimization

**For bulk analysis:**
```csharp
// Reuse analyzer instance
var analyzer = new HarmonyAnalyzer();
var results = new List<AnalysisResult>();

foreach (var progression in progressions) {
    results.Add(analyzer.Analyze(progression));
}
```

**Benchmarks:**
- Single progression (4 chords): ~0.5ms
- Key estimation (10 chords): ~2ms
- Modulation detection (20 chords): ~5ms

*(See `Benchmarks/` for detailed performance tests)*

---

## Related Documentation

- **[README.md](README.md)** - Full feature documentation
- **[COVERAGE_ACHIEVEMENT.md](COVERAGE_ACHIEVEMENT.md)** - Testing methodology and results
- **[LESSONS_LEARNED.md](LESSONS_LEARNED.md)** - Coverage improvement insights
- **[Docs/UserGuide.md](Docs/UserGuide.md)** - Comprehensive user guide

---

## Quick Reference Card

| Task | Command | Output |
|------|---------|--------|
| Analyze roman numerals | `--roman "I; V; I"` | Human-readable analysis |
| Get JSON output | `--roman "I; V; I" --json` | Structured JSON |
| Detect modulation | `--pcs "..." --segments` | Key segments |
| Show JSON schema | `--schema` | JSON schema definition |
| Parse roman to JSON | `--romanJson "I; V6"` | Utility conversion |
| Parse PCs to JSON | `--pcsJson "0,4,7; ..."` | Utility conversion |

---

## Support & Contributing

- **Issues:** [GitHub Issues](https://github.com/majiros/MusicTheory/issues)
- **Discussions:** [GitHub Discussions](https://github.com/majiros/MusicTheory/discussions)
- **Coverage:** 84.8% line, 74.8% branch, 92.4% method (915 tests)
- **CI/CD:** Automated testing with 75% coverage gate

**Quality Assurance:**
- ✅ All PRs require passing tests
- ✅ Coverage gate enforced (75% minimum)
- ✅ Automated linting and formatting
- ✅ JSON schema validation

---

*Last Updated: 2025-10-18 | Version: 1.0 | Coverage: 84.8%*
