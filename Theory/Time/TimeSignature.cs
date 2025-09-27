using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicTheory.Theory.Time;

/// <summary>
/// 拍子記号。例: 4/4, 3/8 など。PPQ=Duration.TicksPerQuarter を前提に ticks 計算。
/// </summary>
public readonly struct TimeSignature : IEquatable<TimeSignature>
{
    public int Numerator { get; }
    public int Denominator { get; } // (1,2,4,8,16,...)
    public TimeSignature(int numerator, int denominator)
    {
        if (numerator <= 0) throw new ArgumentOutOfRangeException(nameof(numerator));
        if (denominator <= 0 || (denominator & (denominator - 1)) != 0) throw new ArgumentException("Denominator must be power of two.", nameof(denominator));
        Numerator = numerator; Denominator = denominator;
    }
    public int TicksPerBeat => Duration.TicksPerQuarter * 4 / Denominator;
    public int TicksPerBar => TicksPerBeat * Numerator;
    public bool Equals(TimeSignature other) => Numerator == other.Numerator && Denominator == other.Denominator;
    public override bool Equals(object? obj) => obj is TimeSignature ts && Equals(ts);
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);
    public override string ToString() => $"{Numerator}/{Denominator}";
}

/// <summary>小節位置 (bar, beat, tickWithinBeat)。bar, beat は 0 基点。</summary>
public readonly struct BarBeatTick : IEquatable<BarBeatTick>
{
    public int Bar { get; }
    public int Beat { get; }
    public int TickWithinBeat { get; }
    public BarBeatTick(int bar, int beat, int tickWithinBeat)
    { if (bar<0||beat<0||tickWithinBeat<0) throw new ArgumentOutOfRangeException(); Bar=bar; Beat=beat; TickWithinBeat=tickWithinBeat; }
    public bool Equals(BarBeatTick other) => Bar==other.Bar && Beat==other.Beat && TickWithinBeat==other.TickWithinBeat;
    public override bool Equals(object? obj)=> obj is BarBeatTick bbt && Equals(bbt);
    public override int GetHashCode()=> HashCode.Combine(Bar,Beat,TickWithinBeat);
    public override string ToString()=> $"{Bar}:{Beat}+{TickWithinBeat}";
}

/// <summary>絶対 tick と拍子変換用。可変拍子対応は将来的に (マップを保持) 拡張。</summary>
public readonly struct TimePosition
{
    public TimeSignature Signature { get; }
    public long AbsoluteTicks { get; }
    public BarBeatTick BarBeatTick { get; }
    public TimePosition(TimeSignature sig, long absoluteTicks)
    {
        if (absoluteTicks < 0) throw new ArgumentOutOfRangeException(nameof(absoluteTicks));
        Signature = sig; AbsoluteTicks = absoluteTicks;
        var tpBar = sig.TicksPerBar;
        var bar = (int)(absoluteTicks / tpBar);
        var rem = (int)(absoluteTicks % tpBar);
        var beat = rem / sig.TicksPerBeat;
        var tickInBeat = rem % sig.TicksPerBeat;
        BarBeatTick = new BarBeatTick(bar, beat, tickInBeat);
    }
    private TimePosition(TimeSignature sig, long abs, BarBeatTick bbt)
    { Signature=sig; AbsoluteTicks=abs; BarBeatTick=bbt; }
    public static TimePosition FromBarBeatTick(TimeSignature sig, BarBeatTick bbt)
    {
        if (bbt.Beat >= sig.Numerator) throw new ArgumentOutOfRangeException(nameof(bbt),"Beat >= numerator");
        if (bbt.TickWithinBeat >= sig.TicksPerBeat) throw new ArgumentOutOfRangeException(nameof(bbt),"TickWithinBeat >= ticksPerBeat");
        long abs = (long)bbt.Bar * sig.TicksPerBar + bbt.Beat * sig.TicksPerBeat + bbt.TickWithinBeat;
        return new TimePosition(sig, abs, bbt);
    }
    public TimePosition Add(Duration d) => new(Signature, AbsoluteTicks + d.Ticks);
    public override string ToString() => $"{BarBeatTick} ({AbsoluteTicks} ticks)";
}

