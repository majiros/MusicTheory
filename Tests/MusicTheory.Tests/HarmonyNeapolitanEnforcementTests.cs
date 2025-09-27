using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class HarmonyNeapolitanEnforcementTests
{
    private static int Pc(int midi) => ((midi % 12) + 12) % 12;

    [Fact]
    public void Enforce_bII6_WithVoicing_OnRootPositionOr64()
    {
        var key = new Key(60, true); // C major
        var opts = new HarmonyOptions { EnforceNeapolitanFirstInversion = true };

        // Root position Db-F-Ab over Db in bass
        var pcsRoot = new[] { Pc(61), Pc(65), Pc(68) };
        var vRoot = new FourPartVoicing(S: 77, A: 73, T: 69, B: 61); // S=F, A=Db, T=Ab, B=Db
        var r1 = HarmonyAnalyzer.AnalyzeTriad(pcsRoot, key, opts, vRoot);
        Assert.True(r1.Success);
        Assert.Equal("bII6", r1.RomanText);

        // Second inversion Db-F-Ab over Ab in bass
        var pcs64 = new[] { Pc(61), Pc(65), Pc(68) };
        var v64 = new FourPartVoicing(S: 77, A: 73, T: 69, B: 68); // B=Ab
        var r2 = HarmonyAnalyzer.AnalyzeTriad(pcs64, key, opts, v64);
        Assert.True(r2.Success);
        Assert.Equal("bII6", r2.RomanText);
    }

    [Fact]
    public void Enforce_bII6_WithoutVoicing_CoercesHead()
    {
        var key = new Key(60, true); // C major
        var opts = new HarmonyOptions { EnforceNeapolitanFirstInversion = true };

        var pcs = new[] { Pc(61), Pc(65), Pc(68) }; // Db F Ab
        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, null);
        Assert.True(r.Success);
        Assert.Equal("bII6", r.RomanText);
    }

    [Fact]
    public void Enforce_bII6_KeepsAlreadyFirstInversion()
    {
        var key = new Key(60, true); // C major
        var opts = new HarmonyOptions { EnforceNeapolitanFirstInversion = true };

        var pcs = new[] { Pc(61), Pc(65), Pc(68) }; // Db F Ab
        // First inversion voicing (F in bass)
        var v6 = new FourPartVoicing(S: 77, A: 73, T: 69, B: 65);
        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v6);
        Assert.True(r.Success);
        Assert.Equal("bII6", r.RomanText);
    }

    [Fact]
    public void Enforce_bII6_WorksInMinor()
    {
        var key = new Key(57, false); // A minor
        var opts = new HarmonyOptions { EnforceNeapolitanFirstInversion = true };

        // Bb D F -> bII in A minor
        var pcs = new[] { Pc(58), Pc(62), Pc(65) };
        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, null);
        Assert.True(r.Success);
        Assert.Equal("bII6", r.RomanText);
    }

    [Fact]
    public void Default_NoEnforce_KeepsActualInversion()
    {
        var key = new Key(60, true); // C major
        // Root position Db-F-Ab with Db in bass
        var pcs = new[] { Pc(61), Pc(65), Pc(68) };
        var vRoot = new FourPartVoicing(S: 77, A: 73, T: 69, B: 61);
        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, HarmonyOptions.Default, vRoot);
        Assert.True(r.Success);
        Assert.Equal("bII", r.RomanText);
    }

    [Fact]
    public void Enforcement_DoesNotAffect_bII7()
    {
        var key = new Key(60, true); // C major
        var opts = new HarmonyOptions { EnforceNeapolitanFirstInversion = true };
        // bII7: Db F Ab Cb -> pitch classes {1,5,8,11}
        var pcs = new[] { Pc(61), Pc(65), Pc(68), Pc(71) }; // Use B natural as Cb enharmonic
        var v = new FourPartVoicing(S: 83, A: 77, T: 71, B: 61);
        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, opts, v);
        Assert.True(r.Success);
        Assert.StartsWith("bII", r.RomanText);
        Assert.Contains("7", r.RomanText);
        Assert.NotEqual("bII6", r.RomanText);
    }

    [Fact]
    public void PedagogicalPreset_Enforces_bII6()
    {
        var key = new Key(60, true); // C major
        var pcs = new[] { Pc(61), Pc(65), Pc(68) }; // Db F Ab
        var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key, HarmonyOptions.Pedagogical, null);
        Assert.True(r.Success);
        Assert.Equal("bII6", r.RomanText);
    }
}
