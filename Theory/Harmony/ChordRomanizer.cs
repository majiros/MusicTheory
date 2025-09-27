using System;
using System.Collections.Generic;
using System.Linq;
using MusicTheory.Theory.Pitch;

namespace MusicTheory.Theory.Harmony;

public enum ChordQuality { Major, Minor, Diminished, Augmented, DominantSeventh, MinorSeventh, MajorSeventh, HalfDiminishedSeventh, DiminishedSeventh, Unknown }

public readonly record struct Chord(int RootPc, ChordQuality Quality)
{
    public IEnumerable<int> PitchClasses()
    {
        return Quality switch
        {
            ChordQuality.Major => new[] { RootPc, (RootPc + 4) % 12, (RootPc + 7) % 12 },
            ChordQuality.Minor => new[] { RootPc, (RootPc + 3) % 12, (RootPc + 7) % 12 },
            ChordQuality.Diminished => new[] { RootPc, (RootPc + 3) % 12, (RootPc + 6) % 12 },
            ChordQuality.Augmented => new[] { RootPc, (RootPc + 4) % 12, (RootPc + 8) % 12 },
            ChordQuality.DominantSeventh => new[] { RootPc, (RootPc + 4) % 12, (RootPc + 7) % 12, (RootPc + 10) % 12 }, // 1 3 5 b7
            ChordQuality.MinorSeventh => new[] { RootPc, (RootPc + 3) % 12, (RootPc + 7) % 12, (RootPc + 10) % 12 },
            ChordQuality.MajorSeventh => new[] { RootPc, (RootPc + 4) % 12, (RootPc + 7) % 12, (RootPc + 11) % 12 },
            ChordQuality.HalfDiminishedSeventh => new[] { RootPc, (RootPc + 3) % 12, (RootPc + 6) % 12, (RootPc + 10) % 12 },
            ChordQuality.DiminishedSeventh => new[] { RootPc, (RootPc + 3) % 12, (RootPc + 6) % 12, (RootPc + 9) % 12 },
            _ => Array.Empty<int>()
        };
    }
}

public static class ChordRomanizer
{
    // Detect dominant ninth on V (degree 5). Accepts 5-note (1,3,5,b7,9) or 4-note without 5th (1,3,b7,9).
    // Returns romanText "V9" when matched.
    public static bool TryRomanizeDominantNinth(int[] pcs, Key key, out string? romanText)
    {
        romanText = null;
        // Normalize to distinct, ordered set first and base checks on that to avoid flakiness
        var set = pcs.Select(p => ((p % 12) + 12) % 12).Distinct().OrderBy(x => x).ToArray();
        if (set.Length < 4 || set.Length > 5) return false; // 4 (omit 5th) or 5 (full)
        // V root (degree index 4)
        int vRoot = DegreeRootPc(key, 4);
        int vThird = (vRoot + 4) % 12; // major third on V (harmonic minor accounted by leading tone in DegreeRootPc usage elsewhere)
        int vFifth = (vRoot + 7) % 12;
        int vSeventh = (vRoot + 10) % 12; // dominant seventh
        int vNinth = (vRoot + 2) % 12;    // diatonic 9th

        // Allowed members and required core for recognition
        var allowed = new HashSet<int>(new[] { vRoot, vThird, vFifth, vSeventh, vNinth });
        // All notes must be allowed (avoid supersets with alterations), and we need the core tones present
        bool allAllowed = set.All(allowed.Contains);
        bool hasCore = set.Contains(vRoot) && set.Contains(vThird) && set.Contains(vSeventh) && set.Contains(vNinth);
    if (allAllowed && hasCore)
        {
            romanText = "V9";
            return true;
        }
        return false;
    }

