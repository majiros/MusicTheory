using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Jobs;
using MusicTheory.Theory.Analysis;

namespace MusicTheory.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80, warmupCount:1, iterationCount:3, launchCount:1)]
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

public static class Program
{
	public static void Main(string[] args)
	{
		BenchmarkRunner.Run<AnalyzerBenchmarks>();
	}
}
