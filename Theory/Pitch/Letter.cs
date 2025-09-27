namespace MusicTheory.Theory.Pitch
{
    // 綴り
    public enum Letter { C, D, E, F, G, A, B }

    public readonly record struct SpelledPitch(Letter Letter, Accidental Acc)
    {
        public override string ToString() => $"{Letter}{Acc}";
    }
}