using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicTheory.Theory.Harmony;

public static class KeyEstimator
{
    /// <summary>
    /// Tunable heuristics for per-chord key estimation. All weights are integer and small by design
    /// to keep scoring explainable. Defaults aim for mild inertia and cadence awareness.
    /// </summary>
    public sealed class Options
    {
        /// <summary>Sliding window radius around the current index used for diatonic fit (0 = current only).</summary>
        public int Window { get; init; } = 2;              // sliding window radius
        /// <summary>Bias to keep the previous key (inertia). Added when candidate equals previous.</summary>
        public int PrevKeyBias { get; init; } = 1;          // inertia bias towards previous key
        /// <summary>Bias toward the initial key. Added when candidate equals the initial key.</summary>
        public int InitialKeyBias { get; init; } = 0;       // default: no bias to initial key
        /// <summary>Bonus when the current chord exactly matches V triad (dominant) of the candidate key.</summary>
        public int DominantTriadBonus { get; init; } = 2;   // if current chord = V triad in candidate key
        /// <summary>Bonus when the current chord exactly matches V7 (dominant seventh) of the candidate key.</summary>
        public int DominantSeventhBonus { get; init; } = 3; // if current chord = V7 in candidate key
        /// <summary>Bonus on V(7) → I(maj7/m7含む) motion in the candidate key.</summary>
        public int CadenceBonus { get; init; } = 4;         // if prev=V(7) and curr=I in candidate key
        /// <summary>Hysteresis margin: require competitor advantage &gt; margin to switch from the previous key.</summary>
        public int SwitchMargin { get; init; } = 1;         // easier to switch when evidence appears
        /// <summary>Bonus if the current chord is diatonic to both the previous key and the candidate key.</summary>
        public int PivotChordBonus { get; init; } = 1;      // if chord is diatonic to both prev and candidate key
        /// <summary>Bonus for secondary dominant triads (V/x) relative to the candidate key.</summary>
        public int SecondaryDominantTriadBonus { get; init; } = 1;  // if current chord = V/x triad (x is diatonic degree other than I)
        /// <summary>Bonus for secondary dominant sevenths (V/x7) relative to the candidate key.</summary>
        public int SecondaryDominantSeventhBonus { get; init; } = 2; // if current chord = V/x7
    /// <summary>Bonus when a secondary dominant (V/x or V/x7) resolves immediately to its target chord x.</summary>
    public int SecondaryResolutionBonus { get; init; } = 0; // default off for backward compatibility
        /// <summary>When true, emit a detailed per-chord trace of scoring and diagnostics.</summary>
        public bool CollectTrace { get; init; } = false;     // emit per-chord score breakdown
        /// <summary>Subtract this amount per non-diatonic pitch class in the current chord for each candidate (0 = disabled).</summary>
        public int OutOfKeyPenaltyPerPc { get; init; } = 0;  // subtract per non-diatonic pc in current chord (0=disabled)
    /// <summary>
    /// Forbid switching away from the previous key before this chord index. Use 0 for default behavior.
    /// Example: set to a large value to lock the initial key for a short smoke test.
    /// </summary>
    public int MinSwitchIndex { get; init; } = 0;
    }

