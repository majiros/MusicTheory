using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicTheory.Theory.Time;

/// <summary>
/// 可変テンポマップ。Tick -> RealTime (マイクロ秒 / TimeSpan) 変換と逆変換の基本実装。
/// MIDI SMF では Tempo MetaEvent(FF 51) がこれに相当するが、
/// ライブラリ内で前計算/問い合わせを容易にするため独立構造を持つ。
/// </summary>
public sealed class TempoMap
{
    private readonly List<(long tick, Tempo tempo)> _entries = new();
    private long[] _ticks = Array.Empty<long>();
    private long[] _cumulativeUs = Array.Empty<long>(); // 各エントリ開始 tick 時点での累積マイクロ秒
    public TempoMap(Tempo initial) { _entries.Add((0, initial)); RebuildCache(); }

    /// <summary>テンポ変更追加 (同 tick が既に存在する場合置換)。</summary>
    public void AddChange(long tick, Tempo tempo)
    {
        if (tick < 0) throw new ArgumentOutOfRangeException(nameof(tick));
        _entries.RemoveAll(e => e.tick == tick);
        _entries.Add((tick, tempo));
        _entries.Sort((a,b)=>a.tick.CompareTo(b.tick));
        RebuildCache();
    }

    /// <summary>対象 tick 時点の有効テンポを取得。</summary>
    public Tempo GetAt(long tick)
    {
        if (tick < 0) throw new ArgumentOutOfRangeException(nameof(tick));
        int idx = UpperBound(_ticks, tick) - 1; // 最後の開始 tick
        if (idx < 0) idx = 0; if (idx >= _entries.Count) idx = _entries.Count-1;
        return _entries[idx].tempo;
    }

    /// <summary>
    /// 0 から指定 tick までの経過時間 (microseconds) を計算。
    /// </summary>
    public long TickToMicroseconds(long tick)
    {
        if (tick < 0) throw new ArgumentOutOfRangeException(nameof(tick));
        int idx = UpperBound(_ticks, tick) - 1; if (idx < 0) idx = 0;
        long baseUs = _cumulativeUs[idx];
        var (startTick, tempo) = _entries[idx];
        long delta = tick - startTick;
        baseUs += delta * tempo.MicrosecondsPerQuarter / Duration.TicksPerQuarter;
        return baseUs;
    }

    public TimeSpan TickToTimeSpan(long tick) => TimeSpan.FromMilliseconds(TickToMicroseconds(tick) / 1000.0);

    /// <summary>
    /// microseconds から tick への概算逆変換 (現在のテンポ区間を前から積分)。
    /// 完全逆写像ではなく、テンポ境界を跨ぐ場合は境界単位で探索。
    /// </summary>
    public long MicrosecondsToTick(long microseconds)
    {
        if (microseconds < 0) throw new ArgumentOutOfRangeException(nameof(microseconds));
        int idx = UpperBound(_cumulativeUs, microseconds) - 1; if (idx < 0) idx = 0;
        long baseUs = _cumulativeUs[idx];
        var (startTick, tempo) = _entries[idx];
        long remainUs = microseconds - baseUs;
        if (remainUs < 0) remainUs = 0;
        long ticks = remainUs * Duration.TicksPerQuarter / tempo.MicrosecondsPerQuarter;
        return startTick + ticks;
    }

    /// <summary>内部エントリ列挙 (デバッグ/表示用途)。</summary>
    public IReadOnlyList<(long tick, Tempo tempo)> Entries => _entries;

    private void RebuildCache()
    {
        _ticks = _entries.Select(e=>e.tick).ToArray();
        _cumulativeUs = new long[_entries.Count];
        if (_entries.Count == 0) return;
        _cumulativeUs[0] = 0;
        for (int i=1;i<_entries.Count;i++)
        {
            var (prevTick, prevTempo) = _entries[i-1];
            long deltaTicks = _entries[i].tick - prevTick;
            long us = deltaTicks * prevTempo.MicrosecondsPerQuarter / Duration.TicksPerQuarter;
            _cumulativeUs[i] = _cumulativeUs[i-1] + us;
        }
    }

    private static int UpperBound(long[] arr, long value)
    {
        int l=0,r=arr.Length; // first > value
        while (l<r){int m=(l+r)/2; if (arr[m] <= value) l=m+1; else r=m;} return l;
    }
}