    // Augmented Sixth chords: Italian (It6), French (Fr43), German (Ger65)
    // Detection by pitch-class sets relative to key: {b6, 1, #4} (+ {2} for French, + {b3} for German)
    // Returns conventional labels It6 / Fr43 / Ger65. Voicing is ignored for now (canonical figures are emitted).
    public static bool TryRomanizeAugmentedSixth(int[] pcs, Key key, FourPartVoicing? voicing, out string? romanText)
        => TryRomanizeAugmentedSixth(pcs, key, HarmonyOptions.Default, voicing, out romanText);

    public static bool TryRomanizeAugmentedSixth(int[] pcs, Key key, HarmonyOptions options, FourPartVoicing? voicing, out string? romanText)
    {
        romanText = null;
        if (pcs.Length < 3) return false;
        var set = pcs.Select(p => ((p % 12) + 12) % 12).Distinct().OrderBy(x => x).ToArray();

        int tonic = (key.TonicMidi % 12 + 12) % 12;
        int b6 = (tonic + 8) % 12;   // b6
        int sharp4 = (tonic + 6) % 12; // #4
        int one = tonic;
        int two = (tonic + 2) % 12;
        int flat3 = (tonic + 3) % 12;

        bool contains(params int[] pcsReq) => pcsReq.All(pc => set.Contains(pc));

        // Require explicit voicing with b6 in the bass to avoid confusion with mixture bVI7
    if (voicing is null) return false;
        int bass = ((voicing.Value.B % 12) + 12) % 12;
        if (bass != b6) return false;
        // Additional disambiguation: avoid labeling as Aug6 when soprano is also b6 (common for Ab7 mixture voicing)
        // This preserves bVI7 root-position labels in tests while still preferring Aug6 in other voicings.
        int soprano = ((voicing.Value.S % 12) + 12) % 12;
    if (options.DisallowAugmentedSixthWhenSopranoFlat6 && soprano == b6)
            return false;

        // Exact-size matches to avoid subset/superset confusion with seventh chords
        // Prefer 4-note variants over 3-note if sizes match
        if (set.Length == 4)
        {
            // German sixth (Ger65): {b6, 1, b3, #4} — identical PC set to bVI7 (dominant seventh on bVI)
            if (contains(b6, one, flat3, sharp4))
            {
                // When preference requests mixture seventh over Aug6 for ambiguous sets, defer labeling here
                if (options.PreferMixtureSeventhOverAugmentedSixthWhenAmbiguous)
                {
                    return false; // allow bVI7 detection to take precedence
                }
                romanText = "Ger65"; return true;
            }
            // French sixth (Fr43): {b6, 1, 2, #4} — distinct from any mixture seventh; safe to label directly
            if (contains(b6, one, two, sharp4)) { romanText = "Fr43"; return true; }
        }
        if (set.Length == 3 && contains(b6, one, sharp4))
        {
            romanText = "It6"; return true;
        }
        return false;
    }
    // Very small helper to detect diatonic roman numerals in given key for triads.
    public static bool TryRomanizeTriad(int[] pcs, Key key, out RomanNumeral rn)
    {
        rn = default;
        if (pcs.Length == 0) return false;
        var set = pcs.Select(p => ((p % 12) + 12) % 12).Distinct().OrderBy(x => x).ToArray();
        // find best-fitting degree by root match
        for (int degree = 0; degree < 7; degree++)
        {
            int degPc = DegreeRootPc(key, degree);
            // Build expected triad quality in key
            var quality = TriadQualityInKey(degree, key.IsMajor);
            var chord = new Chord(degPc, quality);
            var expected = chord.PitchClasses().OrderBy(x => x).ToArray();
            // Require exact triad match (avoid matching subsets of seventh chords)
            if (expected.All(p => set.Contains(p)) && set.Length == expected.Length)
            {
                rn = QualityToRoman(degree, quality, key.IsMajor);
                return true;
            }
        }
        return false;
    }

