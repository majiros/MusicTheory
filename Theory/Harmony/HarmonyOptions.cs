namespace MusicTheory.Theory.Harmony;

// Public, instance-style options to control disambiguation and preference rules.
// Defaults mirror existing, validated behavior. Pass a customized instance to analyzer APIs
// to tweak behavior without mutating global state.
public sealed class HarmonyOptions
{
    /// <summary>
    /// Prefer Augmented Sixth (It6/Fr43/Ger65) over bVI7 when bass equals b6 and the pitch-class set matches.
    /// Default: true.
    /// </summary>
    public bool PreferAugmentedSixthOverMixtureWhenBassFlat6 { get; set; } = true;

    /// <summary>
    /// When the soprano is also b6, suppress Augmented Sixth labeling to keep the common Ab7 voicing as bVI7.
    /// Default: true.
    /// </summary>
    public bool DisallowAugmentedSixthWhenSopranoFlat6 { get; set; } = true;

    /// <summary>
    /// When an augmented-sixth set and a mixture seventh (e.g., bVI7) are enharmonically identical,
    /// prefer labeling as the mixture seventh instead of Augmented Sixth. This changes only the
    /// disambiguation order; exact matching rules remain the same. Default: false (prefer Aug6 first).
    /// </summary>
    public bool PreferMixtureSeventhOverAugmentedSixthWhenAmbiguous { get; set; } = false;

    /// <summary>
    /// Prefer vii°7/V as the target for ambiguous fully-diminished seventh sets (secondary leading-tone preference).
    /// Default: true.
    /// </summary>
    public bool PreferSecondaryLeadingToneTargetV { get; set; } = true;

    /// <summary>
    /// In minor keys, prefer the diatonic iiø7 over any secondary interpretation.
    /// Default: true.
    /// </summary>
    public bool PreferDiatonicIiHalfDimInMinor { get; set; } = true;

    /// <summary>
    /// Neapolitan (bII) enforcement: when enabled, triadic bII is labeled as bII6 even if voicing indicates root or second inversion.
    /// Default: false (preserve factual inversion labeling and only emit warnings).
    /// </summary>
    public bool EnforceNeapolitanFirstInversion { get; set; } = false;

    /// <summary>
    /// Show non-cadential 6-4 classifications (Passing / Pedal) in detailed cadence results.
    /// When false, non-cadential entries (Type==None with Passing/Pedal) are suppressed.
    /// Cadential 6-4 (I64→V→I) still contributes to the following cadence entry (e.g., Authentic) and remains attached to it.
    /// Default: true.
    /// </summary>
    public bool ShowNonCadentialSixFour { get; set; } = true;

    /// <summary>
    /// If true, prefer the notation "V7(9)" instead of "V9" for dominant ninth chords.
    /// This only affects the roman text label; the functional analysis remains Dominant (V).
    /// Default is false (use "V9").
    /// </summary>
    public bool PreferV7Paren9OverV9 { get; set; } = false;

    /// <summary>
    /// Treat cadential 6-4 (typically notated as I64 preceding V→I) as a dominant appoggiatura
    /// and label it as "V64-53" with Dominant function. This is a notational preference only;
    /// analysis and cadence detection are unchanged. Effective in detailed progression analysis
    /// where cadential 6-4 is detected.
    /// Default: false (keep I64 labeling).
    /// </summary>
    public bool PreferCadentialSixFourAsDominant { get; set; } = false;

    /// <summary>
    /// When labeling inversions of diatonic major-seventh chords (e.g., IVmaj7),
    /// include "maj" in the inversion figure labels. For example: IVmaj65/IVmaj43/IVmaj42.
    /// Root position remains "IVmaj7" regardless of this option. Default: false (use IV65/IV43/IV42).
    /// </summary>
    public bool IncludeMajInSeventhInversions { get; set; } = false;

    /// <summary>
    /// When true, Perfect Authentic Cadence (PAC) 判定を、ドミナント側が拡張なしの V（三和音）で、かつトニック側がプレーン I（三和音）である場合に限定します。
    /// 既定:false （V7 / V9, Imaj7 も PAC とみなす従来挙動）。
    /// </summary>
    public bool StrictPacPlainTriadsOnly { get; set; } = false;

    /// <summary>
    /// When true, ドミナント側に七度/九度拡張 (V7 / V9 / V7(9)) が含まれている場合は PAC へ昇格しません。
    /// 既定:false （拡張付きでも root 位置であれば PAC）。 StrictPacPlainTriadsOnly と併用されるケースを想定し分離。
    /// </summary>
    public bool StrictPacDisallowDominantExtensions { get; set; } = false;

    /// <summary>
    /// When true, PAC requires the soprano of the cadence goal (tonic chord) to contain the tonic scale degree.
    /// If voicing information is unavailable for the goal chord, PAC is not awarded under this option.
    /// Default: false.
    /// </summary>
    public bool StrictPacRequireSopranoTonic { get; set; } = false;

    /// <summary>
    /// When true, PAC additionally requires a leading-tone (7̂) in the soprano of the dominant resolving by step (↑ or ↓ enharmonic) to tonic in the goal chord.
    /// Effective only if <see cref="StrictPacRequireSopranoTonic"/> is also true. If voicings are missing, PAC is not awarded.
    /// Default: false.
    /// </summary>
    public bool StrictPacRequireSopranoLeadingToneResolution { get; set; } = false;

    /// <summary>
    /// Shared default instance used when no options are supplied.
    /// </summary>
    public static HarmonyOptions Default { get; } = new HarmonyOptions();

    /// <summary>
    /// Pedagogical preset: keeps defaults but enforces Neapolitan first inversion (bII6).
    /// Useful for teaching-focused outputs where bII is always shown as bII6.
    /// </summary>
    public static HarmonyOptions Pedagogical { get; } = new HarmonyOptions
    {
        PreferAugmentedSixthOverMixtureWhenBassFlat6 = true,
        DisallowAugmentedSixthWhenSopranoFlat6 = true,
        PreferMixtureSeventhOverAugmentedSixthWhenAmbiguous = false,
        PreferSecondaryLeadingToneTargetV = true,
        PreferDiatonicIiHalfDimInMinor = true,
        EnforceNeapolitanFirstInversion = true,
    ShowNonCadentialSixFour = true,
    PreferV7Paren9OverV9 = false,
    PreferCadentialSixFourAsDominant = false,
    };

    /// <summary>
    /// Notation preset: prefer "V7(9)" labeling for dominant ninths.
    /// Other options remain default-friendly for analysis.
    /// </summary>
    public static HarmonyOptions NotationV7Paren9 { get; } = new HarmonyOptions
    {
        PreferV7Paren9OverV9 = true,
    };

    /// <summary>
    /// Preset: より厳密な PAC 判定（V → I のプレーン triad のみ）。
    /// </summary>
    public static HarmonyOptions StrictPac { get; } = new HarmonyOptions
    {
        StrictPacPlainTriadsOnly = true,
        StrictPacDisallowDominantExtensions = true,
        StrictPacRequireSopranoTonic = true,
    };
}
