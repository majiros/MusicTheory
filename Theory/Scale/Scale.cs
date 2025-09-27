using System;
using System.Linq;
using System.Collections.Generic;
using MusicTheory.Theory.Pitch; // PitchClass / PitchUtils / SpelledPitch

namespace MusicTheory.Theory.Scale
{
    public interface IScale
    {
        string Name { get; }
        IReadOnlyList<int> GetSemitoneSet();
        bool ContainsSemitone(int semitone) => GetSemitoneSet().Contains(semitone % 12);
    }
    
    public static class KeySignatureInference
    {
        // Major root (0=C) → sharps (負=flats)
        private static readonly Dictionary<int,int> MajorSharps = new(){ {0,0},{7,1},{2,2},{9,3},{4,4},{11,5},{6,6},{1,7},{5,-1},{10,-2},{3,-3},{8,-4} };
        private static readonly int[] Ionian = {0,2,4,5,7,9,11};
        private static readonly int[] Aeolian = {0,2,3,5,7,8,10};
        private static readonly (string name,int[] pattern,int relShiftToIonian)[] ModePatterns = new[]{
            ("Ionian", new[]{0,2,4,5,7,9,11}, 0),
            ("Dorian", new[]{0,2,3,5,7,9,10}, -2), // Dorian root +10 = Ionian root (2 半音下)
            ("Phrygian", new[]{0,1,3,5,7,8,10}, -4),
            ("Lydian", new[]{0,2,4,6,7,9,11}, +5),
            ("Mixolydian", new[]{0,2,4,5,7,9,10}, -7),
            ("Aeolian", new[]{0,2,3,5,7,8,10}, -9),
            ("Locrian", new[]{0,1,3,5,6,8,10}, -11)
        };
        public static int? InferKeySignature(IScale scale)
        {
            var set = scale.GetSemitoneSet().Select(x=>x%12).Distinct().OrderBy(x=>x).ToArray();
            if (set.Length!=7) return null; // 対象外
            for(int root=0; root<12; root++)
            {
                var rotated = set.Select(s=>(s-root+12)%12).OrderBy(x=>x).ToArray();
                foreach (var mode in ModePatterns)
                {
                    if (rotated.SequenceEqual(mode.pattern))
                    {
                        // Ionian (major) の root を求める (mode.relShiftToIonian は modeRoot から IonianRoot への半音差)
                        int majorRoot = (root + (12 + mode.relShiftToIonian)) % 12;
                        if (MajorSharps.TryGetValue(majorRoot, out var val)) return val;
                    }
                }
            }
            return null;
        }
    }
    /// <summary>
    /// 拍子（メトリック・シグネチャ）を表現するクラス
    /// </summary>
    public class TimeSignature
    {
        /// <summary>
        /// 分子（拍数、例：4/4拍子なら4）
        /// </summary>
        public int Numerator { get; set; }

        /// <summary>
        /// 分母（1拍の音価、例：4/4拍子なら4は四分音符）
        /// </summary>
        public int Denominator { get; set; }

        public TimeSignature(int numerator, int denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
        }
    }

    /// <summary>
    /// ピッチクラスのみで構成されるスケール（和声音集合）
    /// 他ファイルの ModalScale との衝突回避のため PcScale と命名
    /// </summary>
    public readonly record struct PcScale(string Name, PitchClass[] Degrees) : IScale
    {
        public bool Contains(PitchClass pc) => Degrees.Any(d => d.Pc == pc.Pc);
        public override string ToString() => $"{Name}: [{string.Join(", ", Degrees.Select(d => d.ToString()))}]";

        public PcScale Transposed(int rootPc)
        {
            var transposed = Degrees.Select(d => new PitchClass((d.Pc + rootPc) % 12)).ToArray();
            return new PcScale($"{Name} (Root: {rootPc})", transposed);
        }

        public string[] GetJapaneseDegreeNames()
        {
            return PcScaleLibrary.JapaneseDegreeNames;
        }

        public IEnumerable<(SpelledPitch, string)> GetSpelledDegreesWithNames(SpelledPitch root)
        {
            var rootPc = PitchUtils.ToPc(root).Pc;
            var transposed = Degrees.Select(d => new PitchClass((d.Pc + rootPc) % 12)).ToArray();
            for (int i = 0; i < transposed.Length; i++)
            {
                var spelled = PitchUtils.GetEnharmonicSpellings(new PitchClass(transposed[i].Pc)).FirstOrDefault();
                var name = PcScaleLibrary.JapaneseDegreeNames[i];
                yield return (spelled, name);
            }
        }

    public IReadOnlyList<int> GetSemitoneSet() => Degrees.Select(d => d.Pc).ToArray();
    }

