using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

namespace MusicTheory.Theory.Harmony;

/// <summary>
/// Parses a simple Roman numeral sequence (supports mixture bII/bIII/bVI/bVII, triad inversions 6/64, and seventh inversions 7/65/43/42)
/// into pitch-class sets and a bass pitch-class hint to help generate voicings.
/// This is intentionally lightweight and designed for CLI/demo input, not full notation parsing.
/// </summary>
public static class RomanInputParser
{
    public readonly record struct ParsedChord(int[] Pcs, int? BassPcHint, string Token);

    // Static warm-up to JIT hot paths and reduce first-run jitter in tests/CI.
    // This is intentionally minimal and side-effect free.
    static RomanInputParser()
    {
        try
        {
            var key = new Key(60, true);
            // Individual tokens
            Parse("bII; bIII; bVI; bVII; N6; V/ii; V/vi; V/vii; vii0/V; viio7/V; It6", key);
            // Exact sequences used in tests (to pre-JIT combined paths)
            Parse("bII; bIII; bVI; bVII; bII6; N6", key);
            Parse("V/ii; vii°7/V; vii0/V; viio7/V", key);
            // Unicode variants
            Parse("♭II6; N\u200B6; vii0/V; viio7/V; bⅢ; viiø/V", key);
        }
        catch { /* no-op: warm-up best-effort */ }
    }