/// <summary>
/// Tuplet パターン生成ユーティリティ。
/// </summary>
public static class TupletPatterns
{
    /// <summary>任意範囲で n:k 連符 (n!=k) を生成 (既約化は行わず生の組)。</summary>
    public static IEnumerable<Tuplet> Generate(int maxActual=13, int maxNormal=12)
    {
        for (int normal=2; normal<=maxNormal; normal++)
            for (int actual=2; actual<=maxActual; actual++)
                if (actual!=normal)
                    yield return new Tuplet(actual, normal);
    }
}

/// <summary>MIDI 非依存の簡易イベント。Status は 0x9x / 0x8x 相当を想定。</summary>
public readonly struct MidiNoteEvent
{
    public int Channel { get; }
    public int Pitch { get; }
    public int Velocity { get; }
    public long Tick { get; }
    public bool IsNoteOn { get; }
    public MidiNoteEvent(int channel, int pitch, int velocity, long tick, bool isNoteOn)
    { Channel=channel; Pitch=pitch; Velocity=velocity; Tick=tick; IsNoteOn=isNoteOn; }
    public override string ToString()=> $"{(IsNoteOn?"On":"Off")}[ch{Channel}] p{Pitch} v{Velocity} @{Tick}";
}

/// <summary>Note から NoteOn/NoteOff イベント列を生成するヘルパ。</summary>
public static class MidiNoteBuilder
{
    /// <summary>シンプルな単音生成。</summary>
    public static IEnumerable<MidiNoteEvent> BuildSingle(int channel, int pitch, int velocity, long startTick, Duration dur)
    {
        yield return new MidiNoteEvent(channel, pitch, velocity, startTick, true);
        yield return new MidiNoteEvent(channel, pitch, 0, startTick + dur.Ticks, false);
    }

    /// <summary>シーケンス (pitch, start, duration, velocity 可変) からソート済イベントを生成。</summary>
    public static IEnumerable<MidiNoteEvent> BuildMany(IEnumerable<(int pitch,long start,Duration dur,int velocity,int channel)> notes)
        => notes.SelectMany(n => BuildSingle(n.channel, n.pitch, n.velocity, n.start, n.dur))
                .OrderBy(e=>e.Tick).ThenBy(e=>!e.IsNoteOn); // 同 tick では NoteOff 先行で重なり制御可 (用途で逆も)
}

/// <summary>拍子変更マップ (単純: ソート済境界 + 検索)。</summary>
public sealed class TimeSignatureMap
{
    private readonly List<(long tick, TimeSignature sig)> _entries = new();
    public TimeSignatureMap(TimeSignature initial) { _entries.Add((0, initial)); }
    public void AddChange(long tick, TimeSignature sig)
    {
        if (tick < 0) throw new ArgumentOutOfRangeException(nameof(tick));
        if (_entries.Any(e=>e.tick==tick)) _entries.RemoveAll(e=>e.tick==tick);
        _entries.Add((tick, sig));
        _entries.Sort((a,b)=>a.tick.CompareTo(b.tick));
    }
    public TimeSignature GetAt(long tick)
    {
        TimeSignature current = _entries[0].sig;
        foreach (var e in _entries)
        {
            if (e.tick > tick) break;
            current = e.sig;
        }
        return current;
    }
    public TimePosition ToPosition(long tick)
        => new(GetAt(tick), tick);
}

/// <summary>テンポ (microseconds per quarter note)。</summary>
public readonly struct Tempo : IEquatable<Tempo>
{
    public int MicrosecondsPerQuarter { get; }
    public Tempo(int usPerQuarter)
    { if (usPerQuarter<=0) throw new ArgumentOutOfRangeException(nameof(usPerQuarter)); MicrosecondsPerQuarter=usPerQuarter; }
    public double Bpm => 60_000_000.0 / MicrosecondsPerQuarter;
    public static Tempo FromBpm(double bpm) => new((int)(60_000_000 / bpm));
    public bool Equals(Tempo other)=> MicrosecondsPerQuarter==other.MicrosecondsPerQuarter;
    public override bool Equals(object? o)=> o is Tempo t && Equals(t);
    public override int GetHashCode()=> MicrosecondsPerQuarter;
    public override string ToString()=>$"{Bpm:F2} BPM";
}

public readonly struct TempoChange { public long Tick { get; } public Tempo Tempo { get; } public TempoChange(long t, Tempo tempo){Tick=t;Tempo=tempo;} }