    // Lightweight per-chord breakdown (for the chosen key)
    /// <summary>
    /// A compact per-chord breakdown for the chosen key, including score components,
    /// competition snapshot, hysteresis flag, and optional voice-leading diagnostics.
    /// </summary>
    public sealed class TraceEntry
    {
        /// <summary>Chord index in the analyzed sequence.</summary>
        public int Index { get; init; }
        /// <summary>The key chosen at this index after tie-break and hysteresis.</summary>
        public Key ChosenKey { get; init; }
        /// <summary>Diatonic fit score summed over the sliding window.</summary>
        public int BaseWindowScore { get; init; }
        /// <summary>PrevKeyBias applied (if any).</summary>
        public int PrevKeyBias { get; init; }
        /// <summary>InitialKeyBias applied (if any).</summary>
        public int InitialKeyBias { get; init; }
        /// <summary>Bonus when the current chord matched V triad in the chosen key.</summary>
        public int DominantTriad { get; init; }
        /// <summary>Bonus when the current chord matched V7 in the chosen key.</summary>
        public int DominantSeventh { get; init; }
        /// <summary>Bonus when the motion prev: V(7) → curr: I(maj7/m7) in the chosen key.</summary>
        public int Cadence { get; init; }
        /// <summary>Bonus when the current chord is diatonic to both previous and chosen keys.</summary>
        public int Pivot { get; init; }
        /// <summary>Bonus when the current chord matched a secondary dominant triad V/x.</summary>
        public int SecondaryDominantTriad { get; init; }
        /// <summary>Bonus when the current chord matched a secondary dominant seventh V/x7.</summary>
        public int SecondaryDominantSeventh { get; init; }
    /// <summary>Bonus when a secondary dominant resolved to its target chord (applied at the resolution chord).</summary>
    public int SecondaryResolution { get; init; }
        /// <summary>Total score for the chosen key (sum of parts and penalty).</summary>
        public int Total { get; init; }
        /// <summary>True when the key stayed due to hysteresis/tie logic.</summary>
        public bool StayedDueToHysteresis { get; init; }
        // Debug/diagnostic metadata (optional): per-step competition snapshot
        /// <summary>Maximum score among all candidate keys at this step.</summary>
        public int MaxScore { get; init; }
        /// <summary>Second-best score (max among non-chosen candidates, 0 if n/a).</summary>
        public int SecondBestScore { get; init; }
        /// <summary>Number of candidates tied for the maximum score.</summary>
        public int NumTopKeys { get; init; }
        /// <summary>Compact list of top candidates (e.g., "M@0:5,m@9:5").</summary>
        public string? TopKeysSummary { get; init; }
        /// <summary>Penalty (negative) applied for non-diatonic pitch classes relative to the chosen key.</summary>
        public int OutOfKeyPenalty { get; init; }
    // Optional: voice-leading diagnostics (if voicings were provided)
        /// <summary>Voice-leading: any part out of its conventional range.</summary>
        public bool VLRangeViolation { get; init; }
        /// <summary>Voice-leading: spacing violations (S-A/A-T larger than allowed).</summary>
        public bool VLSpacingViolation { get; init; }
        /// <summary>Voice-leading: overlap between adjacent voices.</summary>
        public bool VLOverlap { get; init; }
        /// <summary>Voice-leading: parallel perfect fifths/octaves detected to previous chord.</summary>
        public bool VLParallelPerfects { get; init; }

        public override string ToString()
        {
            var m = ChosenKey.IsMajor ? "M" : "m";
            int tp = ((ChosenKey.TonicMidi % 12) + 12) % 12;
            return $"idx={Index} key={m}@{tp} base={BaseWindowScore} prev={PrevKeyBias} init={InitialKeyBias} " +
                   $"V={DominantTriad} V7={DominantSeventh} cad={Cadence} pivot={Pivot} " +
       $"V/x={SecondaryDominantTriad} V/x7={SecondaryDominantSeventh} out={OutOfKeyPenalty} total={Total} stay={StayedDueToHysteresis} " +
           $"vl=[r={(VLRangeViolation?1:0)},s={(VLSpacingViolation?1:0)},o={(VLOverlap?1:0)},p={(VLParallelPerfects?1:0)}] " +
                   $"max={MaxScore} 2nd={SecondBestScore} tops={NumTopKeys} [{TopKeysSummary}]";
        }
    }

    // Backward-compatible API
    public static List<Key> EstimatePerChord(IReadOnlyList<int[]> pcsList, Key initialKey, int window = 2)
        => EstimatePerChord(pcsList, initialKey, new Options { Window = window });

    // Enhanced estimator with cadence weighting and hysteresis.
    public static List<Key> EstimatePerChord(IReadOnlyList<int[]> pcsList, Key initialKey, Options options)
        => EstimatePerChord(pcsList, initialKey, options, out _);

    // Overload returning trace
    public static List<Key> EstimatePerChord(
        IReadOnlyList<int[]> pcsList, Key initialKey, Options options, out List<TraceEntry> trace)
        => EstimatePerChord(pcsList, initialKey, options, out trace, voicings: null);

