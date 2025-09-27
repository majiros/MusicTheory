using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MusicTheory.Theory.Harmony;
using System.Text.Json;
using System.Text.Json.Serialization;
using MusicTheory.Theory.Time;

static int Pc(int m) => ((m % 12) + 12) % 12;

static void PrintUsage()
{
    Console.WriteLine("MusicTheory.Cli - Harmony analysis demo");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project Samples/MusicTheory.Cli/MusicTheory.Cli.csproj -c Release [--key <NAME>] [--minor] [--pcs \"g1;g2;...\"] [--pcsJson \"g1;g2;...\"] [--roman \"I;V6;I64;V7;V65\"] [--romanJson \"I;V6;...\"] [--dur \"SPEC[;SPEC...]\"] [--durJson \"SPEC[;SPEC...]\"] [--hide64] [--cad64Dominant] [--v7n9] [--maj7Inv] [--preferMixture7] [--enforceN6] [--strictPacTriads] [--strictPacNoExt] [--tuplets star|paren] [--segments] [--trace] [--json] [--schema [util:dur|roman|pcs]] [--preset stable|permissive] [--minSegLen N] [--minSegConf X] [--window N] [--minSwitch N] [--prevBias N] [--initBias N] [--switchMargin N] [--outPenalty N]");
    Console.WriteLine();
    Console.WriteLine("Options:");
    Console.WriteLine("  --key <NAME>   Tonic name (C, C#, Db, D, Eb, E, F, F#, Gb, G, Ab, A, Bb, B). Default: C");
    Console.WriteLine("  --minor        Use minor key (default is major)");
    Console.WriteLine("  --pcs          Progression as groups of pitch classes, ';' between chords, ',' within: e.g. \"0,4,7; 7,11,2; 0,4,7\"");
    Console.WriteLine("  --pcsJson      Same as --pcs, but emit a JSON array with { input, pcs } where pcs are normalized to 0..11");
    Console.WriteLine("  --roman        Roman numeral sequence: 6/64, 7/65/43/42, secondaries (V/x, vii°/x, vii°7/x, viiø7/x), Aug6 tokens (It6/Fr43/Ger65), Neapolitan N (e.g. 'N6'); ASCII 'o'/'0' allowed for °/ø. E.g. \"I; V6; I64; V7; V65; V/ii; vii°7/V; It6; Ger65; N6\"");
    Console.WriteLine("  --romanJson    Same as --roman, but emit a JSON array with { input, pcs, bassPcHint? } using the current key/mode");
    Console.WriteLine("  --dur          Duration spec(s): abbrev (Q., E, W..), fraction (e.g. 1/8..), optional tuplets '(a:b)' or '*a:b'; multiple specs separated by ';'");
    Console.WriteLine("  --durJson      Same as --dur, but emit a JSON array with { input, ticks, fraction, notation }");
    Console.WriteLine("  --hide64       Hide non-cadential Passing/Pedal 6-4 entries");
    Console.WriteLine("  --cad64Dominant Label cadential 6-4 (I64→V→I) as V64-53 (dominant)");
    Console.WriteLine("  --v7n9         Display V9 as V7(9)");
    Console.WriteLine("  --maj7Inv      Include 'maj' in major-seventh inversions (e.g., IVmaj65)");
    Console.WriteLine("  --preferMixture7 Prefer mixture-7th (e.g., bVI7) over Aug6 when ambiguous");
    Console.WriteLine("  --enforceN6    Normalize Neapolitan triads to first inversion (bII6)");
    Console.WriteLine("  --strictPacTriads   PAC を V→I のプレーン triad のみに制限 (Imaj7 を PAC から除外)");
    Console.WriteLine("  --strictPacNoExt    PAC 判定で V7/V9/V7(9) を除外 (V triad のみ)");
    Console.WriteLine("  --strictPacSoprano  PAC 判定にソプラノ=トニック要件を追加");
    Console.WriteLine("  --strictPacLtResolve PAC 判定に導音→トニック(半音/全音)解決(ソプラノ)要件を追加 (暗黙で --strictPacSoprano を含む)");
    Console.WriteLine("  --tuplets      Tuplet notation style for durations: 'star' (=*a:b) or 'paren' (=(a:b)). Default: paren");
    Console.WriteLine("  --segments     Show estimated key segments (start..end and key)");
    Console.WriteLine("  --trace        Print per-chord estimator margins (debug)");
    Console.WriteLine("  --schema       Print the JSON schema for --json output and exit");
    Console.WriteLine("                 Optional value: util:dur | util:roman | util:pcs");
    Console.WriteLine("  --json         Emit results as JSON (chords, cadences, segments, trace)");
    Console.WriteLine("  --preset       Estimator preset: 'stable' or 'permissive' (explicit flags override preset)");
    Console.WriteLine("  --minSegLen    Minimum length (chords) to keep a key segment (optional)");
    Console.WriteLine("  --minSegConf   Minimum confidence [0..1] to keep a key segment (optional)");
    Console.WriteLine("  --window       KeyEstimator window radius (0=current only). Default: 1 (CLI)");
    Console.WriteLine("  --minSwitch    Forbid switching key before this chord index (MinSwitchIndex). Default: 0");
    Console.WriteLine("  --prevBias     Inertia: bias to keep previous key. Default: 1");
    Console.WriteLine("  --initBias     Bias toward the initial key. Default: 0");
    Console.WriteLine("  --switchMargin Hysteresis margin to switch to competitor. Default: 1");
    Console.WriteLine("  --outPenalty   Out-of-key penalty per non-diatonic pc (0 disables). Default: 0");
    Console.WriteLine();
    Console.WriteLine("If --pcs is omitted, a built-in demo (I64→V→I in C major) is used.");
}

