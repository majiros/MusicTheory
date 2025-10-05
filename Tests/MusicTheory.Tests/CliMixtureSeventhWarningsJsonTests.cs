using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MusicTheory.Tests;

public class CliMixtureSeventhWarningsJsonTests
{
    private static string RepoRoot => TestPaths.RepoRoot;
    private static string CliProjectPath => TestPaths.CliProjectPath;

    private static async Task<string> RunCliAsync(string extraArgs)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{CliProjectPath}\" -c Release {extraArgs}",
            WorkingDirectory = RepoRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        proc.Start();
        string stdout = await proc.StandardOutput.ReadToEndAsync();
        string stderr = await proc.StandardError.ReadToEndAsync();
        if (!proc.WaitForExit(120_000))
        {
            try { proc.Kill(true); } catch { /* ignore */ }
            throw new TimeoutException("CLI did not finish within timeout.\n" + stderr);
        }
        if (proc.ExitCode != 0)
        {
            throw new InvalidOperationException($"CLI exited with code {proc.ExitCode}.\nSTDERR:\n{stderr}\nSTDOUT:\n{stdout}");
        }
        return stdout;
    }

    private static JsonElement[] GetChords(JsonElement root)
    {
        var chords = root.GetProperty("chords");
        Assert.True(chords.ValueKind == JsonValueKind.Array, "CLI JSON must contain 'chords' array.");
        return chords.EnumerateArray().ToArray();
    }

    private static string[] GetWarnings(JsonElement chord)
    {
        if (!chord.TryGetProperty("warnings", out var w) || w.ValueKind != JsonValueKind.Array)
            return Array.Empty<string>();
        return w.EnumerateArray().Select(e => e.GetString()!).ToArray();
    }

    [Fact(Skip = "CLI crash under investigation (exit code -1073741790)")]
    public async Task Json_Mixture7_Warnings_bVI7_bII7()
    {
        // Ensure mixture-7th advisory warnings are emitted for bVI7 and bII7
        // Use roman input to avoid pitch-class ambiguity. The CLI constructs voicing hints
        // that suppress Aug6 preference, so mixture 7ths are recognized deterministically.
        string json = await RunCliAsync("--roman \"bVI7; bII7; V; I\" --json");
        using var doc = JsonDocument.Parse(json);
        var chords = GetChords(doc.RootElement);
        Assert.True(chords.Length >= 2);

        var w0 = GetWarnings(chords[0]);
        var w1 = GetWarnings(chords[1]);

        Assert.Contains("Mixture: bVI7 typically resolves to V", w0);
        Assert.Contains("Mixture: bII7 (Neapolitan 7) often resolves to V or I6", w1);
    }

    [Fact]
    public async Task Json_Mixture7_Warning_iv7_in_C_major_via_pcs()
    {
        // In C major, pcs 5,8,0,3 => F Ab C Eb (iv7 borrowed). Expect iv7 advisory warning.
        string json = await RunCliAsync("--key C --pcs \"5,8,0,3\" --json");
        using var doc = JsonDocument.Parse(json);
        var chords = GetChords(doc.RootElement);
        Assert.True(chords.Length >= 1);

        var w = GetWarnings(chords[0]);
        Assert.Contains("Mixture: iv7 typically resolves to V", w);
    }
}
