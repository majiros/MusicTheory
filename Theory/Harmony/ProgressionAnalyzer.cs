using System.Collections.Generic;

namespace MusicTheory.Theory.Harmony;

public readonly record struct ProgressionResult(
    List<HarmonyAnalysisResult> Chords,
    List<(int indexFrom, CadenceType cadence)> Cadences
);

public static class ProgressionAnalyzer
{
    public static ProgressionResult Analyze(IReadOnlyList<int[]> pcsList, Key key, IReadOnlyList<FourPartVoicing?>? voicings = null)
    {
        var chords = new List<HarmonyAnalysisResult>(pcsList.Count);
        var cads = new List<(int, CadenceType)>();

        FourPartVoicing? prevV = null;
        RomanNumeral? prevRn = null;
        for (int i = 0; i < pcsList.Count; i++)
        {
            var pcs = pcsList[i];
            var v = voicings != null && i < voicings.Count ? voicings[i] : null;
            var res = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v, prevV);
            chords.Add(res);
            if (prevRn != null && res.Roman != null)
            {
                var cad = CadenceAnalyzer.Detect(prevRn, res.Roman, key.IsMajor);
                if (cad != CadenceType.None)
                    cads.Add((i - 1, cad));
            }
            prevV = v;
            prevRn = res.Roman;
        }

