using System;
using System.Collections.Generic;
using System.Linq;
using MusicTheory.Theory.Chord;
using MusicTheory.Theory.Scale;
using MusicTheory.Theory.Interval;

namespace MusicTheory.Theory.Analysis;

public record struct ScaleFitDetail(
    string ScaleName,
    IReadOnlyList<int> Covered,
    IReadOnlyList<int> Uncovered,
    IReadOnlyList<int> AvoidNotes,
    IReadOnlyList<int> ColorNotes,
    double RawCoverage,
    double AvoidPenalty,
    double ColorBoost,
    double TensionCoverage,
    IReadOnlyList<int> PresentTensions,
    IReadOnlyList<int> MissingTensions,
    double CoreCoverage,
    IReadOnlyList<int> PresentCores,
    IReadOnlyList<int> MissingCores,
    IReadOnlyList<string> AlternativeScaleNames
);

public record struct ChordCandidate(
    string RootName,
    ChordFormula Formula,
    int CoverageScore,
    double CompletionRatio,
    ModalScale? SuggestedScale,
    IReadOnlyList<ModalScale> AlternativeScales,
    IReadOnlyList<AlternativeScaleRankInfo> AlternativeScaleRanks,
    int[] NormalizedSet,
    string FormulaSignature,
    string InputSignature,
    string? ScaleName,
    double ScaleFitScore,
    IReadOnlyList<int> MissingIntervals,
    IReadOnlyList<string> MissingIntervalNames,
    IReadOnlyList<int> MissingCoreIntervals,
    IReadOnlyList<int> MissingTensionIntervals,
    double AvoidPenalty,
    double ColorBoost,
    double TotalScore,
    ScaleFitDetail? ScaleDetail
);

public record struct ChordAnalyzerOptions(
    bool RankResults = true,
    double MinCompletion = 0.5,
    bool AllowInversions = true,
    double CoverageWeight = 1.0,
    double ScaleFitWeight = 1.0,
    double AvoidPenaltyWeight = 1.0,
    double ColorBoostWeight = 1.0,
    double CoreCoverageWeight = 0.5,
        double TensionCoverageWeight = 0.5,
        int[]? ScaleRankingCoreSemitones = null
);

public record struct AlternativeScaleRankInfo(
    string ScaleName,
    double SharedRatio,
    double TensionPresence,
    int SharedCount,
    int CoveredTensionCount,
    int TotalTensionCount,
        double ExactCoverBonus,
    double Priority
);

public static class ChordAnalyzer
{
    private static readonly string[] RootNames = {"C","C#","D","Eb","E","F","F#","G","Ab","A","Bb","B"};

    private static readonly (ChordFormula Formula, string Signature, HashSet<int> IntervalSet)[] FormulaSignatures =
        ChordFormulas.All.Select(f => (f,
            SignatureFromIntervals(f.CoreIntervals.Concat(f.Tensions)),
            f.CoreIntervals.Concat(f.Tensions).Select(i => i.Semitones % 12).Where(s=>s!=0).ToHashSet()
        )).ToArray();

    private static string SignatureFromIntervals(IEnumerable<FunctionalInterval> intervals)
        => AnalysisUtils.Signature(intervals.Select(i => i.Semitones));

    private static readonly Dictionary<string, ModalScale[]> ScaleHints = new()
    {
        {"7", new[]{ ExtendedScales.Mixolydian, ExtendedScales.LydianDominant, ExtendedScales.Altered, ExtendedScales.WholeTone }},
        {"m7", new[]{ ExtendedScales.Dorian, ExtendedScales.Aeolian }},
        {"maj7", new[]{ ExtendedScales.Ionian, ExtendedScales.Lydian, ExtendedScales.BebopMajor }},
        {"7alt", new[]{ ExtendedScales.Altered }},
        {"m7b5", new[]{ ExtendedScales.Locrian, ExtendedScales.DiminishedHalfWhole }},
        {"dim7", new[]{ ExtendedScales.DiminishedWholeHalf }},
        {"9", new[]{ ExtendedScales.Mixolydian, ExtendedScales.LydianDominant }},
        {"13", new[]{ ExtendedScales.Mixolydian, ExtendedScales.LydianDominant, ExtendedScales.BebopDominant }},
        {"maj13", new[]{ ExtendedScales.Ionian, ExtendedScales.Lydian }},
    };

