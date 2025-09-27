namespace MusicTheory.Theory.Scale;

using MusicTheory.Theory.Interval;

public class ModalScale : IScale
{
    public string Name { get; }
    private readonly int[] semitones;
    public ModalScale(string name, int[] semitones)
    {
        Name = name;
        this.semitones = semitones;
    }
    public IReadOnlyList<int> GetSemitoneSet() => semitones;
    public bool Contains(FunctionalInterval ivl) => semitones.Contains(ivl.Semitones % 12);
    public bool ContainsSemitone(int semitone) => semitones.Contains(((semitone % 12)+12)%12);
}