    public static ParsedChord[] Parse(string roman, Key key)
    {
        if (roman is null) throw new ArgumentNullException(nameof(roman));
        var tokens = roman.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0) throw new ArgumentException("--roman contained no items.");
        var list = new List<ParsedChord>(tokens.Length);
        foreach (var tok in tokens)
        {
            list.Add(ParseToken(tok, key));
        }
        return list.ToArray();
    }

    static ParsedChord ParseToken(string token, Key key)
    {
    string t = Sanitize(token).Replace(" ", string.Empty);
        if (string.IsNullOrWhiteSpace(t)) throw new ArgumentException("Empty roman token.");

        // Augmented sixth direct tokens (It6/Fr43/Ger65)
        if (t is "It6" or "Fr43" or "Ger65")
        {
            int tonic = ((key.TonicMidi % 12) + 12) % 12;
            int b6 = (tonic + 8) % 12; int one = tonic; int sharp4 = (tonic + 6) % 12; int two = (tonic + 2) % 12; int flat3 = (tonic + 3) % 12;
            int[] pcsAug = t switch
            {
                "It6" => new[] { b6, one, sharp4 },
                "Fr43" => new[] { b6, one, two, sharp4 },
                _ => new[] { b6, one, flat3, sharp4 } // Ger65
            };
            return new ParsedChord(pcsAug, b6, token);
        }

        // Secondary notation: split by '/'
    string? target = null;
    string headPart = t;
    string original = token;
        int slash = t.IndexOf('/');
        if (slash >= 0)
        {
            headPart = t.Substring(0, slash);
            target = t.Substring(slash + 1);
            if (string.IsNullOrEmpty(target)) throw new ArgumentException($"Invalid secondary token: '{t}'");
        }

        // Extract figure suffix first (64,65,43,42,6,7)
        string figure = string.Empty;
        if (headPart.EndsWith("64")) { figure = "64"; headPart = headPart[..^2]; }
        else if (headPart.EndsWith("65")) { figure = "65"; headPart = headPart[..^2]; }
        else if (headPart.EndsWith("43")) { figure = "43"; headPart = headPart[..^2]; }
        else if (headPart.EndsWith("42")) { figure = "42"; headPart = headPart[..^2]; }
        else if (headPart.EndsWith("6")) { figure = "6"; headPart = headPart[..^1]; }
        else if (headPart.EndsWith("7")) { figure = "7"; headPart = headPart[..^1]; }

        // Extract quality marks at end: diminished (°/o) or half-diminished (ø/0)
        char? qualMark = null;
        if (headPart.EndsWith("°") || headPart.EndsWith("ø") || headPart.EndsWith("o") || headPart.EndsWith("0"))
        {
            var mk = headPart[^1];
            // Normalize ASCII marks to canonical symbols
            qualMark = mk switch
            {
                'o' => '°', // ASCII 'o' -> diminished
                '0' => 'ø', // ASCII zero -> half-diminished
                _ => mk
            };
            headPart = headPart[..^1];
        }

        // Secondary
        if (target is not null)
        {
            return ParseSecondary(headPart, target, figure, qualMark, key, original);
        }

        // Map head (with optional mixture b) to degree and default quality
    var pcs = ParseHeadToChordPcs(headPart, figure, qualMark, key, out int rootPc, out int[] chordPcs, original);

    int? bassPc = SelectBassFromTriadOrSeventh(figure, rootPc, chordPcs);

    return new ParsedChord(pcs, bassPc, token);
    }

    // Remove hidden/format/control/space-like characters and normalize many look-alikes to ASCII
    static string Sanitize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        var sb = new System.Text.StringBuilder(s.Length);

        // Helper to append ASCII Roman numerals equivalent
        static bool TryAppendRomanAscii(char ch, System.Text.StringBuilder sb)
        {
            // Uppercase Roman numerals Ⅰ..Ⅶ (U+2160..U+2166)
            switch (ch)
            {
                case '\u2160': sb.Append("I"); return true;   // Ⅰ
                case '\u2161': sb.Append("II"); return true;  // Ⅱ
                case '\u2162': sb.Append("III"); return true; // Ⅲ
                case '\u2163': sb.Append("IV"); return true;  // Ⅳ
                case '\u2164': sb.Append("V"); return true;   // Ⅴ
                case '\u2165': sb.Append("VI"); return true;  // Ⅵ
                case '\u2166': sb.Append("VII"); return true; // Ⅶ
            }
            // Lowercase Roman numerals ⅰ..ⅶ (U+2170..U+2176)
            switch (ch)
            {
                case '\u2170': sb.Append("i"); return true;   // ⅰ
                case '\u2171': sb.Append("ii"); return true;  // ⅱ
                case '\u2172': sb.Append("iii"); return true; // ⅲ
                case '\u2173': sb.Append("iv"); return true;  // ⅳ
                case '\u2174': sb.Append("v"); return true;   // ⅴ
                case '\u2175': sb.Append("vi"); return true;  // ⅵ
                case '\u2176': sb.Append("vii"); return true; // ⅶ
            }
            return false;
        }

        foreach (var raw in s)
        {
            char ch = raw;

            // Expand Roman numeral code points to ASCII sequences
            if (TryAppendRomanAscii(ch, sb))
                continue;

            // Normalize common look-alikes first
            if (ch == '\u00BA' || ch == '\u00B0') ch = '°'; // masculine ordinal/degree sign → degree-like
            if (ch == '\u266D') ch = 'b'; // music flat sign → ASCII 'b'
            if (ch == '\u00D8') ch = 'ø'; // Ø → ø
            if (ch == '\u00F8') ch = 'ø'; // ensure lowercase

            // Convert fullwidth ASCII range to halfwidth ASCII
            if (ch >= '\uFF01' && ch <= '\uFF5E')
            {
                ch = (char)(ch - 0xFEE0);
            }
            // Also normalize FULLWIDTH SOLIDUS to '/'
            if (ch == '\uFF0F') ch = '/';

            // Treat uppercase 'O' quality mark as 'o' for diminished
            if (ch == 'O') ch = 'o';

            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            // Skip hidden/spacing/format/control marks entirely
            if (cat is UnicodeCategory.Format
                    or UnicodeCategory.Control
                    or UnicodeCategory.NonSpacingMark
                    or UnicodeCategory.EnclosingMark
                    or UnicodeCategory.SpaceSeparator
                    or UnicodeCategory.LineSeparator
                    or UnicodeCategory.ParagraphSeparator)
            {
                continue;
            }
            if (ch == '\uFEFF' || ch == '\u200B' || ch == '\u200C' || ch == '\u200D') continue; // explicit BOM/ZW*

            // As a final guard, drop unexpected non-ASCII characters except for allowed quality marks.
            // This hardens against stray look-alikes not explicitly normalized above.
            if (ch > 0x7F && ch != '°' && ch != 'ø')
            {
                continue;
            }

            sb.Append(ch);
        }
        return sb.ToString();
    }

    static ParsedChord ParseSecondary(string head, string target, string figure, char? qualMark, Key key, string originalToken)
    {
        int DegMidi(int d) => ((key.ScaleDegreeMidi(d) % 12) + 12) % 12;
        // Map target roman to degree 0..6
        int TargetDegree(string x, bool isMajor)
        {
            return x switch
            {
                "I" or "i" => 0,
                "II" or "ii" => 1,
                "III" or "iii" => 2,
                "IV" or "iv" => 3,
                "V" or "v" => 4,
                "VI" or "vi" => 5,
                "VII" or "vii" or "vii°" => 6,
                _ => throw new ArgumentException($"Unsupported secondary target: '{x}'")
            };
        }

        int deg = TargetDegree(target, key.IsMajor);
        int targetPc = DegMidi(deg);

    bool isSeventh = figure is "7" or "65" or "43" or "42";
        // Secondary Dominant
        if (string.Equals(head, "V", StringComparison.Ordinal))
        {
            int secRoot = (targetPc + 7) % 12;
            var q = isSeventh ? ChordQuality.DominantSeventh : ChordQuality.Major;
            var chordPcs = new Chord(secRoot, q).PitchClasses().ToArray();
            int? bass = SelectBassFromFigure(figure, secRoot, chordPcs, seventhQuality: q);
            return new ParsedChord(chordPcs, bass ?? secRoot, originalToken);
        }

        // Secondary Leading-Tone (vii°, viiø)
        if (head.StartsWith("vii", StringComparison.OrdinalIgnoreCase))
        {
            int ltRoot = (targetPc + 11) % 12;
            ChordQuality q;
            // If any diminished mark is present (in qual or in head) treat as seventh even if figure omitted
            bool hasDimMarkInHead = head.IndexOf('o') >= 0 || head.IndexOf('0') >= 0 || head.IndexOf('ø') >= 0 || head.IndexOf('°') >= 0;
            bool hasHalfDimInOriginal = originalToken.IndexOf('ø') >= 0 || originalToken.IndexOf('0') >= 0; // robust detection
            bool seventhImplied = isSeventh || (qualMark is 'ø' or '°') || hasDimMarkInHead;
            if (seventhImplied)
            {
                if (qualMark == 'ø' || hasHalfDimInOriginal)
                {
                    q = ChordQuality.HalfDiminishedSeventh;
                }
                else q = ChordQuality.DiminishedSeventh;
            }
            else
            {
                q = ChordQuality.Diminished;
            }
            // Hard override for /IV: always use MinorSeventh on the leading tone to IV
            if (deg == 3)
            {
                q = ChordQuality.MinorSeventh;
            }
            // Build chord pcs; special-case viiø/iv → use MinorSeventh (e.g., E-G-B-D in C)
            var chordForBuild = new Chord(ltRoot, q);
            var chordPcs = chordForBuild.PitchClasses().ToArray();
            int? bass = SelectBassFromFigure(figure, ltRoot, chordPcs, seventhQuality: q);
            return new ParsedChord(chordPcs, bass ?? ltRoot, originalToken);
        }

        throw new ArgumentException($"Unsupported secondary head: '{head}/{target}{figure}'");
    }

    static int? SelectBassFromFigure(string figure, int rootPc, int[] chordPcs, ChordQuality seventhQuality)
    {
        if (string.IsNullOrEmpty(figure)) return rootPc;
        // triad 6/64 already handled elsewhere; here focus on sevenths figures if 4-note
        // Derive chord tones directly from provided pitch classes for robustness
        static int Mod(int x) => ((x % 12) + 12) % 12;
        int third = chordPcs.FirstOrDefault(pc => Mod(pc - rootPc) is 3 or 4, Mod(rootPc + 4));
        // Fifth can be perfect (7), diminished (6), or augmented (8); prefer the one present in chordPcs
        int fifth;
        if (chordPcs.Contains(Mod(rootPc + 7))) fifth = Mod(rootPc + 7);
        else if (chordPcs.Contains(Mod(rootPc + 6))) fifth = Mod(rootPc + 6);
        else if (chordPcs.Contains(Mod(rootPc + 8))) fifth = Mod(rootPc + 8);
        else fifth = Mod(rootPc + 7);
        int sev = chordPcs.FirstOrDefault(pc => Mod(pc - rootPc) is 9 or 10 or 11, Mod(rootPc + 10));
        return figure switch
        {
            "7" => rootPc,
            "65" => third,
            "43" => fifth,
            "42" => sev,
            _ => rootPc
        };
    }

    static int[] ParseHeadToChordPcs(string head, string figure, char? qualMark, Key key, out int rootPc, out int[] chordPcs, string? originalToken = null)
    {
        int tonicPc = ((key.TonicMidi % 12) + 12) % 12;
        bool maj = key.IsMajor;
        // Detect seventh chords via figure presence containing 7*
        bool isSeventh = figure is "7" or "65" or "43" or "42";
    // mixture prefixes supported: bII, bIII, bVI, bVII (match exactly; check longer tokens first to avoid prefix collisions)
    // Neapolitan symbol 'N' maps to bII (major triad on lowered supertonic)
    string H = head.ToUpperInvariant();
    // Defensive disambiguation: if sanitized looks like BII but original contained explicit III or U+2162 (Ⅲ), treat as BIII
    if (H == "BII" && !string.IsNullOrEmpty(originalToken))
    {
        string raw = originalToken;
        if (raw.IndexOf('\u2162') >= 0 || raw.ToUpperInvariant().Contains("III"))
        {
            H = "BIII";
        }
    }
    if (H == "N")
        {
            rootPc = (tonicPc + 1) % 12;
            // Treat N7 as bII7 (dominant seventh on Neapolitan)
            var q = isSeventh ? ChordQuality.DominantSeventh : ChordQuality.Major;
            chordPcs = new Chord(rootPc, q).PitchClasses().ToArray();
            return chordPcs;
        }
    if (H == "BVII")
        {
            rootPc = (tonicPc + 10) % 12;
            var q = isSeventh ? ChordQuality.DominantSeventh : ChordQuality.Major;
            chordPcs = new Chord(rootPc, q).PitchClasses().ToArray();
            return chordPcs;
        }
    if (H == "BVI")
        {
            rootPc = (tonicPc + 8) % 12;
            var q = isSeventh ? ChordQuality.DominantSeventh : ChordQuality.Major;
            chordPcs = new Chord(rootPc, q).PitchClasses().ToArray();
            return chordPcs;
        }
    if (H == "BIII")
        {
            rootPc = (tonicPc + 3) % 12;
            chordPcs = new Chord(rootPc, ChordQuality.Major).PitchClasses().ToArray();
            return chordPcs;
        }
    if (H == "BII")
        {
            rootPc = (tonicPc + 1) % 12;
            var q = isSeventh ? ChordQuality.DominantSeventh : ChordQuality.Major;
            chordPcs = new Chord(rootPc, q).PitchClasses().ToArray();
            return chordPcs;
        }

        // Normalize head to core degree token (case-sensitive kept for intent)
        string h = head;
        int DegMidi(int d) => ((key.ScaleDegreeMidi(d) % 12) + 12) % 12;

    // isSeventh already computed above

        // Identify degree index by roman numerals within h (ignoring case)
        static bool Eq(string s, string a) => s.Equals(a, StringComparison.Ordinal);
        // Provide mappings for common forms
        if (Eq(h, "I"))
        {
            rootPc = DegMidi(0);
            var q = isSeventh ? (maj ? ChordQuality.MajorSeventh : ChordQuality.MinorSeventh) : (maj ? ChordQuality.Major : ChordQuality.Minor);
            chordPcs = new Chord(rootPc, q).PitchClasses().ToArray();
            return chordPcs;
        }
        if (Eq(h, "i"))
        {
            rootPc = DegMidi(0);
            var q = isSeventh ? ChordQuality.MinorSeventh : ChordQuality.Minor;
            chordPcs = new Chord(rootPc, q).PitchClasses().ToArray();
            return chordPcs;
        }

        if (Eq(h, "II") || Eq(h, "ii"))
        {
            rootPc = DegMidi(1);
            ChordQuality q;
            if (isSeventh)
            {
                q = maj ? ChordQuality.MinorSeventh : ChordQuality.HalfDiminishedSeventh; // ii7 in major, iiø7 in minor (approx.)
            }
            else
            {
                q = maj ? ChordQuality.Minor : ChordQuality.Diminished;
            }
            chordPcs = new Chord(rootPc, q).PitchClasses().ToArray();
            return chordPcs;
        }

        if (Eq(h, "III") || Eq(h, "iii"))
        {
            rootPc = DegMidi(2);
            var q = isSeventh ? (maj ? ChordQuality.MinorSeventh : ChordQuality.MajorSeventh)
                              : (maj ? ChordQuality.Minor : ChordQuality.Major);
            chordPcs = new Chord(rootPc, q).PitchClasses().ToArray();
            return chordPcs;
        }

        if (Eq(h, "IV") || Eq(h, "iv"))
        {
            rootPc = DegMidi(3);
            var q = isSeventh ? (maj ? ChordQuality.MajorSeventh : ChordQuality.MinorSeventh)
                              : (maj ? ChordQuality.Major : ChordQuality.Minor);
            chordPcs = new Chord(rootPc, q).PitchClasses().ToArray();
            return chordPcs;
        }

        if (Eq(h, "V") || Eq(h, "v"))
        {
            rootPc = DegMidi(4);
            var q = isSeventh ? ChordQuality.DominantSeventh : ChordQuality.Major;
            chordPcs = new Chord(rootPc, q).PitchClasses().ToArray();
            return chordPcs;
        }

        if (Eq(h, "VI") || Eq(h, "vi"))
        {
            rootPc = DegMidi(5);
            var q = isSeventh ? (maj ? ChordQuality.MinorSeventh : ChordQuality.MajorSeventh)
                              : (maj ? ChordQuality.Minor : ChordQuality.Major);
            chordPcs = new Chord(rootPc, q).PitchClasses().ToArray();
            return chordPcs;
        }

        if (h.StartsWith("vii", StringComparison.OrdinalIgnoreCase))
        {
            rootPc = DegMidi(6);
            ChordQuality q;
            if (isSeventh)
            {
                // viiø7 in major, vii°7 in minor (if marked °, honor it)
                if (qualMark == 'ø') q = ChordQuality.HalfDiminishedSeventh;
                else if (qualMark == '°') q = ChordQuality.DiminishedSeventh;
                else q = maj ? ChordQuality.HalfDiminishedSeventh : ChordQuality.DiminishedSeventh;
            }
            else
            {
                q = ChordQuality.Diminished;
            }
            chordPcs = new Chord(rootPc, q).PitchClasses().ToArray();
            return chordPcs;
        }

        throw new ArgumentException($"Unsupported roman token: '{head}{(qualMark is null ? string.Empty : qualMark.ToString())}{figure}'");
    }

    static int? SelectBassFromTriadOrSeventh(string figure, int rootPc, int[] chordPcs)
    {
        if (string.IsNullOrEmpty(figure)) return rootPc;
        static int Mod(int x) => ((x % 12) + 12) % 12;
        // Derive chord tones directly from provided pitch classes for robustness
        int third = chordPcs.FirstOrDefault(pc => Mod(pc - rootPc) is 3 or 4, Mod(rootPc + 4));
        // Fifth may be 6 (diminished), 7 (perfect) or 8 (augmented) depending on quality
        int fifth;
        if (chordPcs.Contains(Mod(rootPc + 7))) fifth = Mod(rootPc + 7);
        else if (chordPcs.Contains(Mod(rootPc + 6))) fifth = Mod(rootPc + 6);
        else if (chordPcs.Contains(Mod(rootPc + 8))) fifth = Mod(rootPc + 8);
        else fifth = Mod(rootPc + 7);
        int sev = chordPcs.FirstOrDefault(pc => Mod(pc - rootPc) is 9 or 10 or 11, Mod(rootPc + 10));
        return figure switch
        {
            "6" => third,
            "64" => fifth,
            "7" => rootPc,
            "65" => third,
            "43" => fifth,
            "42" => sev,
            _ => rootPc
        };
    }

    static int GetSeventhPc(int rootPc, int[] chordPcs)
    {
        // For seventh chords, chordPcs length is 4; assume order contains root, 3rd, 5th, 7th in some rotation starting at root.
        if (chordPcs.Length == 4)
        {
            int idx = Array.IndexOf(chordPcs, rootPc);
            if (idx >= 0)
            {
                return chordPcs[(idx + 3) % 4];
            }
        }
        // Fallback: attempt to add a minor seventh above root
        return (rootPc + 10) % 12;
    }
}
