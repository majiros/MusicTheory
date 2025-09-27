namespace MusicTheory
{

    // 12-TET チューニング
    public interface ITuning { double ToFrequency(int midi); }
    public sealed class EqualTemperament12 : ITuning
    {
        public double A4Hz { get; }
        public EqualTemperament12(double a4Hz = 440.0) => A4Hz = a4Hz;
        public double ToFrequency(int midi) => A4Hz * Math.Pow(2.0, (midi - 69) / 12.0);
    }
}