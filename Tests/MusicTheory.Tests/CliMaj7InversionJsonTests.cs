using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MusicTheory.Tests;

public class CliMaj7InversionJsonTests
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

    using var proc = new Process { StartInfo = startInfo };
        proc.Start();
        string stdout = await proc.StandardOutput.ReadToEndAsync();
        string stderr = await proc.StandardError.ReadToEndAsync();
        if (!proc.WaitForExit(120000))
        {
            try { proc.Kill(true); } catch { }
            throw new TimeoutException("CLI did not finish within timeout.\n" + stderr);
        }
        if (proc.ExitCode != 0)
        {
            throw new InvalidOperationException($"CLI exited with code {proc.ExitCode}.\nSTDERR:\n{stderr}\nSTDOUT:\n{stdout}");
        }
        return stdout;
    }

    [Fact]
    public async Task Json_Default_NoMajToken_In_Inversion()
    {
        // Roman input IV65 should remain IV65 when maj7Inv is not set
        string json = await RunCliAsync("--roman IV65 --json");
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal(1, root.GetProperty("version").GetInt32());
        Assert.False(root.GetProperty("options").GetProperty("maj7Inv").GetBoolean());
        var chords = root.GetProperty("chords");
        Assert.True(chords.GetArrayLength() >= 1);
        Assert.Equal("IV65", chords[0].GetProperty("roman").GetString());
    }

    [Fact]
    public async Task Json_Maj7Inv_True_Adds_MajToken_In_Inversion()
    {
        // With --maj7Inv, major-seventh inversion should include 'maj' token: IVmaj65
        string json = await RunCliAsync("--roman IV65 --maj7Inv --json");
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal(1, root.GetProperty("version").GetInt32());
        Assert.True(root.GetProperty("options").GetProperty("maj7Inv").GetBoolean());
        var chords = root.GetProperty("chords");
        Assert.True(chords.GetArrayLength() >= 1);
        Assert.Equal("IVmaj65", chords[0].GetProperty("roman").GetString());
    }
}
