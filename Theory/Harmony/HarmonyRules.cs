namespace MusicTheory.Theory.Harmony;

// Lightweight global switches to tune harmony disambiguation without breaking existing APIs.
// Defaults are set to the current, test-validated behavior.
public static class HarmonyRules
{
    // These static properties remain for backward compatibility but forward to HarmonyOptions.Default.
    public static bool PreferAugmentedSixthOverMixtureWhenBassFlat6
    {
        get => HarmonyOptions.Default.PreferAugmentedSixthOverMixtureWhenBassFlat6;
        set => HarmonyOptions.Default.PreferAugmentedSixthOverMixtureWhenBassFlat6 = value;
    }

    public static bool DisallowAugmentedSixthWhenSopranoFlat6
    {
        get => HarmonyOptions.Default.DisallowAugmentedSixthWhenSopranoFlat6;
        set => HarmonyOptions.Default.DisallowAugmentedSixthWhenSopranoFlat6 = value;
    }

    public static bool PreferSecondaryLeadingToneTargetV
    {
        get => HarmonyOptions.Default.PreferSecondaryLeadingToneTargetV;
        set => HarmonyOptions.Default.PreferSecondaryLeadingToneTargetV = value;
    }

    public static bool PreferDiatonicIiHalfDimInMinor
    {
        get => HarmonyOptions.Default.PreferDiatonicIiHalfDimInMinor;
        set => HarmonyOptions.Default.PreferDiatonicIiHalfDimInMinor = value;
    }
}