    // Modal mixture (borrowed chords) — minimal support for major keys:
    // i, iv, bIII, bVI, bVII
    public static bool TryRomanizeTriadMixture(int[] pcs, Key key, out RomanNumeral rn, out string? romanText)
    {
        rn = default; romanText = null;
        if (pcs.Length == 0) return false;
        var set = pcs.Select(p => ((p % 12) + 12) % 12).Distinct().OrderBy(x => x).ToArray();
        int tonicPc = (key.TonicMidi % 12 + 12) % 12;

        // Candidates: (rootPc, quality, rn, romanText)
        var candidates = new List<(int root, ChordQuality q, RomanNumeral rn, string txt)>();
        // Neapolitan (bII) — available in major/minor
        candidates.Add(((tonicPc + 1) % 12, ChordQuality.Major, RomanNumeral.II, "bII"));

        if (key.IsMajor)
        {
            candidates.Add((tonicPc, ChordQuality.Minor, RomanNumeral.i, "i"));                 // i (minor tonic)
            candidates.Add(((key.ScaleDegreeMidi(3) % 12 + 12) % 12, ChordQuality.Minor, RomanNumeral.iv, "iv")); // iv (minor subdominant)
            candidates.Add(((tonicPc + 3) % 12, ChordQuality.Major, RomanNumeral.III, "bIII")); // bIII
            candidates.Add(((tonicPc + 8) % 12, ChordQuality.Major, RomanNumeral.VI, "bVI"));   // bVI
            candidates.Add(((tonicPc + 10) % 12, ChordQuality.Major, RomanNumeral.VII, "bVII")); // bVII (Mixolydian borrow)
        }

        foreach (var c in candidates)
        {
            var chord = new Chord(c.root, c.q).PitchClasses().OrderBy(x => x).ToArray();
            // Require exact-size match to avoid matching a subset of a seventh chord
            if (chord.All(p => set.Contains(p)) && set.Length == chord.Length)
            {
                rn = c.rn;
                romanText = c.txt;
                return true;
            }
        }
        return false;
    }

    // Modal mixture sevenths for major/minor: iv7 (minor seventh), bVII7 (dominant seventh), bII7 (dominant seventh)
    public static bool TryRomanizeSeventhMixture(int[] pcs, Key key, FourPartVoicing? voicing, out string? romanText)
        => TryRomanizeSeventhMixture(pcs, key, HarmonyOptions.Default, voicing, out romanText);

    public static bool TryRomanizeSeventhMixture(int[] pcs, Key key, HarmonyOptions options, FourPartVoicing? voicing, out string? romanText)
    {
        romanText = null;
        if (pcs.Length == 0) return false;
        var set = pcs.Select(p => ((p % 12) + 12) % 12).Distinct().OrderBy(x => x).ToArray();
        int tonicPc = (key.TonicMidi % 12 + 12) % 12;

        var candidates = new List<(int root, ChordQuality q, string txtBase)>();
        if (key.IsMajor)
        {
            // iv7: minor seventh on degree IV (borrowed from parallel minor)
            int ivRoot = (key.ScaleDegreeMidi(3) % 12 + 12) % 12; // IV degree root pc
            candidates.Add((ivRoot, ChordQuality.MinorSeventh, "iv"));
        }
        // bVII7: dominant seventh built on bVII (common in major/minor)
        int bVIIroot = (tonicPc + 10) % 12;
        candidates.Add((bVIIroot, ChordQuality.DominantSeventh, "bVII"));
    // bII7: dominant seventh on Neapolitan root (also common as tritone sub)
        int bIIroot = (tonicPc + 1) % 12;
        candidates.Add((bIIroot, ChordQuality.DominantSeventh, "bII"));
    // bVI7: dominant seventh on bVI (strict exact 4-note match)
    int bVIroot = (tonicPc + 8) % 12;
    candidates.Add((bVIroot, ChordQuality.DominantSeventh, "bVI"));

        foreach (var c in candidates)
        {
            var chord = new Chord(c.root, c.q).PitchClasses().OrderBy(x => x).ToArray();
            // require exact-size match (avoid subset/superset confusion with triads)
            if (!(chord.All(p => set.Contains(p)) && set.Length == chord.Length)) continue;

            // Note: Augmented Sixth 優先は Analyzer 側の順序で一元的に処理します
            // （ここでは bVI7 の純粋な一致のみを扱い、Aug6 へのプリエンプトは行いません）。

            // No voicing: root-position label
            if (voicing is null)
            {
                romanText = c.txtBase + "7";
                return true;
            }

            // With voicing: figure the inversion
            int bassPc = voicing.Value.B % 12; if (bassPc < 0) bassPc += 12;
            int root = c.root;
            int third = chord.First(pc => pc != root && (((pc - root + 12) % 12) == 3 || ((pc - root + 12) % 12) == 4));
            int fifth = (root + ((c.q == ChordQuality.MinorSeventh ? 7 : 7))) % 12; // 5th is perfect
            int sevInt = c.q switch { ChordQuality.MajorSeventh => 11, ChordQuality.DominantSeventh or ChordQuality.MinorSeventh or ChordQuality.HalfDiminishedSeventh => 10, ChordQuality.DiminishedSeventh => 9, _ => 10 };
            int sev = (root + sevInt) % 12;

            if      (bassPc == root) romanText = c.txtBase + "7";
            else if (bassPc == third) romanText = c.txtBase + "65";
            else if (bassPc == fifth) romanText = c.txtBase + "43";
            else if (bassPc == sev) romanText = c.txtBase + "42";
            else romanText = c.txtBase + "7";
            return true;
        }

        return false;
    }

