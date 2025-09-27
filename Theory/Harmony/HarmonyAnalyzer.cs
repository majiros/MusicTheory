using System.Collections.Generic;
using System.Linq;

namespace MusicTheory.Theory.Harmony;

/// <summary>
/// Result of a single harmony analysis.
/// </summary>
/// <param name="Success">True if any roman numeral labeling was determined.</param>
/// <param name="Roman">Structured roman numeral head (nullable for special cases like Aug6 only text).</param>
/// <param name="Function">Approximate tonal function (Tonic/Subdominant/Dominant/Unknown).</param>
/// <param name="RomanText">Display text including accidentals and inversion figures (e.g., "bII6", "V65/V", "Ger65").</param>
/// <param name="Warnings">Non-blocking diagnostics (voice-leading hints, Neapolitan recommendations, etc.).</param>
/// <param name="Errors">Blocking or structural issues (range/order violations, overlaps, etc.).</param>
public readonly record struct HarmonyAnalysisResult(
    bool Success,
    RomanNumeral? Roman,
    TonalFunction Function,
    string? RomanText,
    List<string> Warnings,
    List<string> Errors
);

public static class HarmonyAnalyzer
{
    private static void AddMixtureSeventhWarnings(List<string> warnings, string mix7Label, Key key, FourPartVoicing? voicing)
    {
        try
        {
            if (string.IsNullOrEmpty(mix7Label)) return;
            // Generic, non-blocking hints about common resolutions
            // Check longer tokens first to avoid prefix collisions (e.g., bVII vs bVI)
            if (mix7Label.StartsWith("bVII", System.StringComparison.Ordinal))
            {
                warnings.Add("Mixture: bVII7 often resolves to I (backdoor)");
            }
            else if (mix7Label.StartsWith("bVI", System.StringComparison.Ordinal))
            {
                warnings.Add("Mixture: bVI7 typically resolves to V");
            }
            else if (mix7Label.StartsWith("bII", System.StringComparison.Ordinal))
            {
                warnings.Add("Mixture: bII7 (Neapolitan 7) often resolves to V or I6");
            }
            else if (mix7Label.StartsWith("iv", System.StringComparison.Ordinal))
            {
                warnings.Add("Mixture: iv7 typically resolves to V");
            }
        }
        catch { /* diagnostics must not throw */ }
    }
    /// <summary>
    /// Analyze a chord snapshot as triad/seventh harmony and return roman numeral labeling.
    /// </summary>
    /// <param name="pcs">All sounding pitch classes (MIDI modulo 12 recommended).</param>
    /// <param name="key">The harmonic context.</param>
    /// <param name="voicing">Optional four-part voicing for inversion and voice-leading diagnostics.</param>
    /// <param name="prev">Optional previous four-part voicing for motion diagnostics (parallels/overlaps).</param>
    /// <returns>HarmonyAnalysisResult with roman text, function and diagnostics.</returns>
    public static HarmonyAnalysisResult AnalyzeTriad(int[] pcs, Key key, FourPartVoicing? voicing = null, FourPartVoicing? prev = null)
        => AnalyzeTriad(pcs, key, HarmonyOptions.Default, voicing, prev);

