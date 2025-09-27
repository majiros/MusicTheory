using System;

namespace MusicTheory.Theory.Interval
{
    public enum IntervalType
    {
        PerfectUnison = 0,
        MinorSecond = 1,
        MajorSecond = 2,
        MinorThird = 3,
        MajorThird = 4,
        PerfectFourth = 5,
        Tritone = 6,
        PerfectFifth = 7,
        MinorSixth = 8,
        MajorSixth = 9,
        MinorSeventh = 10,
        MajorSeventh = 11,
        Octave = 12,
        MinorNinth = 13,
        MajorNinth = 14,
        AugmentedNinth = 15,
        PerfectEleventh = 17,
        AugmentedEleventh = 18,
        MinorThirteenth = 20,
        MajorThirteenth = 21
    }

    public enum TensionFunction
    {
        None,
        Modal,
        Avoid,
        Altered,
        LeadingTone,
        Suspensive,
        ChromaticColor
    }

    // 新名称クラス (今後はこちらを使用)
    public readonly record struct FunctionalInterval
    {
        public IntervalType Type { get; }
        public int Semitones => (int)Type;
        public TensionFunction Function { get; }
        public FunctionalInterval(IntervalType type, TensionFunction function = TensionFunction.None)
        {
            Type = type;
            Function = function;
        }
        public string DisplayName => Type switch
        {
            IntervalType.MinorSecond => "♭9",
            IntervalType.MajorSecond => "9",
            IntervalType.AugmentedNinth => "♯9",
            IntervalType.PerfectEleventh => "11",
            IntervalType.AugmentedEleventh => "♯11",
            IntervalType.MinorThirteenth => "♭13",
            IntervalType.MajorThirteenth => "13",
            _ => Type.ToString()
        };
    }
}
