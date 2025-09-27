using System;
using System.IO;

namespace MusicTheory.Tests;

internal static class TestPaths
{
    private static readonly Lazy<string> _repoRoot = new(FindRepoRoot, isThreadSafe: true);

    public static string RepoRoot => _repoRoot.Value;

    public static string CliProjectPath => Path.Combine(RepoRoot, "Samples", "MusicTheory.Cli", "MusicTheory.Cli.csproj");

    private static string FindRepoRoot()
    {
        string? dir = AppContext.BaseDirectory;
        for (int i = 0; i < 12 && dir != null; i++)
        {
            var sln = Path.Combine(dir, "MusicTheory.sln");
            var cli = Path.Combine(dir, "Samples", "MusicTheory.Cli", "MusicTheory.Cli.csproj");
            if (File.Exists(sln) && File.Exists(cli))
                return dir;
            dir = Directory.GetParent(dir)?.FullName;
        }
        throw new InvalidOperationException($"Failed to locate repo root containing MusicTheory.sln and Samples/MusicTheory.Cli. BaseDirectory='{AppContext.BaseDirectory}'.");
    }
}