        return new ProgressionResult(chords, cads);
    }

    // Overload: HarmonyOptions を指定して解析（既定は HarmonyOptions.Default と同等）
    public static ProgressionResult Analyze(IReadOnlyList<int[]> pcsList, Key key, HarmonyOptions options, IReadOnlyList<FourPartVoicing?>? voicings = null)
    {
        var chords = new List<HarmonyAnalysisResult>(pcsList.Count);
        var cads = new List<(int, CadenceType)>();

        FourPartVoicing? prevV = null;
        RomanNumeral? prevRn = null;
        for (int i = 0; i < pcsList.Count; i++)
        {
            var pcs = pcsList[i];
            var v = voicings != null && i < voicings.Count ? voicings[i] : null;
            var res = HarmonyAnalyzer.AnalyzeTriad(pcs, key, options, v, prevV);
            chords.Add(res);
            if (prevRn != null && res.Roman != null)
            {
                var cad = CadenceAnalyzer.Detect(prevRn, res.Roman, key.IsMajor);
                if (cad != CadenceType.None)
                    cads.Add((i - 1, cad));
            }
            prevV = v;
            prevRn = res.Roman;
        }

        return new ProgressionResult(chords, cads);
    }

    /// <summary>
    /// Analyze a progression and return per-chord harmony results along with detailed cadence diagnostics.
    /// </summary>
    /// <remarks>
    /// Cadence details include:
    /// - Cadence type (Authentic/Plagal/Half/Deceptive)
    /// - PAC/IAC approximation flag
    /// - Cadential 6-4 flag and generic 6-4 classification (<see cref="SixFourType"/>)
    ///
    /// Half cadence is suppressed when the previous chord is I64 and the current chord is V (I64→V).
    /// In that case, a non-cadential entry is returned with <c>SixFour=Cadential</c>, and the following step will emit the Authentic cadence.
    /// </remarks>
    /// <param name="pcsList">Sequence of pitch-class sets (0..11), per chord.</param>
    /// <param name="key">Tonic and mode used for analysis.</param>
    /// <param name="voicings">Optional four-part voicings aligned to <paramref name="pcsList"/> for inversion/6-4 detection.</param>
    /// <returns>Tuple of <see cref="ProgressionResult"/> and a list of <see cref="CadenceInfo"/>.</returns>
    public static (ProgressionResult result, List<CadenceInfo> cadences) AnalyzeWithDetailedCadences(
        IReadOnlyList<int[]> pcsList, Key key, IReadOnlyList<FourPartVoicing?>? voicings = null)
    {
        var chords = new List<HarmonyAnalysisResult>(pcsList.Count);
        var cadInfos = new List<CadenceInfo>();

    // vMinus1: 直前 (i-1) のボイシング, vMinus2: さらに一つ前 (i-2)
    FourPartVoicing? vMinus1 = null; FourPartVoicing? vMinus2 = null;
        RomanNumeral? prevRn = null; string? prevTxt = null; string? prevPrevTxt = null;
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
            if (i == 0) return txt!;
            return txt!.Substring(0, i);
        }
        for (int i = 0; i < pcsList.Count; i++)
        {
            var pcs = pcsList[i];
            var v = voicings != null && i < voicings.Count ? voicings[i] : null;
            var res = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v, vMinus1);
            chords.Add(res);
            if (prevRn != null && res.Roman != null)
            {
                var info = CadenceAnalyzer.DetectDetailed(i - 1, prevRn, res.Roman, key.IsMajor, key.TonicMidi % 12, prevTxt, res.RomanText, prevPrevTxt, HarmonyOptions.Default, vMinus1, v);
                if (info.Type != CadenceType.None)
                {
                    // Ensure non-cadential 6-4 labels (Passing/Pedal) are not attached to cadence entries
                    var info2 = info;
                    if (info2.SixFour != SixFourType.None && info2.SixFour != SixFourType.Cadential)
                        info2 = new CadenceInfo(info2.IndexFrom, info2.Type, info2.IsPerfectAuthentic, info2.HasCadentialSixFour, SixFourType.None);
                    cadInfos.Add(info2);
                }
                else
                {
                    // Prefer voicing-based classification around x64 when neighbors share the same harmony
                    bool around64 = v != null && vMinus1 != null && vMinus2 != null && !string.IsNullOrEmpty(prevTxt) && prevTxt!.EndsWith("64");
                    if (around64)
                    {
                        var hPrevPrev = Head(prevPrevTxt); // (i-2) の Roman
                        var hCurr = Head(res.RomanText);
                        if (!string.IsNullOrEmpty(hPrevPrev) && hPrevPrev == hCurr)
                        {
                            int bPrevPrev = vMinus2!.Value.B % 12;
                            int bCurr = v!.Value.B % 12;
                            var six = (bPrevPrev == bCurr) ? SixFourType.Pedal : SixFourType.Passing;
                            cadInfos.Add(new CadenceInfo(info.IndexFrom, info.Type, info.IsPerfectAuthentic, info.HasCadentialSixFour, six));
                            goto NextChord1;
                        }
                    }
                    // Fallback: if neighbors share the same harmony and voicings available, decide Pedal vs Passing by bass equality
                    if (v != null && vMinus1 != null && vMinus2 != null)
                    {
                        var hPrevPrev2 = Head(prevPrevTxt);
                        var hCurr2 = Head(res.RomanText);
                        if (!string.IsNullOrEmpty(hPrevPrev2) && hPrevPrev2 == hCurr2)
                        {
                            int bPrevPrev = vMinus2!.Value.B % 12;
                            int bCurr = v!.Value.B % 12;
                            var six = (bPrevPrev == bCurr) ? SixFourType.Pedal : SixFourType.Passing;
                            cadInfos.Add(new CadenceInfo(info.IndexFrom, info.Type, info.IsPerfectAuthentic, info.HasCadentialSixFour, six));
                            goto NextChord1;
                        }
                    }
                    if (info.SixFour != SixFourType.None && info.SixFour != SixFourType.Cadential)
                        cadInfos.Add(info);
                }
            NextChord1: ;
            }
            vMinus2 = vMinus1; vMinus1 = v;
            prevPrevTxt = prevTxt;
            prevRn = res.Roman;
            prevTxt = res.RomanText;
        }

        return (new ProgressionResult(chords, new List<(int, CadenceType)>()), cadInfos);
    }

    /// <summary>
    /// Analyze a progression and return detailed cadence diagnostics with <see cref="HarmonyOptions"/>.
    /// </summary>
    /// <remarks>
    /// The <see cref="HarmonyOptions.ShowNonCadentialSixFour"/> option controls whether non-cadential 6-4 entries
    /// (Passing/Pedal with <c>Type==None</c>) are included in the returned cadence list. When set to <c>false</c>,
    /// such entries are suppressed; cadential 6-4 information attached to a proper cadence (e.g., I64→V→I → Authentic)
    /// remains part of that cadence entry.
    /// </remarks>
    /// <param name="pcsList">Sequence of pitch-class sets (0..11), per chord.</param>
    /// <param name="key">Tonic and mode used for analysis.</param>
    /// <param name="options">Harmony options affecting recognition and display of details.</param>
    /// <param name="voicings">Optional four-part voicings aligned to <paramref name="pcsList"/>.</param>
    /// <returns>Tuple of <see cref="ProgressionResult"/> and a list of <see cref="CadenceInfo"/>.</returns>
    public static (ProgressionResult result, List<CadenceInfo> cadences) AnalyzeWithDetailedCadences(
        IReadOnlyList<int[]> pcsList, Key key, HarmonyOptions options, IReadOnlyList<FourPartVoicing?>? voicings = null)
    {
        var chords = new List<HarmonyAnalysisResult>(pcsList.Count);
        var cadInfos = new List<CadenceInfo>();

    FourPartVoicing? vMinus1 = null; FourPartVoicing? vMinus2 = null;
        RomanNumeral? prevRn = null; string? prevTxt = null; string? prevPrevTxt = null;
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
            if (i == 0) return txt!;
            return txt!.Substring(0, i);
        }
        for (int i = 0; i < pcsList.Count; i++)
        {
            var pcs = pcsList[i];
            var v = voicings != null && i < voicings.Count ? voicings[i] : null;
            var res = HarmonyAnalyzer.AnalyzeTriad(pcs, key, options, v, vMinus1);
            chords.Add(res);
            if (prevRn != null && res.Roman != null)
            {
                var info = CadenceAnalyzer.DetectDetailed(i - 1, prevRn, res.Roman, key.IsMajor, key.TonicMidi % 12, prevTxt, res.RomanText, prevPrevTxt, options, vMinus1, v);
                if (info.Type != CadenceType.None)
                {
                    var info2 = info;
                    if (info2.SixFour != SixFourType.None && info2.SixFour != SixFourType.Cadential)
                        info2 = new CadenceInfo(info2.IndexFrom, info2.Type, info2.IsPerfectAuthentic, info2.HasCadentialSixFour, SixFourType.None);

                    // Relabel cadential 6-4 (I64) as V64-53 when preferred
                    if (options.PreferCadentialSixFourAsDominant && info2.HasCadentialSixFour)
                    {
                        int idxI64 = i - 2;
                        if (idxI64 >= 0 && idxI64 < chords.Count)
                        {
                            var c = chords[idxI64];
                            string? txt = c.RomanText;
                            bool looksI64 = !string.IsNullOrEmpty(txt) && txt!.EndsWith("64");
                            // Only relabel when it looks like a 6-4 triad on tonic (heuristic: startsWith I/i)
                            if (looksI64 && (txt!.StartsWith("I") || txt!.StartsWith("i")))
                            {
                                var relabeled = new HarmonyAnalysisResult(
                                    c.Success,
                                    RomanNumeral.V,
                                    TonalFunction.Dominant,
                                    "V64-53",
                                    c.Warnings,
                                    c.Errors
                                );
                                chords[idxI64] = relabeled;
                            }
                        }
                    }
                    cadInfos.Add(info2);
                }
                else
                {
                    // Prefer voicing-based classification when allowed
                    if (options.ShowNonCadentialSixFour)
                    {
                        bool around64 = v != null && vMinus1 != null && vMinus2 != null && !string.IsNullOrEmpty(prevTxt) && prevTxt!.EndsWith("64");
                        if (around64)
                        {
                            var hPrevPrev = Head(prevPrevTxt);
                            var hCurr = Head(res.RomanText);
                            if (!string.IsNullOrEmpty(hPrevPrev) && hPrevPrev == hCurr)
                            {
                                int bPrevPrev = vMinus2!.Value.B % 12;
                                int bCurr = v!.Value.B % 12;
                                var six = (bPrevPrev == bCurr) ? SixFourType.Pedal : SixFourType.Passing;
                                cadInfos.Add(new CadenceInfo(info.IndexFrom, info.Type, info.IsPerfectAuthentic, info.HasCadentialSixFour, six));
                                goto NextChord2;
                            }
                        }
                        // Fallback: neighbors share harmony + voicings, decide by bass equality
                        if (v != null && vMinus1 != null && vMinus2 != null)
                        {
                            var hPrevPrev2 = Head(prevPrevTxt);
                            var hCurr2 = Head(res.RomanText);
                            if (!string.IsNullOrEmpty(hPrevPrev2) && hPrevPrev2 == hCurr2)
                            {
                                int bPrevPrev = vMinus2!.Value.B % 12;
                                int bCurr = v!.Value.B % 12;
                                var six = (bPrevPrev == bCurr) ? SixFourType.Pedal : SixFourType.Passing;
                                cadInfos.Add(new CadenceInfo(info.IndexFrom, info.Type, info.IsPerfectAuthentic, info.HasCadentialSixFour, six));
                                goto NextChord2;
                            }
                        }
                        if (info.SixFour != SixFourType.None && info.SixFour != SixFourType.Cadential)
                            cadInfos.Add(info);
                    }
                }
            NextChord2: ;
            }
            vMinus2 = vMinus1; vMinus1 = v;
            prevPrevTxt = prevTxt;
            prevRn = res.Roman;
            prevTxt = res.RomanText;
        }

        return (new ProgressionResult(chords, new List<(int, CadenceType)>()), cadInfos);
    }

    public static (ProgressionResult result, List<(int start, int end, Key key)> segments) AnalyzeWithKeyEstimate(
        IReadOnlyList<int[]> pcsList, Key initialKey, IReadOnlyList<FourPartVoicing?>? voicings = null, int window = 2)
    {
        var keys = KeyEstimator.EstimatePerChord(pcsList, initialKey, window);
        var chords = new List<HarmonyAnalysisResult>(pcsList.Count);
        var cads = new List<(int, CadenceType)>();

        FourPartVoicing? prevV = null; RomanNumeral? prevRn = null; Key currKey = initialKey;
        for (int i = 0; i < pcsList.Count; i++)
        {
            currKey = keys[i];
            var v = voicings != null && i < voicings.Count ? voicings[i] : null;
            var res = HarmonyAnalyzer.AnalyzeTriad(pcsList[i], currKey, v, prevV);
            chords.Add(res);
            if (prevRn != null && res.Roman != null)
            {
                var cad = CadenceAnalyzer.Detect(prevRn, res.Roman, currKey.IsMajor);
                if (cad != CadenceType.None) cads.Add((i - 1, cad));
            }
            prevV = v; prevRn = res.Roman;
        }

        // Build key segments
        var segs = new List<(int start, int end, Key key)>();
        if (keys.Count > 0)
        {
            int s = 0;
            for (int i = 1; i < keys.Count; i++)
            {
                if (!SameKey(keys[i - 1], keys[i]))
                {
                    segs.Add((s, i - 1, keys[i - 1]));
                    s = i;
                }
            }
            segs.Add((s, keys.Count - 1, keys[^1]));
        }

        return (new ProgressionResult(chords, cads), segs);
    }

    // Overload: HarmonyOptions を併用したキー推定 + 和音解析
    public static (ProgressionResult result, List<(int start, int end, Key key)> segments) AnalyzeWithKeyEstimate(
        IReadOnlyList<int[]> pcsList, Key initialKey, HarmonyOptions options, IReadOnlyList<FourPartVoicing?>? voicings = null, int window = 2)
    {
        var keys = KeyEstimator.EstimatePerChord(pcsList, initialKey, window);
        var chords = new List<HarmonyAnalysisResult>(pcsList.Count);
        var cads = new List<(int, CadenceType)>();

        FourPartVoicing? prevV = null; RomanNumeral? prevRn = null; Key currKey = initialKey;
        for (int i = 0; i < pcsList.Count; i++)
        {
            currKey = keys[i];
            var v = voicings != null && i < voicings.Count ? voicings[i] : null;
            var res = HarmonyAnalyzer.AnalyzeTriad(pcsList[i], currKey, options, v, prevV);
            chords.Add(res);
            if (prevRn != null && res.Roman != null)
            {
                var cad = CadenceAnalyzer.Detect(prevRn, res.Roman, currKey.IsMajor);
                if (cad != CadenceType.None) cads.Add((i - 1, cad));
            }
            prevV = v; prevRn = res.Roman;
        }

        // Build key segments
        var segs = new List<(int start, int end, Key key)>();
        if (keys.Count > 0)
        {
            int s = 0;
            for (int i = 1; i < keys.Count; i++)
            {
                if (!SameKey(keys[i - 1], keys[i]))
                {
                    segs.Add((s, i - 1, keys[i - 1]));
                    s = i;
                }
            }
            segs.Add((s, keys.Count - 1, keys[^1]));
        }

        return (new ProgressionResult(chords, cads), segs);
    }

    // Overload exposing the per-chord key trace and allowing estimator Options.
    public static (ProgressionResult result, List<(int start, int end, Key key)> segments, List<Key> keys) AnalyzeWithKeyEstimate(
        IReadOnlyList<int[]> pcsList, Key initialKey, KeyEstimator.Options options, IReadOnlyList<FourPartVoicing?>? voicings = null)
    {
        var keys = KeyEstimator.EstimatePerChord(pcsList, initialKey, options);
        // Enforce MinSwitchIndex: force staying on previous key before this index
        if (options.MinSwitchIndex > 0 && keys.Count > 1)
        {
            if (options.MinSwitchIndex >= keys.Count)
            {
                // Full lock: keep entire sequence on the initial key
                for (int i = 0; i < keys.Count; i++) keys[i] = initialKey;
            }
            else
            {
                int limit = Math.Min(options.MinSwitchIndex, keys.Count);
                for (int i = 1; i < limit; i++) keys[i] = keys[i - 1];
            }
        }
        var chords = new List<HarmonyAnalysisResult>(pcsList.Count);
        var cads = new List<(int, CadenceType)>();

        FourPartVoicing? prevV = null; RomanNumeral? prevRn = null; Key currKey = initialKey;
        for (int i = 0; i < pcsList.Count; i++)
        {
            currKey = keys[i];
            var v = voicings != null && i < voicings.Count ? voicings[i] : null;
            var res = HarmonyAnalyzer.AnalyzeTriad(pcsList[i], currKey, v, prevV);
            chords.Add(res);
            if (prevRn != null && res.Roman != null)
            {
                var cad = CadenceAnalyzer.Detect(prevRn, res.Roman, currKey.IsMajor);
                if (cad != CadenceType.None) cads.Add((i - 1, cad));
            }
            prevV = v; prevRn = res.Roman;
        }

        var segs = new List<(int start, int end, Key key)>();
        if (keys.Count > 0)
        {
            int s = 0;
            for (int i = 1; i < keys.Count; i++)
            {
                if (!SameKey(keys[i - 1], keys[i]))
                {
                    segs.Add((s, i - 1, keys[i - 1]));
                    s = i;
                }
            }
            segs.Add((s, keys.Count - 1, keys[^1]));
        }

        return (new ProgressionResult(chords, cads), segs, keys);
    }

    // Overload: KeyEstimator.Options + HarmonyOptions（トレース無し）
    public static (ProgressionResult result, List<(int start, int end, Key key)> segments, List<Key> keys) AnalyzeWithKeyEstimate(
        IReadOnlyList<int[]> pcsList,
        Key initialKey,
        KeyEstimator.Options options,
        HarmonyOptions harmonyOptions,
        IReadOnlyList<FourPartVoicing?>? voicings = null)
    {
        var keys = KeyEstimator.EstimatePerChord(pcsList, initialKey, options);
        // Enforce MinSwitchIndex: force staying on previous key before this index
        if (options.MinSwitchIndex > 0 && keys.Count > 1)
        {
            if (options.MinSwitchIndex >= keys.Count)
            {
                for (int i = 0; i < keys.Count; i++) keys[i] = initialKey;
            }
            else
            {
                int limit = Math.Min(options.MinSwitchIndex, keys.Count);
                for (int i = 1; i < limit; i++) keys[i] = keys[i - 1];
            }
        }
        var chords = new List<HarmonyAnalysisResult>(pcsList.Count);
        var cads = new List<(int, CadenceType)>();

        FourPartVoicing? prevV = null; RomanNumeral? prevRn = null; Key currKey = initialKey;
        for (int i = 0; i < pcsList.Count; i++)
        {
            currKey = keys[i];
            var v = voicings != null && i < voicings.Count ? voicings[i] : null;
            var res = HarmonyAnalyzer.AnalyzeTriad(pcsList[i], currKey, harmonyOptions, v, prevV);
            chords.Add(res);
            if (prevRn != null && res.Roman != null)
            {
                var cad = CadenceAnalyzer.Detect(prevRn, res.Roman, currKey.IsMajor);
                if (cad != CadenceType.None) cads.Add((i - 1, cad));
            }
            prevV = v; prevRn = res.Roman;
        }

        var segs = new List<(int start, int end, Key key)>();
        if (keys.Count > 0)
        {
            int s = 0;
            for (int i = 1; i < keys.Count; i++)
            {
                if (!SameKey(keys[i - 1], keys[i]))
                {
                    segs.Add((s, i - 1, keys[i - 1]));
                    s = i;
                }
            }
            segs.Add((s, keys.Count - 1, keys[^1]));
        }

        return (new ProgressionResult(chords, cads), segs, keys);
    }

    // New overload: expose per-chord key-estimation trace via out parameter.
    public static (ProgressionResult result, List<(int start, int end, Key key)> segments, List<Key> keys) AnalyzeWithKeyEstimate(
        IReadOnlyList<int[]> pcsList,
        Key initialKey,
        KeyEstimator.Options options,
        out List<KeyEstimator.TraceEntry> trace,
        IReadOnlyList<FourPartVoicing?>? voicings = null)
    {
    var keys = KeyEstimator.EstimatePerChord(pcsList, initialKey, options, out trace, voicings);
        // Enforce MinSwitchIndex: force staying on previous key before this index
        if (options.MinSwitchIndex > 0 && keys.Count > 1)
        {
            if (options.MinSwitchIndex >= keys.Count)
            {
                for (int i = 0; i < keys.Count; i++) keys[i] = initialKey;
            }
            else
            {
                int limit = Math.Min(options.MinSwitchIndex, keys.Count);
                for (int i = 1; i < limit; i++) keys[i] = keys[i - 1];
            }
        }
        var chords = new List<HarmonyAnalysisResult>(pcsList.Count);
        var cads = new List<(int, CadenceType)>();

        FourPartVoicing? prevV = null; RomanNumeral? prevRn = null; Key currKey = initialKey;
        for (int i = 0; i < pcsList.Count; i++)
        {
            currKey = keys[i];
            var v = voicings != null && i < voicings.Count ? voicings[i] : null;
            var res = HarmonyAnalyzer.AnalyzeTriad(pcsList[i], currKey, v, prevV);
            chords.Add(res);
            if (prevRn != null && res.Roman != null)
            {
                var cad = CadenceAnalyzer.Detect(prevRn, res.Roman, currKey.IsMajor);
                if (cad != CadenceType.None) cads.Add((i - 1, cad));
            }
            prevV = v; prevRn = res.Roman;
        }

        var segs = new List<(int start, int end, Key key)>();
        if (keys.Count > 0)
        {
            int s = 0;
            for (int i = 1; i < keys.Count; i++)
            {
                if (!SameKey(keys[i - 1], keys[i]))
                {
                    segs.Add((s, i - 1, keys[i - 1]));
                    s = i;
                }
            }
            segs.Add((s, keys.Count - 1, keys[^1]));
        }

        return (new ProgressionResult(chords, cads), segs, keys);
    }

    // Overload: KeyEstimator.Options + HarmonyOptions（トレース有り）
    public static (ProgressionResult result, List<(int start, int end, Key key)> segments, List<Key> keys) AnalyzeWithKeyEstimate(
        IReadOnlyList<int[]> pcsList,
        Key initialKey,
        KeyEstimator.Options options,
        HarmonyOptions harmonyOptions,
        out List<KeyEstimator.TraceEntry> trace,
        IReadOnlyList<FourPartVoicing?>? voicings = null)
    {
        var keys = KeyEstimator.EstimatePerChord(pcsList, initialKey, options, out trace, voicings);
        // Enforce MinSwitchIndex: force staying on previous key before this index
        if (options.MinSwitchIndex > 0 && keys.Count > 1)
        {
            if (options.MinSwitchIndex >= keys.Count)
            {
                for (int i = 0; i < keys.Count; i++) keys[i] = initialKey;
            }
            else
            {
                int limit = Math.Min(options.MinSwitchIndex, keys.Count);
                for (int i = 1; i < limit; i++) keys[i] = keys[i - 1];
            }
        }
        var chords = new List<HarmonyAnalysisResult>(pcsList.Count);
        var cads = new List<(int, CadenceType)>();

        FourPartVoicing? prevV = null; RomanNumeral? prevRn = null; Key currKey = initialKey;
        for (int i = 0; i < pcsList.Count; i++)
        {
            currKey = keys[i];
            var v = voicings != null && i < voicings.Count ? voicings[i] : null;
            var res = HarmonyAnalyzer.AnalyzeTriad(pcsList[i], currKey, harmonyOptions, v, prevV);
            chords.Add(res);
            if (prevRn != null && res.Roman != null)
            {
                var cad = CadenceAnalyzer.Detect(prevRn, res.Roman, currKey.IsMajor);
                if (cad != CadenceType.None) cads.Add((i - 1, cad));
            }
            prevV = v; prevRn = res.Roman;
        }

        var segs = new List<(int start, int end, Key key)>();
        if (keys.Count > 0)
        {
            int s = 0;
            for (int i = 1; i < keys.Count; i++)
            {
                if (!SameKey(keys[i - 1], keys[i]))
                {
                    segs.Add((s, i - 1, keys[i - 1]));
                    s = i;
                }
            }
            segs.Add((s, keys.Count - 1, keys[^1]));
        }

        return (new ProgressionResult(chords, cads), segs, keys);
    }

    // Overload: also compute a simple confidence for each key segment (0..1)
    // confidence is derived from per-chord trace: avg of (max - secondBest) / max within the segment (where max>0)
    public static (ProgressionResult result, List<(int start, int end, Key key, double confidence)> segments, List<Key> keys) AnalyzeWithKeyEstimate(
        IReadOnlyList<int[]> pcsList,
        Key initialKey,
        KeyEstimator.Options options,
        out List<KeyEstimator.TraceEntry> trace,
        IReadOnlyList<FourPartVoicing?>? voicings,
        bool withConfidence)
    {
        var (res, segs, keys) = AnalyzeWithKeyEstimate(pcsList, initialKey, options, out trace, voicings);
        var segsWithConf = new List<(int start, int end, Key key, double confidence)>(segs.Count);
        foreach (var s in segs)
        {
            double sum = 0; int count = 0;
            for (int i = s.start; i <= s.end && i < trace.Count; i++)
            {
                int max = Math.Max(0, trace[i].MaxScore);
                int second = Math.Max(0, trace[i].SecondBestScore);
                if (max > 0)
                {
                    sum += Math.Clamp((max - second) / (double)max, 0.0, 1.0);
                    count++;
                }
            }
            double conf = count > 0 ? sum / count : 0.0;
            segsWithConf.Add((s.start, s.end, s.key, conf));
        }
        return (res, segsWithConf, keys);
    }

    // Overload: KeyEstimator.Options + HarmonyOptions（トレース有り + 信頼度）
    public static (ProgressionResult result, List<(int start, int end, Key key, double confidence)> segments, List<Key> keys) AnalyzeWithKeyEstimate(
        IReadOnlyList<int[]> pcsList,
        Key initialKey,
        KeyEstimator.Options options,
        HarmonyOptions harmonyOptions,
        out List<KeyEstimator.TraceEntry> trace,
        IReadOnlyList<FourPartVoicing?>? voicings,
        bool withConfidence)
    {
        var (res, segs, keys) = AnalyzeWithKeyEstimate(pcsList, initialKey, options, harmonyOptions, out trace, voicings);
        var segsWithConf = new List<(int start, int end, Key key, double confidence)>(segs.Count);
        foreach (var s in segs)
        {
            double sum = 0; int count = 0;
            for (int i = s.start; i <= s.end && i < trace.Count; i++)
            {
                int max = Math.Max(0, trace[i].MaxScore);
                int second = Math.Max(0, trace[i].SecondBestScore);
                if (max > 0)
                {
                    sum += Math.Clamp((max - second) / (double)max, 0.0, 1.0);
                    count++;
                }
            }
            double conf = count > 0 ? sum / count : 0.0;
            segsWithConf.Add((s.start, s.end, s.key, conf));
        }
        return (res, segsWithConf, keys);
    }

    // New: Modulation-aware segments post-filtering
    // - minLength: require at least N chords for a new key segment
    // - minConfidence: require average normalized margin >= threshold (0..1) for the segment
    public static (ProgressionResult result, List<(int start, int end, Key key, double confidence)> segments, List<Key> keys) AnalyzeWithKeyEstimateAndModulation(
        IReadOnlyList<int[]> pcsList,
        Key initialKey,
        KeyEstimator.Options options,
        out List<KeyEstimator.TraceEntry> trace,
        IReadOnlyList<FourPartVoicing?>? voicings,
        int minLength = 2,
        double minConfidence = 0.2)
    {
        var (res, segsWithConf, keys) = AnalyzeWithKeyEstimate(pcsList, initialKey, options, out trace, voicings, withConfidence: true);
        var filtered = new List<(int start, int end, Key key, double confidence)>();
        foreach (var s in segsWithConf)
        {
            int len = s.end - s.start + 1;
            if (len >= minLength && s.confidence >= minConfidence)
            {
                filtered.Add(s);
            }
        }
        // Safety: ensure at least one segment is returned to avoid empty results in borderline cases.
        // If all segments were filtered out by thresholds, return the first segment as a fallback.
        if (filtered.Count == 0 && segsWithConf.Count > 0)
        {
            filtered.Add(segsWithConf[0]);
        }
        return (res, filtered, keys);
    }

    private static bool SameKey(Key a, Key b) => a.IsMajor == b.IsMajor && ((a.TonicMidi % 12 + 12) % 12) == ((b.TonicMidi % 12 + 12) % 12);
}
