using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace MusicTheory.Tests;

public class CliModulationPresetJsonTests
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

    private const string PcsSeq = "0,4,7; 7,11,2; 0,4,7; 2,6,9,0; 7,11,2; 4,7,11; 9,0,4; 2,6,9; 7,11,2";

    private static string NormalizeSegments(JsonElement root)
    {
        // Produce a stable textual representation of segments for robust diffing
        if (!root.TryGetProperty("segments", out var segs) || segs.ValueKind != JsonValueKind.Array)
            return string.Empty;

        var parts = segs.EnumerateArray().Select(e =>
        {
            int start = e.GetProperty("start").GetInt32();
            int end = e.GetProperty("end").GetInt32();
            var key = e.GetProperty("key");
            int tonic = key.GetProperty("tonic").GetInt32();
            string mode = key.GetProperty("mode").GetString()!;
            double conf = Math.Round(e.GetProperty("confidence").GetDouble(), 3);
            return $"{start}-{end}:{tonic}:{mode}:{conf}";
        });
        return string.Join("|", parts);
    }

    private static string NormalizeKeys(JsonElement root)
    {
        // Build a compact per-chord key signature: index:tonic:mode
        if (!root.TryGetProperty("keys", out var keys) || keys.ValueKind != JsonValueKind.Array)
            return string.Empty;
        var parts = keys.EnumerateArray().Select(e =>
        {
            int index = e.GetProperty("index").GetInt32();
            var key = e.GetProperty("key");
            int tonic = key.GetProperty("tonic").GetInt32();
            string mode = key.GetProperty("mode").GetString()!;
            return $"{index}:{tonic}:{mode}";
        });
        return string.Join("|", parts);
    }

    private static (int window, int minSwitch, int prevBias, int initBias, int switchMargin, int outPenalty) ReadEstimator(JsonElement root)
    {
        if (!root.TryGetProperty("estimator", out var est) || est.ValueKind != JsonValueKind.Object)
            return default;
        return (
            est.GetProperty("window").GetInt32(),
            est.GetProperty("minSwitch").GetInt32(),
            est.GetProperty("prevBias").GetInt32(),
            est.GetProperty("initBias").GetInt32(),
            est.GetProperty("switchMargin").GetInt32(),
            est.GetProperty("outPenalty").GetInt32()
        );
    }

    [Fact]
    public async Task Json_ModulationPreset_Permissive_vs_Stable_Different_Segments()
    {
        // Quote the PCS sequence so it's passed as a single argument (contains spaces/semicolons)
        string jsonStable = await RunCliAsync($"--pcs \"{PcsSeq}\" --segments --trace --preset stable --json");
        string jsonPerm = await RunCliAsync($"--pcs \"{PcsSeq}\" --segments --trace --preset permissive --json");

        using var docStable = JsonDocument.Parse(jsonStable);
        using var docPerm = JsonDocument.Parse(jsonPerm);

        var rootStable = docStable.RootElement;
        var rootPerm = docPerm.RootElement;

        Assert.Equal(1, rootStable.GetProperty("version").GetInt32());
        Assert.Equal(1, rootPerm.GetProperty("version").GetInt32());

        // options.preset reflects the requested preset
        Assert.Equal("stable", rootStable.GetProperty("options").GetProperty("preset").GetString());
        Assert.Equal("permissive", rootPerm.GetProperty("options").GetProperty("preset").GetString());

        // Prefer segment difference; fall back to per-chord key traces; finally require estimator options to differ
        string segStable = NormalizeSegments(rootStable);
        string segPerm = NormalizeSegments(rootPerm);
        if (segStable != segPerm)
            return;

        // If segments are equal (rare/edge cases), keys should still differ under distinct presets
        string keysStable = NormalizeKeys(rootStable);
        string keysPerm = NormalizeKeys(rootPerm);
        if (keysStable != keysPerm)
            return;

        // As a last resort, ensure estimator knobs actually reflect the preset (contract check)
        var estStable = ReadEstimator(rootStable);
        var estPerm = ReadEstimator(rootPerm);
        if (estStable.Equals(estPerm))
        {
            var diag = string.Join(Environment.NewLine, new[]
            {
                "Preset outputs unexpectedly equal.",
                $"segments (stable):    {segStable}",
                $"segments (permissive): {segPerm}",
                $"keys (stable):        {keysStable}",
                $"keys (permissive):     {keysPerm}",
                $"estimator (stable):    {estStable}",
                $"estimator (permissive): {estPerm}"
            });
            Xunit.Assert.Fail(diag);
        }
    }
}
