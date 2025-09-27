namespace MusicTheory.Theory.Harmony;

public enum Voice { Soprano, Alto, Tenor, Bass }

public readonly record struct VoiceRange(int MinMidi, int MaxMidi, int WarnMinMidi, int WarnMaxMidi)
{
    public bool InHardRange(int midi) => midi >= MinMidi && midi <= MaxMidi;
    public bool InWarnRange(int midi) => midi >= WarnMinMidi && midi <= WarnMaxMidi;
}

public static class VoiceRanges
{
    // 目安: (Sop C4-A5),(Alt G3-D5),(Ten C3-A4),(Bs F2-D4) とし ±長三度(±4semitones)を警告許容
    private static (int min,int max) Base(string name) => name switch
    {
        "S" => (60, 81), // C4..A5
        "A" => (55, 74), // G3..D5
        "T" => (48, 69), // C3..A4
        "B" => (41, 62), // F2..D4
        _ => (60,81)
    };
    private static VoiceRange Make((int min,int max) b)
        => new(b.min, b.max, b.min-4, b.max+4);

    public static VoiceRange Soprano => Make(Base("S"));
    public static VoiceRange Alto    => Make(Base("A"));
    public static VoiceRange Tenor   => Make(Base("T"));
    public static VoiceRange Bass    => Make(Base("B"));

    public static VoiceRange ForVoice(Voice v) => v switch
    {
        Voice.Soprano => Soprano,
        Voice.Alto => Alto,
        Voice.Tenor => Tenor,
        Voice.Bass => Bass,
        _ => Soprano
    };
}