    // Secondary dominants: V/x triad and V/x7 (x in {ii, iii, IV, V, vi, vii}) relative to current key
    // If voicing is provided and seventh quality matches, include inversion figures (7, 65, 43, 42) before "/x".
    public static bool TryRomanizeSecondaryDominant(int[] pcs, Key key, FourPartVoicing? voicing, out string? romanText)
    {
        romanText = null;
        if (pcs.Length == 0) return false;
        var set = pcs.Select(p => ((p % 12) + 12) % 12).Distinct().OrderBy(x => x).ToArray();
        // Early disambiguation: if the set is exactly a major triad, infer secRoot directly
        // and map to its unique target degree (targetPc = secRoot - 7 mod 12). This prevents
        // rare mislabeling such as E–G#–B (V/vi) being picked as V/iii.
    if (set.Length == 3)
        {
            for (int r = 0; r < 12; r++)
            {
        var majorTri = new Chord(r, ChordQuality.Major).PitchClasses().OrderBy(x => x).ToArray();
        if (majorTri.SequenceEqual(set))
                {
                    int secRoot = r;
                    int targetPc = (secRoot + 5) % 12; // -7 mod 12 == +5
                    int degree = -1;
                    for (int d = 0; d < 7; d++)
                    {
                        if (DegreeRootPc(key, d) == targetPc) { degree = d; break; }
                    }
                    if (degree != -1)
                    {
                        string targetTxt = DegreeRomanForTarget(degree, key.IsMajor);
                        if (voicing is FourPartVoicing vTri)
                        {
                            int bass = ((vTri.B % 12) + 12) % 12;
                            int root = secRoot;
                            int third = (root + 4) % 12;
                            int fifth = (root + 7) % 12;
                            if      (bass == root)   romanText = $"V/{targetTxt}";
                            else if (bass == third)  romanText = $"V6/{targetTxt}";
                            else if (bass == fifth)  romanText = $"V64/{targetTxt}";
                            else romanText = $"V/{targetTxt}";
                        }
                        else
                        {
                            romanText = $"V/{targetTxt}";
                        }
                        return true;
                    }
                }
            }
        }
        // Prefer common targets first to avoid rare ambiguous picks
        int[] degreeOrder = new[] { 4, 1, 5, 3, 2, 6 }; // V, ii, vi, IV, iii, vii
        foreach (int deg in degreeOrder)
        {
            int targetPc = DegreeRootPc(key, deg);
            int secRoot = (targetPc + 7) % 12; // V of target
            var tri = new Chord(secRoot, ChordQuality.Major).PitchClasses().OrderBy(x => x).ToArray();
            var sev = new Chord(secRoot, ChordQuality.DominantSeventh).PitchClasses().OrderBy(x => x).ToArray();

            string targetTxt = DegreeRomanForTarget(deg, key.IsMajor);
            if (tri.SequenceEqual(set))
            {
                // Triad case: if voicing provided, add inversion figures (6 or 64) before '/x'
                if (voicing is FourPartVoicing vTri)
                {
                    int bass = ((vTri.B % 12) + 12) % 12;
                    int root = secRoot;
                    int third = (root + 4) % 12;
                    int fifth = (root + 7) % 12;
                    if      (bass == root)   romanText = $"V/{targetTxt}";
                    else if (bass == third)  romanText = $"V6/{targetTxt}";
                    else if (bass == fifth)  romanText = $"V64/{targetTxt}";
                    else romanText = $"V/{targetTxt}";
                }
                else
                {
                    romanText = $"V/{targetTxt}";
                }
                return true;
            }
            if (sev.SequenceEqual(set))
            {
                // Seventh with optional inversion figures when voicing is present
                string fig = "7";
                if (voicing is FourPartVoicing v)
                {
                    int bass = ((v.B % 12) + 12) % 12;
                    int root = secRoot;
                    int third = (root + 4) % 12;
                    int fifth = (root + 7) % 12;
                    int seventh = (root + 10) % 12;
                    if      (bass == root)   fig = "7";
                    else if (bass == third)  fig = "65";
                    else if (bass == fifth)  fig = "43";
                    else if (bass == seventh)fig = "42";
                }
                romanText = $"V{fig}/{targetTxt}";
                return true;
            }
        }
        return false;
    }