    public static IEnumerable<ChordCandidate> Analyze(IEnumerable<int> pitchClasses, ChordAnalyzerOptions? options = null)
    {
        var opt = options ?? new ChordAnalyzerOptions();
        var pcs = pitchClasses.Select(AnalysisUtils.Mod12).Distinct().ToArray();
        if (pcs.Length == 0) return Enumerable.Empty<ChordCandidate>();

        var candidates = new List<ChordCandidate>();
        foreach (var root in pcs)
        {
            if (!opt.AllowInversions && root != pcs.Min()) continue;
            var normalized = AnalysisUtils.NormalizeToRoot(pcs, root);
            var sig = AnalysisUtils.Signature(normalized.Where(pc => pc != 0));
            foreach (var (formula, fullSig, intervalSet) in FormulaSignatures)
            {
                var formulaSig = AnalysisUtils.Signature(intervalSet);
                var presentSet = sig.Split('-', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToHashSet();
                // 入力 ⊆ フォーミュラ (不足は可)
                if (!presentSet.All(intervalSet.Contains)) continue;
                // コア (除: 完全5度) はすべて含まれること
                var coreSemis = formula.CoreIntervals.Select(i=>i.Semitones %12).Where(s=>s!=0).ToHashSet();
                var essentialCore = coreSemis.Where(s=>s!=7).ToList();
                if (!essentialCore.All(presentSet.Contains)) continue;

                int coverage = presentSet.Count(intervalSet.Contains);
                double completion = intervalSet.Count == 0 ? 1.0 : coverage / (double)intervalSet.Count;
                if (completion < opt.MinCompletion) continue;
                var (alternativeScales, altRankInfos) = RankAlternativeScales(formula.Symbol, normalized, opt);
                var scale = alternativeScales.FirstOrDefault();
                double avoidPenalty = 0.0; double colorBoost = 0.0;
                var scaleFit = scale == null ? 0.0 : ScaleFitScore(scale, normalized, out avoidPenalty, out colorBoost);
                var missing = intervalSet.Except(presentSet).ToList();
                var coreSet = coreSemis;
                var tensionSet = formula.Tensions.Select(i=>i.Semitones%12).Where(s=>s!=0).ToHashSet();
                var missingCore = missing.Where(coreSet.Contains).ToList();
                var missingTension = missing.Where(tensionSet.Contains).ToList();
                var missingNames = missing.Select(ToIntervalDisplayName).ToList();
                double coreCoverageScore = 0.0; double tensionCoverageScore = 0.0;
                if (opt.CoreCoverageWeight > 0 || opt.TensionCoverageWeight > 0)
                {
                    var presentTensionsForScore = tensionSet.Count==0 ? 0 : tensionSet.Count(t => presentSet.Contains(t));
                    var presentCoresForScore = coreSemis.Count==0 ? 0 : coreSemis.Count(c => presentSet.Contains(c));
                    var coreCoverageRatio = coreSemis.Count==0 ? 1.0 : presentCoresForScore / (double)coreSemis.Count;
                    var tensionCoverageRatio = tensionSet.Count==0 ? 1.0 : presentTensionsForScore / (double)tensionSet.Count;
                    coreCoverageScore = coreCoverageRatio * opt.CoreCoverageWeight;
                    tensionCoverageScore = tensionCoverageRatio * opt.TensionCoverageWeight;
                }
                double total = (completion * coverage * opt.CoverageWeight)
                               + (scaleFit * opt.ScaleFitWeight)
                               - (avoidPenalty * opt.AvoidPenaltyWeight)
                               + (colorBoost * opt.ColorBoostWeight)
                               + coreCoverageScore + tensionCoverageScore;
                ScaleFitDetail? scaleDetail = null;
                if (scale != null)
                {
                    var semis = scale.GetSemitoneSet();
                    var covered = normalized.Where(semis.Contains).ToHashSet();
                    var uncovered = normalized.Where(n => !semis.Contains(n)).ToList();
                    var detailAvoid = new List<int>();
                    var detailColor = new List<int>();
                    if (avoidPenalty > 0 && normalized.Contains(5)) detailAvoid.Add(5);
                    if (colorBoost > 0 && normalized.Contains(6)) detailColor.Add(6);
                    var presentTensions = formula.Tensions.Select(i=>i.Semitones%12).Where(s=>s!=0 && presentSet.Contains(s)).ToList();
                    var missingTensions = tensionSet.Intersect(missing).ToList();
                    double tensionCoverage = tensionSet.Count==0 ? 1.0 : presentTensions.Count / (double)tensionSet.Count;
                    var presentCores = coreSemis.Where(presentSet.Contains).ToList();
                    var missingCores = coreSemis.Where(missing.Contains).ToList();
                    double coreCoverage = coreSemis.Count==0 ? 1.0 : presentCores.Count / (double)coreSemis.Count;
                    scaleDetail = new ScaleFitDetail(
                        scale.Name,
                        covered.ToList(),
                        uncovered,
                        detailAvoid,
                        detailColor,
                        covered.Count / (double)normalized.Length,
                        avoidPenalty,
                        colorBoost,
                        tensionCoverage,
                        presentTensions,
                        missingTensions,
                        coreCoverage,
                        presentCores,
                        missingCores,
                        alternativeScales.Select(s=>s.Name).ToList()
                    );
                }
                candidates.Add(new ChordCandidate(
                    RootNames[root],
                    formula,
                    coverage,
                    completion,
                    scale,
                    alternativeScales,
                    altRankInfos.Cast<AlternativeScaleRankInfo>().ToList(),
                    normalized,
                    formulaSig,
                    sig,
                    scale?.Name,
                    scaleFit,
                    missing,
                    missingNames,
                    missingCore,
                    missingTension,
                    avoidPenalty,
                    colorBoost,
                    total,
                    scaleDetail
                ));
            }

        }
        if (opt.RankResults)
            return candidates.OrderByDescending(c => c.TotalScore);
        return candidates;
    }

    public static IEnumerable<ChordCandidate> AnalyzeRanked(IEnumerable<int> pitchClasses, ChordAnalyzerOptions? options = null)
        => Analyze(pitchClasses, options ?? new ChordAnalyzerOptions()).OrderByDescending(c => c.TotalScore);

    public static List<ChordCandidate> ToListRanked(IEnumerable<int> pitchClasses, ChordAnalyzerOptions? options = null)
        => AnalyzeRanked(pitchClasses, options).ToList();

    private static bool FormulaMatches(string candidateSig, HashSet<int> formulaIntervals)
    {
        var candidateSet = candidateSig.Split('-', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToHashSet();
        // 条件: フォーミュラ側の全インターバルが候補内に含まれていれば許容（候補に追加ノート可）
        return formulaIntervals.All(candidateSet.Contains);
    }

    private static int CoverageScore(string candidateSig, string formulaSig)
    {
        var c = candidateSig.Split('-', StringSplitOptions.RemoveEmptyEntries);
        var f = formulaSig.Split('-', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        return c.Count(f.Contains);
    }

    private static ModalScale? SuggestScale(string symbol, int[] normalizedSet)
        => SuggestScales(symbol, normalizedSet).FirstOrDefault();

    public static IEnumerable<ModalScale> SuggestScales(string symbol, int[] normalizedSet)
    {
        if (!ScaleHints.TryGetValue(symbol, out var list)) return Enumerable.Empty<ModalScale>();
        // 完全包含するものを全部返し、なければヒントリスト全体を返す
        var matches = list.Where(scale => {
            var semis = scale.GetSemitoneSet();
            return normalizedSet.All(pc => semis.Contains(pc));
        }).ToList();
        return matches.Any() ? matches : list;
    }

    private static (List<ModalScale> Scales, List<AlternativeScaleRankInfo> Infos) RankAlternativeScales(string symbol, int[] normalizedSet, ChordAnalyzerOptions opt)
    {
        var scales = SuggestScales(symbol, normalizedSet).ToList();
        var normSet = normalizedSet.Where(x=>x!=0).ToHashSet();
        // 実入力ベース tension: オプション指定コア集合以外の音をテンション扱い
        var defaultCore = new HashSet<int>{3,4,7,10,11};
        var coreLike = new HashSet<int>(opt.ScaleRankingCoreSemitones ?? defaultCore.ToArray());
        var inputTensions = normSet.Where(n => !coreLike.Contains(n)).ToHashSet();
        int totalTension = inputTensions.Count;
        var ranked = new List<(ModalScale Scale, AlternativeScaleRankInfo Info)>();
        foreach (var s in scales)
        {
            var semis = s.GetSemitoneSet();
            int shared = normSet.Count(semis.Contains);
            double sharedRatio = shared / (double)(normSet.Count==0?1:normSet.Count);
            int coveredTension = totalTension==0 ? 0 : inputTensions.Count(t=>semis.Contains(t));
            double tensionPresence = totalTension==0 ? 1.0 : coveredTension / (double)totalTension;
            double exactCoverBonus = sharedRatio==1 ? 2 : 0;
            double priority = exactCoverBonus + tensionPresence + sharedRatio;
            ranked.Add((s, new AlternativeScaleRankInfo(s.Name, sharedRatio, tensionPresence, shared, coveredTension, totalTension, exactCoverBonus, priority)));
        }
        var ordered = ranked.OrderByDescending(r=>r.Info.Priority).ThenByDescending(r=>r.Info.SharedCount).ToList();
        return (ordered.Select(r=>r.Scale).ToList(), ordered.Select(r=>r.Info).ToList());
    }
    

    // JSON DTO (循環参照 / 冗長オブジェクト防止)
    public record struct ChordCandidateDto(
        string Root,
        string Symbol,
        double TotalScore,
        double CompletionRatio,
        double ScaleFitScore,
        double AvoidPenalty,
        double ColorBoost,
        IReadOnlyList<int> MissingCore,
        IReadOnlyList<int> MissingTension,
        IReadOnlyList<string> MissingNames,
        string? PrimaryScale,
        IReadOnlyList<string> AlternativeScales,
        IReadOnlyList<AlternativeScaleRankInfo> AlternativeScaleRanks,
        ScaleDetailDto? ScaleDetail
    );

    public record struct ScaleDetailDto(
        string ScaleName,
        double RawCoverage,
        double TensionCoverage,
        double CoreCoverage,
        IReadOnlyList<int> Covered,
        IReadOnlyList<int> Uncovered,
        IReadOnlyList<int> AvoidNotes,
        IReadOnlyList<int> ColorNotes,
        IReadOnlyList<int> PresentTensions,
        IReadOnlyList<int> MissingTensions,
        IReadOnlyList<int> PresentCores,
        IReadOnlyList<int> MissingCores,
        IReadOnlyList<string> AlternativeScaleNames
    );

    public static ChordCandidateDto ToDto(this ChordCandidate c)
        => new(
            c.RootName,
            c.Formula.Symbol,
            c.TotalScore,
            c.CompletionRatio,
            c.ScaleFitScore,
            c.AvoidPenalty,
            c.ColorBoost,
            c.MissingCoreIntervals,
            c.MissingTensionIntervals,
            c.MissingIntervalNames,
            c.ScaleName,
            c.AlternativeScales.Select(s=>s.Name).ToList(),
            c.AlternativeScaleRanks,
            c.ScaleDetail.HasValue ? new ScaleDetailDto(
                c.ScaleDetail.Value.ScaleName,
                c.ScaleDetail.Value.RawCoverage,
                c.ScaleDetail.Value.TensionCoverage,
                c.ScaleDetail.Value.CoreCoverage,
                c.ScaleDetail.Value.Covered,
                c.ScaleDetail.Value.Uncovered,
                c.ScaleDetail.Value.AvoidNotes,
                c.ScaleDetail.Value.ColorNotes,
                c.ScaleDetail.Value.PresentTensions,
                c.ScaleDetail.Value.MissingTensions,
                c.ScaleDetail.Value.PresentCores,
                c.ScaleDetail.Value.MissingCores,
                c.ScaleDetail.Value.AlternativeScaleNames
            ) : null
        );

    public static IEnumerable<ChordCandidateDto> ToDtos(this IEnumerable<ChordCandidate> source)
        => source.Select(c=>c.ToDto()).ToList();

    private static double ScaleFitScore(ModalScale scale, int[] normalizedSet, out double avoidPenalty, out double colorBoost)
    {
        var semis = scale.GetSemitoneSet();
        int covered = normalizedSet.Count(semis.Contains);
        bool hasMajorThird = normalizedSet.Contains(4);
        bool hasNatural11 = normalizedSet.Contains(5);
        bool hasSharp11 = normalizedSet.Contains(6);
        bool hasMinorThird = normalizedSet.Contains(3);
        bool hasMinorSeventh = normalizedSet.Contains(10);
        bool hasMajorSix = normalizedSet.Contains(9);
        bool hasMinorSix = normalizedSet.Contains(8);
        avoidPenalty = 0.0;
        colorBoost = 0.0;
        if (hasMajorThird && hasNatural11 && !hasSharp11 && scale.Name.Contains("Ionian", StringComparison.OrdinalIgnoreCase))
            avoidPenalty += 0.2;
        if (hasSharp11 && scale.Name.Contains("Lydian", StringComparison.OrdinalIgnoreCase))
            colorBoost += 0.15;
        if (hasMinorThird && hasMinorSeventh && hasNatural11)
        {
            if (scale.Name.Contains("Dorian", StringComparison.OrdinalIgnoreCase) && hasMajorSix)
                colorBoost += 0.12;
            if (scale.Name.Contains("Aeolian", StringComparison.OrdinalIgnoreCase) && hasMajorSix)
                avoidPenalty += 0.15;
            if (scale.Name.Contains("Dorian", StringComparison.OrdinalIgnoreCase) && hasMinorSix)
                avoidPenalty += 0.12;
        }
        if (hasMajorThird && hasMinorSeventh)
        {
            if (hasMajorSix && scale.Name.Contains("Mixolydian", StringComparison.OrdinalIgnoreCase))
                colorBoost += 0.1;
            if (hasMinorSix && scale.Name.Contains("Mixolydian", StringComparison.OrdinalIgnoreCase))
                avoidPenalty += 0.1;
        }
        return covered / (double)normalizedSet.Length;
    }

    private static string ToIntervalDisplayName(int semitone)
    {
        int s = ((semitone % 12) + 12) % 12;
        return s switch
        {
            1 => "♭9",
            2 => "9",
            3 => "m3",
            4 => "M3",
            5 => "11",
            6 => "#11",
            7 => "5",
            8 => "♭13",
            9 => "13",
            10 => "♭7",
            11 => "M7",
            _ => s.ToString()
        };
    }
}