static int KeyNameToMidi60Based(string name)
{
    var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
    {
        ["C"] = 0, ["B#"] = 0,
        ["C#"] = 1, ["Db"] = 1,
        ["D"] = 2,
        ["D#"] = 3, ["Eb"] = 3,
        ["E"] = 4, ["Fb"] = 4,
        ["F"] = 5, ["E#"] = 5,
        ["F#"] = 6, ["Gb"] = 6,
        ["G"] = 7,
        ["G#"] = 8, ["Ab"] = 8,
        ["A"] = 9,
        ["A#"] = 10, ["Bb"] = 10,
        ["B"] = 11, ["Cb"] = 11,
    };
    if (!map.TryGetValue(name.Trim(), out var off))
        throw new ArgumentException($"Unknown key name: {name}");
    return 60 + off; // around C4
}

static int[][] ParsePcsArg(string pcsArg)
{
    var chords = new List<int[]>();
    foreach (var g in pcsArg.Split(';'))
    {
        if (string.IsNullOrWhiteSpace(g)) continue;
        var items = g.Split(',');
        var list = new List<int>();
        foreach (var it in items)
        {
            if (string.IsNullOrWhiteSpace(it)) continue;
            if (!int.TryParse(it.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v))
                throw new ArgumentException($"Invalid pitch class: '{it}'");
            list.Add(Pc(v));
        }
        if (list.Count == 0)
            throw new ArgumentException("Empty chord group in --pcs.");
        chords.Add(list.ToArray());
    }
    if (chords.Count == 0)
        throw new ArgumentException("--pcs contained no chords.");
    return chords.ToArray();
}

// Parse args (very small manual parser)
string? keyName = null;
bool isMajor = true;
string? pcsArg = null;
string? pcsJsonArg = null;
string? romanArg = null;
string? romanJsonArg = null;
string? durArg = null;
string? durJsonArg = null;
bool hide64 = false;
bool cad64Dominant = false;
bool v7n9 = false;
bool maj7Inv = false;
bool preferMixture7 = false;
bool enforceN6 = false;
bool strictPacTriads = false;
bool strictPacNoExt = false;
bool strictPacSoprano = false;
bool strictPacLtResolve = false;
bool showHelp = false;
bool showSegments = false;
bool showTrace = false;
bool asJson = false;
bool printSchema = false;
string? schemaSelector = null; // null=main schema when printSchema is true; util:* for utility schemas
string tuplets = "paren"; // or "star"
string? preset = null; // 'stable' | 'permissive'
int? minSegLen = null;
double? minSegConf = null;
int? optWindow = null;
int? optMinSwitch = null;
int? optPrevBias = null;
int? optInitBias = null;
int? optSwitchMargin = null;
int? optOutPenalty = null;

