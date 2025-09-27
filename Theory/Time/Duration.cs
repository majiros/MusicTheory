using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicTheory.Theory.Time;

/// <summary>
/// 基本音符種類 (倍全～128分) - Whole を 1 とする比率は <see cref="BaseNoteValueExtensions"/> を参照。
/// </summary>
public enum BaseNoteValue
{
    DoubleWhole, // Breve (2 * Whole)
    Whole,
    Half,
    Quarter,
    Eighth,
    Sixteenth,
    ThirtySecond,
    SixtyFourth,
    OneHundredTwentyEighth
}

public static class BaseNoteValueExtensions
{
    /// <summary>Whole Note を 1.0 (= 1920 ticks/PPQ480*4) とした時の分数 (分子,分母) を返す。</summary>
    public static (int num, int den) GetFraction(this BaseNoteValue value) => value switch
    {
        BaseNoteValue.DoubleWhole => (2,1),
        BaseNoteValue.Whole => (1,1),
        BaseNoteValue.Half => (1,2),
        BaseNoteValue.Quarter => (1,4),
        BaseNoteValue.Eighth => (1,8),
        BaseNoteValue.Sixteenth => (1,16),
        BaseNoteValue.ThirtySecond => (1,32),
        BaseNoteValue.SixtyFourth => (1,64),
        BaseNoteValue.OneHundredTwentyEighth => (1,128),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
    };
}

/// <summary>連符。ActualCount 個を NormalCount 個分の時間に詰める (例: 三連符 = (3,2))。</summary>
public readonly record struct Tuplet(int ActualCount, int NormalCount)
{
    public double Factor => NormalCount / (double)ActualCount; // 時間倍率
    public RationalFactor FactorRational => new(NormalCount, ActualCount);
    public static Tuplet Create(int actual, int normal) => new(actual, normal);
    public override string ToString() => $"{ActualCount}:{NormalCount}";
}

/// <summary>有理数係数 (簡約済み)。Whole Note=1 を基準とした比率計算用。</summary>
public readonly struct RationalFactor : IEquatable<RationalFactor>
{
    public int Numerator { get; }
    public int Denominator { get; }
    public RationalFactor(int numerator, int denominator)
    {
        if (denominator == 0) throw new DivideByZeroException();
        if (numerator == 0) { Numerator = 0; Denominator = 1; return; }
        var sign = Math.Sign(numerator) * Math.Sign(denominator);
        numerator = Math.Abs(numerator); denominator = Math.Abs(denominator);
        var g = Gcd(numerator, denominator);
        Numerator = sign * (numerator / g);
        Denominator = denominator / g;
    }
    public static RationalFactor FromFraction(int num, int den) => new(num, den);
    public static RationalFactor operator +(RationalFactor a, RationalFactor b)
        => new(a.Numerator * b.Denominator + b.Numerator * a.Denominator, a.Denominator * b.Denominator);
    public static RationalFactor operator *(RationalFactor a, RationalFactor b)
        => new(a.Numerator * b.Numerator, a.Denominator * b.Denominator);
    public static RationalFactor operator *(RationalFactor a, int k) => new(a.Numerator * k, a.Denominator);
    public static RationalFactor operator /(RationalFactor a, int k) => new(a.Numerator, a.Denominator * k);
    public double ToDouble() => Numerator / (double)Denominator;
    public bool Equals(RationalFactor other) => Numerator == other.Numerator && Denominator == other.Denominator;
    public override bool Equals(object? obj) => obj is RationalFactor rf && Equals(rf);
    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);
    public override string ToString() => $"{Numerator}/{Denominator}";
    private static int Gcd(int a, int b) { while (b != 0) (a, b) = (b, a % b); return a; }
}