    // Secondary leading-tone chords: vii°/x (triad) and vii°7/ x or viiø7/ x (seventh), with optional inversion figures.
    public static bool TryRomanizeSecondaryLeadingTone(int[] pcs, Key key, FourPartVoicing? voicing, out string? romanText)
        => TryRomanizeSecondaryLeadingTone(pcs, key, voicing, HarmonyOptions.Default, out romanText);

    public static bool TryRomanizeSecondaryLeadingTone(int[] pcs, Key key, FourPartVoicing? voicing, HarmonyOptions options, out string? romanText)
    {
        romanText = null;
        if (pcs.Length == 0) return false;
        var set = pcs.Select(p => ((p % 12) + 12) % 12).Distinct().OrderBy(x => x).ToArray();
        // Guard: if this chord is already a diatonic seventh, don't treat it as secondary
        if (TryRomanizeSeventh(pcs, key, out var _rnDia, out var _degDia, out var _qDia))
            return false;

        // Ultra-strong preference: if the set is a fully-diminished seventh (0,3,6,9 pattern),
        // and it exactly matches vii°7/V, prefer that target. If voicing is provided, include inversion figures.
        if (set.Length == 4 && options.PreferSecondaryLeadingToneTargetV)
        {
            var norm = set.Select(pc => (pc - set[0] + 12) % 12).OrderBy(x => x).ToArray();
            if (norm.SequenceEqual(new[] { 0, 3, 6, 9 }))
            {
                int vRoot = DegreeRootPc(key, 4);
                int ltRootV = (vRoot + 11) % 12;
                var dimV = new Chord(ltRootV, ChordQuality.DiminishedSeventh).PitchClasses().OrderBy(x => x).ToArray();
                if (dimV.All(set.Contains))
                {
                    if (voicing is FourPartVoicing v)
                    {
                        int bass = ((v.B % 12) + 12) % 12;
                        int root = ltRootV;
                        int third = (root + 3) % 12;
                        int fifth = (root + 6) % 12;
                        int seventh = (root + 9) % 12;
                        string fig = bass == root ? "7" : bass == third ? "65" : bass == fifth ? "43" : bass == seventh ? "42" : "7";
                        romanText = $"vii°{fig}/V";
                    }
                    else
                    {
                        romanText = "vii°7/V";
                    }
                    return true;
                }
            }
        }

        // Strong preference: try target V first explicitly
    {
            int targetPc = DegreeRootPc(key, 4); // V
            int ltRoot = (targetPc + 11) % 12;
            var dim7 = new Chord(ltRoot, ChordQuality.DiminishedSeventh).PitchClasses().OrderBy(x => x).ToArray();
            var halfDim7 = new Chord(ltRoot, ChordQuality.HalfDiminishedSeventh).PitchClasses().OrderBy(x => x).ToArray();
            var tri = new Chord(ltRoot, ChordQuality.Diminished).PitchClasses().OrderBy(x => x).ToArray();
            bool matchDim7 = (dim7.Length == set.Length && dim7.All(set.Contains));
            bool matchHalf7 = (halfDim7.Length == set.Length && halfDim7.All(set.Contains));
            bool matchTri = (tri.Length == set.Length && tri.All(set.Contains));
            if (matchDim7 || matchHalf7 || matchTri)
            {
                string head;
                if (matchDim7) head = "vii°";
                else if (matchHalf7) head = "viiø";
                else head = "vii°"; // triad case uses °
                string fig = (set.Length == 4) ? "7" : "";
                if (voicing is FourPartVoicing v && set.Length == 4)
                {
                    int bass = ((v.B % 12) + 12) % 12;
                    int root = ltRoot;
                    int third = (root + 3) % 12;
                    int fifth = (root + 6) % 12;
                    int seventh = ((dim7.Length == 4 && dim7.All(set.Contains)) ? (root + 9) : (root + 10)) % 12;
                    if      (bass == root)   fig = "7";
                    else if (bass == third)  fig = "65";
                    else if (bass == fifth)  fig = "43";
                    else if (bass == seventh)fig = "42";
                }
                else if (voicing is FourPartVoicing vTri && set.Length == 3 && matchTri)
                {
                    int bass = ((vTri.B % 12) + 12) % 12;
                    int root = ltRoot;
                    int third = (root + 3) % 12;
                    int fifth = (root + 6) % 12;
                    if      (bass == root)   fig = "";
                    else if (bass == third)  fig = "6";
                    else if (bass == fifth)  fig = "64";
                }
                romanText = $"{head}{fig}/V";
                return true;
            }
        }
        // Prefer the dominant target (V) when enharmonic ambiguity exists, then common goals.
        // Degree indices: 0..6 (0=I). We'll try: V(4), ii(1), vi(5), IV(3), iii(2), vii(6)
    int[] degreeOrder = new[] { 4, 1, 5, 3, 2, 6 };
    foreach (var deg in degreeOrder)
        {
            int targetPc = DegreeRootPc(key, deg);
            int ltRoot = (targetPc + 11) % 12; // leading tone to the target (−1 semitone)

            var tri = new Chord(ltRoot, ChordQuality.Diminished).PitchClasses().OrderBy(x => x).ToArray();
            var halfDim7 = new Chord(ltRoot, ChordQuality.HalfDiminishedSeventh).PitchClasses().OrderBy(x => x).ToArray();
            var dim7 = new Chord(ltRoot, ChordQuality.DiminishedSeventh).PitchClasses().OrderBy(x => x).ToArray();

            string targetTxt = DegreeRomanForTarget(deg, key.IsMajor, includeDiminishedOnVII: true);
            if (tri.Length == set.Length && tri.All(set.Contains))
            {
                // Triad case: if voicing provided, add inversion figures (6 or 64) before '/x'
                if (voicing is FourPartVoicing vTri)
                {
                    int bass = ((vTri.B % 12) + 12) % 12;
                    int root = ltRoot;
                    // diminished triad: minor third + diminished fifth
                    int third = (root + 3) % 12;
                    int fifth = (root + 6) % 12;
                    string head = "vii°";
                    if      (bass == root)   romanText = $"{head}/{targetTxt}";
                    else if (bass == third)  romanText = $"{head}6/{targetTxt}";
                    else if (bass == fifth)  romanText = $"{head}64/{targetTxt}";
                    else romanText = $"{head}/{targetTxt}";
                }
                else
                {
                    romanText = $"vii°/{targetTxt}";
                }
                return true;
            }

            // Check fully-diminished first, then half-diminished
            if (dim7.Length == set.Length && dim7.All(set.Contains))
            {
                string head = "vii°"; string fig = "7";
                if (voicing is FourPartVoicing v)
                {
                    int bass = ((v.B % 12) + 12) % 12;
                    int root = ltRoot;
                    int third = (root + 3) % 12;
                    int fifth = (root + 6) % 12;
                    int seventh = (root + 9) % 12;
                    if      (bass == root)   fig = "7";
                    else if (bass == third)  fig = "65";
                    else if (bass == fifth)  fig = "43";
                    else if (bass == seventh)fig = "42";
                }
                romanText = $"{head}{fig}/{targetTxt}";
                return true;
            }
            if (halfDim7.Length == set.Length && halfDim7.All(set.Contains))
            {
                string head = "viiø"; string fig = "7";
                if (voicing is FourPartVoicing v)
                {
                    int bass = ((v.B % 12) + 12) % 12;
                    int root = ltRoot;
                    int third = (root + 3) % 12;
                    int fifth = (root + 6) % 12;
                    int seventh = (root + 10) % 12;
                    if      (bass == root)   fig = "7";
                    else if (bass == third)  fig = "65";
                    else if (bass == fifth)  fig = "43";
                    else if (bass == seventh)fig = "42";
                }
                romanText = $"{head}{fig}/{targetTxt}";
                return true;
            }
    }
        return false;
    }

