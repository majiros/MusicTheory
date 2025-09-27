namespace MusicTheory.Theory.Harmony;

public enum TonalFunction { Tonic, Subdominant, Dominant, Unknown }

public enum RomanNumeral
{
    I, II, III, IV, V, VI, VII,
    i, ii, iii, iv, v, vi, vii
}

public readonly record struct Key(int TonicMidi, bool IsMajor)
{
    public int ScaleDegreeMidi(int degree) => TonicMidi + (IsMajor ? MajorSteps[degree % 7] : MinorSteps[degree % 7]);
    private static readonly int[] MajorSteps = { 0, 2, 4, 5, 7, 9, 11 };
    private static readonly int[] MinorSteps = { 0, 2, 3, 5, 7, 8, 10 };
}

public static class RomanNumeralUtils
{
    public static TonalFunction FunctionOf(RomanNumeral rn) => rn switch
    {
        RomanNumeral.I or RomanNumeral.i => TonalFunction.Tonic,
        RomanNumeral.III or RomanNumeral.iii or RomanNumeral.VI or RomanNumeral.vi => TonalFunction.Tonic,
        RomanNumeral.IV or RomanNumeral.iv or RomanNumeral.II or RomanNumeral.ii => TonalFunction.Subdominant,
        RomanNumeral.V or RomanNumeral.v or RomanNumeral.VII or RomanNumeral.vii => TonalFunction.Dominant,
        _ => TonalFunction.Unknown
    };
}