/// <summary>
/// 単一の音価 (基底 + 付点 + 連符) を表す。Tie は別途加算演算で対応。
/// Whole Note を 1920 ticks (PPQ=480 * 4/4分) とする。
/// </summary>
public readonly struct Duration : IEquatable<Duration>, IComparable<Duration>
{
    public const int TicksPerQuarter = 480;
    public const int TicksPerWhole = TicksPerQuarter * 4; // 1920
    public const int MaxDots = 3; // 慣例上 3 まで

    /// <summary>Whole Note 基準の有理数 (例: 1/4 = Quarter)。</summary>
    public RationalFactor WholeFraction { get; }
    public int Ticks => (int)((long)WholeFraction.Numerator * TicksPerWhole / WholeFraction.Denominator);

    public Duration(RationalFactor fraction) { WholeFraction = fraction; }

    public static Duration FromBase(BaseNoteValue value, int dots = 0, Tuplet? tuplet = null)
    {
        if (dots < 0) throw new ArgumentOutOfRangeException(nameof(dots));
        if (dots > MaxDots) throw new ArgumentException($"dots>{MaxDots} は未サポート", nameof(dots));
        var (n, d) = value.GetFraction();
        var frac = new RationalFactor(n, d);
        if (dots > 0)
        {
            // 付点倍率: (2 - 1/2^dots) = (2*2^dots -1)/2^dots
            int pow = 1 << dots; // 2^dots
            int numMul = 2 * pow - 1;
            frac = frac * new RationalFactor(numMul, pow);
        }
        if (tuplet.HasValue)
        {
            // 連符倍率 (Normal/Actual)
            var t = tuplet.Value;
            frac = frac * t.FactorRational;
        }
        return new Duration(frac);
    }

    public static Duration FromTicks(int ticks)
    {
        if (ticks < 0) throw new ArgumentOutOfRangeException(nameof(ticks));
        // fraction = ticks / TicksPerWhole
        int num = ticks; int den = TicksPerWhole;
        return new Duration(new RationalFactor(num, den));
    }

    public static Duration operator +(Duration a, Duration b) => new(a.WholeFraction + b.WholeFraction);
    public bool Equals(Duration other) => WholeFraction.Equals(other.WholeFraction);
    public override bool Equals(object? obj) => obj is Duration d && Equals(d);
    public override int GetHashCode() => WholeFraction.GetHashCode();
    public override string ToString() => $"{WholeFraction} ({Ticks} ticks)";

    /// <summary>単一の (BaseNoteValue, dots, tuplet=null) で表現できるか判定。</summary>
    public bool TryAsSimple(out BaseNoteValue baseValue, out int dots)
    {
        // 試行: 各 base + dots(0..3) を比較
        foreach (BaseNoteValue v in Enum.GetValues(typeof(BaseNoteValue)))
        {
            for (int d = 0; d <= 3; d++)
            {
                var candidate = FromBase(v, d);
                if (candidate.Equals(this)) { baseValue = v; dots = d; return true; }
            }
        }
        baseValue = default; dots = 0; return false;
    }

    /// <summary>Tuplet を含む表現の探索 (よく使う代表的連符パターンのみ)。</summary>
    public bool TryDecomposeFull(out BaseNoteValue baseValue, out int dots, out Tuplet? tuplet, bool extendedTuplets=false)
    {
        var tuplets = extendedTuplets ? GenerateExtendedTuplets() : GenerateCommonTuplets();
        var bases = Enum.GetValues(typeof(BaseNoteValue)).Cast<BaseNoteValue>().ToArray();
        var matches = new List<(BaseNoteValue v, int d, Tuplet? t, int baseTicks)>();
        foreach (BaseNoteValue v in bases)
        {
            for (int d = 0; d <= MaxDots; d++)
            {
                var simple = FromBase(v, d);
                if (simple.Equals(this)) { baseValue = v; dots = d; tuplet = null; return true; }
                foreach (var t in tuplets)
                {
                    var cand = FromBase(v, d, t);
                    if (cand.Equals(this)) { matches.Add((v, d, t, FromBase(v,0).Ticks)); }
                }
            }
        }
        if (matches.Count > 0)
        {
            int PrefN(Tuplet? tp) => tp.HasValue ? (tp.Value.NormalCount==4?0: tp.Value.NormalCount==2?1:2) : 3;
            var chosen = matches
                .OrderBy(m => PrefN(m.t))
                .ThenBy(m => m.baseTicks)
                .ThenBy(m => m.t.HasValue ? m.t.Value.ActualCount : int.MaxValue)
                .First();
            baseValue = chosen.v; dots = chosen.d; tuplet = chosen.t; return true;
        }
        baseValue = default; dots = 0; tuplet = null; return false;
    }
    private static Tuplet[] GenerateCommonTuplets()
    {
        // 代表的連符 (3,2),(5,4),(7,4),(5,2),(7,8),(9,8)
        return new[]{ new Tuplet(3,2), new Tuplet(5,4), new Tuplet(7,4), new Tuplet(5,2), new Tuplet(7,8), new Tuplet(9,8)};
    }
    private static Tuplet[] GenerateExtendedTuplets()
    {
        // 2..12 範囲で actual!=normal を列挙
        var list = new List<Tuplet>();
        for (int normal=2; normal<=12; normal++)
            for (int actual=2; actual<=12; actual++)
                if (actual!=normal)
                    list.Add(new Tuplet(actual, normal));
        return list.ToArray();
    }

    public int CompareTo(Duration other)
        => (WholeFraction.Numerator * other.WholeFraction.Denominator)
           .CompareTo(other.WholeFraction.Numerator * WholeFraction.Denominator);
}

