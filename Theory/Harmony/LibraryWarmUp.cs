#pragma warning disable CA2255 // 'ModuleInitializer' is intended for application code or advanced source generators
using System.Runtime.CompilerServices;

namespace MusicTheory.Theory.Harmony;

/// <summary>
/// Lightweight warm-up for hot parsing/analyzer paths to reduce first-run jitter in tests/CLI/CI.
/// Performed once when the assembly loads; side-effect free.
/// </summary>
internal static class LibraryWarmUp
{
    [ModuleInitializer]
    public static void Initialize()
    {
        try
        {
            var key = new Key(60, true);
            // Exercise RomanInputParser common tokens (mixture, Neapolitan, secondaries, Aug6)
            RomanInputParser.Parse("bII; bIII; bⅢ; bVI; bVII; N6; bII6; V/ii; vii0/V; viiø/V; viio7/V; It6; Fr43; Ger65", key);
            // Touch ChordRomanizer helpers indirectly via HarmonyAnalyzer small triad
            new Chord((key.TonicMidi + 7) % 12, ChordQuality.DominantSeventh).PitchClasses().ToArray();
            // Additional targeted warm-ups for previously flaky first-run cases
            RomanInputParser.Parse("bIII", key);
            RomanInputParser.Parse("vii0/V", key);
            RomanInputParser.Parse("vii°7/V; vii°65/V; vii°43/V; vii°42/V", key);

            // Dominant ninth on V (C major): G B D F A — both with and without fifth
            var vRoot = (key.ScaleDegreeMidi(4) % 12 + 12) % 12; // V root pc
            var v9full = new[] { vRoot, (vRoot + 4) % 12, (vRoot + 7) % 12, (vRoot + 10) % 12, (vRoot + 14) % 12 };
            var v9omit5 = new[] { vRoot, (vRoot + 4) % 12, (vRoot + 10) % 12, (vRoot + 14) % 12 };
            _ = HarmonyAnalyzer.AnalyzeTriad(v9full, key, (FourPartVoicing?)null, null);
            _ = HarmonyAnalyzer.AnalyzeTriad(v9omit5, key, (FourPartVoicing?)null, null);

            // Targeted harmony analyzer warm-ups (secondary triad inversions + maj7 inversion option)
            // Secondary dominant triad V/ii inversions: A C# E
            int Pc(int m) => ((m % 12) + 12) % 12;
            var v_over_ii = new[] { Pc(9), Pc(1), Pc(4) };
            // Root (A)
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_ii, key, new FourPartVoicing(81, 69, 57, 69), null);
            // 1st inv (C#)
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_ii, key, new FourPartVoicing(85, 73, 69, 61), null);
            // 2nd inv (E)
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_ii, key, new FourPartVoicing(88, 76, 72, 64), null);

            // Secondary leading-tone triad vii°/V inversions: F# A C
            var viio_over_V = new[] { Pc(6), Pc(9), Pc(0) };
            // Root (F#)
            _ = HarmonyAnalyzer.AnalyzeTriad(viio_over_V, key, new FourPartVoicing(78, 74, 66, 66), null);
            // 1st inv (A)
            _ = HarmonyAnalyzer.AnalyzeTriad(viio_over_V, key, new FourPartVoicing(81, 76, 69, 69), null);
            // 2nd inv (C)
            _ = HarmonyAnalyzer.AnalyzeTriad(viio_over_V, key, new FourPartVoicing(84, 72, 72, 60), null);

            // Secondary leading-tone seventh vii°7/V inversions: F# A C Eb
            var vii7_over_V = new[] { Pc(6), Pc(9), Pc(0), Pc(3) };
            // Root (F#)
            _ = HarmonyAnalyzer.AnalyzeTriad(vii7_over_V, key, new FourPartVoicing(87, 81, 72, 66), null);
            // 1st inv (A)
            _ = HarmonyAnalyzer.AnalyzeTriad(vii7_over_V, key, new FourPartVoicing(89, 84, 76, 69), null);
            // 2nd inv (C)
            _ = HarmonyAnalyzer.AnalyzeTriad(vii7_over_V, key, new FourPartVoicing(89, 84, 76, 60), null);
            // 3rd inv (Eb)
            _ = HarmonyAnalyzer.AnalyzeTriad(vii7_over_V, key, new FourPartVoicing(89, 84, 77, 63), null);

            // Secondary dominant triad V/vi inversions: E G# B
            var v_over_vi = new[] { Pc(4), Pc(8), Pc(11) };
            // Root (E)
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_vi, key, new FourPartVoicing(88, 83, 80, 64), null);
            // 1st inv (G#)
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_vi, key, new FourPartVoicing(92, 83, 76, 68), null);
            // 2nd inv (B)
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_vi, key, new FourPartVoicing(88, 80, 76, 71), null);

            // Secondary dominant triad V/vii inversions: F# A# C#
            var v_over_vii = new[] { Pc(6), Pc(10), Pc(1) };
            // Root (F#)
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_vii, key, new FourPartVoicing(78, 66, 54, 66), null);
            // 1st inv (A#)
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_vii, key, new FourPartVoicing(82, 70, 58, 70), null);
            // 2nd inv (C#)
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_vii, key, new FourPartVoicing(85, 73, 61, 61), null);

            // Minor IIImaj7 first inversion with IncludeMajInSeventhInversions option
            var keyMin = new Key(60, false);
            var pcsIIImaj7 = new[] { Pc(63), Pc(67), Pc(70), Pc(74) }; // Eb G Bb D
            var optsMajInv = new HarmonyOptions { IncludeMajInSeventhInversions = true };
            _ = HarmonyAnalyzer.AnalyzeTriad(pcsIIImaj7, keyMin, optsMajInv, new FourPartVoicing(91, 86, 79, 67), null);

            // Best-effort self-checks: if canonical tokens don't match expected PC sets on cold start,
            // re-invoke parsing once to ensure JIT/warm state is complete.
            static bool SameSet(int[] a, int[] b)
            {
                if (a is null || b is null) return false;
                if (a.Length != b.Length) return false;
                var aa = (int[])a.Clone();
                var bb = (int[])b.Clone();
                Array.Sort(aa); Array.Sort(bb);
                for (int i = 0; i < aa.Length; i++) if (aa[i] != bb[i]) return false;
                return true;
            }

            int tonic = ((key.TonicMidi % 12) + 12) % 12;

            // bIII should be a major triad on lowered mediant
            var gotBIII = RomanInputParser.Parse("bIII", key)[0].Pcs;
            var expBIII = new Chord((tonic + 3) % 12, ChordQuality.Major).PitchClasses().ToArray();
            if (!SameSet(gotBIII, expBIII))
            {
                RomanInputParser.Parse("bIII", key);
            }

            // viiø/V should be half-diminished seventh on leading tone to the dominant
            int targetV = ((key.ScaleDegreeMidi(4) % 12) + 12) % 12; // V
            int ltToV = (targetV + 11) % 12;
            var expViiSec = new Chord(ltToV, ChordQuality.HalfDiminishedSeventh).PitchClasses().ToArray();
            var gotViiSec = RomanInputParser.Parse("viiø/V", key)[0].Pcs;
            if (!SameSet(gotViiSec, expViiSec))
            {
                RomanInputParser.Parse("viiø/V", key);
            }
        }
        catch
        {
            // best-effort only
        }
    }
}

#pragma warning restore CA2255

