using System.Text;

namespace MusicTheory.Theory.Harmony;

/// <summary>
/// High-level cadence classification between two chords.
/// </summary>
public enum CadenceType { None, Authentic, Plagal, Half, Deceptive }

/// <summary>
/// Classification for 6-4 chords when identified in context.
/// </summary>
public enum SixFourType { None, Cadential, Passing, Pedal }

/// <summary>
/// Detailed cadence diagnostics without breaking existing CadenceType usage.
/// <para>
/// IndexFrom は、直前の和音（prev）から現在の和音（curr）への遷移の「開始位置」を示します（prev のインデックス）。
/// </para>
/// </summary>
public readonly partial record struct CadenceInfo(
    int IndexFrom,
    CadenceType Type,
    bool IsPerfectAuthentic,
    bool HasCadentialSixFour,
    SixFourType SixFour
);

// Improve diagnostics: customize ToString for clearer test failure messages
public readonly partial record struct CadenceInfo
{
    private bool PrintMembers(StringBuilder builder)
    {
        builder
            .Append("IndexFrom = ").Append(IndexFrom)
            .Append(", Type = ").Append(Type)
            .Append(", IsPerfectAuthentic = ").Append(IsPerfectAuthentic)
            .Append(", HasCadentialSixFour = ").Append(HasCadentialSixFour)
            .Append(", SixFour = ").Append(SixFour);
        return true;
    }
}

public static class CadenceAnalyzer
{
    /// <summary>
    /// Basic cadence detection (Authentic, Plagal, Half, Deceptive) using roman numeral heads.
    /// </summary>
    /// <param name="prev">Previous roman numeral.</param>
    /// <param name="curr">Current roman numeral.</param>
    /// <param name="isMajor">Mode flag for tonic-relative checks.</param>
    /// <returns>CadenceType classification.</returns>
    public static CadenceType Detect(RomanNumeral? prev, RomanNumeral? curr, bool isMajor)
    {
        if (prev is null || curr is null) return CadenceType.None;
        var p = prev.Value; var c = curr.Value;

        // Normalize tonic VI roman per mode for comparison
        bool isTonic(RomanNumeral r) => isMajor ? r == RomanNumeral.I : r == RomanNumeral.i;
        bool isSubdominant(RomanNumeral r) => r == RomanNumeral.IV || r == RomanNumeral.iv;
        bool isDominant(RomanNumeral r) => r == RomanNumeral.V || r == RomanNumeral.v; // v rarely used; include defensively
        bool isRelativeVI(RomanNumeral r) => isMajor ? r == RomanNumeral.vi : r == RomanNumeral.VI;

        if (isDominant(p) && isTonic(c)) return CadenceType.Authentic;
        if (isSubdominant(p) && isTonic(c)) return CadenceType.Plagal;
        if (isDominant(p) && isRelativeVI(c)) return CadenceType.Deceptive;
    if (isDominant(c)) return CadenceType.Half;

        return CadenceType.None;
    }