/// <summary>休符。Duration ラッパー。</summary>
public readonly partial record struct Rest(Duration Duration);

/// <summary>音符 (将来的に Pitch / Velocity / Articulation を拡張)。</summary>
public readonly partial record struct Note(Duration Duration, int Pitch = 60, int Velocity = 100, int Channel = 0)
{
    public Note Tie(Note next) => new(Duration + next.Duration);
}

public static class DurationFactory
{
    public static Duration DoubleWhole(int dots=0) => Duration.FromBase(BaseNoteValue.DoubleWhole, dots);
    public static Duration Whole(int dots=0) => Duration.FromBase(BaseNoteValue.Whole, dots);
    public static Duration Half(int dots=0) => Duration.FromBase(BaseNoteValue.Half, dots);
    public static Duration Quarter(int dots=0) => Duration.FromBase(BaseNoteValue.Quarter, dots);
    public static Duration Eighth(int dots=0) => Duration.FromBase(BaseNoteValue.Eighth, dots);
    public static Duration Sixteenth(int dots=0) => Duration.FromBase(BaseNoteValue.Sixteenth, dots);
    public static Duration ThirtySecond(int dots=0) => Duration.FromBase(BaseNoteValue.ThirtySecond, dots);
    public static Duration SixtyFourth(int dots=0) => Duration.FromBase(BaseNoteValue.SixtyFourth, dots);
    public static Duration OneHundredTwentyEighth(int dots=0) => Duration.FromBase(BaseNoteValue.OneHundredTwentyEighth, dots);
    public static Duration Tuplet(BaseNoteValue baseValue, Tuplet tuplet, int dots=0) => Duration.FromBase(baseValue, dots, tuplet);
}

/// <summary>
/// 休符生成ヘルパ (Note 用 DurationFactory と対称)。
/// </summary>
public static class RestFactory
{
    public static Rest DoubleWhole(int dots=0) => new(DurationFactory.DoubleWhole(dots));
    public static Rest Whole(int dots=0) => new(DurationFactory.Whole(dots));
    public static Rest Half(int dots=0) => new(DurationFactory.Half(dots));
    public static Rest Quarter(int dots=0) => new(DurationFactory.Quarter(dots));
    public static Rest Eighth(int dots=0) => new(DurationFactory.Eighth(dots));
    public static Rest Sixteenth(int dots=0) => new(DurationFactory.Sixteenth(dots));
    public static Rest ThirtySecond(int dots=0) => new(DurationFactory.ThirtySecond(dots));
    public static Rest SixtyFourth(int dots=0) => new(DurationFactory.SixtyFourth(dots));
    public static Rest OneHundredTwentyEighth(int dots=0) => new(DurationFactory.OneHundredTwentyEighth(dots));
    public static Rest Tuplet(BaseNoteValue baseValue, Tuplet tuplet, int dots=0) => new(DurationFactory.Tuplet(baseValue, tuplet, dots));
    public static Rest FromTicks(int ticks) => new(Duration.FromTicks(ticks));
}

/// <summary>Note / Rest 共通インターフェース。</summary>
public interface IDurationEntity { Duration Duration { get; } }
public readonly partial record struct Note : IDurationEntity {}
public readonly partial record struct Rest : IDurationEntity {}

