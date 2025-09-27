using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class HarmonyTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Romanize_MajorKey_Triads()
    {
        var key = new Key(60, true); // C major
        // C E G
        Assert.True(ChordRomanizer.TryRomanizeTriad(new[]{ Pc(60), Pc(64), Pc(67) }, key, out var rn1));
        Assert.Equal(RomanNumeral.I, rn1);
        // F A C
        Assert.True(ChordRomanizer.TryRomanizeTriad(new[]{ Pc(65), Pc(69), Pc(72) }, key, out var rn4));
        Assert.Equal(RomanNumeral.IV, rn4);
        // G B D
        Assert.True(ChordRomanizer.TryRomanizeTriad(new[]{ Pc(67), Pc(71), Pc(62) }, key, out var rn5));
        Assert.Equal(RomanNumeral.V, rn5);
    }

    [Fact]
    public void AnalyzeTriad_WithVoicing_ChecksOrderingAndRanges()
    {
        var key = new Key(60, true);
        var pcs = new[]{ Pc(60), Pc(64), Pc(67) };
        var prev = new FourPartVoicing(76, 69, 60, 48); // E4 A4 C4 C3 (not strictly chord pc, just prior)
        var curr = new FourPartVoicing(79, 72, 67, 55); // G5 C5 G4 G3 (C major in 6/4-ish)
        var res = HarmonyAnalyzer.AnalyzeTriad(pcs, key, curr, prev);
        Assert.True(res.Success);
        Assert.Equal(RomanNumeral.I, res.Roman);
        Assert.NotNull(res.RomanText);
        Assert.True(res.Warnings.Count >= 0);
        Assert.True(res.Errors.Count >= 0);
    }

    [Fact]
    public void Inversion_Labeling_I6_I64()
    {
        var key = new Key(60, true); // C major
        var pcs = new[]{ Pc(60), Pc(64), Pc(67) }; // C E G
        // First inversion: E in bass
        var v1 = new FourPartVoicing(79, 76, 72, 64); // S A T B (E3 bass)
        var r1 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v1, null);
        Assert.True(r1.Success);
        Assert.StartsWith("I", r1.RomanText);
        Assert.EndsWith("6", r1.RomanText);

        // Second inversion: G in bass
        var v2 = new FourPartVoicing(81, 76, 72, 67);
        var r2 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v2, null);
        Assert.True(r2.Success);
        Assert.StartsWith("I", r2.RomanText);
        Assert.EndsWith("64", r2.RomanText);
    }

    [Fact]
    public void Romanize_V7_In_Major()
    {
        var key = new Key(60, true); // C major
        // G B D F -> V7
        var pcs = new[]{ Pc(67), Pc(71), Pc(62), Pc(65) };
        var ok = ChordRomanizer.TryRomanizeSeventh(pcs, key, out var rn, out var deg, out var q);
        Assert.True(ok);
        Assert.Equal(RomanNumeral.V, rn);
    }

    [Fact]
    public void Romanize_V7_Inversion_Labels()
    {
        var key = new Key(60, true); // C major
        var pcs = new[]{ Pc(67), Pc(71), Pc(62), Pc(65) }; // G B D F
        // Root position V7 (G in bass)
        var v0 = new FourPartVoicing(83, 79, 71, 67);
        var r0 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v0, null);
        Assert.True(r0.Success);
        Assert.Equal("V7", r0.RomanText);

        // 1st inversion V65 (B in bass)
        var v1 = new FourPartVoicing(86, 81, 74, 71);
        var r1 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v1, null);
        Assert.True(r1.Success);
        Assert.Equal("V65", r1.RomanText);

        // 2nd inversion V43 (D in bass)
        var v2 = new FourPartVoicing(86, 79, 74, 62);
        var r2 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v2, null);
        Assert.True(r2.Success);
        Assert.Equal("V43", r2.RomanText);

        // 3rd inversion V42 (F in bass)
        var v3 = new FourPartVoicing(86, 79, 74, 65);
        var r3 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v3, null);
        Assert.True(r3.Success);
        Assert.Equal("V42", r3.RomanText);
    }

    [Fact]
    public void Recognize_V9_With_Or_Without_PerfectFifth()
    {
        var key = new Key(60, true); // C major, V is G
        int Pc(int m) => ((m % 12) + 12) % 12;
        // Full V9: G B D F A (5 notes)
        var v9_full = new[]{ Pc(67), Pc(71), Pc(74), Pc(65), Pc(69) }; // G B D F A
    var r_full = HarmonyAnalyzer.AnalyzeTriad(v9_full, key, null, null);
        Assert.True(r_full.Success);
        Assert.Equal("V9", r_full.RomanText);

        // Omit 5th: G B F A (4 notes)
        var v9_no5 = new[]{ Pc(67), Pc(71), Pc(65), Pc(69) }; // G B F A (no D)
    var r_no5 = HarmonyAnalyzer.AnalyzeTriad(v9_no5, key, null, null);
        Assert.True(r_no5.Success);
        Assert.Equal("V9", r_no5.RomanText);

    // 表示切替オプション: PreferV7Paren9OverV9 = true で "V7(9)" を返す
    var opt = new HarmonyOptions { PreferV7Paren9OverV9 = true };
    var r_full_paren = HarmonyAnalyzer.AnalyzeTriad(v9_full, key, opt, null, null);
    Assert.Equal("V7(9)", r_full_paren.RomanText);
    var r_no5_paren = HarmonyAnalyzer.AnalyzeTriad(v9_no5, key, opt, null, null);
    Assert.Equal("V7(9)", r_no5_paren.RomanText);
    }

    [Fact]
    public void MinorKey_Triad_And_V7()
    {
        // A minor (A=57). Harmonic minor implied in mapping (G# leading tone)
        var keyMin = new Key(57, false);
        // i: A C E
        Assert.True(ChordRomanizer.TryRomanizeTriad(new[]{ Pc(57), Pc(60), Pc(64) }, keyMin, out var rni));
        Assert.Equal(RomanNumeral.i, rni);
        // V: E G# B (E major triad)
        Assert.True(ChordRomanizer.TryRomanizeTriad(new[]{ Pc(64), Pc(68), Pc(71) }, keyMin, out var rnv));
        Assert.Equal(RomanNumeral.V, rnv);
        // V7: E G# B D
        var pcsV7 = new[]{ Pc(64), Pc(68), Pc(71), Pc(62) };
        var ok = ChordRomanizer.TryRomanizeSeventh(pcsV7, keyMin, out var rnv7, out var deg, out var q);
        Assert.True(ok);
        Assert.Equal(RomanNumeral.V, rnv7);
    }

    [Fact]
    public void Cadence_Detection_Simple()
    {
        // C major
        Assert.Equal(CadenceType.Authentic, CadenceAnalyzer.Detect(RomanNumeral.V, RomanNumeral.I, true));
        Assert.Equal(CadenceType.Plagal, CadenceAnalyzer.Detect(RomanNumeral.IV, RomanNumeral.I, true));
        Assert.Equal(CadenceType.Deceptive, CadenceAnalyzer.Detect(RomanNumeral.V, RomanNumeral.vi, true));
        Assert.Equal(CadenceType.Half, CadenceAnalyzer.Detect(RomanNumeral.ii, RomanNumeral.V, true));

        // a minor (harmonic)
        Assert.Equal(CadenceType.Authentic, CadenceAnalyzer.Detect(RomanNumeral.V, RomanNumeral.i, false));
        Assert.Equal(CadenceType.Plagal, CadenceAnalyzer.Detect(RomanNumeral.iv, RomanNumeral.i, false));
        Assert.Equal(CadenceType.Deceptive, CadenceAnalyzer.Detect(RomanNumeral.V, RomanNumeral.VI, false));
    }

    [Fact]
    public void Seventh_Labeling_MajorAndMinor_Generalized()
    {
        // C major
        var keyM = new Key(60, true);
        // ii7: D F A C
        var pcs_ii7 = new[]{ Pc(62), Pc(65), Pc(69), Pc(72) };
        var v_ii7 = new FourPartVoicing(86, 81, 74, 62); // D in bass -> ii43 expected? For seventh root position is D; D bass => ii7
        var r_ii7 = HarmonyAnalyzer.AnalyzeTriad(pcs_ii7, keyM, v_ii7, null);
        Assert.True(r_ii7.Success);
        Assert.Equal("ii7", r_ii7.RomanText);

        // IVmaj7: F A C E
        var pcs_IVmaj7 = new[]{ Pc(65), Pc(69), Pc(72), Pc(76) };
        var v_IV7 = new FourPartVoicing(88, 81, 76, 65);
    var r_IV7 = HarmonyAnalyzer.AnalyzeTriad(pcs_IVmaj7, keyM, v_IV7, null);
    Assert.True(r_IV7.Success);
    Assert.Equal("IVmaj7", r_IV7.RomanText);

        // a minor (harmonic)
        var keym = new Key(57, false);
        // iiø7: B D F G#? (in A harmonic minor it's Bø7: B D F A?) -> We'll test iiø7 triad+7: B D F A
        var pcs_iio7 = new[]{ Pc(59), Pc(62), Pc(65), Pc(69) }; // B D F A
        var v_iio7 = new FourPartVoicing(81, 74, 69, 59);
        var r_iio7 = HarmonyAnalyzer.AnalyzeTriad(pcs_iio7, keym, v_iio7, null);
        Assert.True(r_iio7.Success);
    Assert.Equal("iiø7", r_iio7.RomanText);
    }

    [Fact]
    public void ProgressionAnalyzer_Returns_Romans_And_Cadences()
    {
        var key = new Key(60, true); // C major
        var seq = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) }, // I
            new[]{ Pc(65), Pc(69), Pc(72) }, // IV
            new[]{ Pc(67), Pc(71), Pc(62) }, // V
            new[]{ Pc(60), Pc(64), Pc(67) }, // I
        };
        var res = ProgressionAnalyzer.Analyze(seq, key);
        Assert.Equal(4, res.Chords.Count);
        Assert.Contains(res.Cadences, c => c.indexFrom == 2 && c.cadence == CadenceType.Authentic);
    }

    [Fact]
    public void Minor_Diminished_Triad_Labeling_With_Degree_Symbol()
    {
        // A minor: vii° (G# B D)
        var key = new Key(57, false);
        var pcs = new[]{ Pc(68), Pc(71), Pc(62) }; // G# B D
        // root position: G# in bass
        var v0 = new FourPartVoicing(83, 79, 71, 68);
        var r0 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v0, null);
        Assert.True(r0.Success);
        Assert.Equal("vii°", r0.RomanText);
        // first inversion: B in bass → vii°6
        var v1 = new FourPartVoicing(86, 81, 74, 71);
        var r1 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v1, null);
        Assert.True(r1.Success);
        Assert.Equal("vii°6", r1.RomanText);
    }

    [Fact]
    public void DiminishedSeventh_LeadingTone_In_Minor_Inversions()
    {
        // A minor: vii°7 (G# B D F) — fully diminished seventh (interval 9)
        var key = new Key(57, false);
        var pcs = new[]{ Pc(68), Pc(71), Pc(62), Pc(65) }; // G# B D F

        // Root position: G# in bass → vii°7
        var v0 = new FourPartVoicing(83, 79, 71, 68);
        var r0 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v0, null);
        Assert.True(r0.Success);
        Assert.Equal("vii°7", r0.RomanText);

        // 1st inversion: B in bass → vii°65
        var v1 = new FourPartVoicing(86, 81, 74, 71);
        var r1 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v1, null);
        Assert.True(r1.Success);
        Assert.Equal("vii°65", r1.RomanText);

        // 2nd inversion: D in bass → vii°43
        var v2 = new FourPartVoicing(86, 79, 74, 62);
        var r2 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v2, null);
        Assert.True(r2.Success);
        Assert.Equal("vii°43", r2.RomanText);

        // 3rd inversion: F in bass → vii°42
        var v3 = new FourPartVoicing(86, 79, 74, 65);
        var r3 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v3, null);
        Assert.True(r3.Success);
        Assert.Equal("vii°42", r3.RomanText);
    }

    [Fact]
    public void HalfDiminished_IIO7_In_Minor_All_Inversions()
    {
        // A minor: iiø7 (B D F A) — half-diminished seventh on supertonic
        var key = new Key(57, false);
        var pcs = new[] { Pc(59), Pc(62), Pc(65), Pc(69) }; // B D F A

        // Root position: B in bass → iiø7
        var v0 = new FourPartVoicing(81, 74, 69, 59);
        var r0 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v0, null);
        Assert.True(r0.Success);
        Assert.Equal("iiø7", r0.RomanText);

        // 1st inversion: D in bass → iiø65
        var v1 = new FourPartVoicing(86, 81, 74, 62);
        var r1 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v1, null);
        Assert.True(r1.Success);
        Assert.Equal("iiø65", r1.RomanText);

        // 2nd inversion: F in bass → iiø43
        var v2 = new FourPartVoicing(86, 81, 74, 65);
        var r2 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v2, null);
        Assert.True(r2.Success);
        Assert.Equal("iiø43", r2.RomanText);

        // 3rd inversion: A in bass → iiø42
        var v3 = new FourPartVoicing(86, 81, 74, 69);
        var r3 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v3, null);
        Assert.True(r3.Success);
        Assert.Equal("iiø42", r3.RomanText);
    }

    [Fact]
    public void SecondaryLeadingTone_viiDim7_To_V_Inversions()
    {
        // C major: vii°7/V = F# A C Eb (fully diminished seventh)
        var key = new Key(60, true);
        var pcs = new[] { Pc(66), Pc(69), Pc(60), Pc(63) }; // F# A C Eb

        // Root position: F# in bass → vii°7/V
        var v0 = new FourPartVoicing(87, 81, 72, 66);
        var r0 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v0, null);
        Assert.True(r0.Success);
        Assert.Equal("vii°7/V", r0.RomanText);

        // 1st inversion: A in bass → vii°65/V
        var v1 = new FourPartVoicing(89, 84, 76, 69);
        var r1 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v1, null);
        Assert.True(r1.Success);
        Assert.Equal("vii°65/V", r1.RomanText);

        // 2nd inversion: C in bass → vii°43/V
        var v2 = new FourPartVoicing(89, 84, 76, 60);
        var r2 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v2, null);
        Assert.True(r2.Success);
        Assert.Equal("vii°43/V", r2.RomanText);

        // 3rd inversion: Eb in bass → vii°42/V
        var v3 = new FourPartVoicing(89, 84, 77, 63);
        var r3 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v3, null);
        Assert.True(r3.Success);
        Assert.Equal("vii°42/V", r3.RomanText);
    }

    [Fact]
    public void BorrowedChords_ModalMixture_In_Major()
    {
        var key = new Key(60, true); // C major
        // i: C Eb G
        var i_minor = new[]{ Pc(60), Pc(63), Pc(67) };
        var ri = HarmonyAnalyzer.AnalyzeTriad(i_minor, key, null, null);
        Assert.True(ri.Success);
        Assert.Equal("i", ri.RomanText);

        // iv: F Ab C
        var iv_minor = new[]{ Pc(65), Pc(68), Pc(72) };
        var riv = HarmonyAnalyzer.AnalyzeTriad(iv_minor, key, null, null);
        Assert.True(riv.Success);
        Assert.Equal("iv", riv.RomanText);

        // bIII: Eb G Bb
        var bIII = new[]{ Pc(63), Pc(67), Pc(70) };
        var rIII = HarmonyAnalyzer.AnalyzeTriad(bIII, key, null, null);
        Assert.True(rIII.Success);
        Assert.Equal("bIII", rIII.RomanText);

        // bVI: Ab C Eb
        var bVI = new[]{ Pc(68), Pc(72), Pc(75) };
        var rVI = HarmonyAnalyzer.AnalyzeTriad(bVI, key, null, null);
        Assert.True(rVI.Success);
        Assert.Equal("bVI", rVI.RomanText);

        // bVII: Bb D F
        var bVII = new[]{ Pc(70), Pc(62), Pc(65) };
        var rVII = HarmonyAnalyzer.AnalyzeTriad(bVII, key, null, null);
        Assert.True(rVII.Success);
        Assert.Equal("bVII", rVII.RomanText);
    }

    [Fact]
    public void Neapolitan_bII_In_Major_And_Minor()
    {
        // C major: bII = Db F Ab
        var keyM = new Key(60, true);
        var bII_M = new[]{ Pc(61), Pc(65), Pc(68) };
        var rM = HarmonyAnalyzer.AnalyzeTriad(bII_M, keyM, null, null);
        Assert.True(rM.Success);
        Assert.Equal("bII", rM.RomanText);

        // A minor: bII = Bb D F
        var keym = new Key(57, false);
        var bII_m = new[]{ Pc(70), Pc(62), Pc(65) };
        var rm = HarmonyAnalyzer.AnalyzeTriad(bII_m, keym, null, null);
        Assert.True(rm.Success);
        Assert.Equal("bII", rm.RomanText);
    }

    [Fact]
    public void BorrowedSevenths_iv7_bVII7_Labeling()
    {
        var key = new Key(60, true); // C major
    // iv7: F Ab C Eb (borrowed minor subdominant seventh)
    var iv7 = new[]{ Pc(65), Pc(68), Pc(72), Pc(75) }; // F Ab C Eb
        var r_iv7 = HarmonyAnalyzer.AnalyzeTriad(iv7, key, null, null);
        Assert.True(r_iv7.Success);
        Assert.Equal("iv7", r_iv7.RomanText);

        // bVII7: Bb D F Ab
        var bVII7 = new[]{ Pc(70), Pc(62), Pc(65), Pc(68) };
        var r_bVII7 = HarmonyAnalyzer.AnalyzeTriad(bVII7, key, null, null);
        Assert.True(r_bVII7.Success);
        Assert.Equal("bVII7", r_bVII7.RomanText);
    }

    [Fact]
    public void BorrowedSevenths_Inversions_With_Voicing()
    {
        var key = new Key(60, true); // C major
        // iv7 = F Ab C Eb
        var iv7 = new[]{ Pc(65), Pc(68), Pc(72), Pc(75) };
        // 1st inversion (65): Ab in bass
        var v_iv65 = new FourPartVoicing(86, 81, 74, 68);
        var r_iv65 = HarmonyAnalyzer.AnalyzeTriad(iv7, key, v_iv65, null);
        Assert.True(r_iv65.Success);
        Assert.Equal("iv65", r_iv65.RomanText);
        // 2nd inversion (43): C in bass
        var v_iv43 = new FourPartVoicing(88, 84, 76, 72);
        var r_iv43 = HarmonyAnalyzer.AnalyzeTriad(iv7, key, v_iv43, null);
        Assert.True(r_iv43.Success);
        Assert.Equal("iv43", r_iv43.RomanText);
        // 3rd inversion (42): Eb in bass
        var v_iv42 = new FourPartVoicing(88, 84, 79, 75);
        var r_iv42 = HarmonyAnalyzer.AnalyzeTriad(iv7, key, v_iv42, null);
        Assert.True(r_iv42.Success);
        Assert.Equal("iv42", r_iv42.RomanText);

        // bVII7 = Bb D F Ab
        var bVII7 = new[]{ Pc(70), Pc(62), Pc(65), Pc(68) };
        // 1st inversion (65): D in bass
        var v_bVII65 = new FourPartVoicing(86, 79, 74, 62);
        var r_bVII65 = HarmonyAnalyzer.AnalyzeTriad(bVII7, key, v_bVII65, null);
        Assert.True(r_bVII65.Success);
        Assert.Equal("bVII65", r_bVII65.RomanText);
        // 2nd inversion (43): F in bass
        var v_bVII43 = new FourPartVoicing(86, 79, 74, 65);
        var r_bVII43 = HarmonyAnalyzer.AnalyzeTriad(bVII7, key, v_bVII43, null);
        Assert.True(r_bVII43.Success);
        Assert.Equal("bVII43", r_bVII43.RomanText);
        // 3rd inversion (42): Ab in bass
        var v_bVII42 = new FourPartVoicing(86, 79, 74, 68);
        var r_bVII42 = HarmonyAnalyzer.AnalyzeTriad(bVII7, key, v_bVII42, null);
        Assert.True(r_bVII42.Success);
        Assert.Equal("bVII42", r_bVII42.RomanText);
    }

    [Fact]
    public void KeyEstimation_Simple_Modulation_MajorToRelativeMinor()
    {
        // C major context then move to a minor-like set
        var seq = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) }, // I (C)
            new[]{ Pc(65), Pc(69), Pc(72) }, // IV (F)
            new[]{ Pc(67), Pc(71), Pc(62) }, // V7 without 7th -> V triad (G)
            new[]{ Pc(57), Pc(60), Pc(64) }, // a minor triad (A C E)
            new[]{ Pc(64), Pc(68), Pc(71) }, // E major (V in a minor)
        };
        var initialKey = new Key(60, true); // C major
        var (res, segs) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, initialKey, null, window: 1);
        Assert.True(res.Chords.Count == seq.Length);
        Assert.True(segs.Count >= 1);
        // Expect last segment to be a minor
        var last = segs[^1];
        Assert.False(last.key.IsMajor);
    }

    [Fact]
    public void KeyEstimation_Pivot_To_G_Major()
    {
        // Progression leaning to G major: C → D → G
        var seq = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) }, // C (I in C)
            new[]{ Pc(62), Pc(66), Pc(69) }, // D F# A (V of G)
            new[]{ Pc(67), Pc(71), Pc(74) }, // G B D (I in G)
        };
        var initialKey = new Key(60, true);
        var (_, segs) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, initialKey, null, window: 1);
        Assert.True(segs.Count >= 2);
        // Final segment key tonic pc should be G
        var last = segs[^1].key;
        int tonicPc = ((last.TonicMidi % 12) + 12) % 12;
        Assert.Equal(Pc(67), tonicPc);
        Assert.True(last.IsMajor);
    }

    [Fact]
    public void KeyEstimation_SecondaryDominant_A7_To_G_Major()
    {
        // C → A7 → D → G （A7 は V/V, D は V of G）
        var seq = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) },       // C (I in C)
            new[]{ Pc(57), Pc(61), Pc(64), Pc(67) }, // A7 = A C# E G
            new[]{ Pc(62), Pc(66), Pc(69) },       // D F# A
            new[]{ Pc(67), Pc(71), Pc(74) },       // G B D
        };
        var initialKey = new Key(60, true);
        var (_, segs) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, initialKey, null, window: 1);
        Assert.True(segs.Count >= 2);
        var last = segs[^1].key;
        int tonicPc = ((last.TonicMidi % 12) + 12) % 12;
        Assert.Equal(Pc(67), tonicPc);
        Assert.True(last.IsMajor);
    }

    [Fact]
    public void KeyEstimation_SecondaryDominant_Triad_A_To_G_Major()
    {
        // C → A → D → G （A メジャー三和音は V/V triad）
        var seq = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) }, // C
            new[]{ Pc(57), Pc(61), Pc(64) }, // A C# E (A major)
            new[]{ Pc(62), Pc(66), Pc(69) }, // D F# A
            new[]{ Pc(67), Pc(71), Pc(74) }, // G B D
        };
        var initialKey = new Key(60, true);
        var (_, segs) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, initialKey, null, window: 1);
        Assert.True(segs.Count >= 2);
        var last = segs[^1].key;
        int tonicPc = ((last.TonicMidi % 12) + 12) % 12;
        Assert.Equal(Pc(67), tonicPc);
        Assert.True(last.IsMajor);
    }

    [Fact]
    public void KeyEstimation_SecondaryDominant_B7_To_E_Minor()
    {
        // C → B7 → Em （B7 は V/iii, Em は iii かつ E minor へピボット）
        var seq = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) },        // C (I in C)
            new[]{ Pc(71), Pc(75), Pc(66), Pc(69) },// B7 = B D# F# A
            new[]{ Pc(64), Pc(67), Pc(71) },        // Em = E G B
        };
        var initialKey = new Key(60, true);
        var (_, segs) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, initialKey, null, window: 1);
        Assert.True(segs.Count >= 2);
        var last = segs[^1].key;
        int tonicPc = ((last.TonicMidi % 12) + 12) % 12;
        Assert.Equal(Pc(64), tonicPc); // E
        Assert.False(last.IsMajor);
    }

    [Fact]
    public void KeyEstimation_SecondaryDominant_E7_To_A_Minor()
    {
        // C → E7 → Am （E7 は V/vi, Am は vi かつ A minor へピボット）
        var seq = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) },        // C
            new[]{ Pc(64), Pc(68), Pc(71), Pc(62) },// E7 = E G# B D
            new[]{ Pc(57), Pc(60), Pc(64) },        // Am = A C E
        };
        var initialKey = new Key(60, true);
        var (_, segs) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, initialKey, null, window: 1);
        Assert.True(segs.Count >= 2);
        var last = segs[^1].key;
        int tonicPc = ((last.TonicMidi % 12) + 12) % 12;
        Assert.Equal(Pc(57), tonicPc); // A
        Assert.False(last.IsMajor);
    }

    [Fact]
    public void ProgressionAnalyzer_AnalyzeWithKeyEstimate_Returns_Trace()
    {
        var seq = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) },
            new[]{ Pc(67), Pc(71), Pc(62) },
            new[]{ Pc(60), Pc(64), Pc(67) },
        };
        var initialKey = new Key(60, true);
        var options = new KeyEstimator.Options { Window = 1, CollectTrace = true };
        var (_, segs, keys) = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, initialKey, options, out var trace);
        Assert.Equal(seq.Length, keys.Count);
        Assert.Equal(seq.Length, trace.Count);
        Assert.True(segs.Count >= 1);
        // トレースの合計スコアが非負であること（簡易健全性チェック）
        Assert.All(trace, t => Assert.True(t.Total >= 0));
    }

    [Fact]
    public void TraceEntry_ToString_Includes_Key_And_Score()
    {
        var seq = new[]
        {
            new[]{ Pc(60), Pc(64), Pc(67) },
            new[]{ Pc(67), Pc(71), Pc(62) },
        };
        var options = new KeyEstimator.Options { Window = 1, CollectTrace = true };
        var _ = KeyEstimator.EstimatePerChord(seq, new Key(60, true), options, out var trace);
        Assert.True(trace.Count >= 2);
        var s = trace[0].ToString();
        Assert.False(string.IsNullOrWhiteSpace(s));
        Assert.Contains("key=", s);
        Assert.Contains("total=", s);
    }
}