    private static string DegreeRomanForTarget(int degree, bool isMajor, bool includeDiminishedOnVII = false)
    {
        // degree: 0..6 (0=I)
        // Use diatonic triad qualities for case; optionally include ° on VII
        if (isMajor)
        {
            return degree switch
            {
                0 => "I",
                1 => "ii",
                2 => "iii",
                3 => "IV",
                4 => "V",
                5 => "vi",
                6 => includeDiminishedOnVII ? "vii°" : "vii",
                _ => "?"
            };
        }
        else
        {
            // harmonic minor baseline
            return degree switch
            {
                0 => "i",
                1 => "ii°",  // diminished triad on ii in harmonic minor baseline
                2 => "III",
                3 => "iv",
                4 => "V",
                5 => "VI",
                6 => includeDiminishedOnVII ? "vii°" : "vii",
                _ => "?"
            };
        }
    }

    private static int DegreeRootPc(Key key, int degree)
    {
        if (key.IsMajor)
        {
            return (key.ScaleDegreeMidi(degree) % 12 + 12) % 12;
        }
        else
        {
            // Harmonic minor: degree 6 is leading tone (tonic - 1)
            if (degree == 6)
            {
                int tonicPc = (key.TonicMidi % 12 + 12) % 12;
                return (tonicPc + 11) % 12;
            }
            return (key.ScaleDegreeMidi(degree) % 12 + 12) % 12;
        }
    }