/// <summary>休符や音符列の正規化ユーティリティ。</summary>
public static class DurationSequenceUtils
{
    /// <summary>隣接する休符をまとめ、可能なら単純形 (例: 8分+8分=4分) に縮約。</summary>
    public static IReadOnlyList<IDurationEntity> NormalizeRests(IEnumerable<IDurationEntity> seq, bool advancedSplit=false, IEnumerable<Tuplet>? additionalTuplets=null, bool mergeTuplets=false, bool extendedTuplets=false, bool allowSplit=false)
    {
        var list = new List<IDurationEntity>();
        // 連続休符をランとして収集し、flush 時にまとめて推論。
        List<Duration>? restRun = null;
        var customTuplets = additionalTuplets?.ToList() ?? new List<Tuplet>();
        void FlushPending()
        {
            if (restRun==null || restRun.Count==0) return;
            var sumTicks = restRun.Sum(d=>d.Ticks);
            var sumDur = Duration.FromTicks(sumTicks);
            // allowSplit: 複数要素から柔軟 tuplets に縮約
            if (allowSplit)
            {
                var inferred = InferCompositeTupletFlexible(restRun, extendedTuplets);
                if (inferred.HasValue && inferred.Value.TryDecomposeFull(out var baseVal2, out var dots2, out var tuplet2, extendedTuplets) && tuplet2.HasValue)
                {
                    // 要素ごとに分解して同一長の Rest を並べる (記譜最適化: tuplet 表記の休符)
                    int total = restRun.Sum(x=>x.Ticks);
                    int elemTicks = total / tuplet2.Value.ActualCount;
                    var elemDur = Duration.FromBase(baseVal2, dots2, tuplet2.Value);
                    for (int i=0;i<tuplet2.Value.ActualCount;i++)
                        list.Add(new Rest(elemDur));
                    restRun.Clear(); return;
                }
            }
            // 単純形 / 既存分解
            if (sumDur.TryAsSimple(out var b, out var dots))
                list.Add(new Rest(Duration.FromBase(b, dots)));
            else if (sumDur.TryDecomposeFull(out var b2, out var dots2, out var tp, extendedTuplets) || TryCustomTuplets(sumDur, customTuplets, out b2, out dots2, out tp))
                list.Add(new Rest(Duration.FromBase(b2, dots2, tp)));
            else
            {
                if (advancedSplit && TrySplitDotted(sumDur, out var parts)) foreach (var p in parts) list.Add(new Rest(p));
                else list.Add(new Rest(sumDur));
            }
            restRun.Clear();
        }

        foreach (var e in seq)
        {
            if (e is Rest r)
            {
                restRun ??= new List<Duration>();
                restRun.Add(r.Duration);
            }
            else
            {
                FlushPending();
                list.Add(e);
            }
        }
        FlushPending();
        if (mergeTuplets)
            list = MergeIntoTuplets(list, extendedTuplets).ToList();
        return list;
    }

    // 付点音価を (基底)+(派生) に分割 (例: dotted quarter = quarter + eighth)。失敗なら false
    private static bool TrySplitDotted(Duration dur, out IEnumerable<Duration> parts)
    {
        parts = Array.Empty<Duration>();
        if (dur.TryAsSimple(out var b, out var dots) && dots>0)
        {
            // 付点展開: base * (1 + 1/2 + 1/4 ...) = base + base/2 + base/4 ...
            var (n, d) = b.GetFraction();
            var baseDur = Duration.FromBase(b, 0);
            var list = new List<Duration>{ baseDur };
            var current = baseDur;
            for(int i=1;i<=dots;i++)
            {
                current = new Duration(new RationalFactor(current.WholeFraction.Numerator, current.WholeFraction.Denominator*2));
                list.Add(current);
            }
            parts = list;
            return true;
        }
        return false;
    }

    private static bool TryCustomTuplets(Duration dur, List<Tuplet> tuplets, out BaseNoteValue baseValue, out int dots, out Tuplet? tuplet)
    {
        baseValue = default; dots = 0; tuplet = null;
        if (tuplets.Count==0) return false;
        foreach (BaseNoteValue v in Enum.GetValues(typeof(BaseNoteValue)))
        {
            for (int d = 0; d <= Duration.MaxDots; d++)
            {
                foreach (var t in tuplets)
                {
                    var candidate = Duration.FromBase(v, d, t);
                    if (candidate.Equals(dur)) { baseValue = v; dots = d; tuplet = t; return true; }
                }
            }
        }
        return false;
    }

