using System;
using System.Linq;
using System.Collections.Generic;
using MusicTheory.Theory.Pitch;
using MTInterval = MusicTheory.Theory.Interval.FunctionalInterval;
using MusicTheory.Theory.Interval;

namespace MusicTheory.Theory.Chord
{
    // Interval 型は Interval namespace へ移動

    public class ChordFormula
    {
        public string Name { get; }
        public string Symbol { get; }
    public MTInterval[] CoreIntervals { get; }
    public MTInterval[] Tensions { get; }
        public string[] Aliases { get; }

    public ChordFormula(string name, string symbol, MTInterval[] coreIntervals, MTInterval[] tensions, params string[] aliases)
        {
            Name = name;
            Symbol = symbol;
            CoreIntervals = coreIntervals;
            Tensions = tensions;
            Aliases = aliases ?? Array.Empty<string>();
        }

    public string GetDisplayName()
        {
            var tensionPart = Tensions.Any() ? "(" + string.Join(",", Tensions.Select(t => t.DisplayName)) + ")" : string.Empty;
            return Symbol + tensionPart;
        }

        // 代表的コード（最低限）
    public static readonly ChordFormula Maj7 = new("Maj7", "maj7", new[] { new MTInterval(IntervalType.MajorThird), new MTInterval(IntervalType.PerfectFifth), new MTInterval(IntervalType.MajorSeventh) }, Array.Empty<MTInterval>(), "M7", "Δ7");
    public static readonly ChordFormula Min7 = new("Min7", "m7", new[] { new MTInterval(IntervalType.MinorThird), new MTInterval(IntervalType.PerfectFifth), new MTInterval(IntervalType.MinorSeventh) }, Array.Empty<MTInterval>(), "-7");
    public static readonly ChordFormula Dom7 = new("Dom7", "7", new[] { new MTInterval(IntervalType.MajorThird), new MTInterval(IntervalType.PerfectFifth), new MTInterval(IntervalType.MinorSeventh) }, Array.Empty<MTInterval>());
    }
    public class ChordName
    {
        public string Root { get; }
        public ChordFormula Formula { get; }

        public ChordName(string root, ChordFormula formula)
        {
            Root = root;
            Formula = formula;
        }

    public override string ToString() => $"{Root}{Formula.Symbol}";
    }