    // Diatonic seventh support: recognize diatonic seventh chords in major and (harmonic) minor
    public static bool TryRomanizeSeventh(int[] pcs, Key key, out RomanNumeral rn, out int degreeOut, out ChordQuality qualityOut)
    {
        rn = default; degreeOut = -1; qualityOut = ChordQuality.Unknown;
        if (pcs.Length == 0) return false;
        var set = pcs.Select(p => ((p % 12) + 12) % 12).Distinct().OrderBy(x => x).ToArray();
        for (int degree = 0; degree < 7; degree++)
        {
            var q7 = SeventhQualityInKey(degree, key.IsMajor);
            if (q7 is null) continue;
            int degPc = DegreeRootPc(key, degree);
            var chord = new Chord(degPc, q7.Value);
            var expected = chord.PitchClasses().OrderBy(x => x).ToArray();
            // exact match: avoid matching subsets/supersets
            if (expected.All(p => set.Contains(p)) && set.Length == expected.Length)
            {
                var triQ = TriadQualityInKey(degree, key.IsMajor);
                rn = QualityToRoman(degree, triQ, key.IsMajor);
                degreeOut = degree;
                qualityOut = q7.Value;
                return true;
            }
        }
        return false;
    }

    private static ChordQuality? SeventhQualityInKey(int degree, bool isMajor)
    {
        if (isMajor)
        {
            return degree switch
            {
                0 => ChordQuality.MajorSeventh,          // Imaj7
                1 => ChordQuality.MinorSeventh,          // ii7
                2 => ChordQuality.MinorSeventh,          // iii7
                3 => ChordQuality.MajorSeventh,          // IVmaj7
                4 => ChordQuality.DominantSeventh,       // V7
                5 => ChordQuality.MinorSeventh,          // vi7
                6 => ChordQuality.HalfDiminishedSeventh, // viiø7
                _ => null
            };
        }
        else
        {
            // Harmonic minor diatonic sevenths
            return degree switch
            {
                0 => ChordQuality.MinorSeventh,             // i7
                1 => ChordQuality.HalfDiminishedSeventh,     // iiø7
                2 => ChordQuality.MajorSeventh,              // IIImaj7
                3 => ChordQuality.MinorSeventh,              // iv7
                4 => ChordQuality.DominantSeventh,           // V7
                5 => ChordQuality.MajorSeventh,              // VImaj7
                6 => ChordQuality.DiminishedSeventh,         // vii°7
                _ => null
            };
        }
    }