    // Overload that can annotate trace with voice-leading diagnostics if voicings are provided (no scoring impact).
    public static List<Key> EstimatePerChord(
        IReadOnlyList<int[]> pcsList, Key initialKey, Options options, out List<TraceEntry> trace, IReadOnlyList<FourPartVoicing?>? voicings)
    {
        trace = new List<TraceEntry>(pcsList?.Count ?? 0);
        if (pcsList == null || pcsList.Count == 0) return new List<Key>();

        // Early full-lock: if MinSwitchIndex locks beyond the sequence length,
        // just return the initial key for all chords and (optionally) emit trivial trace.
        if (options.MinSwitchIndex > 0 && options.MinSwitchIndex >= pcsList.Count)
        {
            var locked = new List<Key>(pcsList.Count);
            for (int i = 0; i < pcsList.Count; i++) locked.Add(initialKey);
            if (options.CollectTrace)
            {
                // Build minimal trace entries to preserve length; scoring fields remain 0, stayed/hysteresis marked.
                var candidatesLock = BuildAllKeys();
                var diatonicMapLock = candidatesLock.ToDictionary(k => k, k => DiatonicSet(k));
                for (int i = 0; i < pcsList.Count; i++)
                {
                    var te = BuildTraceEntry(
                        index: i,
                        chosenKey: initialKey,
                        prevKey: i == 0 ? (Key?)null : initialKey,
                        stayed: i > 0, // stayed from previous
                        pcsList: pcsList,
                        options: options,
                        diatonicMap: diatonicMapLock,
                        initialKey: initialKey,
                        maxScore: 0,
                        secondBestScore: 0,
                        numTopKeys: 1,
                        topKeysSummary: null,
                        voicings: voicings
                    );
                    trace.Add(te);
                }
            }
            return locked;
        }

        var candidates = BuildAllKeys();
        var diatonicMap = candidates.ToDictionary(k => k, k => DiatonicSet(k));

        var result = new List<Key>(pcsList.Count);
        Key? prev = null;
        for (int i = 0; i < pcsList.Count; i++)
        {
            if (i == 0)
            {
                result.Add(initialKey);
                prev = initialKey;
                if (options.CollectTrace)
                {
                    var te0 = BuildTraceEntry(i, initialKey, prevKey: null, stayed: false, pcsList, options, diatonicMap, initialKey,
                        maxScore: 0, secondBestScore: 0, numTopKeys: 1, topKeysSummary: null, voicings: voicings);
                    trace.Add(te0);
                }
                continue;
            }

            var currSet = ToPcSet(pcsList[i]);
            var prevSet = i > 0 ? ToPcSet(pcsList[i - 1]) : s_emptySet;

            var scoreByKey = new Dictionary<Key, int>();
            foreach (var k in candidates)
            {
                int score = 0;
                var dia = diatonicMap[k];
                int start = Math.Max(0, i - options.Window);
                int end = Math.Min(pcsList.Count - 1, i + options.Window);
                for (int j = start; j <= end; j++)
                {
                    var set = pcsList[j].Select(NormPc).Distinct();
                    foreach (var pc in set)
                        if (dia.Contains(pc)) score++;
                }

                if (prev is Key pk && SameKey(pk, k)) score += options.PrevKeyBias;
                if (SameKey(initialKey, k)) score += options.InitialKeyBias;

                var (vTri, v7, iTri, i7maj, i7min) = GetKeyReferenceSets(k);
                if (SetEquals(currSet, v7)) score += options.DominantSeventhBonus;
                else if (SetEquals(currSet, vTri)) score += options.DominantTriadBonus;

                if ((SetEquals(prevSet, v7) || SetEquals(prevSet, vTri)) &&
                    (SetEquals(currSet, iTri) || SetEquals(currSet, (k.IsMajor ? i7maj : i7min))))
                {
                    score += options.CadenceBonus;
                }

                if (prev is Key prevK)
                {
                    var prevDia = diatonicMap[prevK];
                    bool inCand = currSet.All(pc => dia.Contains(pc));
                    bool inPrev = currSet.All(pc => prevDia.Contains(pc));
                    if (inCand && inPrev)
                        score += options.PivotChordBonus;
                }

                int tonicPc = NormPc(k.TonicMidi);
                for (int deg = 1; deg <= 6; deg++)
                {
                    int degPc = k.IsMajor
                        ? NormPc(k.ScaleDegreeMidi(deg))
                        : (deg == 6 ? (tonicPc + 11) % 12 : NormPc(k.ScaleDegreeMidi(deg)));

                    int secRoot = (degPc + 7) % 12;
                    var secVTri = new Chord(secRoot, ChordQuality.Major).PitchClasses().ToHashSet();
                    var secV7 = new Chord(secRoot, ChordQuality.DominantSeventh).PitchClasses().ToHashSet();
                    if (SetEquals(currSet, secV7)) { score += options.SecondaryDominantSeventhBonus; break; }
                    if (SetEquals(currSet, secVTri)) { score += options.SecondaryDominantTriadBonus; break; }
                }

                // Secondary resolution bonus: if previous chord was V/x (triad or 7th) and current chord equals x triad or seventh
                if (options.SecondaryResolutionBonus != 0 && i > 0)
                {
                    var prevSetLocal = prevSet;
                    for (int deg = 1; deg <= 6; deg++)
                    {
                        int degPc = k.IsMajor
                            ? NormPc(k.ScaleDegreeMidi(deg))
                            : (deg == 6 ? (tonicPc + 11) % 12 : NormPc(k.ScaleDegreeMidi(deg)));
                        int secRoot = (degPc + 7) % 12;
                        var secVTri = new Chord(secRoot, ChordQuality.Major).PitchClasses().ToHashSet();
                        var secV7 = new Chord(secRoot, ChordQuality.DominantSeventh).PitchClasses().ToHashSet();
                        bool prevWasSec = SetEquals(prevSetLocal, secVTri) || SetEquals(prevSetLocal, secV7);
                        if (!prevWasSec) continue;
                        var targetTri = new Chord(degPc, k.IsMajor ? TriadQualityForDegreeMajor(deg) : TriadQualityForDegreeMinorHarm(deg)).PitchClasses().ToHashSet();
                        var target7Maj = new Chord(degPc, ChordQuality.MajorSeventh).PitchClasses().ToHashSet();
                        var target7Min = new Chord(degPc, ChordQuality.MinorSeventh).PitchClasses().ToHashSet();
                        if (SetEquals(currSet, targetTri) || SetEquals(currSet, (k.IsMajor ? target7Maj : target7Min)))
                        {
                            score += options.SecondaryResolutionBonus;
                            break;
                        }
                    }
                }

                // Optional: penalize non-diatonic PCs in the current chord for this candidate key
                if (options.OutOfKeyPenaltyPerPc != 0 && currSet.Count > 0)
                {
                    int nonDiaCount = 0;
                    foreach (var pc in currSet) if (!dia.Contains(pc)) nonDiaCount++;
                    if (nonDiaCount > 0) score -= nonDiaCount * options.OutOfKeyPenaltyPerPc;
                }

                scoreByKey[k] = score;
            }

            int maxScore = scoreByKey.Values.Max();
            var topKeys = scoreByKey.Where(kv => kv.Value == maxScore).Select(kv => kv.Key).ToList();
            bool tieExists = topKeys.Count > 1;
            // Deterministic tie-break: prefer Major, then lower tonic pitch class
            Key candidateChoice = topKeys
                .OrderByDescending(k => k.IsMajor)
                .ThenBy(k => NormPc(k.TonicMidi))
                .First();

            Key chosen;
            bool stayedDueToHysteresis = false;
            // Hard lock: don't allow switching before MinSwitchIndex (always stay on previous when available)
            if (options.MinSwitchIndex > 0 && i < options.MinSwitchIndex && prev is Key prevLock)
            {
                chosen = prevLock;
                // Build trace and continue
                if (options.CollectTrace)
                {
                    string? tops = null;
                    if (scoreByKey.Count > 0)
                    {
                        var lockTopKeys = scoreByKey.Where(kv => kv.Value == scoreByKey.Values.Max()).Select(kv => kv.Key).ToList();
                        var parts = lockTopKeys
                            .OrderBy(k => NormPc(k.TonicMidi))
                            .ThenByDescending(k => k.IsMajor)
                            .Select(k => $"{(k.IsMajor ? "M" : "m")}@{NormPc(k.TonicMidi)}:{scoreByKey[k]}");
                        tops = string.Join(",", parts);
                    }
                    int secondBest = 0;
                    if (scoreByKey.Count > 1)
                        secondBest = scoreByKey.Where(kv => !SameKey(kv.Key, prevLock)).Select(kv => kv.Value).DefaultIfEmpty(0).Max();
                    var teLock = BuildTraceEntry(i, prevLock, prev, stayed: true, pcsList, options, diatonicMap, initialKey,
                        maxScore: scoreByKey.Values.Max(), secondBestScore: secondBest,
                        numTopKeys: scoreByKey.Count(kv => kv.Value == scoreByKey.Values.Max()), topKeysSummary: tops, voicings: voicings);
                    trace.Add(teLock);
                }
                result.Add(chosen);
                prev = chosen;
                continue;
            }
            if (prev is Key prevKey)
            {
                int prevScore = scoreByKey[prevKey];
                if (!SameKey(candidateChoice, prevKey) && prevScore + options.SwitchMargin > maxScore)
                {
                    chosen = prevKey; // stay due to insufficient advantage
                    stayedDueToHysteresis = true;
                }
                else
                {
                    if (topKeys.Any(k => SameKey(k, prevKey)))
                    {
                        chosen = prevKey;
                        // In a tie, explicitly flag that we stayed (even if deterministic tie-break would also pick prev)
                        if (tieExists) stayedDueToHysteresis = true;
                    }
                    else if (topKeys.Any(k => SameKey(k, initialKey)))
                    {
                        chosen = initialKey;
                    }
                    else
                    {
                        chosen = candidateChoice;
                        // If candidate equals prev (e.g., tie broken to prev) and tie existed, mark as hysteresis stay
                        if (tieExists && SameKey(chosen, prevKey)) stayedDueToHysteresis = true;
                    }

                    // Additionally, if we ended up staying on prevKey and there exists another key
                    // with score >= prevScore but the advantage is < SwitchMargin, consider it a hysteresis stay.
                    if (SameKey(chosen, prevKey) && options.SwitchMargin > 0)
                    {
                        int otherMax = int.MinValue;
                        foreach (var kv in scoreByKey)
                        {
                            if (SameKey(kv.Key, prevKey)) continue;
                            if (kv.Value > otherMax) otherMax = kv.Value;
                        }
                        if (otherMax != int.MinValue)
                        {
                            int prevScoreLocal = prevScore;
                            if (otherMax >= prevScoreLocal && otherMax < prevScoreLocal + options.SwitchMargin)
                            {
                                stayedDueToHysteresis = true;
                            }
                        }
                    }
                }
            }
            else
            {
                chosen = candidateChoice;
            }

            // Finalize hysteresis flag after chosen key is determined: if we stayed on prev
            // and there was a tie or insufficient advantage or any competitor matched/exceeded prev,
            // mark as hysteresis for transparency. Also, when SwitchMargin>0, any stay counts as hysteresis.
            if (prev is Key prevAfter && SameKey(chosen, prevAfter))
            {
                int prevScoreAfter = scoreByKey[prevAfter];
                bool competitorWithinMargin = scoreByKey.Any(kv => !SameKey(kv.Key, prevAfter) && kv.Value >= prevScoreAfter && kv.Value < prevScoreAfter + options.SwitchMargin);
                if (tieExists || options.SwitchMargin > 0 || prevScoreAfter + options.SwitchMargin > maxScore || competitorWithinMargin)
                {
                    stayedDueToHysteresis = true;
                }
            }

            // Ensure flag determinism: if we stayed on previous key, and either a tie existed
            // or hysteresis margin is enabled, reflect it as a hysteresis stay in trace.
            if (prev is Key prevForFlag && SameKey(prevForFlag, chosen) && (options.SwitchMargin > 0 || tieExists))
            {
                stayedDueToHysteresis = true;
            }

            result.Add(chosen);
            if (options.CollectTrace)
            {
                // Build a short summary of top keys (same max score)
                string? tops = null;
                if (scoreByKey.Count > 0)
                {
                    var parts = topKeys
                        .OrderBy(k => NormPc(k.TonicMidi))
                        .ThenByDescending(k => k.IsMajor)
                        .Select(k => $"{(k.IsMajor ? "M" : "m")}@{NormPc(k.TonicMidi)}:{scoreByKey[k]}");
                    tops = string.Join(",", parts);
                }
                // second best (largest score among non-chosen keys), or 0 if none
                int secondBest = 0;
                if (scoreByKey.Count > 1)
                {
                    secondBest = scoreByKey.Where(kv => !SameKey(kv.Key, chosen)).Select(kv => kv.Value).DefaultIfEmpty(0).Max();
                }
                var te = BuildTraceEntry(i, chosen, prev, stayedDueToHysteresis, pcsList, options, diatonicMap, initialKey,
                    maxScore: maxScore, secondBestScore: secondBest, numTopKeys: topKeys.Count, topKeysSummary: tops, voicings: voicings);
                trace.Add(te);
            }
            prev = chosen;
        }
        return result;
    }