    public static class ChordFormulas
    {
        private static readonly ChordFormula[] formulas = new[]
        {
            new ChordFormula("Major Triad", "", new[] { I(IntervalType.MajorThird), I(IntervalType.PerfectFifth) }, Array.Empty<MTInterval>(), "maj", "M"),
            new ChordFormula("Minor Triad", "m", new[] { I(IntervalType.MinorThird), I(IntervalType.PerfectFifth) }, Array.Empty<MTInterval>(), "min", "-"),
            new ChordFormula("Diminished Triad", "dim", new[] { I(IntervalType.MinorThird), I(IntervalType.Tritone) }, Array.Empty<MTInterval>(), "o"),
            new ChordFormula("Augmented Triad", "aug", new[] { I(IntervalType.MajorThird), I(IntervalType.MinorSixth) }, Array.Empty<MTInterval>(), "+"),
            new ChordFormula("Major 7", "maj7", new[] { I(IntervalType.MajorThird), I(IntervalType.PerfectFifth), I(IntervalType.MajorSeventh)}, Array.Empty<MTInterval>(), "M7","Δ7"),
            new ChordFormula("Dominant 7", "7", new[] { I(IntervalType.MajorThird), I(IntervalType.PerfectFifth), I(IntervalType.MinorSeventh)}, Array.Empty<MTInterval>()),
            new ChordFormula("Minor 7", "m7", new[] { I(IntervalType.MinorThird), I(IntervalType.PerfectFifth), I(IntervalType.MinorSeventh)}, Array.Empty<MTInterval>(), "-7"),
            new ChordFormula("Half Diminished", "m7b5", new[] { I(IntervalType.MinorThird), I(IntervalType.Tritone), I(IntervalType.MinorSeventh)}, Array.Empty<MTInterval>(), "ø7"),
            new ChordFormula("Diminished 7", "dim7", new[] { I(IntervalType.MinorThird), I(IntervalType.Tritone), I(IntervalType.MajorSixth)}, Array.Empty<MTInterval>()),
            new ChordFormula("Dominant 9", "9", new[] { I(IntervalType.MajorThird), I(IntervalType.PerfectFifth), I(IntervalType.MinorSeventh)}, new[] { I(IntervalType.MajorNinth) }),
            new ChordFormula("Dominant 13", "13", new[] { I(IntervalType.MajorThird), I(IntervalType.PerfectFifth), I(IntervalType.MinorSeventh)}, new[] { I(IntervalType.MajorNinth), I(IntervalType.PerfectEleventh), I(IntervalType.MajorThirteenth) }),
            new ChordFormula("Altered Dom7 (b9 #9 #11 b13)", "7alt", new[] { I(IntervalType.MajorThird), I(IntervalType.PerfectFifth), I(IntervalType.MinorSeventh)}, new[] { I(IntervalType.MinorNinth), I(IntervalType.AugmentedNinth), I(IntervalType.AugmentedEleventh), I(IntervalType.MinorThirteenth) }),
            new ChordFormula("Major 13", "maj13", new[] { I(IntervalType.MajorThird), I(IntervalType.PerfectFifth), I(IntervalType.MajorSeventh)}, new[] { I(IntervalType.MajorNinth), I(IntervalType.PerfectEleventh), I(IntervalType.MajorThirteenth) }, "M13","Δ13"),
        };

    private static MTInterval I(IntervalType t) => new MTInterval(t);

        public static IEnumerable<ChordFormula> All => formulas;

        public static IEnumerable<ChordFormula> MatchByNotes(IEnumerable<int> inputNotes)
        {
            foreach (var formula in formulas)
            {
                var semitoneSet = formula.CoreIntervals.Concat(formula.Tensions).Select(i => i.Semitones).ToHashSet();
                if (inputNotes.All(note => semitoneSet.Contains(note)))
                    yield return formula;
            }
        }

        public static IEnumerable<ChordName> GenerateChordNames(string root)
        {
            foreach (var formula in formulas)
            {
                yield return new ChordName(root, formula);
            }
        }

        public static ChordName? ParseChordName(string chordText)
        {
            if (string.IsNullOrWhiteSpace(chordText)) return null;
            // 最長一致（シンボルが空の場合は triad などの判定として fallback）
            ChordName? best = null;
            int bestLen = -1;
            // シンボル長で降順ソートして衝突回避 (maj7 vs m7 など)
            var ordered = formulas.Select(f => new { Formula = f, Symbols = new[] { f.Symbol }.Concat(f.Aliases).Where(s => !string.IsNullOrEmpty(s)).OrderByDescending(s => s.Length).ToList() });
            foreach (var entry in ordered)
            {
                foreach (var symbol in entry.Symbols)
                {
                    if (chordText.EndsWith(symbol, StringComparison.Ordinal))
                    {
                        var root = chordText[..^symbol.Length];
                        if (root.Length > 0 && symbol.Length > bestLen)
                        {
                            best = new ChordName(root, entry.Formula);
                            bestLen = symbol.Length;
                            break; // 長いシンボル優先で決定
                        }
                    }
                }
            }
            // triad 等シンボル空の処理: 既に best があれば返す。なければ root 全体を root とみなし、空シンボルを持つ最初の formula を当てる。
            if (best != null) return best;
            var emptySymbolFormula = formulas.FirstOrDefault(f => string.IsNullOrEmpty(f.Symbol));
            if (emptySymbolFormula != null)
                return new ChordName(chordText, emptySymbolFormula);
            return null;
        }
    }

}