for (int i = 0; i < args.Length; i++)
{
    var a = args[i];
    switch (a)
    {
        case "-h":
        case "--help":
            showHelp = true;
            break;
        case "--key":
            if (i + 1 >= args.Length) throw new ArgumentException("--key requires a value");
            keyName = args[++i];
            break;
        case "--minor":
            isMajor = false;
            break;
        case "--pcs":
            if (i + 1 >= args.Length) throw new ArgumentException("--pcs requires a value");
            pcsArg = args[++i];
            break;
        case "--pcsJson":
            if (i + 1 >= args.Length) throw new ArgumentException("--pcsJson requires a value");
            pcsJsonArg = args[++i];
            break;
        case "--roman":
            if (i + 1 >= args.Length) throw new ArgumentException("--roman requires a value");
            romanArg = args[++i];
            break;
        case "--romanJson":
            if (i + 1 >= args.Length) throw new ArgumentException("--romanJson requires a value");
            romanJsonArg = args[++i];
            break;
        case "--dur":
            if (i + 1 >= args.Length) throw new ArgumentException("--dur requires a value");
            durArg = args[++i];
            break;
        case "--durJson":
            if (i + 1 >= args.Length) throw new ArgumentException("--durJson requires a value");
            durJsonArg = args[++i];
            break;
        case "--hide64":
            hide64 = true;
            break;
        case "--cad64Dominant":
            cad64Dominant = true;
            break;
        case "--v7n9":
            v7n9 = true;
            break;
        case "--maj7Inv":
            maj7Inv = true;
            break;
        case "--preferMixture7":
            preferMixture7 = true;
            break;
        case "--enforceN6":
            enforceN6 = true;
            break;
        case "--strictPacTriads":
            strictPacTriads = true;
            break;
        case "--strictPacNoExt":
            strictPacNoExt = true;
            break;
        case "--strictPacSoprano":
            strictPacSoprano = true;
            break;
        case "--strictPacLtResolve":
            strictPacLtResolve = true; strictPacSoprano = true; // implies soprano requirement
            break;
        case "--tuplets":
            if (i + 1 >= args.Length) throw new ArgumentException("--tuplets requires a value");
            var tv = args[++i].Trim().ToLowerInvariant();
            if (tv != "star" && tv != "paren") throw new ArgumentException("--tuplets must be 'star' or 'paren'");
            tuplets = tv;
            break;
        case "--segments":
            showSegments = true;
            break;
        case "--trace":
            showTrace = true;
            break;
        case "--json":
            asJson = true;
            break;
        case "--preset":
            if (i + 1 >= args.Length) throw new ArgumentException("--preset requires a value");
            var pv = args[++i].Trim().ToLowerInvariant();
            if (pv != "stable" && pv != "permissive") throw new ArgumentException("--preset must be 'stable' or 'permissive'");
            preset = pv;
            break;
        case "--preset:stable":
            preset = "stable";
            break;
        case "--preset:permissive":
            preset = "permissive";
            break;
        case "--schema":
            printSchema = true;
            // optional selector value
            if (i + 1 < args.Length)
            {
                var peek = args[i + 1];
                if (!string.IsNullOrWhiteSpace(peek) && !peek.StartsWith("-"))
                {
                    var v = peek.Trim().ToLowerInvariant();
                    if (v == "util:dur" || v == "util:roman" || v == "util:pcs")
                    {
                        schemaSelector = v;
                        i++;
                    }
                }
            }
            break;
        case "--minSegLen":
            if (i + 1 >= args.Length) throw new ArgumentException("--minSegLen requires a value");
            if (!int.TryParse(args[++i], out var mlen) || mlen < 1) throw new ArgumentException("--minSegLen must be >= 1");
            minSegLen = mlen;
            break;
        case "--minSegConf":
            if (i + 1 >= args.Length) throw new ArgumentException("--minSegConf requires a value");
            if (!double.TryParse(args[++i], NumberStyles.Float, CultureInfo.InvariantCulture, out var mconf) || mconf < 0 || mconf > 1)
                throw new ArgumentException("--minSegConf must be in [0..1]");
            minSegConf = mconf;
            break;
        case "--window":
            if (i + 1 >= args.Length) throw new ArgumentException("--window requires a value");
            if (!int.TryParse(args[++i], out var w) || w < 0) throw new ArgumentException("--window must be >= 0");
            optWindow = w;
            break;
        case "--minSwitch":
        case "--minSwitchIndex":
            if (i + 1 >= args.Length) throw new ArgumentException("--minSwitch requires a value");
            if (!int.TryParse(args[++i], out var ms) || ms < 0) throw new ArgumentException("--minSwitch must be >= 0");
            optMinSwitch = ms;
            break;
        case "--prevBias":
            if (i + 1 >= args.Length) throw new ArgumentException("--prevBias requires a value");
            if (!int.TryParse(args[++i], out var pb)) throw new ArgumentException("--prevBias must be an integer");
            optPrevBias = pb;
            break;
        case "--initBias":
            if (i + 1 >= args.Length) throw new ArgumentException("--initBias requires a value");
            if (!int.TryParse(args[++i], out var ib)) throw new ArgumentException("--initBias must be an integer");
            optInitBias = ib;
            break;
        case "--switchMargin":
            if (i + 1 >= args.Length) throw new ArgumentException("--switchMargin requires a value");
            if (!int.TryParse(args[++i], out var sm)) throw new ArgumentException("--switchMargin must be an integer");
            optSwitchMargin = sm;
            break;
        case "--outPenalty":
            if (i + 1 >= args.Length) throw new ArgumentException("--outPenalty requires a value");
            if (!int.TryParse(args[++i], out var op)) throw new ArgumentException("--outPenalty must be an integer");
            optOutPenalty = op;
            break;
        default:
            // ignore unknown (could be dotnet passthrough)
            break;
    }
}

