using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class HarmonyRulesForwardingTests
{
    [Fact]
    public void Forwarding_ReadWrite_PreferAug6_And_DisallowAug6_SyncsWithDefault()
    {
        var backupPrefer = HarmonyOptions.Default.PreferAugmentedSixthOverMixtureWhenBassFlat6;
        var backupDisallow = HarmonyOptions.Default.DisallowAugmentedSixthWhenSopranoFlat6;
        try
        {
            HarmonyRules.PreferAugmentedSixthOverMixtureWhenBassFlat6 = !backupPrefer;
            HarmonyRules.DisallowAugmentedSixthWhenSopranoFlat6 = !backupDisallow;

            Assert.Equal(HarmonyOptions.Default.PreferAugmentedSixthOverMixtureWhenBassFlat6,
                         HarmonyRules.PreferAugmentedSixthOverMixtureWhenBassFlat6);
            Assert.Equal(HarmonyOptions.Default.DisallowAugmentedSixthWhenSopranoFlat6,
                         HarmonyRules.DisallowAugmentedSixthWhenSopranoFlat6);
        }
        finally
        {
            HarmonyOptions.Default.PreferAugmentedSixthOverMixtureWhenBassFlat6 = backupPrefer;
            HarmonyOptions.Default.DisallowAugmentedSixthWhenSopranoFlat6 = backupDisallow;
        }
    }

    [Fact]
    public void Forwarding_ReadWrite_SecondaryLT_And_DiatonicIiMinor_SyncsWithDefault()
    {
        var backupSec = HarmonyOptions.Default.PreferSecondaryLeadingToneTargetV;
        var backupIi = HarmonyOptions.Default.PreferDiatonicIiHalfDimInMinor;
        try
        {
            HarmonyRules.PreferSecondaryLeadingToneTargetV = !backupSec;
            HarmonyRules.PreferDiatonicIiHalfDimInMinor = !backupIi;

            Assert.Equal(HarmonyOptions.Default.PreferSecondaryLeadingToneTargetV,
                         HarmonyRules.PreferSecondaryLeadingToneTargetV);
            Assert.Equal(HarmonyOptions.Default.PreferDiatonicIiHalfDimInMinor,
                         HarmonyRules.PreferDiatonicIiHalfDimInMinor);
        }
        finally
        {
            HarmonyOptions.Default.PreferSecondaryLeadingToneTargetV = backupSec;
            HarmonyOptions.Default.PreferDiatonicIiHalfDimInMinor = backupIi;
        }
    }
}
