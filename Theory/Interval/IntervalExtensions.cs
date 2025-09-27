namespace MusicTheory.Theory.Interval;

public static class IntervalExtensions
{
    public static SemitoneInterval ToSemitone(this FunctionalInterval ivl) => new(ivl.Semitones);
}