if (showHelp)
{
    PrintUsage();
    return;
}

try
{
    // Standalone utility mode: --pcsJson
    if (!string.IsNullOrWhiteSpace(pcsJsonArg))
    {
        var items = pcsJsonArg.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (items.Length == 0) throw new ArgumentException("--pcsJson contained no items");
        // Validate and normalize using existing parser
        var pcsSeq = ParsePcsArg(pcsJsonArg);
        var list = new List<object>(pcsSeq.Length);
        for (int i = 0; i < pcsSeq.Length; i++)
        {
            list.Add(new { input = items[i], pcs = pcsSeq[i] });
        }
        var optsJson = new JsonSerializerOptions { WriteIndented = true };
        Console.WriteLine(JsonSerializer.Serialize(list, optsJson));
        return;
    }

    // Standalone utility mode: --romanJson
    if (!string.IsNullOrWhiteSpace(romanJsonArg))
    {
        var items = romanJsonArg.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (items.Length == 0) throw new ArgumentException("--romanJson contained no items");
        var tonicMidiR = KeyNameToMidi60Based(keyName ?? "C");
        var keyR = new Key(tonicMidiR, isMajor);
        var parsed = RomanInputParser.Parse(romanJsonArg, keyR);
        var list = new List<object>(parsed.Length);
        int n = Math.Min(items.Length, parsed.Length);
        for (int i = 0; i < n; i++)
        {
            var p = parsed[i];
            int[] pcs = p.Pcs;
            int? b = p.BassPcHint;
            list.Add(new
            {
                input = items[i],
                pcs = pcs,
                bassPcHint = (int?)b
            });
        }
        var optsJson = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        Console.WriteLine(JsonSerializer.Serialize(list, optsJson));
        return;
    }

    // Standalone utility mode: --dur
    if (!string.IsNullOrWhiteSpace(durArg))
    {
        bool extended = string.Equals(tuplets, "star", StringComparison.OrdinalIgnoreCase);
        var items = durArg.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (items.Length == 0) throw new ArgumentException("--dur contained no items");
        Console.WriteLine($"Tuplets style: {(extended ? "star" : "paren")}\n");
        for (int i=0;i<items.Length;i++)
        {
            var spec = items[i];
            if (!DurationNotation.TryParse(spec, out var d))
                throw new ArgumentException($"Invalid duration: '{spec}'");
            string notation = DurationNotation.ToNotation(d, extendedTuplets: extended);
            var frac = d.WholeFraction;
            Console.WriteLine($"[{i}] in='{spec}' -> ticks={d.Ticks}, fraction={frac.Numerator}/{frac.Denominator}, notation='{notation}'");
        }
        return;
    }

    // Standalone utility mode: --durJson
    if (!string.IsNullOrWhiteSpace(durJsonArg))
    {
        bool extended = string.Equals(tuplets, "star", StringComparison.OrdinalIgnoreCase);
        var items = durJsonArg.Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (items.Length == 0) throw new ArgumentException("--durJson contained no items");
        var list = new List<object>(items.Length);
        for (int i = 0; i < items.Length; i++)
        {
            var spec = items[i];
            if (!DurationNotation.TryParse(spec, out var d))
                throw new ArgumentException($"Invalid duration: '{spec}'");
            var frac = d.WholeFraction;
            list.Add(new
            {
                input = spec,
                ticks = d.Ticks,
                fraction = new { numerator = frac.Numerator, denominator = frac.Denominator },
                notation = DurationNotation.ToNotation(d, extendedTuplets: extended)
            });
        }
        var optsJson = new JsonSerializerOptions { WriteIndented = true };
        Console.WriteLine(JsonSerializer.Serialize(list, optsJson));
        return;
    }

    if (printSchema)
    {
        string baseDir = AppContext.BaseDirectory;
        string fileName = schemaSelector switch
        {
            "util:dur" => "music-theory.cli.util.dur.v1.schema.json",
            "util:roman" => "music-theory.cli.util.roman.v1.schema.json",
            "util:pcs" => "music-theory.cli.util.pcs.v1.schema.json",
            _ => "music-theory.cli.v1.schema.json"
        };

        string[] candidates = new[]
        {
            System.IO.Path.Combine(baseDir, fileName),
            System.IO.Path.Combine(baseDir, "schema", fileName)
        };
        string? path = candidates.FirstOrDefault(System.IO.File.Exists);
        if (path == null)
        {
            // Fallback: relative to repo (development)
            var devPath = System.IO.Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Samples", "MusicTheory.Cli", "schema", fileName);
            if (System.IO.File.Exists(devPath)) path = devPath;
        }
        if (path != null)
        {
            Console.WriteLine(System.IO.File.ReadAllText(path));
            return;
        }
        else
        {
            Console.Error.WriteLine($"Schema file not found. Expected at 'schema/{fileName}'.");
            Environment.ExitCode = 2;
            return;
        }
    }
    var tonicMidi = KeyNameToMidi60Based(keyName ?? "C");
    var key = new Key(tonicMidi, isMajor);

    int[][] seq;
    FourPartVoicing?[] voicings;

    if (!string.IsNullOrWhiteSpace(pcsArg))
    {
        seq = ParsePcsArg(pcsArg);
        voicings = new FourPartVoicing?[seq.Length]; // no voicings provided
    }
    else if (!string.IsNullOrWhiteSpace(romanArg))
    {
        var parsed = RomanInputParser.Parse(romanArg!, key);
        seq = parsed.Select(p => p.Pcs).ToArray();
        // Create minimal voicing hints: put bass at desired bass pc if possible
        voicings = new FourPartVoicing?[seq.Length];
        for (int i = 0; i < parsed.Length; i++)
        {
            if (parsed[i].BassPcHint is int bPc)
            {
                // Build a crude four-part voicing around MIDI ~C4 area ensuring bass pc matches
                int bass = 48 + bPc; // near C3..B3 range
                int[] pcs = seq[i];
                int pick(int pc, int baseMidi) => baseMidi + ((pc - (baseMidi % 12) + 12) % 12);
                // Heuristic: if token suggests mixture seventh on bVI/bII/N (e.g., bVI7, bII7, N7), avoid soprano=b6 to prevent Aug6 preference heuristics.
                string tok = parsed[i].Token?.Trim() ?? string.Empty;
                bool isMixtureSeventhLikely = tok.StartsWith("bVI", StringComparison.OrdinalIgnoreCase)
                                              || tok.Equals("N7", StringComparison.OrdinalIgnoreCase)
                                              || tok.StartsWith("bII", StringComparison.OrdinalIgnoreCase)
                                              || tok.StartsWith("bVII", StringComparison.OrdinalIgnoreCase);
                int tBase = 60;
                int t = pick(pcs[0], tBase);
                int a = pick(pcs.Length > 1 ? pcs[1] : pcs[0], 64);
                int s;
                if (isMixtureSeventhLikely)
                {
                    int tonicPc = ((key.TonicMidi % 12) + 12) % 12;
                    int b6pc = (tonicPc + 8) % 12;
                    s = pick(b6pc, 71); // ensure soprano=b6 to suppress Aug6
                }
                else
                {
                    s = pick(pcs.Length > 2 ? pcs[2] : pcs[0], 67);
                }
                voicings[i] = new FourPartVoicing(s, a, t, bass);
            }
        }
    }
    else
    {
        // Built-in demo: I64 → V → I in C major with voicings
        var I = new[] { Pc(60), Pc(64), Pc(67) };
        var V = new[] { Pc(67), Pc(71), Pc(62) };
        seq = new[] { I, V, I };
        voicings = new FourPartVoicing?[]
        {
            new FourPartVoicing(81, 76, 72, 67), // I64 (bass=G)
            new FourPartVoicing(83, 79, 74, 67), // V
            new FourPartVoicing(84, 76, 72, 60), // I
        };
        key = new Key(60, true); // ensure C major for demo
    }

    var opts = new HarmonyOptions
    {
        ShowNonCadentialSixFour = !hide64,
        PreferCadentialSixFourAsDominant = cad64Dominant,
        PreferV7Paren9OverV9 = v7n9,
        IncludeMajInSeventhInversions = maj7Inv,
        PreferMixtureSeventhOverAugmentedSixthWhenAmbiguous = preferMixture7,
        EnforceNeapolitanFirstInversion = enforceN6,
        StrictPacPlainTriadsOnly = strictPacTriads,
        StrictPacDisallowDominantExtensions = strictPacNoExt,
        StrictPacRequireSopranoTonic = strictPacSoprano,
        StrictPacRequireSopranoLeadingToneResolution = strictPacLtResolve,
    };

    var (result, cadences) = ProgressionAnalyzer.AnalyzeWithDetailedCadences(seq, key, opts, voicings);

    if (!asJson)
    {
        Console.WriteLine($"Key: {(isMajor ? "Major" : "Minor")} {keyName ?? "C"}");
    Console.WriteLine($"Options: hide64={(hide64 ? "on" : "off")}, cad64Dominant={(cad64Dominant ? "on" : "off")}, V9={(v7n9 ? "V7(9)" : "V9")}, maj7Inv={(maj7Inv ? "on" : "off")}, preferMixture7={(preferMixture7 ? "on" : "off")}, enforceN6={(enforceN6 ? "on" : "off")}, strictPacTriads={(strictPacTriads ? "on" : "off")}, strictPacNoExt={(strictPacNoExt ? "on" : "off")}, strictPacSoprano={(strictPacSoprano ? "on" : "off")}, strictPacLtResolve={(strictPacLtResolve ? "on" : "off")}, tuplets={tuplets}");
        Console.WriteLine();

        Console.WriteLine("Chords:");
        for (int i = 0; i < result.Chords.Count; i++)
        {
            var r = result.Chords[i];
            Console.WriteLine($"[{i}] {r.RomanText}  ({r.Function})");
        }

        Console.WriteLine();
        Console.WriteLine("Cadences:");
        try
        {
            bool any = false;
            foreach (var c in cadences)
            {
                any = true;
                Console.WriteLine($"@{c.IndexFrom}: {c.Type}  PAC={(c.IsPerfectAuthentic ? "Yes" : "No")}  Cad64={(c.HasCadentialSixFour ? "Yes" : "No")}  SixFour={c.SixFour}");
            }
            if (!any)
            {
                Console.WriteLine("(none)");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"(unavailable: {ex.Message})");
        }
        Console.WriteLine();
    }

    List<object>? jsonSegments = null;
    List<object>? jsonTrace = null;
    List<object>? jsonKeys = null;
    object? jsonEstimator = null;
    if (showSegments || showTrace || asJson)
    {
        try
        {
            // Preset defaults
            int presetWindow = 1;
            int presetMinSwitch = 0;
            int presetPrevBias = 1;
            int presetInitBias = 0;
            int presetSwitchMargin = 1;
            int presetOutPenalty = 0;
            int presetMinSegLen = 2;
            double presetMinSegConf = 0.2;

            if (preset == "permissive")
            {
                presetWindow = 2;
                presetMinSwitch = 0;
                presetPrevBias = 0;
                presetInitBias = 0;
                presetSwitchMargin = 0;
                presetOutPenalty = 0;
                presetMinSegLen = 1;
                presetMinSegConf = 0.0;
            }
            else if (preset == "stable")
            {
                presetWindow = 1;
                presetMinSwitch = 2;
                presetPrevBias = 2;
                presetInitBias = 0;
                presetSwitchMargin = 2;
                presetOutPenalty = 0;
                presetMinSegLen = 2;
                presetMinSegConf = 0.2;
            }

            var keyOpts = new KeyEstimator.Options
            {
                Window = optWindow ?? presetWindow,
                MinSwitchIndex = optMinSwitch ?? presetMinSwitch,
                PrevKeyBias = optPrevBias ?? presetPrevBias,
                InitialKeyBias = optInitBias ?? presetInitBias,
                SwitchMargin = optSwitchMargin ?? presetSwitchMargin,
                OutOfKeyPenaltyPerPc = optOutPenalty ?? presetOutPenalty,
                CollectTrace = true
            };
            List<(int start, int end, Key key, double confidence)> segs;
            List<Key> keys;
            List<KeyEstimator.TraceEntry> trace;
            // Apply segmentation thresholds, with explicit flags overriding preset
            int effMinSegLen = minSegLen ?? presetMinSegLen;
            double effMinSegConf = minSegConf ?? presetMinSegConf;
            if (minSegLen is int ml || minSegConf is double mc || preset != null)
            {
                var res = ProgressionAnalyzer.AnalyzeWithKeyEstimateAndModulation(seq, key, keyOpts, out trace, voicings, minLength: effMinSegLen, minConfidence: effMinSegConf);
                segs = res.segments;
                keys = res.keys;
            }
            else
            {
                var res = ProgressionAnalyzer.AnalyzeWithKeyEstimate(seq, key, keyOpts, out trace, voicings, withConfidence: true);
                segs = res.segments;
                keys = res.keys;
            }
            if (!asJson && (showSegments || showTrace))
            {
                Console.WriteLine($"Estimator: preset={(preset ?? "(none)")}, window={keyOpts.Window}, minSwitch={keyOpts.MinSwitchIndex}, prevBias={keyOpts.PrevKeyBias}, initBias={keyOpts.InitialKeyBias}, switchMargin={keyOpts.SwitchMargin}, outPenalty={keyOpts.OutOfKeyPenaltyPerPc}");
                Console.WriteLine();
            }

            if (showSegments && !asJson)
            {
                Console.WriteLine("Key Segments:");
                foreach (var s in segs)
                {
                    var tonic = (s.key.TonicMidi % 12 + 12) % 12;
                    string tn = new[] {"C","C#","D","Eb","E","F","F#","G","Ab","A","Bb","B"}[tonic];
                    Console.WriteLine($"[{s.start}..{s.end}] {tn} {(s.key.IsMajor ? "Major" : "Minor")} (conf={s.confidence:F2})");
                }
                Console.WriteLine();
            }
            if (showTrace && trace is not null && !asJson)
            {
                Console.WriteLine("Estimator Trace (margin = (max-second)/max):");
                for (int i = 0; i < trace.Count; i++)
                {
                    int max = Math.Max(0, trace[i].MaxScore);
                    int second = Math.Max(0, trace[i].SecondBestScore);
                    double margin = max > 0 ? Math.Clamp((max - second) / (double)max, 0.0, 1.0) : 0.0;
                    Console.WriteLine($"#{i}: max={max}, second={second}, margin={margin:F2}");
                }
                Console.WriteLine();
            }

            if (asJson)
            {
                jsonSegments = segs.Select(s => new
                {
                    start = s.start,
                    end = s.end,
                    key = new { tonic = ((s.key.TonicMidi % 12 + 12) % 12), mode = s.key.IsMajor ? "major" : "minor" },
                    confidence = s.confidence
                } as object).ToList();
                if (trace is not null)
                {
                    jsonTrace = new List<object>();
                    for (int i = 0; i < trace.Count; i++)
                    {
                        int max = Math.Max(0, trace[i].MaxScore);
                        int second = Math.Max(0, trace[i].SecondBestScore);
                        double margin = max > 0 ? Math.Clamp((max - second) / (double)max, 0.0, 1.0) : 0.0;
                        jsonTrace.Add(new { index = i, max, second, margin });
                    }
                }
                // per-chord estimated keys
                jsonKeys = new List<object>();
                for (int i = 0; i < keys.Count; i++)
                {
                    var k = keys[i];
                    jsonKeys.Add(new
                    {
                        index = i,
                        key = new { tonic = ((k.TonicMidi % 12 + 12) % 12), mode = k.IsMajor ? "major" : "minor" }
                    });
                }
                // estimator options actually used
                jsonEstimator = new
                {
                    window = keyOpts.Window,
                    minSwitch = keyOpts.MinSwitchIndex,
                    prevBias = keyOpts.PrevKeyBias,
                    initBias = keyOpts.InitialKeyBias,
                    switchMargin = keyOpts.SwitchMargin,
                    outPenalty = keyOpts.OutOfKeyPenaltyPerPc
                };
            }
        }
        catch (Exception ex)
        {
            if (!asJson)
            {
                Console.WriteLine($"Estimator: unavailable ({ex.Message})");
                Console.WriteLine();
            }
        }
    }

    if (asJson)
    {
        var json = new
        {
            version = 1,
            key = new { tonic = ((key.TonicMidi % 12 + 12) % 12), mode = isMajor ? "major" : "minor" },
            options = new { hide64, cad64Dominant, v9 = v7n9 ? "V7(9)" : "V9", maj7Inv, preferMixture7, enforceN6, tuplets, preset },
            chords = result.Chords.Select((c, i) => new { index = i, roman = c.RomanText, function = c.Function.ToString(), warnings = (IReadOnlyList<string>)c.Warnings, errors = (IReadOnlyList<string>)c.Errors }).ToArray(),
            cadences = cadences.Select(c => new { indexFrom = c.IndexFrom, type = c.Type.ToString(), pac = c.IsPerfectAuthentic, cad64 = c.HasCadentialSixFour, sixFour = c.SixFour.ToString() }).ToArray(),
            estimator = jsonEstimator,
            keys = jsonKeys,
            segments = jsonSegments,
            trace = jsonTrace
        };
        var optsJson = new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        Console.WriteLine(JsonSerializer.Serialize(json, optsJson));
    }
    else
    {
        Console.WriteLine("Done.");
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    Console.Error.WriteLine();
    PrintUsage();
    Environment.ExitCode = 1;
}

// Roman parser helpers removed in favor of RomanInputParser