    private static TraceEntry BuildTraceEntry(
        int index,
        Key chosenKey,
        Key? prevKey,
        bool stayed,
        IReadOnlyList<int[]> pcsList,
        Options options,
        Dictionary<Key, HashSet<int>> diatonicMap,
        Key initialKey,
        int maxScore,
        int secondBestScore,
        int numTopKeys,
    string? topKeysSummary,
    IReadOnlyList<FourPartVoicing?>? voicings)
    {
        int baseWindow = 0;
        var dia = diatonicMap[chosenKey];
        int start = Math.Max(0, index - options.Window);
        int end = Math.Min(pcsList.Count - 1, index + options.Window);
        for (int j = start; j <= end; j++)
        {
            var set = pcsList[j].Select(NormPc).Distinct();
            foreach (var pc in set)
                if (dia.Contains(pc)) baseWindow++;
        }

        int prevBias = (prevKey is Key pk && SameKey(pk, chosenKey)) ? options.PrevKeyBias : 0;
        int initBias = SameKey(initialKey, chosenKey) ? options.InitialKeyBias : 0;

        var curr = pcsList[index].Select(NormPc).ToHashSet();
        var prevSetLocal = index > 0 ? pcsList[index - 1].Select(NormPc).ToHashSet() : s_emptySet;
        var (vTri, v7, iTri, i7maj, i7min) = GetKeyReferenceSets(chosenKey);
        int dom7 = SetEquals(curr, v7) ? options.DominantSeventhBonus : 0;
        int domTri = (dom7 == 0 && SetEquals(curr, vTri)) ? options.DominantTriadBonus : 0;

        int cadence = 0;
        if ((SetEquals(prevSetLocal, v7) || SetEquals(prevSetLocal, vTri)) &&
            (SetEquals(curr, iTri) || SetEquals(curr, (chosenKey.IsMajor ? i7maj : i7min))))
        {
            cadence = options.CadenceBonus;
        }

        int pivot = 0;
        if (prevKey is Key prevK)
        {
            var prevDia = diatonicMap[prevK];
            bool inCand = curr.All(pc => dia.Contains(pc));
            bool inPrev = curr.All(pc => prevDia.Contains(pc));
            if (inCand && inPrev) pivot = options.PivotChordBonus;
        }

        int secTri = 0, sec7 = 0;
        int secResolve = 0;
        {
            int tonicPc = NormPc(chosenKey.TonicMidi);
            for (int deg = 1; deg <= 6; deg++)
            {
                int degPc = chosenKey.IsMajor
                    ? NormPc(chosenKey.ScaleDegreeMidi(deg))
                    : (deg == 6 ? (tonicPc + 11) % 12 : NormPc(chosenKey.ScaleDegreeMidi(deg)));

                int secRoot = (degPc + 7) % 12;
                var secVTri = new Chord(secRoot, ChordQuality.Major).PitchClasses().ToHashSet();
                var secV7 = new Chord(secRoot, ChordQuality.DominantSeventh).PitchClasses().ToHashSet();
                if (SetEquals(curr, secV7)) { sec7 = options.SecondaryDominantSeventhBonus; break; }
                if (SetEquals(curr, secVTri)) { secTri = options.SecondaryDominantTriadBonus; break; }
            }
            // Resolution bonus (applied at current chord if previous was V/x)
            if (options.SecondaryResolutionBonus != 0 && index > 0)
            {
                var prevSet = pcsList[index - 1].Select(NormPc).ToHashSet();
                for (int deg = 1; deg <= 6; deg++)
                {
                    int degPc = chosenKey.IsMajor
                        ? NormPc(chosenKey.ScaleDegreeMidi(deg))
                        : (deg == 6 ? (tonicPc + 11) % 12 : NormPc(chosenKey.ScaleDegreeMidi(deg)));
                    int secRoot = (degPc + 7) % 12;
                    var secVTri = new Chord(secRoot, ChordQuality.Major).PitchClasses().ToHashSet();
                    var secV7 = new Chord(secRoot, ChordQuality.DominantSeventh).PitchClasses().ToHashSet();
                    bool prevWasSec = SetEquals(prevSet, secVTri) || SetEquals(prevSet, secV7);
                    if (!prevWasSec) continue;
                    var targetTri = new Chord(degPc, chosenKey.IsMajor ? TriadQualityForDegreeMajor(deg) : TriadQualityForDegreeMinorHarm(deg)).PitchClasses().ToHashSet();
                    var target7Maj = new Chord(degPc, ChordQuality.MajorSeventh).PitchClasses().ToHashSet();
                    var target7Min = new Chord(degPc, ChordQuality.MinorSeventh).PitchClasses().ToHashSet();
                    if (SetEquals(curr, targetTri) || SetEquals(curr, (chosenKey.IsMajor ? target7Maj : target7Min)))
                    {
                        secResolve = options.SecondaryResolutionBonus;
                        break;
                    }
                }
            }
        }

        // Out-of-key penalty for transparency (applied only against chosenKey for trace)
        int outPenalty = 0;
        if (options.OutOfKeyPenaltyPerPc != 0 && curr.Count > 0)
        {
            int nonDiaCount = 0;
            foreach (var pc in curr) if (!dia.Contains(pc)) nonDiaCount++;
            if (nonDiaCount > 0) outPenalty = -nonDiaCount * options.OutOfKeyPenaltyPerPc;
        }

    int total = baseWindow + prevBias + initBias + domTri + dom7 + cadence + pivot + secTri + sec7 + secResolve + outPenalty;
    // Ensure tie/hysteresis-driven stays are always reflected in the trace even if earlier logic didn't set it
    bool stayedFlag;
    if (prevKey is Key pk2 && SameKey(pk2, chosenKey))
    {
        // If we stayed on previous key, mark as hysteresis when either:
        // - a tie existed among top keys, or
        // - hysteresis is active (SwitchMargin > 0), which can implicitly bias staying
        stayedFlag = stayed || numTopKeys > 1 || options.SwitchMargin > 0;
    }
    else
    {
        stayedFlag = stayed;
    }

        // Voice-leading diagnostics (if voicings provided)
        bool vlRange = false, vlSpace = false, vlOverlap = false, vlPar = false;
        if (voicings != null && index < voicings.Count)
        {
            var v = voicings[index];
            if (v is FourPartVoicing vx)
            {
                vlRange = VoiceLeadingRules.HasRangeViolation(vx);
                vlSpace = VoiceLeadingRules.HasSpacingViolations(vx);
                if (index > 0)
                {
                    var pv = voicings[index - 1];
                    if (pv is FourPartVoicing pvx)
                    {
                        vlOverlap = VoiceLeadingRules.HasOverlap(pvx, vx);
                        vlPar = VoiceLeadingRules.HasParallelPerfects(pvx, vx);
                    }
                }
            }
        }
        return new TraceEntry
        {
            Index = index,
            ChosenKey = chosenKey,
            BaseWindowScore = baseWindow,
            PrevKeyBias = prevBias,
            InitialKeyBias = initBias,
            DominantTriad = domTri,
            DominantSeventh = dom7,
            Cadence = cadence,
            Pivot = pivot,
            SecondaryDominantTriad = secTri,
            SecondaryDominantSeventh = sec7,
            SecondaryResolution = secResolve,
            Total = total,
            StayedDueToHysteresis = stayedFlag,
            MaxScore = maxScore,
            SecondBestScore = secondBestScore,
            NumTopKeys = numTopKeys,
            TopKeysSummary = topKeysSummary,
            OutOfKeyPenalty = outPenalty,
            VLRangeViolation = vlRange,
            VLSpacingViolation = vlSpace,
            VLOverlap = vlOverlap,
            VLParallelPerfects = vlPar,
        };
    }