    /// <summary>
    /// Detailed detection with simple heuristics for PAC/IAC and 6-4 classification.
    /// RomanText labels (e.g., "V7", "I64") are used to detect inversions.
    /// </summary>
    /// <param name="indexFrom">Index where the cadence is considered to start.</param>
    /// <param name="prev">Previous roman head.</param>
    /// <param name="curr">Current roman head.</param>
    /// <param name="isMajor">Mode flag.</param>
    /// <param name="prevText">RomanText of previous chord.</param>
    /// <param name="currText">RomanText of current chord.</param>
    /// <param name="prevPrevText">RomanText of the chord before previous.</param>
    /// <returns>CadenceInfo including PAC flag, cadential 6-4 flag, and SixFourType.</returns>
    public static CadenceInfo DetectDetailed(int indexFrom, RomanNumeral? prev, RomanNumeral? curr, bool isMajor, int tonicPc,
        string? prevText, string? currText, string? prevPrevText, HarmonyOptions? options = null,
        FourPartVoicing? prevVoicing = null, FourPartVoicing? currVoicing = null)
    {
        options ??= HarmonyOptions.Default;
        var t = Detect(prev, curr, isMajor);
        bool isPAC = false;
        bool has64 = false;
        SixFourType six4 = SixFourType.None;

        static string Head(string? txt)
        {
            if (string.IsNullOrEmpty(txt)) return string.Empty;
            int i = 0;
            while (i < txt!.Length)
            {
                char ch = txt[i];
                bool ok = ch == 'b' || ch == '#' || ch == 'i' || ch == 'v' || ch == 'I' || ch == 'V' || ch == '/';
                if (!ok) break;
                i++;
            }
            if (i == 0) return txt!; // fallback: return whole
            return txt!.Substring(0, i);
        }
        // Suppress Half cadence if preceded by cadential 6-4 (I64) and followed by an authentic cadence at next step.
        // We can't look ahead here, but we can avoid emitting Half when pattern I64 -> V is seen; the caller will emit Authentic at next step.
        if (t == CadenceType.Half)
        {
            if (!string.IsNullOrEmpty(prevText) && prevText.EndsWith("64"))
            {
                // Likely cadential 6-4 → V; defer to next step where V→I authentic will be captured.
                return new CadenceInfo(indexFrom, CadenceType.None, false, true, SixFourType.Cadential);
            }
            // Suppress Half cadence for predominant augmented sixth -> V transitions (treat as internal pre-dominant motion).
            // Detect common Aug6 labels: It6, Fr43, Ger65 (case-insensitive start).
            if (!string.IsNullOrEmpty(prevText))
            {
                var ptxt = prevText;
                if (ptxt.Contains("It6", StringComparison.OrdinalIgnoreCase) ||
                    ptxt.Contains("Fr", StringComparison.OrdinalIgnoreCase) ||
                    ptxt.Contains("Ger", StringComparison.OrdinalIgnoreCase))
                {
                    return new CadenceInfo(indexFrom, CadenceType.None, false, false, SixFourType.None);
                }
                // Also suppress when previous is a mixture seventh acting as predominant: bVI7/bVI65/bVI43/bVI42 (and bII7 / bVII7 / iv7)
                static bool IsMixtureSeventhLabel(string s)
                {
                    if (string.IsNullOrEmpty(s)) return false;
                    // Check longer tokens first
                    if (s.StartsWith("bVI", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("bII", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("bVII", StringComparison.OrdinalIgnoreCase) ||
                        s.StartsWith("iv", StringComparison.OrdinalIgnoreCase))
                    {
                        return s.EndsWith("7") || s.EndsWith("65") || s.EndsWith("43") || s.EndsWith("42");
                    }
                    return false;
                }
                if (IsMixtureSeventhLabel(ptxt))
                {
                    return new CadenceInfo(indexFrom, CadenceType.None, false, false, SixFourType.None);
                }
                // Suppress Half also for secondary leading-tone to V: vii°/V or vii°7/V (including ø variants)
                bool isSecLtToV = ptxt.StartsWith("vii", StringComparison.OrdinalIgnoreCase) && ptxt.Contains("/V");
                if (isSecLtToV)
                {
                    return new CadenceInfo(indexFrom, CadenceType.None, false, false, SixFourType.None);
                }
            }
        }

        // Fallback authentic detection: only when prev head is strictly V (of the key) and curr head is I.
        // Use Head() to avoid misreading strings like "vii°7/V" as starting with 'V'.
        if (t == CadenceType.None && !string.IsNullOrEmpty(prevText) && !string.IsNullOrEmpty(currText))
        {
            var prevHead = Head(prevText);
            var currHead = Head(currText);
            bool prevIsDominantHead = string.Equals(prevHead, "V", StringComparison.Ordinal)
                                   || string.Equals(prevHead, "v", StringComparison.Ordinal);
            bool currIsTonicHead = string.Equals(currHead, isMajor ? "I" : "i", StringComparison.Ordinal);
            if (prevIsDominantHead && currIsTonicHead)
            {
                t = CadenceType.Authentic; // PAC evaluation still governed below by flags
            }
        }

        // Explicit suppression: secondary leading-tone to V (vii°/V, vii°7/V, viiø7/V) resolving directly to I is not an Authentic cadence.
        // This covers the Ger65 enharmonic set path that may romanize as vii°7/V without an intervening V.
        if (!string.IsNullOrEmpty(prevText) && !string.IsNullOrEmpty(currText))
        {
            var currHead2 = Head(currText);
            if (currHead2 == (isMajor ? "I" : "i"))
            {
                var pt = prevText;
                bool prevIsSecLtToV = pt.StartsWith("vii", StringComparison.OrdinalIgnoreCase) && pt.Contains("/V", StringComparison.Ordinal);
                if (prevIsSecLtToV)
                {
                    t = CadenceType.None;
                }
            }
        }
        if (t == CadenceType.Authentic)
        {
            // Cadential 6-4: I64 → V → I
            if (!string.IsNullOrEmpty(prevPrevText))
            {
                bool i64 = prevPrevText!.StartsWith(isMajor ? "I" : "i") && prevPrevText!.EndsWith("64");
                if (i64) { has64 = true; six4 = SixFourType.Cadential; }
            }
            // Perfect authentic (approximation): V(7) in root → I(root or Imaj7), no inversion suffix visible
            if (!string.IsNullOrEmpty(prevText) && !string.IsNullOrEmpty(currText))
            {
                bool vIsPlainTriad = prevText == "V"; // plain dominant triad root position
                bool vAllowsExtensions = prevText == "V7" || prevText == "V9" || prevText == "V7(9)";
                bool iPlainTriad = currText == (isMajor ? "I" : "i");
                bool iAllowsMaj7 = currText == (isMajor ? "Imaj7" : "imaj7");

                bool allowDominantExt = !options.StrictPacDisallowDominantExtensions;
                bool allowTonicMaj7 = !options.StrictPacPlainTriadsOnly; // triad only when strict

                bool dominantOk = vIsPlainTriad || (allowDominantExt && vAllowsExtensions);
                bool tonicOk = iPlainTriad || (allowTonicMaj7 && iAllowsMaj7);

                // Require dominant to be in root position for PAC when voicing is available (covers V9/V7(9) inversions where text has no figure)
                bool dominantRootOk = true;
                if (prevVoicing is FourPartVoicing vPrevRoot)
                {
                    int domRootPc = ((tonicPc + 7) % 12 + 12) % 12; // V root relative to tonic
                    int bassPrev = (vPrevRoot.B % 12 + 12) % 12;
                    dominantRootOk = (bassPrev == domRootPc);
                }

                if (dominantOk && tonicOk && dominantRootOk)
                {
                    // If strict plain triads requested, require both sides plain triads
                    if (options.StrictPacPlainTriadsOnly)
                        isPAC = vIsPlainTriad && iPlainTriad;
                    else
                        isPAC = true;

                    // Soprano tonic requirement (now absolute PC based)
                    if (isPAC && options.StrictPacRequireSopranoTonic)
                    {
                        if (currVoicing is FourPartVoicing vCurr)
                        {
                            int sopranoPc = (vCurr.S % 12 + 12) % 12;
                            bool sopranoHasTonic = sopranoPc == ((tonicPc % 12) + 12) % 12;
                            if (!sopranoHasTonic)
                                isPAC = false;
                            else if (options.StrictPacRequireSopranoLeadingToneResolution)
                            {
                                if (prevVoicing is FourPartVoicing vPrev)
                                {
                                    int prevS = (vPrev.S % 12 + 12) % 12;
                                    int currS = sopranoPc;
                                    int leadingTonePc = ((tonicPc + 11) % 12 + 12) % 12; // raised 7 in both major & harmonic minor assumption
                                    if (prevS == leadingTonePc)
                                    {
                                        // Require semitone upward resolution (LT -> T). Allow enharmonic wrap (11->0).
                                        bool resolves = currS == ((prevS + 1) % 12) && currS == tonicPc;
                                        if (!resolves)
                                            isPAC = false;
                                    }
                                    // If prev soprano not leading tone we only required tonic on goal; no extra demotion.
                                }
                                else
                                {
                                    // cannot verify resolution, demote
                                    isPAC = false;
                                }
                            }
                        }
                        else
                        {
                            isPAC = false; // cannot verify soprano
                        }
                    }
                }
            }
        }
        // Generic 6-4 classification (Passing / Pedal) is only attached on non-cadential steps
        // to avoid mixing with proper cadence entries (which may carry Cadential 6-4 info only).
        if (t == CadenceType.None)
        {
            if (!string.IsNullOrEmpty(prevText) && prevText!.EndsWith("64"))
            {
                var hPrevPrev = Head(prevPrevText);
                var hCurr = Head(currText);
                if (!string.IsNullOrEmpty(hPrevPrev) && !string.IsNullOrEmpty(hCurr))
                {
                    if (hPrevPrev == hCurr)
                    {
                        // Same harmony around x64
                        // Passing patterns:
                        //  - Ascending: root → 64 → 6 (curr ends with "6" but not "64")
                        //  - Descending: 6 → 64 → root (prevPrev ends with "6" but not "64" and curr is root position)
                        bool currIsFirstInversion = !string.IsNullOrEmpty(currText) && currText!.EndsWith("6") && !currText!.EndsWith("64");
                        bool prevPrevIsFirstInversion = !string.IsNullOrEmpty(prevPrevText) && prevPrevText!.EndsWith("6") && !prevPrevText!.EndsWith("64");
                        bool currIsRoot = !string.IsNullOrEmpty(currText) && !currText!.EndsWith("6");

                        if (currIsFirstInversion || (prevPrevIsFirstInversion && currIsRoot))
                            six4 = six4 == SixFourType.None ? SixFourType.Passing : six4;
                        else
                            six4 = six4 == SixFourType.None ? SixFourType.Pedal : six4;
                    }
                }
            }
        }

        // Safety: Do not attach generic Passing/Pedal 6-4 labels to cadence entries.
        // Cadential 6-4 is allowed on Authentic; otherwise force SixFour to None when a cadence is present.
        if (t != CadenceType.None && six4 != SixFourType.Cadential)
        {
            six4 = SixFourType.None;
        }

        return new CadenceInfo(indexFrom, t, isPAC, has64, six4);
    }
}
