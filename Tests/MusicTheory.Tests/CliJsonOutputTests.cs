using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MusicTheory.Tests;

public class CliJsonOutputTests
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
        if (!proc.WaitForExit(120000))
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

    [Fact]
    public async Task Json_Default_V9_Label()
    {
    string json = await RunCliAsync("--pcs \"7,11,2,5,9\" --json");
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal(1, root.GetProperty("version").GetInt32());
        Assert.Equal("V9", root.GetProperty("options").GetProperty("v9").GetString());
        var chords = root.GetProperty("chords");
        Assert.True(chords.GetArrayLength() >= 1);
        Assert.Equal("V9", chords[0].GetProperty("roman").GetString());
    }

    [Fact]
    public async Task Json_V7Paren9_Label_When_Flag()
    {
    string json = await RunCliAsync("--pcs \"7,11,2,5,9\" --v7n9 --json");
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal(1, root.GetProperty("version").GetInt32());
        Assert.Equal("V7(9)", root.GetProperty("options").GetProperty("v9").GetString());
        var chords = root.GetProperty("chords");
        Assert.True(chords.GetArrayLength() >= 1);
        Assert.Equal("V7(9)", chords[0].GetProperty("roman").GetString());
    }
}
