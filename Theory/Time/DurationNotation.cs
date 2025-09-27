using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MusicTheory.Theory.Time;

/// <summary>
/// Duration の簡易文字列表現パーサ/フォーマッタ。
/// サポート:
/// - 基本値の略号: W(whole), H(half), Q(quarter), E(eighth), S(sixteenth), T(thirty-second), X(sixty-fourth), O(128th), D/B(double whole)
/// - 分数表記: "1/4", "3/8" など（Whole=1 を基準）
/// - 付点: 終端の '.' を 1〜3 個（例: "Q.", "1/8.."）
/// - 連符: 末尾に "(a:b)" または "*a:b" を付与（例: "E(3:2)", "1/8*5:4"）
/// 未対応/不正な表記は TryParse=false を返し、Parse は ArgumentException を投げます。
/// </summary>
public static class DurationNotation
{
    public static bool TryParse(string text, out Duration duration)
    {
        duration = default;
        if (string.IsNullOrWhiteSpace(text)) return false;
        var s = text.Trim();

        // 1) 連符抽出: (a:b) or *a:b （末尾にあるとみなす）
        Tuplet? tuplet = null;
        var mParen = Regex.Match(s, "\\((\\d+)\\s*:\\s*(\\d+)\\)\\s*$");
        var mStar = !mParen.Success ? Regex.Match(s, "\\*(\\d+)\\s*:\\s*(\\d+)\\s*$") : Match.Empty;
        if (mParen.Success || mStar.Success)
        {
            var m = mParen.Success ? mParen : mStar;
            if (!int.TryParse(m.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var a)) return false;
            if (!int.TryParse(m.Groups[2].Value, NumberStyles.None, CultureInfo.InvariantCulture, out var b)) return false;
            if (a <= 0 || b <= 0) return false;
            tuplet = new Tuplet(a, b);
            s = s.Substring(0, (mParen.Success ? mParen.Index : mStar.Index)).TrimEnd();
        }

        // 2) 付点抽出: 末尾 '.' を数える（最大3）
        int dots = 0;
        for (int i = s.Length - 1; i >= 0 && s[i] == '.'; i--) dots++;
        if (dots > Duration.MaxDots) return false;
        if (dots > 0) s = s.Substring(0, s.Length - dots).TrimEnd();

        // 3) 本体: 略号 もしくは 分数 n/d
        RationalFactor fraction;
        if (TryParseAbbrev(s, out var baseValue))
        {
            var (n, d) = baseValue.GetFraction();
            fraction = new RationalFactor(n, d);
        }
        else if (TryParseFraction(s, out var num, out var den))
        {
            fraction = new RationalFactor(num, den);
        }
        else
        {
            return false;
        }

        // 4) 付点適用
        if (dots > 0)
        {
            int pow = 1 << dots; // 2^dots
            int numMul = 2 * pow - 1;
            fraction = fraction * new RationalFactor(numMul, pow);
        }

        // 5) 連符適用
        if (tuplet.HasValue)
        {
            fraction = fraction * tuplet.Value.FactorRational; // normal/actual
        }

        duration = new Duration(fraction);
        return true;
    }

    public static Duration Parse(string text)
        => TryParse(text, out var d) ? d : throw new ArgumentException($"Invalid duration notation: '{text}'");

    /// <summary>
    /// Duration を簡易記法へフォーマット。分解に成功すれば略号+付点(+連符)を返し、失敗時は既約分数で返す。
    /// 例: Quarter+dots1 → "Q." / Eighth triplet → "E(3:2)" / 既知以外 → "num/den"
    /// </summary>
    public static string ToNotation(Duration duration, bool extendedTuplets = false)
    {
        if (duration.TryDecomposeFull(out var baseValue, out var dots, out var tuplet, extendedTuplets))
        {
            string core = AbbrevFor(baseValue);
            core += new string('.', dots);
            if (tuplet.HasValue)
            {
                var t = tuplet.Value;
                if (extendedTuplets)
                {
                    core += $"*{t.ActualCount}:{t.NormalCount}";
                }
                else
                {
                    core += $"({t.ActualCount}:{t.NormalCount})";
                }
            }
            return core;
        }
        // fallback: fraction
        var f = duration.WholeFraction;
        return $"{f.Numerator}/{f.Denominator}";
    }

    private static bool TryParseAbbrev(string s, out BaseNoteValue value)
    {
        value = default;
        if (string.IsNullOrEmpty(s)) return false;
        switch (s.Trim().ToUpperInvariant())
        {
            case "D": case "B": value = BaseNoteValue.DoubleWhole; return true; // Breve/Double
            case "W": value = BaseNoteValue.Whole; return true;
            case "H": value = BaseNoteValue.Half; return true;
            case "Q": value = BaseNoteValue.Quarter; return true;
            case "E": value = BaseNoteValue.Eighth; return true;
            case "S": value = BaseNoteValue.Sixteenth; return true;
            case "T": value = BaseNoteValue.ThirtySecond; return true;
            case "X": value = BaseNoteValue.SixtyFourth; return true;
            case "O": value = BaseNoteValue.OneHundredTwentyEighth; return true;
            default: return false;
        }
    }

    private static bool TryParseFraction(string s, out int num, out int den)
    {
        num = den = 0;
        var parts = s.Split('/', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return false;
        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out num)) return false;
        if (!int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out den)) return false;
        if (den == 0) return false;
        return true;
    }

    private static string AbbrevFor(BaseNoteValue v)
        => v switch
        {
            BaseNoteValue.DoubleWhole => "D",
            BaseNoteValue.Whole => "W",
            BaseNoteValue.Half => "H",
            BaseNoteValue.Quarter => "Q",
            BaseNoteValue.Eighth => "E",
            BaseNoteValue.Sixteenth => "S",
            BaseNoteValue.ThirtySecond => "T",
            BaseNoteValue.SixtyFourth => "X",
            BaseNoteValue.OneHundredTwentyEighth => "O",
            _ => "?"
        };
}
