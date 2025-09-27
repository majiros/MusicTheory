using System;
using System.Diagnostics;
using MusicTheory.Theory.Analysis;
using MusicTheory.Theory.Chord;
using Xunit;

namespace MusicTheory.Tests;

public class PerformanceBench
{
    // 簡易パフォーマンス計測: フォーミュラ拡張を想定し 1000 回解析の平均時間を確認
    [Fact(Skip="Manual performance benchmark; enable when profiling.")]
    public void Analyze_Performance_Smoke()
    {
        var rnd = new Random(123);
        var testSets = new int[200][];
        for (int i=0;i<testSets.Length;i++)
        {
            // ランダム 4〜7 音 (0-11)
            var size = rnd.Next(4,8);
            var arr = new int[size];
            for (int j=0;j<size;j++) arr[j] = rnd.Next(0,12);
            testSets[i] = arr;
        }
        var sw = Stopwatch.StartNew();
        int totalCandidates = 0;
        for (int iter=0; iter<1000; iter++)
        {
            foreach (var set in testSets)
                totalCandidates += ChordAnalyzer.ToListRanked(set).Count;
        }
        sw.Stop();
        var perIterationMs = sw.Elapsed.TotalMilliseconds / 1000.0;
        Console.WriteLine($"Iterations=1000 TotalCandidates={totalCandidates} AvgPerIteration={perIterationMs:F3} ms");
        // スモーク: 1 反復平均 が 10ms 未満を目標 (環境依存のため厳格断定はしない)
        Assert.True(perIterationMs < 10.0, $"Average iteration {perIterationMs:F2} ms too slow");
    }
}
