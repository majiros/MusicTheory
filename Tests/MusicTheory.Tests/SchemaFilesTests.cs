using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace MusicTheory.Tests;

public class SchemaFilesTests
{
    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 10 && dir != null; i++)
        {
            if (File.Exists(Path.Combine(dir.FullName, "MusicTheory.sln")) || Directory.Exists(Path.Combine(dir.FullName, "Samples")))
                return dir.FullName;
            dir = dir.Parent!;
        }
        throw new DirectoryNotFoundException("Repository root not found from test base directory.");
    }

    private static JsonDocument Load(string relativePath)
    {
        var full = Path.Combine(RepoRoot(), relativePath);
        Assert.True(File.Exists(full), $"Schema file not found: {full}");
        var json = File.ReadAllText(full);
        return JsonDocument.Parse(json);
    }

    [Fact]
    public void Main_Cli_Schema_Has_Maj7Inv_And_V9_Enum()
    {
        using var doc = Load(Path.Combine("Samples", "MusicTheory.Cli", "schema", "music-theory.cli.v1.schema.json"));
        var root = doc.RootElement;

        Assert.Equal("urn:music-theory:cli:schema:v1", root.GetProperty("$id").GetString());
        Assert.Equal(1, root.GetProperty("properties").GetProperty("version").GetProperty("const").GetInt32());

        var options = root.GetProperty("properties").GetProperty("options");
        var required = options.GetProperty("required").EnumerateArray().Select(e => e.GetString()).ToArray();
        foreach (var name in new[] { "hide64", "cad64Dominant", "v9", "maj7Inv", "tuplets" })
            Assert.Contains(name, required);

        var v9Enum = options.GetProperty("properties").GetProperty("v9").GetProperty("enum").EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Contains("V9", v9Enum);
        Assert.Contains("V7(9)", v9Enum);
    }

    [Fact]
    public void Util_Roman_Schema_Minimum_Structure()
    {
        using var doc = Load(Path.Combine("Samples", "MusicTheory.Cli", "schema", "music-theory.cli.util.roman.v1.schema.json"));
        var root = doc.RootElement;
        Assert.Equal("urn:music-theory:cli:schema:util:roman:v1", root.GetProperty("$id").GetString());
        Assert.Equal("array", root.GetProperty("type").GetString());
        var items = root.GetProperty("items");
        var req = items.GetProperty("required").EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Contains("input", req);
        Assert.Contains("pcs", req);
        var props = items.GetProperty("properties");
        Assert.True(props.TryGetProperty("bassPcHint", out _));
    }

    [Fact]
    public void Util_Dur_Schema_Minimum_Structure()
    {
        using var doc = Load(Path.Combine("Samples", "MusicTheory.Cli", "schema", "music-theory.cli.util.dur.v1.schema.json"));
        var root = doc.RootElement;
        Assert.Equal("urn:music-theory:cli:schema:util:dur:v1", root.GetProperty("$id").GetString());
        Assert.Equal("array", root.GetProperty("type").GetString());
        var items = root.GetProperty("items");
        var req = items.GetProperty("required").EnumerateArray().Select(e => e.GetString()).ToArray();
        foreach (var name in new[] { "input", "ticks", "fraction", "notation" }) Assert.Contains(name, req);
        var fraction = items.GetProperty("properties").GetProperty("fraction");
        var freq = fraction.GetProperty("required").EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Contains("numerator", freq);
        Assert.Contains("denominator", freq);
    }

    [Fact]
    public void Util_Pcs_Schema_Minimum_Structure()
    {
        using var doc = Load(Path.Combine("Samples", "MusicTheory.Cli", "schema", "music-theory.cli.util.pcs.v1.schema.json"));
        var root = doc.RootElement;
        Assert.Equal("urn:music-theory:cli:schema:util:pcs:v1", root.GetProperty("$id").GetString());
        Assert.Equal("array", root.GetProperty("type").GetString());
        var items = root.GetProperty("items");
        var req = items.GetProperty("required").EnumerateArray().Select(e => e.GetString()).ToArray();
        Assert.Contains("input", req);
        Assert.Contains("pcs", req);
    }
}
