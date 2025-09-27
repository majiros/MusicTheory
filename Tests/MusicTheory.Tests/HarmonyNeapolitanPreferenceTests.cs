using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class HarmonyNeapolitanPreferenceTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Neapolitan_bII_root_position_emits_prefer_bII6_warning()
    {
        var key = new Key(60, true); // C major
        var bII = new[] { Pc(61), Pc(65), Pc(68) }; // Db F Ab
        var vRoot = new FourPartVoicing(81, 77, 73, 61); // bass = Db (root)

        var res = HarmonyAnalyzer.AnalyzeTriad(bII, key, vRoot);
        Assert.True(res.Success);
        Assert.Equal("bII", res.RomanText);
        Assert.Contains(res.Warnings, w => w.Contains("prefer bII6"));
    }

    [Fact]
    public void Neapolitan_bII64_emits_prefer_bII6_warning()
    {
        var key = new Key(60, true); // C major
        var bII = new[] { Pc(61), Pc(65), Pc(68) }; // Db F Ab
        var v64 = new FourPartVoicing(86, 80, 68, 68); // bass = Ab (fifth)

        var res = HarmonyAnalyzer.AnalyzeTriad(bII, key, v64);
        Assert.True(res.Success);
        Assert.Equal("bII64", res.RomanText);
        Assert.Contains(res.Warnings, w => w.Contains("prefer bII6"));
    }

    [Fact]
    public void Neapolitan_any_form_emits_resolution_hint_to_V()
    {
        var key = new Key(60, true);
        var bII = new[] { Pc(61), Pc(65), Pc(68) };
        var v6 = new FourPartVoicing(77, 73, 65, 65); // bass = F (third) → bII6
        var res = HarmonyAnalyzer.AnalyzeTriad(bII, key, v6);
        Assert.True(res.Success);
        Assert.Equal("bII6", res.RomanText);
        Assert.Contains(res.Warnings, w => w.Contains("typical resolution to V"));
    }
}
