namespace MusicTheory.Theory.Pitch
{

    // 等音（pc）
    public readonly record struct PitchClass(int Value)
{
    public int Pc => ((Value % 12) + 12) % 12;
    public static PitchClass FromMidi(int midi) => new(midi % 12);
    public static PitchClass operator +(PitchClass a, int semitones) => new(a.Pc + semitones);
    public override string ToString() => Pc.ToString("00");
}


    // 絶対音（綴り＋オクターブ）
    public readonly record struct Pitch(SpelledPitch Spelling, int Octave)
    {
        public override string ToString() => $"{Spelling}{Octave}";
    }

}