    public static class PcScaleLibrary
    {
        public static PcScale Major => new("Major", new[] { new PitchClass(0), new PitchClass(2), new PitchClass(4), new PitchClass(5), new PitchClass(7), new PitchClass(9), new PitchClass(11) });
        public static PcScale Minor => new("Minor", new[] { new PitchClass(9), new PitchClass(11), new PitchClass(0), new PitchClass(2), new PitchClass(4), new PitchClass(5), new PitchClass(7) });

        public static PcScale[] ChurchModes => new[]
        {
            new PcScale("Ionian", new[] { new PitchClass(0), new PitchClass(2), new PitchClass(4), new PitchClass(5), new PitchClass(7), new PitchClass(9), new PitchClass(11) }),
            new PcScale("Dorian", new[] { new PitchClass(0), new PitchClass(2), new PitchClass(3), new PitchClass(5), new PitchClass(7), new PitchClass(9), new PitchClass(10) }),
            new PcScale("Phrygian", new[] { new PitchClass(0), new PitchClass(1), new PitchClass(3), new PitchClass(5), new PitchClass(7), new PitchClass(8), new PitchClass(10) }),
            new PcScale("Lydian", new[] { new PitchClass(0), new PitchClass(2), new PitchClass(4), new PitchClass(6), new PitchClass(7), new PitchClass(9), new PitchClass(11) }),
            new PcScale("Mixolydian", new[] { new PitchClass(0), new PitchClass(2), new PitchClass(4), new PitchClass(5), new PitchClass(7), new PitchClass(9), new PitchClass(10) }),
            new PcScale("Aeolian", new[] { new PitchClass(0), new PitchClass(2), new PitchClass(3), new PitchClass(5), new PitchClass(7), new PitchClass(8), new PitchClass(10) }),
            new PcScale("Locrian", new[] { new PitchClass(0), new PitchClass(1), new PitchClass(3), new PitchClass(5), new PitchClass(6), new PitchClass(8), new PitchClass(10) })
        };

        public static PcScale Chromatic => new("Chromatic", Enumerable.Range(0, 12).Select(i => new PitchClass(i)).ToArray());
        public static PcScale WholeTone => new("Whole Tone", new[] { new PitchClass(0), new PitchClass(2), new PitchClass(4), new PitchClass(6), new PitchClass(8), new PitchClass(10) });
        public static PcScale PentatonicMajor => new("Pentatonic Major", new[] { new PitchClass(0), new PitchClass(2), new PitchClass(4), new PitchClass(7), new PitchClass(9) });
        public static PcScale PentatonicMinor => new("Pentatonic Minor", new[] { new PitchClass(0), new PitchClass(3), new PitchClass(5), new PitchClass(7), new PitchClass(10) });
        public static PcScale Blues => new("Blues", new[] { new PitchClass(0), new PitchClass(3), new PitchClass(5), new PitchClass(6), new PitchClass(7), new PitchClass(10) });

        public static readonly string[] JapaneseDegreeNames = new[]
        {
            "主音",
            "上主音",
            "中音",
            "下属音",
            "属音",
            "下中音",
            "導音"
        };

        public static IEnumerable<PcScale> FindScalesContaining(PitchClass pc)
        {
            return new[]
            {
                Major,
                Minor,
                Chromatic,
                WholeTone,
                PentatonicMajor,
                PentatonicMinor,
                Blues
            }.Concat(ChurchModes).Where(s => s.Contains(pc));
        }
    }

    // ModalScale / ExtendedScales は別ファイルへ分離

}