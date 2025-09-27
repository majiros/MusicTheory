using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MusicTheory.Theory.Analysis;

namespace MusicTheory.Tests.Benchmarks;

[MemoryDiagnoser]
public class AnalyzerBenchmarks
{
    private int[][] _sets = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new Random(123);
        _sets = Enumerable.Range(0, 500)
            .Select(_ => Enumerable.Range(0, rnd.Next(4,8)).Select(__ => rnd.Next(0,12)).ToArray())
            .ToArray();
    }

    [Benchmark]
    public int AnalyzeMany()
    {
        int total = 0;
        foreach (var s in _sets)
            total += ChordAnalyzer.ToListRanked(s).Count;
        return total;
    }
}

// 実行は別コンソールプロジェクト / 手動 (BenchmarkDotNet 推奨) で行う。テスト DLL にはエントリポイントを持たせない。
