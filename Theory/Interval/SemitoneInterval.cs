namespace MusicTheory.Theory.Interval;

/// <summary>
/// 純粋な半音数のみを保持する軽量インターバル。
/// 機能的属性 (TensionFunction など) を持たない。
/// </summary>
public readonly record struct SemitoneInterval(int Semitones)
{
    public static implicit operator SemitoneInterval(int semitones) => new(semitones);
    public static implicit operator int(SemitoneInterval si) => si.Semitones;
    public override string ToString() => $"{Semitones}st";
}