    // 分割された (基底 + 1/2 + 1/4...) パターンから付点 / 連符へ再統合 (簡易)
    private static IEnumerable<IDurationEntity> MergeIntoTuplets(IEnumerable<IDurationEntity> src, bool extendedTuplets)
    {
        // 現状: 連続 Rest の合算を再度 TryAsSimple / TryDecomposeFull するだけ
        Rest? pending = null; var outList = new List<IDurationEntity>();
        void Flush()
        {
            if (!pending.HasValue) return; var d = pending.Value.Duration;
            if (d.TryAsSimple(out var b, out var dots)) outList.Add(new Rest(Duration.FromBase(b, dots)));
            else if (d.TryDecomposeFull(out var b2, out var dots2, out var tp, extendedTuplets)) outList.Add(new Rest(Duration.FromBase(b2, dots2, tp)));
            else outList.Add(pending.Value);
            pending = null;
        }
        foreach (var e in src)
        {
            if (e is Rest r)
            {
                pending = pending.HasValue ? new Rest(pending.Value.Duration + r.Duration) : r;
            }
            else { Flush(); outList.Add(e); }
        }
        Flush();
        return outList;
    }

    /// <summary>
    /// 異種音価の連続 (例: 8分 + 16分 + 16分) が 等分割 n 個で構成される単一連符グループ (例: 3:2 の 8分三連) に縮約できるか推論。
    /// 入力は Duration (音符/休符問わず) の列。成功時: 単一 Duration (基底+Tuplet) を返し、失敗時 null。
    /// アルゴリズム: 総 tick / n で均等割可能かつ 各部品 tick が baseValue(付点なし) * factor * (normal/actual) に一致するかを検証。
    /// </summary>
    public static Duration? InferCompositeTuplet(IEnumerable<Duration> parts, bool extendedTuplets=false)
    {
        var list = parts.ToList(); if (list.Count == 0) return null;
        int total = list.Sum(p=>p.Ticks);
        // 最小単位 (全要素 gcd) を取得 (混在長を underlying subdivision に展開可能か確認するため)
        int gcd = list.Select(p=>p.Ticks).Aggregate(Gcd);
        if (gcd == 0) return null;
        // underlying subdivision 個数
    int unitCount = total / gcd;
        // tuplets 候補集合
    var tupletsAll = extendedTuplets ? GenerateExtendedTupletsStatic() : GenerateCommonTupletsStatic();
    int PrefN(int n) => n==4?0 : n==2?1 : 2;
    foreach (var t in tupletsAll
                 .OrderBy(tt=>PrefN(tt.NormalCount))
                 .ThenBy(tt=>tt.ActualCount)
                 .ThenBy(tt=>tt.NormalCount))
        {
            // 基底音価 ticks = total / normalCount
            if (total % t.NormalCount != 0) continue;
            int candidateBaseTicks = total / t.NormalCount; // dots 無し想定 (簡易)
            // baseValue (+dots) 探索: dots 0..3 で一致するか
            foreach (BaseNoteValue v in Enum.GetValues(typeof(BaseNoteValue)))
            {
                for (int dots=0; dots<=Duration.MaxDots; dots++)
                {
                    var baseDur = Duration.FromBase(v, dots);
                    if (baseDur.Ticks != candidateBaseTicks) continue;
                    // 連符適用後の最小単位 ticks = baseDur.Ticks * (normal/actual) / (baseDur 単位) = candidateBaseTicks * t.Factor
                    // 連符適用後一要素 (ideal) 長さ = baseDur.Ticks * t.Factor
                    double elem = baseDur.Ticks * t.Factor;
                    int elemTicks = (int)Math.Round(elem);
                    if (Math.Abs(elem - elemTicks) > 0.0001) continue;
                    // 元要素合計検証 (冗長だが安全)
                    if (list.Sum(p=>p.Ticks) != elemTicks * t.ActualCount) continue;
                    // 各要素 tick は elemTicks を超えない & elemTicks の約数の和で構成 (例: 240,120,120 を 160+160+160 の再構成とみなす) -> 要素を elemTicks 単位に分割可能か
                    int neededUnits = t.ActualCount; int consumedUnits=0; bool feasible=true;
                    foreach (var p in list)
                    {
                        if (p.Ticks > elemTicks) { // 要素が長すぎる場合は elemTicks で割れるかチェック
                            if (p.Ticks % elemTicks !=0) { feasible=false; break; }
                            consumedUnits += p.Ticks / elemTicks;
                        }
                        else if (elemTicks % p.Ticks ==0)
                        {
                            consumedUnits += (elemTicks / p.Ticks); // 小さい音価複数が1要素相当を構成
                        }
                        else { feasible=false; break; }
                        if (consumedUnits > neededUnits){ feasible=false; break; }
                    }
                    if (!feasible || consumedUnits!=neededUnits) continue;
                    return Duration.FromBase(v, dots, t);
                }
            }
        }
        return null;
    }

