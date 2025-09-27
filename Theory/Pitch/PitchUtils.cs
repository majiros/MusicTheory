using System;
using System.Collections.Generic;

namespace MusicTheory.Theory.Pitch
{

    // 変換ユーティリティ（綴り→pc、Pitch↔MIDI）
    public static class PitchUtils
    {
        // C=0, D=2, E=4, F=5, G=7, A=9, B=11 を基準に綴り→pc
        public static int LetterBasePc(Letter l) => l switch { Letter.C => 0, Letter.D => 2, Letter.E => 4, Letter.F => 5, Letter.G => 7, Letter.A => 9, Letter.B => 11, _ => 0 };
        public static PitchClass ToPc(SpelledPitch s) => new(LetterBasePc(s.Letter) + s.Acc.AccidentalValue);
        public static PitchClass ToPc(Pitch p) => ToPc(p.Spelling);

        // MIDI は C-1=0 とし、綴りのオクターブ系を MIDI に投影
        public static int ToMidi(Pitch p)
        {
            // 例: C4=60（慣習）
            int octaveOffset = (p.Octave + 1) * 12;
            return octaveOffset + ToPc(p).Pc;
        }
        public static Pitch FromMidi(int midi, SpelledPitch prefer = default, int? preferOctave = null)
        {
            // 初期版: 等音→簡易綴り（黒鍵は♯寄り）で復元
            int pc = ((midi % 12) + 12) % 12;
            var (letter, acc) = pc switch
            {
                0 => (Letter.C, 0),
                1 => (Letter.C, 1),
                2 => (Letter.D, 0),
                3 => (Letter.D, 1),
                4 => (Letter.E, 0),
                5 => (Letter.F, 0),
                6 => (Letter.F, 1),
                7 => (Letter.G, 0),
                8 => (Letter.G, 1),
                9 => (Letter.A, 0),
                10 => (Letter.A, 1),
                11 => (Letter.B, 0),
                _ => (Letter.C, 0)
            };
            int oct = (midi / 12) - 1;
            return new Pitch(new SpelledPitch(letter, new Accidental(acc)), preferOctave ?? oct);
        }


        //複音程

        public static Pitch Transpose(Pitch p, MusicTheory.Theory.Interval.SemitoneInterval ivl)
            => FromMidi(ToMidi(p) + ivl.Semitones);
        public static PitchClass Transpose(PitchClass pc, MusicTheory.Theory.Interval.SemitoneInterval ivl)
            => new PitchClass(pc.Pc + ivl.Semitones);


        //異名同音の表記
        public static IEnumerable<SpelledPitch> GetEnharmonicSpellings(PitchClass pc)
        {
            foreach (Letter l in Enum.GetValues(typeof(Letter)))
            {
                int basePc = LetterBasePc(l);
                int acc = pc.Pc - basePc;
                if (acc >= -3 && acc <= 3)
                    yield return new SpelledPitch(l, new Accidental(acc));
            }
        }
    }

}