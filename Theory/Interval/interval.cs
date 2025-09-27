using MusicTheory.Theory.Pitch;

namespace MusicTheory.Theory.Interval
{
    public enum IntervalQuality { Perfect, Major, Minor, Augmented, Diminished, DoublyAugmented, DoublyDiminished }

    public readonly record struct IntervalName(IntervalQuality Quality, int Number)
    {
        public string ToJapaneseString() => Quality switch
        {
            IntervalQuality.Perfect => $"完全{Number}",
            IntervalQuality.Major => $"長{Number}",
            IntervalQuality.Minor => $"短{Number}",
            IntervalQuality.Augmented => $"増{Number}",
            IntervalQuality.Diminished => $"減{Number}",
            IntervalQuality.DoublyAugmented => $"重増{Number}",
            IntervalQuality.DoublyDiminished => $"重減{Number}",
            _ => $"?{Number}"
        };

        public override string ToString() => $"{Quality} {Number}";
    }

    public static class IntervalUtils
    {
    public static IntervalName GetIntervalName(FunctionalInterval ivl)
        {
            int semitones = ivl.Semitones;
            int octaves = semitones / 12;
            int remainder = semitones % 12;
            int number = octaves * 7 + (remainder switch
            {
                0 => 1,
                1 => 2,
                2 => 2,
                3 => 2,
                4 => 3,
                5 => 3,
                6 => 4,
                7 => 4,
                8 => 4,
                9 => 5,
                10 => 6,
                11 => 6,
                _ => 1
            });

            IntervalQuality quality = semitones switch
            {
                0 => IntervalQuality.Perfect,
                1 => IntervalQuality.DoublyDiminished,
                2 => IntervalQuality.Minor,
                3 => IntervalQuality.Major,
                4 => IntervalQuality.Minor,
                5 => IntervalQuality.Major,
                6 => IntervalQuality.Perfect,
                7 => IntervalQuality.Augmented,
                8 => IntervalQuality.DoublyAugmented,
                9 => IntervalQuality.Perfect,
                10 => IntervalQuality.Minor,
                11 => IntervalQuality.Major,
                12 => IntervalQuality.Minor,
                13 => IntervalQuality.Major,
                14 => IntervalQuality.Perfect,
                _ => IntervalQuality.Augmented
            };

            return new IntervalName(quality, number);
        }

    public static FunctionalInterval Between(MusicTheory.Theory.Pitch.Pitch a, MusicTheory.Theory.Pitch.Pitch b) => new FunctionalInterval((IntervalType)(PitchUtils.ToMidi(b) - PitchUtils.ToMidi(a)));

    public static int DegreeBetween(MusicTheory.Theory.Pitch.Pitch a, MusicTheory.Theory.Pitch.Pitch b)
        {
            int aIndex = (int)a.Spelling.Letter + a.Octave * 7;
            int bIndex = (int)b.Spelling.Letter + b.Octave * 7;
            return bIndex - aIndex + 1;
        }

        public static bool IsEnharmonic(MusicTheory.Theory.Pitch.Pitch a, MusicTheory.Theory.Pitch.Pitch b)
            => PitchUtils.ToPc(a).Pc == PitchUtils.ToPc(b).Pc;
    }
}