    private static Tuplet[] GenerateCommonTupletsStatic() => new[]{ new Tuplet(3,2), new Tuplet(5,4), new Tuplet(7,4), new Tuplet(5,2), new Tuplet(7,8), new Tuplet(9,8)};
    private static Tuplet[] GenerateExtendedTupletsStatic()
    {
        var list = new List<Tuplet>();
        for (int normal=2; normal<=12; normal++) for (int actual=2; actual<=12; actual++) if (actual!=normal) list.Add(new Tuplet(actual, normal));
        return list.ToArray();
    }

    private static int Gcd(int a, int b){ while (b!=0) (a,b) = (b, a % b); return Math.Abs(a); }

    /// <summary>
    /// 分割許容版 (allowSplit=true) の連符推論。要素内で自由に境界を切れると仮定し、総時間と候補連符から逆算。
    /// 例: 8分(240)+16分(120)+16分(120)=480 を 8分三連 (3:2) (要素長 160) として推論可。
    /// 制約: base + dots + tuplet の組み合わせのうち最小 (base値の長い順優先) を返す。
    /// </summary>
    public static Duration? InferCompositeTupletFlexible(IEnumerable<Duration> parts, bool extendedTuplets=false)
    {
        var list = parts.ToList(); if (list.Count < 2) return null;
        int total = list.Sum(p=>p.Ticks);
        var tuplets = extendedTuplets ? GenerateExtendedTupletsStatic() : GenerateCommonTupletsStatic();
        var bases = Enum.GetValues(typeof(BaseNoteValue)).Cast<BaseNoteValue>().OrderByDescending(v=>Duration.FromBase(v).Ticks).ToList();
        int minPart = list.Min(p=>p.Ticks);
        var candidates = new List<(Duration dur, Tuplet t, int elemTicks, int baseTicks)>();
        foreach (var t in tuplets)
        {
            if (total % t.NormalCount != 0) continue;
            int candidateBaseTicks = total / t.NormalCount;
            foreach (var b in bases)
            {
                for (int dots=0; dots<=Duration.MaxDots; dots++)
                {
                    var baseDur = Duration.FromBase(b, dots);
                    if (baseDur.Ticks != candidateBaseTicks) continue;
                    int elemTicks = (int)Math.Round(baseDur.Ticks * t.Factor);
                    if (elemTicks * t.ActualCount != total) continue;
                    var cand = Duration.FromBase(b, dots, t);
                    candidates.Add((cand, t, elemTicks, baseDur.Ticks));
                }
            }
        }
        if (candidates.Count > 0)
        {
            // 優先順位:
            // 1) 要素長が minPart 以上
            // 2) ActualCount の優先度 [5,3,7,9,11,4,6,8,10,12,2]（extended で 5 を優先）
            // 3) NormalCount の慣例優先 (4 -> 2 -> その他)
            // 4) baseTicks 小さい順
            int IndexActual(int a)
            {
                int[] pref = new[]{5,3,7,9,11,4,6,8,10,12,2};
                int idx = Array.IndexOf(pref, a);
                return idx >= 0 ? idx : pref.Length + a; // 未掲載は後方
            }
            int PrefNormal(int n) => n==4?0 : n==2?1 : 2;
            var chosen = candidates
                .OrderByDescending(c => c.t.ActualCount >= c.t.NormalCount)
                .ThenByDescending(c => c.elemTicks >= minPart)
                .ThenBy(c => IndexActual(c.t.ActualCount))
                .ThenBy(c => PrefNormal(c.t.NormalCount))
                .ThenBy(c => c.baseTicks)
                .First();
            return chosen.dur;
        }
        return null;
    }
}
