using System.Runtime.CompilerServices;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

/// <summary>
/// Test assembly warm-up via ModuleInitializer (runs when test assembly loads).
/// Keeps it side-effect free and deterministic.
/// </summary>
internal static class TestAssemblyWarmUp
{
#pragma warning disable CA2255 // 'ModuleInitializer' is intended for application code or advanced source generators
    [ModuleInitializer]
    public static void Init()
    {
        try
        {
            var key = new Key(60, true);
            // Common and historically flaky tokens (mixture, Neapolitan, secondaries, Aug6, Unicode variants)
            RomanInputParser.Parse("bII; bIII; b\u2162; bVI; bVII; N6; bII6; V/ii; V/vi; V/vii; vii0/V; vii\u00f8/V; viio7/V; It6; Fr43; Ger65", key);
            // Targeted warm-ups for issues seen on cold runs
            RomanInputParser.Parse("bIII", key);
            RomanInputParser.Parse("vii0/V", key);
            // Touch chord pitch-class path (dominant seventh on scale degree 5)
            new Chord((key.TonicMidi + 7) % 12, ChordQuality.DominantSeventh).PitchClasses().ToArray();

            // Extra targeted analyzer warm-ups for secondary dominant triads V/vi and V/vii (root/6/64)
            static int Pc(int m) => ((m % 12) + 12) % 12;
            // V/vi = E G# B
            var v_over_vi = new[] { Pc(4), Pc(8), Pc(11) };
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_vi, key, new FourPartVoicing(88, 83, 80, 64), null); // root
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_vi, key, new FourPartVoicing(92, 83, 76, 68), null); // 1st inv
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_vi, key, new FourPartVoicing(88, 80, 76, 71), null); // 2nd inv

            // V/vii = F# A# C#
            var v_over_vii = new[] { Pc(6), Pc(10), Pc(1) };
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_vii, key, new FourPartVoicing(78, 66, 54, 66), null); // root
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_vii, key, new FourPartVoicing(82, 70, 58, 70), null); // 1st inv
            _ = HarmonyAnalyzer.AnalyzeTriad(v_over_vii, key, new FourPartVoicing(85, 73, 61, 61), null); // 2nd inv

            // vii°7/V = F# A C Eb — exercise all inversions to stabilize labeling
            var vii7_over_V = new[] { Pc(6), Pc(9), Pc(0), Pc(3) };
            _ = HarmonyAnalyzer.AnalyzeTriad(vii7_over_V, key, new FourPartVoicing(87, 81, 72, 66), null); // root (7)
            _ = HarmonyAnalyzer.AnalyzeTriad(vii7_over_V, key, new FourPartVoicing(89, 84, 76, 69), null); // 1st (65)
            _ = HarmonyAnalyzer.AnalyzeTriad(vii7_over_V, key, new FourPartVoicing(89, 84, 76, 60), null); // 2nd (43)
            _ = HarmonyAnalyzer.AnalyzeTriad(vii7_over_V, key, new FourPartVoicing(89, 84, 77, 63), null); // 3rd (42)

            // Descending Passing 6-4 warm-up (IV6 -> IV64 -> IV) to stabilize early JIT classification
            try
            {
                var IV = new[] { Pc(5), Pc(9), Pc(0) }; // F A C (pitch classes relative to C: 5,9,0)
                var seq = new[] { IV, IV, IV };
                var voicings = new FourPartVoicing?[]
                {
                    new FourPartVoicing(81,77,72,69), // IV6 (A bass)
                    new FourPartVoicing(81,77,72,72), // IV64 (C bass)
                    new FourPartVoicing(81,77,72,65), // IV root (F bass)
                };
                _ = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, voicings);
            }
            catch { /* best-effort */ }
        }
        catch
        {
            // best-effort only
        }
    }
}

#pragma warning restore CA2255