/// <summary>メタイベント (簡易)。</summary>
public abstract record MetaEvent(long Tick);
public sealed record TempoEvent(long Tick, Tempo Tempo): MetaEvent(Tick);
public sealed record TimeSignatureEvent(long Tick, TimeSignature Signature): MetaEvent(Tick);
public sealed record KeySignatureEvent(long Tick, int SharpsFlats, bool IsMinor): MetaEvent(Tick); // SharpsFlats: -7..+7
public sealed record TextEvent(long Tick, string Text): MetaEvent(Tick);
public sealed record TrackNameEvent(long Tick, string Name): MetaEvent(Tick);
public sealed record MarkerEvent(long Tick, string Label): MetaEvent(Tick);

/// <summary>MIDI トラック的コンテナ (ノートイベント + メタイベント)。</summary>
public sealed class MidiTrack
{
    private readonly List<MidiNoteEvent> _notes = new();
    private readonly List<MetaEvent> _meta = new();
    public IReadOnlyList<MidiNoteEvent> Notes => _notes;
    public IReadOnlyList<MetaEvent> MetaEvents => _meta;
    public void AddNote(MidiNoteEvent e) => _notes.Add(e);
    public void AddMeta(MetaEvent e) => _meta.Add(e);
    public void AddNoteRange(IEnumerable<MidiNoteEvent> events) => _notes.AddRange(events);
    public void Sort()
    {
        _notes.Sort((a,b)=> a.Tick==b.Tick ? (a.IsNoteOn==b.IsNoteOn ? 0 : (a.IsNoteOn ? 1:-1)) : a.Tick.CompareTo(b.Tick));
        _meta.Sort((a,b)=> a.Tick.CompareTo(b.Tick));
    }
}

/// <summary>クオンタイズ & スイングユーティリティ。</summary>
public static class Quantize
{
    /// <summary>指定分解能 (ticks) に丸め (最近傍)。</summary>
    public static long ToGrid(long tick, int gridTicks)
    {
        if (gridTicks<=0) return tick;
        long q = tick / gridTicks;
        long r = tick % gridTicks;
        return r < gridTicks/2 ? q*gridTicks : (q+1)*gridTicks;
    }
    /// <summary>スイング (2つ目の8分を遅らせる) Ratio=0..1 (0=ストレート, 0.5=三連近似)。</summary>
    public static long ApplySwing(long tick, int subdivisionTicks, double ratio)
    {
        if (ratio<=0) return tick;
        int pair = subdivisionTicks*2;
        long inPair = tick % pair;
        if (inPair < subdivisionTicks) return tick; // 1つ目はそのまま
        double delay = subdivisionTicks * ratio;
        return tick + (long)delay;
    }

    /// <summary>
    /// 強拍優先クオンタイズ: 四捨五入結果と隣接強拍のどちらが近いか + バイアス係数で決める。
    /// strongBeatsTicksWithinBar は 拍子内 (0..TicksPerBar) の強拍 tick (0 基点) リスト。
    /// bias (0..1) 高いほど強拍へ吸着しやすい。
    /// </summary>
    public static long ToGridStrongBeat(long tick, int gridTicks, int ticksPerBar, IReadOnlyList<int> strongBeatsTicksWithinBar, double bias=0.25)
    {
        if (gridTicks<=0 || strongBeatsTicksWithinBar.Count==0) return ToGrid(tick, gridTicks);
        long rounded = ToGrid(tick, gridTicks);
        long barIndex = tick / ticksPerBar;
        long posInBar = tick % ticksPerBar;
        int nearestStrong = strongBeatsTicksWithinBar.OrderBy(b => Math.Abs(b - posInBar)).First();
        long strongTick = barIndex * ticksPerBar + nearestStrong;
        long distRounded = Math.Abs(rounded - tick);
        long distStrong = Math.Abs(strongTick - tick);
        if (distStrong * (1.0 - bias) < distRounded) return strongTick; // バイアス適用
        return rounded;
    }

    /// <summary>
    /// グルーヴテンプレート適用: pattern[i]=tick オフセット (±) を 1 サイクル (pattern.Length グリッド) で繰返し適用。
    /// 先にグリッドに丸めた後、オフセットを加算。
    /// </summary>
    public static long ApplyGroove(long tick, int gridTicks, IReadOnlyList<int> pattern)
    {
        if (gridTicks<=0 || pattern.Count==0) return tick;
        long baseGrid = ToGrid(tick, gridTicks);
        long index = (baseGrid / gridTicks) % pattern.Count;
        return baseGrid + pattern[(int)index];
    }
}
