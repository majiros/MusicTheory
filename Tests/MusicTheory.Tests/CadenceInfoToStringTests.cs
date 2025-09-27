using MusicTheory.Theory.Harmony;
using Xunit;

namespace MusicTheory.Tests;

public class CadenceInfoToStringTests
{
    [Fact]
    public void CadenceInfo_ToString_Prints_All_Fields()
    {
        var info = new CadenceInfo(
            IndexFrom: 3,
            Type: CadenceType.Authentic,
            IsPerfectAuthentic: true,
            HasCadentialSixFour: true,
            SixFour: SixFourType.Cadential
        );

        var s = info.ToString();

        Assert.Contains("IndexFrom = 3", s);
        Assert.Contains("Type = Authentic", s);
        Assert.Contains("IsPerfectAuthentic = True", s);
        Assert.Contains("HasCadentialSixFour = True", s);
        Assert.Contains("SixFour = Cadential", s);
    }
}
