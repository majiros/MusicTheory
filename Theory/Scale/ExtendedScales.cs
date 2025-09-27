namespace MusicTheory.Theory.Scale;

public static class ExtendedScales
{
    public static readonly ModalScale Ionian = new("Ionian", new[] {0,2,4,5,7,9,11});
    public static readonly ModalScale Dorian = new("Dorian", new[] {0,2,3,5,7,9,10});
    public static readonly ModalScale Phrygian = new("Phrygian", new[] {0,1,3,5,7,8,10});
    public static readonly ModalScale Lydian = new("Lydian", new[] {0,2,4,6,7,9,11});
    public static readonly ModalScale Mixolydian = new("Mixolydian", new[] {0,2,4,5,7,9,10});
    public static readonly ModalScale Aeolian = new("Aeolian", new[] {0,2,3,5,7,8,10});
    public static readonly ModalScale Locrian = new("Locrian", new[] {0,1,3,5,6,8,10});

    public static readonly ModalScale LydianDominant = new("Lydian Dominant", new[] {0,2,4,6,7,9,10});
    public static readonly ModalScale Altered = new("Altered", new[] {0,1,3,4,6,8,10});
    public static readonly ModalScale BebopMajor = new("Bebop Major", new[] {0,2,4,5,7,8,9,11});
    public static readonly ModalScale BebopDominant = new("Bebop Dominant", new[] {0,2,4,5,7,9,10,11});
    public static readonly ModalScale WholeTone = new("Whole Tone", new[] {0,2,4,6,8,10});
    public static readonly ModalScale DiminishedHalfWhole = new("Diminished (H-W)", new[] {0,1,3,4,6,7,9,10});
    public static readonly ModalScale DiminishedWholeHalf = new("Diminished (W-H)", new[] {0,2,3,5,6,8,9,11});

    public static IEnumerable<ModalScale> All
    {
        get
        {
            yield return Ionian; yield return Dorian; yield return Phrygian; yield return Lydian; yield return Mixolydian; yield return Aeolian; yield return Locrian;
            yield return LydianDominant; yield return Altered; yield return BebopMajor; yield return BebopDominant; yield return WholeTone; yield return DiminishedHalfWhole; yield return DiminishedWholeHalf;
        }
    }
}