    /// <summary>
    /// Analyze with custom <see cref="HarmonyOptions"/> for disambiguation preferences.
    /// </summary>
    /// <param name="pcs">All sounding pitch classes.</param>
    /// <param name="key">The harmonic context.</param>
    /// <param name="options">Preference toggles (Aug6 vs mixture, Neapolitan enforcement, etc.).</param>
    /// <param name="voicing">Optional four-part voicing.</param>
    /// <param name="prev">Optional previous four-part voicing for motion checks.</param>
    public static HarmonyAnalysisResult AnalyzeTriad(int[] pcs, Key key, HarmonyOptions options, FourPartVoicing? voicing = null, FourPartVoicing? prev = null)
    {
        var warnings = new List<string>();
        var errors = new List<string>();

        // Normalize inputs
        int[] raw = pcs.Select(p => ((p % 12) + 12) % 12).ToArray();
        int[] distinct = raw.Distinct().ToArray();
    int[] ordered = distinct.OrderBy(x => x).ToArray();
    bool preferSeventh = ordered.Length >= 4; // 4音以上は7th/テンション系を最優先（V9 等も含む）

        // 1) V9 は常に最優先で即時リターン（distinct/raw の両方で判定）
    if (ChordRomanizer.TryRomanizeDominantNinth(ordered, key, out var v9Early)
     || ChordRomanizer.TryRomanizeDominantNinth(raw, key, out v9Early))
        {
        var v9label = options is not null && options.PreferV7Paren9OverV9 ? "V7(9)" : v9Early;
        var funcV = RomanNumeralUtils.FunctionOf(RomanNumeral.V);
        return new HarmonyAnalysisResult(true, RomanNumeral.V, funcV, v9label, warnings, errors);
        }

    // (moved) Augmented Sixth detection occurs after mixture sevenths, to preserve bVI7 labeling unless bass=b6 voicing is present.

        // 2) ダイアトニック7th（4音）: ボイシングがあれば反転を付与（最優先で、曖昧な二次導七等より優先）
        //    ただし、voicingがあり bass=b6 の正規Augmented Sixth (It6/Fr43/Ger65) に一致する場合は、
        //    先にAug6を優先して即時リターン（まれな誤認を防ぐための早期チェック）。
        if (preferSeventh && voicing is FourPartVoicing vAugEarly && !options.PreferMixtureSeventhOverAugmentedSixthWhenAmbiguous)
        {
            int tonicPcE = (key.TonicMidi % 12 + 12) % 12;
            int b6pcE = (tonicPcE + 8) % 12;
            int sopranoPcE = (vAugEarly.S % 12 + 12) % 12;
            bool suppressAug6E = options.DisallowAugmentedSixthWhenSopranoFlat6 && sopranoPcE == b6pcE;
            if (!suppressAug6E && ChordRomanizer.TryRomanizeAugmentedSixth(pcs, key, options, vAugEarly, out var aug6EarlyPre))
            {
                return new HarmonyAnalysisResult(true, null, TonalFunction.Subdominant, aug6EarlyPre, warnings, errors);
            }
        }
            if (preferSeventh && ChordRomanizer.TryRomanizeSeventh(pcs, key, out var rn7, out var degree7, out var q7))
        {
            // Guard: if this voicing also forms an Augmented Sixth and suppression is not requested,
            // prefer Aug6 over a diatonic seventh label (safety for rare ambiguous edge cases).
            if (voicing is FourPartVoicing vAug2 && !options.PreferMixtureSeventhOverAugmentedSixthWhenAmbiguous)
            {
                int tonicPcG = (key.TonicMidi % 12 + 12) % 12;
                int b6pcG = (tonicPcG + 8) % 12;
                int sopranoPcG = (vAug2.S % 12 + 12) % 12;
                bool suppressAug6OnSop = options.DisallowAugmentedSixthWhenSopranoFlat6 && sopranoPcG == b6pcG;
                if (!suppressAug6OnSop && ChordRomanizer.TryRomanizeAugmentedSixth(pcs, key, options, vAug2, out var aug6OverDia))
                {
                    return new HarmonyAnalysisResult(true, null, TonalFunction.Subdominant, aug6OverDia, warnings, errors);
                }
            }
            string? label;
            if (voicing.HasValue)
            {
                var v7 = voicing.Value;
                label = BuildSeventhLabel(rn7, degree7, q7, key, v7, options);
            }
            else
            {
                label = rn7 + SeventhRootSuffix(q7);
                label = EnsureSeventhAccidental(rn7, q7, label);
            }
            var func7 = RomanNumeralUtils.FunctionOf(rn7);
            // Final sanitation: ensure °/ø prefix on diminished-type sevenths
            if ((q7 == ChordQuality.DiminishedSeventh || q7 == ChordQuality.HalfDiminishedSeventh) && label is string l0)
            {
                if (!l0.Contains('°') && !l0.Contains('ø'))
                {
                    string sym = q7 == ChordQuality.DiminishedSeventh ? "°" : "ø";
                    if (l0.StartsWith("vii")) l0 = "vii" + sym + l0.Substring(3);
                    else if (l0.StartsWith("VII")) l0 = "VII" + sym + l0.Substring(3);
                    label = l0;
                }
            }
            return new HarmonyAnalysisResult(true, rn7, func7, label, warnings, errors);
        }

        // 3) 借用7th（iv7/bVII7/bII7/bVI7）と Aug6 の優先関係はオプションで切替
        //    PreferMixtureSeventhOverAugmentedSixthWhenAmbiguous=true の場合、Mixture7th を先に試し、
        //    そうでなければ Aug6 を先に試す（従来挙動）。いずれも S=b6 抑制を尊重。
        if (preferSeventh && voicing is FourPartVoicing vMix)
        {
            // Guard: if options disallow Aug6 when soprano is b6, avoid early Aug6 return here
            int tonicPc0 = (key.TonicMidi % 12 + 12) % 12;
            int b6pc0 = (tonicPc0 + 8) % 12;
            int sopranoPc0 = (vMix.S % 12 + 12) % 12;
            bool suppressAug6Early = options.DisallowAugmentedSixthWhenSopranoFlat6 && sopranoPc0 == b6pc0;
            if (options.PreferMixtureSeventhOverAugmentedSixthWhenAmbiguous)
            {
                if (ChordRomanizer.TryRomanizeSeventhMixture(pcs, key, options, vMix, out var mix7))
                {
                    RomanNumeral baseRn = mix7!.StartsWith("iv") ? RomanNumeral.iv
                        : mix7!.StartsWith("bII") ? RomanNumeral.II
                        : mix7!.StartsWith("bVI") ? RomanNumeral.VI
                        : RomanNumeral.VII;
                    var funcMix = RomanNumeralUtils.FunctionOf(baseRn);
                    AddMixtureSeventhWarnings(warnings, mix7!, key, vMix);
                    return new HarmonyAnalysisResult(true, baseRn, funcMix, mix7, warnings, errors);
                }
                if (!suppressAug6Early && ChordRomanizer.TryRomanizeAugmentedSixth(pcs, key, options, vMix, out var aug6Early))
                {
                    return new HarmonyAnalysisResult(true, null, TonalFunction.Subdominant, aug6Early, warnings, errors);
                }
            }
            else
            {
                if (!suppressAug6Early && ChordRomanizer.TryRomanizeAugmentedSixth(pcs, key, options, vMix, out var aug6Early))
                {
                    return new HarmonyAnalysisResult(true, null, TonalFunction.Subdominant, aug6Early, warnings, errors);
                }
                if (ChordRomanizer.TryRomanizeSeventhMixture(pcs, key, options, vMix, out var mix7))
                {
                    RomanNumeral baseRn = mix7!.StartsWith("iv") ? RomanNumeral.iv
                        : mix7!.StartsWith("bII") ? RomanNumeral.II
                        : mix7!.StartsWith("bVI") ? RomanNumeral.VI
                        : RomanNumeral.VII;
                    var funcMix = RomanNumeralUtils.FunctionOf(baseRn);
                    AddMixtureSeventhWarnings(warnings, mix7!, key, vMix);
                    return new HarmonyAnalysisResult(true, baseRn, funcMix, mix7, warnings, errors);
                }
            }
        }

        // 4) 借用7th（iv7/bVII7）: ボイシング無し（root position 表記）
    if (preferSeventh && ChordRomanizer.TryRomanizeSeventhMixture(pcs, key, options, null, out var mix7NoVoicing))
        {
            RomanNumeral baseRn = mix7NoVoicing!.StartsWith("iv") ? RomanNumeral.iv
                : mix7NoVoicing!.StartsWith("bII") ? RomanNumeral.II
                : mix7NoVoicing!.StartsWith("bVI") ? RomanNumeral.VI
                : RomanNumeral.VII;
            var funcMix = RomanNumeralUtils.FunctionOf(baseRn);
            AddMixtureSeventhWarnings(warnings, mix7NoVoicing!, key, null);
            return new HarmonyAnalysisResult(true, baseRn, funcMix, mix7NoVoicing, warnings, errors);
        }

        // 4.2) Augmented Sixth (requires bass=b6 in voicing to disambiguate from bVI7)
        //      ただし、ソプラノも b6 かつ抑制オプション有効時はここでもスキップして bVI7 を優先
        if (voicing is FourPartVoicing vAug)
        {
            int tonicPc1 = (key.TonicMidi % 12 + 12) % 12;
            int b6pc1 = (tonicPc1 + 8) % 12;
            int sopranoPc1 = (vAug.S % 12 + 12) % 12;
            bool suppressAug6 = options.DisallowAugmentedSixthWhenSopranoFlat6 && sopranoPc1 == b6pc1;
            if (!options.PreferMixtureSeventhOverAugmentedSixthWhenAmbiguous)
            {
                if (!suppressAug6 && ChordRomanizer.TryRomanizeAugmentedSixth(pcs, key, options, vAug, out var aug6))
                {
                    return new HarmonyAnalysisResult(true, null, TonalFunction.Subdominant, aug6, warnings, errors);
                }
            }
        }

        // 4.5) Minor key safeguard: prefer diatonic iiø7 over any secondary interpretation
    if (preferSeventh && !key.IsMajor && options.PreferDiatonicIiHalfDimInMinor)
        {
            int degPc = DegreeRootPcLocal(key, 1); // ii root in minor
            var iiDim7 = new Chord(degPc, ChordQuality.HalfDiminishedSeventh).PitchClasses().OrderBy(x => x).ToArray();
            var set = ordered;
            if (iiDim7.Length == set.Length && iiDim7.All(set.Contains))
            {
                string label = "iiø7";
                if (voicing is FourPartVoicing vMinor)
                {
                    int bass = (vMinor.B % 12 + 12) % 12;
                    int third = (degPc + 3) % 12;
                    int fifth = (degPc + 6) % 12;
                    int seventh = (degPc + 10) % 12;
                    if      (bass == degPc)   label = "iiø7";
                    else if (bass == third)   label = "iiø65";
                    else if (bass == fifth)   label = "iiø43";
                    else if (bass == seventh) label = "iiø42";
                }
                return new HarmonyAnalysisResult(true, RomanNumeral.ii, TonalFunction.Subdominant, label, warnings, errors);
            }
        }

        // 5) Secondary dominants / secondary leading-tone sevenths（root or inversion when voicingあり）
        if (preferSeventh)
        {
            if (voicing is FourPartVoicing vSec7)
            {
                if (ChordRomanizer.TryRomanizeSecondaryDominant(pcs, key, vSec7, out var secDom7))
                    return new HarmonyAnalysisResult(true, RomanNumeral.V, TonalFunction.Dominant, secDom7, warnings, errors);
                if (ChordRomanizer.TryRomanizeSecondaryLeadingTone(pcs, key, vSec7, options, out var secLt7))
                {
                    // Prefer diatonic seventh if also matches (e.g., iiø7 in minor vs viiø7/III)
                    if (ChordRomanizer.TryRomanizeSeventh(pcs, key, out var rnDia7, out var degDia7, out var qDia7))
                    {
                        // Preserve inversion when voicing is available
                        string diatxt = BuildSeventhLabel(rnDia7, degDia7, qDia7, key, vSec7, options);
                        return new HarmonyAnalysisResult(true, rnDia7, RomanNumeralUtils.FunctionOf(rnDia7), diatxt, warnings, errors);
                    }
                    return new HarmonyAnalysisResult(true, RomanNumeral.vii, TonalFunction.Dominant, secLt7, warnings, errors);
                }
            }
            else
            {
                if (ChordRomanizer.TryRomanizeSecondaryDominant(pcs, key, null, out var secDom7NoV))
                    return new HarmonyAnalysisResult(true, RomanNumeral.V, TonalFunction.Dominant, secDom7NoV, warnings, errors);
                if (ChordRomanizer.TryRomanizeSecondaryLeadingTone(pcs, key, null, options, out var secLt7NoV))
                {
                    if (ChordRomanizer.TryRomanizeSeventh(pcs, key, out var rnDia7b, out var degDia7b, out var qDia7b))
                    {
                        string diatxt = rnDia7b + SeventhRootSuffix(qDia7b);
                        return new HarmonyAnalysisResult(true, rnDia7b, RomanNumeralUtils.FunctionOf(rnDia7b), diatxt, warnings, errors);
                    }
                    return new HarmonyAnalysisResult(true, RomanNumeral.vii, TonalFunction.Dominant, secLt7NoV, warnings, errors);
                }
            }
        }

    // 6) 三和音（ダイアトニック→借用→二次属/二次導）。4音以上（テンション/7th系）は三和音ローマナイズをスキップ
        RomanNumeral rn = default; bool hasRn = false; string? romanText = null;
        string? mixtureText = null;
    // Triad pathway: only when distinct pitch classes count is exactly 3.
    // (Previously <=3 allowed degenerate dyads; tighten per spec: distinct=3 音時のみ)
    if (ordered.Length == 3)
        {
            if (ChordRomanizer.TryRomanizeTriad(pcs, key, out rn))
            {
                hasRn = true; romanText = rn.ToString();
            }
            else if (ChordRomanizer.TryRomanizeTriadMixture(pcs, key, out rn, out mixtureText))
            {
                hasRn = true; romanText = mixtureText ?? rn.ToString();
            }
            else if (ChordRomanizer.TryRomanizeSecondaryDominant(pcs, key, voicing, out var secDomTri))
            {
                hasRn = true; romanText = secDomTri;
                // Immediate reinforcement: add inversion 6/64 for secondary dominant triads if missing
                if (voicing is FourPartVoicing vSecImm && ordered.Length == 3 && romanText is string rtImm && rtImm.StartsWith("V/") && !rtImm.Contains('6'))
                {
                    int slash = rtImm.IndexOf('/');
                    if (slash > 0 && slash < rtImm.Length - 1)
                    {
                        string target = rtImm[(slash + 1)..];
                        int? deg = target switch { "ii" => 1, "iii" => 2, "IV" => 3, "V" => 4, "vi" => 5, "vii" or "vii°" => 6, _ => null };
                        if (deg is int d)
                        {
                            int targetPc = DegreeRootPcLocal(key, d);
                            int root = (targetPc + 7) % 12; // V/x root
                            int bassPc = (vSecImm.B % 12 + 12) % 12;
                            int third = (root + 4) % 12;
                            int fifth = (root + 7) % 12;
                            if (bassPc == third) romanText = $"V6/{target}";
                            else if (bassPc == fifth) romanText = $"V64/{target}";
                        }
                    }
                }
            }
            else if (ChordRomanizer.TryRomanizeSecondaryLeadingTone(pcs, key, voicing, options, out var secLtTri))
            {
                hasRn = true; romanText = secLtTri;
                // Immediate reinforcement: add inversion 6/64 for secondary leading-tone triads if missing
                if (voicing is FourPartVoicing vSecImm2 && ordered.Length == 3 && romanText is string rtImm2 && rtImm2.StartsWith("vii°/") && !rtImm2.Contains('6'))
                {
                    int slash = rtImm2.IndexOf('/');
                    if (slash > 0 && slash < rtImm2.Length - 1)
                    {
                        string target = rtImm2[(slash + 1)..];
                        int? deg = target switch { "ii" => 1, "iii" => 2, "IV" => 3, "V" => 4, "vi" => 5, "vii" or "vii°" => 6, _ => null };
                        if (deg is int d)
                        {
                            int targetPc = DegreeRootPcLocal(key, d);
                            int root = (targetPc + 11) % 12; // leading-tone root to target
                            int bassPc = (vSecImm2.B % 12 + 12) % 12;
                            int third = (root + 3) % 12;
                            int fifth = (root + 6) % 12;
                            if (bassPc == third) romanText = $"vii°6/{target}";
                            else if (bassPc == fifth) romanText = $"vii°64/{target}";
                        }
                    }
                }
            }

            if (hasRn)
            {
                // diminished の度数記号
                var triQ0 = GetTriadQualityFromRoman(rn, key.IsMajor);
                if (triQ0 == ChordQuality.Diminished && (romanText == null || !romanText.Contains("°")))
                    romanText = (romanText ?? rn.ToString()) + "°";

                // 反転は3音のときのみ
                if (voicing is FourPartVoicing vTri && ordered.Length == 3)
                {
                    // Mixture triads like bII/bVI/bVII use non-diatonic roots; handle their inversion figures explicitly.
                    bool appliedMixtureInversion = false;
                    if (!string.IsNullOrEmpty(mixtureText))
                    {
                        int tonic = (key.TonicMidi % 12 + 12) % 12;
                        int? mixRoot = null; ChordQuality mixQ = ChordQuality.Unknown;
                        string mt = mixtureText!;
                        // Check longer tokens first to avoid prefix collisions (e.g., bVII vs bVI, bIII vs bII, iv vs i)
                        if (mt.StartsWith("bVII")) { mixRoot = (tonic + 10) % 12; mixQ = ChordQuality.Major; }
                        else if (mt.StartsWith("bIII")) { mixRoot = (tonic + 3) % 12; mixQ = ChordQuality.Major; }
                        else if (mt.StartsWith("bVI")) { mixRoot = (tonic + 8) % 12; mixQ = ChordQuality.Major; }
                        else if (mt.StartsWith("bII")) { mixRoot = (tonic + 1) % 12; mixQ = ChordQuality.Major; }
                        else if (mt.StartsWith("iv")) { mixRoot = DegreeRootPcLocal(key, 3); mixQ = ChordQuality.Minor; }
                        else if (mt.StartsWith("i")) { mixRoot = tonic; mixQ = ChordQuality.Minor; }

                        if (mixRoot is int mr && mixQ != ChordQuality.Unknown)
                        {
                            var tri = new Chord(mr, mixQ).PitchClasses().ToArray();
                            var set = ordered.ToHashSet();
                            if (tri.All(set.Contains))
                            {
                                int bassPc = (vTri.B % 12 + 12) % 12;
                                int thirdInt = (mixQ == ChordQuality.Minor || mixQ == ChordQuality.Diminished) ? 3 : 4;
                                int fifthInt = mixQ switch { ChordQuality.Diminished => 6, ChordQuality.Augmented => 8, _ => 7 };
                                int thirdPc = (mr + thirdInt) % 12;
                                int fifthPc = (mr + fifthInt) % 12;
                                string baseRoman = mt; // already includes accidental (e.g., bII)
                                if      (bassPc == mr) romanText = baseRoman;
                                else if (bassPc == thirdPc) romanText = baseRoman + "6";
                                else if (bassPc == fifthPc) romanText = baseRoman + "64";
                                appliedMixtureInversion = true;
                            }
                        }
                    }

                    if (!appliedMixtureInversion)
                    {
                        int degree = DegreeFromRoman(rn, key.IsMajor) ?? -1;
                        if (degree >= 0 && triQ0.HasValue)
                        {
                            int degPc = DegreeRootPcLocal(key, degree);
                            var tri = new Chord(degPc, triQ0.Value).PitchClasses().ToArray();
                            var set = ordered.ToHashSet();
                            if (tri.All(set.Contains))
                            {
                                int bassPc = (vTri.B % 12 + 12) % 12;
                                int thirdInt = (triQ0 == ChordQuality.Minor || triQ0 == ChordQuality.Diminished) ? 3 : 4;
                                int fifthInt = triQ0 switch { ChordQuality.Diminished => 6, ChordQuality.Augmented => 8, _ => 7 };
                                int thirdPc = (degPc + thirdInt) % 12;
                                int fifthPc = (degPc + fifthInt) % 12;
                                // Preserve mixture accidental (e.g., bII) when present
                                string baseRoman = (mixtureText ?? rn.ToString());
                                if (triQ0 == ChordQuality.Diminished && !baseRoman.Contains("°")) baseRoman += "°";
                                if      (bassPc == degPc) romanText = baseRoman;
                                else if (bassPc == thirdPc) romanText = baseRoman + "6";
                                else if (bassPc == fifthPc) romanText = baseRoman + "64";
                            }
                        }

                        // Fallback: if mixtureText was not available but current romanText indicates a mixture triad
                        // (e.g., "bVII"), still compute inversion figures from voicing.
                        if (voicing is FourPartVoicing vExtra && !string.IsNullOrEmpty(romanText))
                        {
                            string rt = romanText!;
                            int tonic = (key.TonicMidi % 12 + 12) % 12;
                            int? mixRoot2 = null; ChordQuality mixQ2 = ChordQuality.Unknown;
                            // Same longer-first ordering
                            if (rt.StartsWith("bVII")) { mixRoot2 = (tonic + 10) % 12; mixQ2 = ChordQuality.Major; }
                            else if (rt.StartsWith("bIII")) { mixRoot2 = (tonic + 3) % 12; mixQ2 = ChordQuality.Major; }
                            else if (rt.StartsWith("bVI")) { mixRoot2 = (tonic + 8) % 12; mixQ2 = ChordQuality.Major; }
                            else if (rt.StartsWith("bII")) { mixRoot2 = (tonic + 1) % 12; mixQ2 = ChordQuality.Major; }
                            else if (rt.StartsWith("iv")) { mixRoot2 = DegreeRootPcLocal(key, 3); mixQ2 = ChordQuality.Minor; }
                            else if (rt.StartsWith("i")) { mixRoot2 = tonic; mixQ2 = ChordQuality.Minor; }

                            if (mixRoot2 is int mr2 && mixQ2 != ChordQuality.Unknown)
                            {
                                var tri2 = new Chord(mr2, mixQ2).PitchClasses().ToArray();
                                var set2 = ordered.ToHashSet();
                                if (tri2.All(set2.Contains))
                                {
                                    int bassPc2 = (vExtra.B % 12 + 12) % 12;
                                    int thirdInt2 = (mixQ2 == ChordQuality.Minor || mixQ2 == ChordQuality.Diminished) ? 3 : 4;
                                    int fifthInt2 = mixQ2 switch { ChordQuality.Diminished => 6, ChordQuality.Augmented => 8, _ => 7 };
                                    int thirdPc2 = (mr2 + thirdInt2) % 12;
                                    int fifthPc2 = (mr2 + fifthInt2) % 12;
                                    if      (bassPc2 == mr2) { /* keep base */ }
                                    else if (bassPc2 == thirdPc2 && !rt.EndsWith("6")) romanText = rt + "6";
                                    else if (bassPc2 == fifthPc2 && !rt.EndsWith("64")) romanText = rt + "64";
                                }
                            }
                        }

                        // Supplement: add inversion to secondary triads (V/x or vii°/x) when voicing indicates it
                        if (voicing is FourPartVoicing vSec && !string.IsNullOrEmpty(romanText))
                        {
                            string rt2 = romanText!;
                            bool isSecondaryV = rt2.StartsWith("V/");
                            bool isSecondaryVii = rt2.StartsWith("vii°/");
                            if ((isSecondaryV || isSecondaryVii) && ordered.Length == 3 && !(rt2.Contains("6")))
                            {
                                // parse target after '/'
                                int slash = rt2.IndexOf('/');
                                if (slash > 0 && slash < rt2.Length - 1)
                                {
                                    string target = rt2[(slash + 1)..];
                                    int? deg = target switch
                                    {
                                        "ii" => 1,
                                        "iii" => 2,
                                        "IV" => 3,
                                        "V" => 4,
                                        "vi" => 5,
                                        "vii" or "vii°" => 6,
                                        _ => null
                                    };
                                    if (deg is int d)
                                    {
                                        int targetPc = DegreeRootPcLocal(key, d);
                                        int root = isSecondaryV ? (targetPc + 7) % 12 : (targetPc + 11) % 12; // V/x or lt of x
                                        int bassPc = (vSec.B % 12 + 12) % 12;
                                        int third = (root + (isSecondaryV ? 4 : 3)) % 12;
                                        int fifth = (root + (isSecondaryV ? 7 : 6)) % 12;
                                        if (bassPc == third) romanText = (isSecondaryV ? "V6/" : "vii°6/") + target;
                                        else if (bassPc == fifth) romanText = (isSecondaryV ? "V64/" : "vii°64/") + target;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // 7) 最終セーフティ: V9 を常に再確認（上書き許可）
    if (ChordRomanizer.TryRomanizeDominantNinth(ordered, key, out var v9b)
     || ChordRomanizer.TryRomanizeDominantNinth(raw, key, out v9b))
        {
        var funcV2 = RomanNumeralUtils.FunctionOf(RomanNumeral.V);
        var v9label2 = options is not null && options.PreferV7Paren9OverV9 ? "V7(9)" : v9b;
        return new HarmonyAnalysisResult(true, RomanNumeral.V, funcV2, v9label2, warnings, errors);
        }

    // 8) 最終セーフティ: 4音以上かつボイシングありで借用7th/二次属7th/二次導七に一致するなら上書き
    if (voicing is FourPartVoicing vFinal && ordered.Length >= 4 && ChordRomanizer.TryRomanizeSeventhMixture(pcs, key, options, vFinal, out var mix7Final))
        {
            RomanNumeral baseRn = mix7Final!.StartsWith("iv") ? RomanNumeral.iv
                : mix7Final!.StartsWith("bII") ? RomanNumeral.II
                : mix7Final!.StartsWith("bVI") ? RomanNumeral.VI
                : RomanNumeral.VII;
            var funcMix = RomanNumeralUtils.FunctionOf(baseRn);
            AddMixtureSeventhWarnings(warnings, mix7Final!, key, vFinal);
            return new HarmonyAnalysisResult(true, baseRn, funcMix, mix7Final, warnings, errors);
        }
        if (voicing is FourPartVoicing vFinal2 && ordered.Length >= 4)
        {
            if (ChordRomanizer.TryRomanizeSecondaryDominant(pcs, key, vFinal2, out var secDomFinal))
                return new HarmonyAnalysisResult(true, RomanNumeral.V, TonalFunction.Dominant, secDomFinal, warnings, errors);
            if (ChordRomanizer.TryRomanizeSecondaryLeadingTone(pcs, key, vFinal2, options, out var secLtFinal))
                return new HarmonyAnalysisResult(true, RomanNumeral.vii, TonalFunction.Dominant, secLtFinal, warnings, errors);
        }

        // 8) ボイシング診断（任意）
        if (voicing is FourPartVoicing v)
        {
            if (VoiceLeadingRules.HasRangeViolation(v)) errors.Add("Range violation");
            foreach (var (voice, midi) in v.Notes())
            {
                var range = VoiceRanges.ForVoice(voice);
                if (!range.InHardRange(midi) && range.InWarnRange(midi)) warnings.Add($"{voice} near range (±M3)");
            }
            if (VoiceLeadingRules.HasSpacingViolations(v)) warnings.Add("Wide spacing S-A or A-T");
            if (!v.IsOrderedTopDown()) errors.Add("Voices not ordered S>=A>=T>=B");
            if (prev is FourPartVoicing p)
            {
                if (VoiceLeadingRules.HasOverlap(p, v)) errors.Add("Voice overlap");
                if (VoiceLeadingRules.HasParallelPerfects(p, v)) warnings.Add("Parallel perfects detected");
            }
        }

        // 8.5) セーフティ: 一般ダイアトニック7thの再判定（万一ここまで未検出の場合）
        if (ChordRomanizer.TryRomanizeSeventh(pcs, key, out var rn7Late, out var degree7Late, out var q7Late))
        {
            string? labelLate;
            if (voicing is FourPartVoicing v7Late)
            {
                int rootLate = DegreeRootPcLocal(key, degree7Late);
                var chord7Late = new Chord(rootLate, q7Late).PitchClasses().ToArray();
                int bassPcLate = (v7Late.B % 12 + 12) % 12;
                int thirdLate = chord7Late.First(pc => pc != rootLate && (((pc - rootLate + 12) % 12) == 3 || ((pc - rootLate + 12) % 12) == 4));
                int fifthIntLate = q7Late is ChordQuality.DiminishedSeventh or ChordQuality.HalfDiminishedSeventh ? 6 : 7;
                int fifthLate = (rootLate + fifthIntLate) % 12;
                int sevIntLate = q7Late switch { ChordQuality.MajorSeventh => 11, ChordQuality.DiminishedSeventh => 9, _ => 10 };
                int sevLate = (rootLate + sevIntLate) % 12;
                string headAccL = q7Late switch
                {
                    ChordQuality.MajorSeventh when options.IncludeMajInSeventhInversions => rn7Late + "maj",
                    ChordQuality.HalfDiminishedSeventh => rn7Late + "ø",
                    ChordQuality.DiminishedSeventh => rn7Late + "°",
                    _ => rn7Late.ToString()
                };
                if      (bassPcLate == rootLate) labelLate = rn7Late + SeventhRootSuffix(q7Late);
                else if (bassPcLate == thirdLate) labelLate = headAccL + "65";
                else if (bassPcLate == fifthLate) labelLate = headAccL + "43";
                else if (bassPcLate == sevLate)   labelLate = headAccL + "42";
                else                              labelLate = rn7Late + SeventhRootSuffix(q7Late);
                labelLate = EnsureSeventhAccidental(rn7Late, q7Late, labelLate);
                // Strong enforcement for diminished-type sevenths: recompute figure unconditionally from voicing
                if (q7Late == ChordQuality.DiminishedSeventh || q7Late == ChordQuality.HalfDiminishedSeventh)
                {
                    string head = rn7Late + (q7Late == ChordQuality.DiminishedSeventh ? "°" : "ø");
                    if      (bassPcLate == rootLate) labelLate = head + "7";
                    else if (bassPcLate == thirdLate) labelLate = head + "65";
                    else if (bassPcLate == fifthLate) labelLate = head + "43";
                    else if (bassPcLate == sevLate)   labelLate = head + "42";
                }
                if ((q7Late == ChordQuality.DiminishedSeventh || q7Late == ChordQuality.HalfDiminishedSeventh)
                    && labelLate.EndsWith("7") && !labelLate.EndsWith("maj7"))
                {
                    int bpc = bassPcLate;
                    int thirdPc = (rootLate + 3) % 12;
                    int fifthPc = (rootLate + (q7Late == ChordQuality.DiminishedSeventh || q7Late == ChordQuality.HalfDiminishedSeventh ? 6 : 7)) % 12;
                    int sevPc = (rootLate + (q7Late == ChordQuality.DiminishedSeventh ? 9 : 10)) % 12;
                    string head = rn7Late + (q7Late == ChordQuality.DiminishedSeventh ? "°" : (q7Late == ChordQuality.HalfDiminishedSeventh ? "ø" : string.Empty));
                    if      (bpc == thirdPc) labelLate = head + "65";
                    else if (bpc == fifthPc) labelLate = head + "43";
                    else if (bpc == sevPc)   labelLate = head + "42";
                }
                if ((q7Late == ChordQuality.DiminishedSeventh || q7Late == ChordQuality.HalfDiminishedSeventh)
                    && !labelLate.Contains('°') && !labelLate.Contains('ø') && labelLate.StartsWith(rn7Late.ToString()))
                {
                    labelLate = labelLate.Insert(rn7Late.ToString().Length, q7Late == ChordQuality.DiminishedSeventh ? "°" : "ø");
                }
            }
            else
            {
                labelLate = rn7Late + SeventhRootSuffix(q7Late);
                labelLate = EnsureSeventhAccidental(rn7Late, q7Late, labelLate);
            }
            var func7Late = RomanNumeralUtils.FunctionOf(rn7Late);
            if ((q7Late == ChordQuality.DiminishedSeventh || q7Late == ChordQuality.HalfDiminishedSeventh) && labelLate is string l1)
            {
                if (!l1.Contains('°') && !l1.Contains('ø'))
                {
                    string sym = q7Late == ChordQuality.DiminishedSeventh ? "°" : "ø";
                    if (l1.StartsWith("vii")) l1 = "vii" + sym + l1.Substring(3);
                    else if (l1.StartsWith("VII")) l1 = "VII" + sym + l1.Substring(3);
                    labelLate = l1;
                }
            }
            return new HarmonyAnalysisResult(true, rn7Late, func7Late, labelLate, warnings, errors);
        }

        if (hasRn)
        {
            // Final safety: if this is a mixture triad and inversion suffix is missing, enforce based on voicing
            try
            {
                if (ordered.Length == 3 && voicing is FourPartVoicing vMixt && !string.IsNullOrEmpty(romanText))
                {
                    string rt = romanText!;
                    // Target only mixture heads where triad inversion figures apply
                    int tonic = (key.TonicMidi % 12 + 12) % 12;
                    int? mixRootF = null; ChordQuality mixQF = ChordQuality.Unknown;
                    if (rt.StartsWith("bVII")) { mixRootF = (tonic + 10) % 12; mixQF = ChordQuality.Major; }
                    else if (rt.StartsWith("bIII")) { mixRootF = (tonic + 3) % 12; mixQF = ChordQuality.Major; }
                    else if (rt.StartsWith("bVI")) { mixRootF = (tonic + 8) % 12; mixQF = ChordQuality.Major; }
                    else if (rt.StartsWith("bII")) { mixRootF = (tonic + 1) % 12; mixQF = ChordQuality.Major; }
                    else if (rt.StartsWith("iv")) { mixRootF = DegreeRootPcLocal(key, 3); mixQF = ChordQuality.Minor; }
                    else if (rt.StartsWith("i")) { mixRootF = tonic; mixQF = ChordQuality.Minor; }

                    if (mixRootF is int mrF && mixQF != ChordQuality.Unknown)
                    {
                        var triF = new Chord(mrF, mixQF).PitchClasses().ToArray();
                        var setF = ordered.ToHashSet();
                        if (triF.All(setF.Contains))
                        {
                            int bassF = (vMixt.B % 12 + 12) % 12;
                            int thirdIntF = (mixQF == ChordQuality.Minor || mixQF == ChordQuality.Diminished) ? 3 : 4;
                            int fifthIntF = mixQF switch { ChordQuality.Diminished => 6, ChordQuality.Augmented => 8, _ => 7 };
                            int thirdPcF = (mrF + thirdIntF) % 12;
                            int fifthPcF = (mrF + fifthIntF) % 12;
                            bool hasFig6 = rt.EndsWith("6");
                            bool hasFig64 = rt.EndsWith("64");
                            if (!hasFig64 && !hasFig6)
                            {
                                if (bassF == thirdPcF) romanText = rt + "6";
                                else if (bassF == fifthPcF) romanText = rt + "64";
                            }
                        }
                    }
                }
            }
            catch { /* safety must not throw */ }
            var func = RomanNumeralUtils.FunctionOf(rn);
            // Neapolitan (bII) diagnostics & optional enforcement
            try
            {
                // Only triads (distinct 3 PCs) participate in inversion diagnostics here
                if (ordered.Length == 3)
                {
                    var rt = romanText ?? rn.ToString();
                    if (!string.IsNullOrEmpty(rt) && rt.StartsWith("bII"))
                    {
                        // Resolution hint (informational)
                        warnings.Add("Neapolitan: typical resolution to V");

                        // Prefer bII6 over root or 64 when voicing indicates those
                        if (voicing is FourPartVoicing vNeap)
                        {
                            int tonicPcN = (key.TonicMidi % 12 + 12) % 12;
                            int neapRoot = (tonicPcN + 1) % 12; // bII root
                            int bassPcN = (vNeap.B % 12 + 12) % 12;
                            bool isRootPos = bassPcN == neapRoot;
                            // compute fifth pc for completeness
                            int fifthPcN = (neapRoot + 7) % 12;
                            bool isSecondInv = bassPcN == fifthPcN;
                            if ((rt == "bII" && isRootPos) || (rt == "bII64" && isSecondInv))
                            {
                                warnings.Add("Neapolitan: prefer bII6 (first inversion)");
                            }

                            // Optional enforcement: relabel to bII6 when enabled
                            if (options.EnforceNeapolitanFirstInversion)
                            {
                                // Enforce only on plain triads (not sevenths), which we are in
                                // If already 6, leave as is. Otherwise coerce to 6 when head is bII
                                if (!rt.EndsWith("6") || rt.EndsWith("64"))
                                {
                                    romanText = "bII6";
                                }
                            }
                        }
                        else if (options.EnforceNeapolitanFirstInversion)
                        {
                            // Without voicing, still coerce to bII6 when enabled (pedagogical style)
                            if (rt == "bII" || rt == "bII64") romanText = "bII6";
                        }
                    }
                }
            }
            catch { /* diagnostics must not break analysis */ }
            return new HarmonyAnalysisResult(true, rn, func, romanText, warnings, errors);
        }

        // 9) 何も一致しない
        return new HarmonyAnalysisResult(false, null, TonalFunction.Unknown, null, warnings, errors);
    }

    private static ChordQuality? GetTriadQualityFromRoman(RomanNumeral rn, bool isMajor)
    {
        return rn switch
        {
            RomanNumeral.I or RomanNumeral.IV or RomanNumeral.V => ChordQuality.Major,
            RomanNumeral.ii or RomanNumeral.iii or RomanNumeral.vi => ChordQuality.Minor,
            RomanNumeral.vii => ChordQuality.Diminished,
            RomanNumeral.i or RomanNumeral.iv or RomanNumeral.v => ChordQuality.Minor,
            RomanNumeral.III or RomanNumeral.VII => isMajor ? null : ChordQuality.Major,
            _ => null
        };
    }

    private static string BuildSeventhLabel(RomanNumeral rn7, int degree, ChordQuality q7, Key key, FourPartVoicing voicing, HarmonyOptions options)
    {
        int root = DegreeRootPcLocal(key, degree);
        int bassPc = (voicing.B % 12 + 12) % 12;
        int thirdInt = q7 switch
        {
            ChordQuality.MajorSeventh or ChordQuality.DominantSeventh => 4,
            _ => 3
        };
        int fifthInt = q7 is ChordQuality.DiminishedSeventh or ChordQuality.HalfDiminishedSeventh ? 6 : 7;
        int sevInt = q7 switch { ChordQuality.MajorSeventh => 11, ChordQuality.DiminishedSeventh => 9, _ => 10 };
        int delta = (bassPc - root + 12) % 12;
        string head = q7 switch
        {
            ChordQuality.MajorSeventh when options.IncludeMajInSeventhInversions => rn7 + "maj",
            ChordQuality.HalfDiminishedSeventh => rn7 + "ø",
            ChordQuality.DiminishedSeventh => rn7 + "°",
            _ => rn7.ToString()
        };
        string fig = delta switch
        {
            0 => SeventhRootSuffix(q7),
            var d when d == thirdInt => "65",
            var d when d == fifthInt => "43",
            var d when d == sevInt => "42",
            _ => SeventhRootSuffix(q7)
        };
        // Avoid duplicating accidentals: when fig already includes '°7' or 'ø7', do not prepend head with symbol.
        string label = fig switch
        {
            "7" => rn7 + fig,
            "maj7" => rn7 + fig,
            "°7" => rn7 + fig,
            "ø7" => rn7 + fig,
            _ => head + fig
        };
        label = EnsureSeventhAccidental(rn7, q7, label);
        return label;
    }

    private static int? DegreeFromRoman(RomanNumeral rn, bool isMajor)
    {
        return rn switch
        {
            RomanNumeral.I or RomanNumeral.i => 0,
            RomanNumeral.II or RomanNumeral.ii => 1,
            RomanNumeral.III or RomanNumeral.iii => 2,
            RomanNumeral.IV or RomanNumeral.iv => 3,
            RomanNumeral.V or RomanNumeral.v => 4,
            RomanNumeral.VI or RomanNumeral.vi => 5,
            RomanNumeral.VII or RomanNumeral.vii => 6,
            _ => null
        };
    }

    private static ChordQuality? SeventhQualityInMajor(int degree)
    {
        return degree switch
        {
            0 => ChordQuality.MajorSeventh,     // Imaj7
            1 => ChordQuality.MinorSeventh,     // ii7
            2 => ChordQuality.MinorSeventh,     // iii7
            3 => ChordQuality.MajorSeventh,     // IVmaj7
            4 => ChordQuality.DominantSeventh,  // V7
            5 => ChordQuality.MinorSeventh,     // vi7
            6 => ChordQuality.HalfDiminishedSeventh, // viiø7
            _ => null
        };
    }

    private static ChordQuality? SeventhQualityInMinor(int degree)
    {
        // Harmonic minor diatonic sevenths:
        // i7, iiø7, IIImaj7, iv7, V7, VImaj7, vii°7 (or ø7 depending). We'll use fully diminished for leading-tone seventh.
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

    private static string SeventhRootSuffix(ChordQuality q)
    {
        return q switch
        {
            ChordQuality.MajorSeventh => "maj7",
            ChordQuality.HalfDiminishedSeventh => "ø7",
            ChordQuality.DiminishedSeventh => "°7",
            _ => "7"
        };
    }

    private static int DegreeRootPcLocal(Key key, int degree)
    {
        if (key.IsMajor)
        {
            return (key.ScaleDegreeMidi(degree) % 12 + 12) % 12;
        }
        else
        {
            if (degree == 6)
            {
                int tonicPc = (key.TonicMidi % 12 + 12) % 12;
                return (tonicPc + 11) % 12;
            }
            return (key.ScaleDegreeMidi(degree) % 12 + 12) % 12;
        }
    }

    private static string EnsureSeventhAccidental(RomanNumeral rn, ChordQuality q, string label)
    {
        // Ensure '°' or 'ø' is present before inversion figures for diminished/half-diminished sevenths
        if (q != ChordQuality.DiminishedSeventh && q != ChordQuality.HalfDiminishedSeventh) return label;
        if (label.Contains('°') || label.Contains('ø')) return label;
        var head = rn.ToString();
        int idx = label.IndexOf(head);
        if (idx < 0)
        {
            // Fallback: insert after the initial roman head letters (e.g., 'vii' in 'vii65')
            int pos = 0;
            while (pos < label.Length && char.IsLetter(label[pos])) pos++;
            if (pos == 0) return label; // no letters found; give up
            int insertPosFallback = pos;
            string symFallback = q == ChordQuality.DiminishedSeventh ? "°" : "ø";
            return label.Insert(insertPosFallback, symFallback);
        }
        int insertPos = idx + head.Length;
        string sym = q == ChordQuality.DiminishedSeventh ? "°" : "ø";
        return label.Insert(insertPos, sym);
    }
}
