namespace MusicTheory.Theory.Pitch
{

    public enum AccidentalNotation
    {
        Unicode, // ♯, ♭, 𝄪
        ASCII,   // #, b, ##
        ASCII_X  // #, b, x
    }

    public readonly record struct Accidental(int Semitones)
    {
        public static AccidentalNotation Notation { get; set; } = AccidentalNotation.Unicode;
        public int AccidentalValue => Math.Clamp(Semitones, -3, +3);

        public override string ToString() => Notation switch
        {
            AccidentalNotation.Unicode => this.AccidentalValue switch
            {
                -3 => "♭♭♭",
                -2 => "♭♭",
                -1 => "♭",
                0 => "",
                1 => "♯",
                2 => "𝄪",
                3 => "♯𝄪",
                _ => ""
            },

            AccidentalNotation.ASCII => this.AccidentalValue switch
            {
                -3 => "bbb",
                -2 => "bb",
                -1 => "b",
                0 => "",
                1 => "#",
                2 => "##",
                3 => "###",
                _ => ""
            },
            AccidentalNotation.ASCII_X => this.AccidentalValue switch
            {
                -3 => "bbb",
                -2 => "bb",
                -1 => "b",
                0 => "",
                1 => "#",
                2 => "x",
                3 => "#x",
                _ => ""
            },
            _ => ""
        };
    }

}