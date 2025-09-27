using System.Linq;
using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class BorrowedSeventhWarningsTests
{
    private static int Pc(int m) => ((m % 12) + 12) % 12;

    [Theory]
    [InlineData("bVI7", "Mixture: bVI7 typically resolves to V")]
    [InlineData("bII7", "Mixture: bII7 (Neapolitan 7) often resolves to V or I6")]
    [InlineData("bVII7", "Mixture: bVII7 often resolves to I (backdoor)")]
    public void Analyzer_Emits_Warnings_For_Mixture_Sevenths_ParseCases(string token, string expected)
    {
        var key = new Key(60, true); // C major
        var parsed = RomanInputParser.Parse(token, key)[0];
        var pcs = parsed.Pcs.Select(Pc).ToArray();
        var res = HarmonyAnalyzer.AnalyzeTriad(pcs, key);
        Assert.True(res.Success);
        Assert.Contains(expected, res.Warnings);
    }

    // Note: bVI7/bII7/bVII7 は Roman パース経由で検証。
    // iv7 は Roman パーサが diatonic IVmaj7 に正規化するため、明示PCSで別途検証する。

    [Fact]
    public void Analyzer_Emits_Warning_For_iv7_With_Explicit_Pcs()
    {
        var key = new Key(60, true); // C major
        // iv7 in C: F Ab C Eb -> pcs {5,8,0,3}
        var pcs = new[] { Pc(5), Pc(8), Pc(0), Pc(3) };
        var res = HarmonyAnalyzer.AnalyzeTriad(pcs, key);
        Assert.True(res.Success);
        Assert.Equal("iv7", res.RomanText);
        Assert.Contains("Mixture: iv7 typically resolves to V", res.Warnings);
    }

    [Fact]
    public void Cli_Json_Contains_Warnings_For_bVI7()
    {
        // Use CLI to ensure JSON includes warnings array
        var root = TestPaths.RepoRoot;
        var cli = TestPaths.CliProjectPath;
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{cli}\" -c Release --key C --roman bVI7 --json",
            WorkingDirectory = root
        };
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        using var p = System.Diagnostics.Process.Start(psi)!;
        string stdout = p!.StandardOutput.ReadToEnd();
        string stderr = p!.StandardError.ReadToEnd();
        p.WaitForExit();
        Assert.True(p.ExitCode == 0, $"CLI failed. STDERR=\n{stderr}\nSTDOUT=\n{stdout}");

        using var doc = System.Text.Json.JsonDocument.Parse(stdout);
        var chords = doc.RootElement.GetProperty("chords");
        Assert.True(chords.GetArrayLength() >= 1);
        var first = chords[0];
        Assert.Equal("bVI7", first.GetProperty("roman").GetString());
        var warnings = first.GetProperty("warnings");
        Assert.True(warnings.GetArrayLength() >= 1);
        var any = warnings.EnumerateArray().Any(e => e.GetString()!.Contains("bVI7"));
        Assert.True(any, "warnings should mention bVI7 advisory");
    }

    [Fact]
    public void Cli_Json_Contains_Warnings_For_bVII7()
    {
        var root = TestPaths.RepoRoot;
        var cli = TestPaths.CliProjectPath;
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{cli}\" -c Release --key C --roman bVII7 --json",
            WorkingDirectory = root
        };
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        using var p = System.Diagnostics.Process.Start(psi)!;
        string stdout = p!.StandardOutput.ReadToEnd();
        string stderr = p!.StandardError.ReadToEnd();
        p.WaitForExit();
        Assert.True(p.ExitCode == 0, $"CLI failed. STDERR=\n{stderr}\nSTDOUT=\n{stdout}");

        using var doc = System.Text.Json.JsonDocument.Parse(stdout);
        var chords = doc.RootElement.GetProperty("chords");
        Assert.True(chords.GetArrayLength() >= 1);
        var first = chords[0];
        Assert.Equal("bVII7", first.GetProperty("roman").GetString());
        var warnings = first.GetProperty("warnings");
        Assert.True(warnings.GetArrayLength() >= 1);
        var any = warnings.EnumerateArray().Any(e => e.GetString()!.Contains("bVII7") || e.GetString()!.Contains("backdoor"));
        Assert.True(any, "warnings should mention bVII7 advisory (backdoor)");
    }

    [Fact]
    public void Cli_Json_Contains_Warnings_For_iv7()
    {
        var root = TestPaths.RepoRoot;
        var cli = TestPaths.CliProjectPath;
        var psi = new System.Diagnostics.ProcessStartInfo
        {
            FileName = "dotnet",
            // Use explicit pcs for iv7 (F Ab C Eb). Quote the pcs to keep as single arg on all shells.
            Arguments = $"run --project \"{cli}\" -c Release --key C --pcs \"5,8,0,3\" --json",
            WorkingDirectory = root
        };
        psi.RedirectStandardOutput = true;
        psi.RedirectStandardError = true;
        using var p = System.Diagnostics.Process.Start(psi)!;
        string stdout = p!.StandardOutput.ReadToEnd();
        string stderr = p!.StandardError.ReadToEnd();
        p.WaitForExit();
        Assert.True(p.ExitCode == 0, $"CLI failed. STDERR=\n{stderr}\nSTDOUT=\n{stdout}");

        using var doc = System.Text.Json.JsonDocument.Parse(stdout);
        var chords = doc.RootElement.GetProperty("chords");
        Assert.True(chords.GetArrayLength() >= 1);
        var first = chords[0];
        Assert.Equal("iv7", first.GetProperty("roman").GetString());
        var warnings = first.GetProperty("warnings");
        Assert.True(warnings.GetArrayLength() >= 1);
        var any = warnings.EnumerateArray().Any(e => e.GetString()!.Contains("iv7") || e.GetString()!.Contains("resolves to V"));
        Assert.True(any, "warnings should mention iv7 advisory");
    }
}
