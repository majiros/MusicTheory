using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MusicTheory.Tests;

public class CliNeapolitanJsonTests
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

    private static string FirstChordRoman(JsonElement root)
    {
        var chords = root.GetProperty("chords");
        Assert.True(chords.GetArrayLength() >= 1, "CLI JSON must contain at least one chord.");
        var first = chords[0];
        return first.GetProperty("roman").GetString()!;
    }

    [Fact]
    public async Task Json_Neapolitan_EnforceN6_Roman_bII_to_bII6()
    {
        // roman 入力で bII を --enforceN6 により bII6 へ正規化
        string json = await RunCliAsync("--roman \"bII; V; I\" --enforceN6 --json");
        using var doc = JsonDocument.Parse(json);
        string roman0 = FirstChordRoman(doc.RootElement);
        Assert.Equal("bII6", roman0);
    }

    [Fact]
    public async Task Json_Neapolitan_EnforceN6_Pcs_bII_to_bII6()
    {
        // pcs 入力でも bII（三和音）が bII6 に正規化される
        string json = await RunCliAsync("--key C --pcs \"1,5,8; 7,11,2; 0,4,7\" --enforceN6 --json");
        using var doc = JsonDocument.Parse(json);
        string roman0 = FirstChordRoman(doc.RootElement);
        Assert.Equal("bII6", roman0);
    }
}