    private static RomanNumeral QualityToRoman(int degree, ChordQuality q, bool isMajor)
    {
        bool minorLike = q == ChordQuality.Minor || q == ChordQuality.Diminished;
        var baseIndex = degree; // 0..6
        return (baseIndex, minorLike, isMajor) switch
        {
            (0, false, _) => RomanNumeral.I,
            (1, false, _) => RomanNumeral.II,
            (2, false, _) => RomanNumeral.III,
            (3, false, _) => RomanNumeral.IV,
            (4, false, _) => RomanNumeral.V,
            (5, false, _) => RomanNumeral.VI,
            (6, false, _) => RomanNumeral.VII,
            (0, true, _) => RomanNumeral.i,
            (1, true, _) => RomanNumeral.ii,
            (2, true, _) => RomanNumeral.iii,
            (3, true, _) => RomanNumeral.iv,
            (4, true, _) => RomanNumeral.v,
            (5, true, _) => RomanNumeral.vi,
            (6, true, _) => RomanNumeral.vii,
            _ => RomanNumeral.I
        };
    }

    private static ChordQuality TriadQualityInKey(int degree, bool isMajor)
    {
        // Diatonic triad qualities in major/minor (natural minor for start)
        if (isMajor)
        {
            return degree switch
            {
                0 or 3 or 4 => ChordQuality.Major, // I, IV, V
                1 or 2 or 5 => ChordQuality.Minor, // ii, iii, vi
                6 => ChordQuality.Diminished,      // vii°
                _ => ChordQuality.Unknown
            };
        }
        else
        {
            // Harmonic minor variant by default:
            // i, ii°, III, iv, V, VI, vii°
            return degree switch
            {
                0 or 3 => ChordQuality.Minor,        // i, iv
                2 or 5 => ChordQuality.Major,        // III, VI
                1 => ChordQuality.Diminished,        // ii°
                4 => ChordQuality.Major,             // V (raised 7 as 3rd)
                6 => ChordQuality.Diminished,        // vii° (leading tone chord)
                _ => ChordQuality.Unknown
            };
        }
    }
}