    private static List<Key> BuildAllKeys()
    {
        var keys = new List<Key>(24);
        for (int pc = 0; pc < 12; pc++)
        {
            keys.Add(new Key(60 + pc, true));  // majors
            keys.Add(new Key(60 + pc, false)); // minors
        }
        return keys;
    }

    private static HashSet<int> DiatonicSet(Key key)
    {
        int tonicPc = NormPc(key.TonicMidi);
        int[] intervals = key.IsMajor
            ? new[] { 0, 2, 4, 5, 7, 9, 11 }
            : new[] { 0, 2, 3, 5, 7, 8, 11 }; // harmonic minor baseline (raised 7th)
        return intervals.Select(i => (tonicPc + i) % 12).ToHashSet();
    }

    private static int NormPc(int midiOrPc) => ((midiOrPc % 12) + 12) % 12;
    private static readonly HashSet<int> s_emptySet = new();
    private static HashSet<int> ToPcSet(int[] pcs) => pcs.Select(NormPc).ToHashSet();

    private static (HashSet<int> vTri, HashSet<int> v7, HashSet<int> iTri, HashSet<int> i7maj, HashSet<int> i7min) GetKeyReferenceSets(Key key)
    {
        int tonic = NormPc(key.TonicMidi);
        int dom = (tonic + 7) % 12;
        var vTri = new Chord(dom, ChordQuality.Major).PitchClasses().ToHashSet();
        var v7 = new Chord(dom, ChordQuality.DominantSeventh).PitchClasses().ToHashSet();
        var iTri = new Chord(tonic, key.IsMajor ? ChordQuality.Major : ChordQuality.Minor).PitchClasses().ToHashSet();
        var i7maj = new Chord(tonic, ChordQuality.MajorSeventh).PitchClasses().ToHashSet();
        var i7min = new Chord(tonic, ChordQuality.MinorSeventh).PitchClasses().ToHashSet();
        return (vTri, v7, iTri, i7maj, i7min);
    }

    private static bool SetEquals(HashSet<int> a, HashSet<int> b)
    {
        if (a.Count != b.Count) return false;
        foreach (var x in a) if (!b.Contains(x)) return false;
        return true;
    }

    private static bool SameKey(Key a, Key b) => a.IsMajor == b.IsMajor && NormPc(a.TonicMidi) == NormPc(b.TonicMidi);

    // Helpers: diatonic triad qualities by degree (0..6) for scoring secondary resolution targets
    private static ChordQuality TriadQualityForDegreeMajor(int degree)
        => degree switch
        {
            0 or 3 or 4 => ChordQuality.Major,
            1 or 2 or 5 => ChordQuality.Minor,
            6 => ChordQuality.Diminished,
            _ => ChordQuality.Unknown
        };

    private static ChordQuality TriadQualityForDegreeMinorHarm(int degree)
        => degree switch
        {
            0 or 3 => ChordQuality.Minor,
            2 or 5 => ChordQuality.Major,
            1 => ChordQuality.Diminished,
            4 => ChordQuality.Major,  // V
            6 => ChordQuality.Diminished,
            _ => ChordQuality.Unknown
        };
}
