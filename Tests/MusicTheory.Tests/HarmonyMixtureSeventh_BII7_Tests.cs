using Xunit;
using MusicTheory.Theory.Harmony;

namespace MusicTheory.Tests;

public class HarmonyMixtureSeventh_BII7_Tests
{
	private static int Pc(int midi) => ((midi % 12) + 12) % 12;

	[Fact]
	public void Borrowed_bII7_RootLabel_Without_Voicing()
	{
		var key = new Key(60, true); // C major
		// Db F Ab B (Cb enharmonic) => pcs {1,5,8,11}
		var pcs = new[] { Pc(61), Pc(65), Pc(68), Pc(71) };
		var r = HarmonyAnalyzer.AnalyzeTriad(pcs, key);
		Assert.True(r.Success);
		Assert.Equal("bII7", r.RomanText);
	}

	[Fact]
	public void Borrowed_bII7_Inversions_With_Voicing()
	{
		var key = new Key(60, true); // C major
		var pcs = new[] { Pc(61), Pc(65), Pc(68), Pc(71) }; // Db F Ab B

		// Root position (Db bass): bII7
		var v7 = new FourPartVoicing(85, 77, 69, 61);
		var r7 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v7, null);
		Assert.True(r7.Success);
		Assert.Equal("bII7", r7.RomanText);

		// 1st inversion (third in bass: F): bII65
		var v65 = new FourPartVoicing(86, 81, 73, 65);
		var r65 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v65, null);
		Assert.True(r65.Success);
		Assert.Equal("bII65", r65.RomanText);

		// 2nd inversion (fifth in bass: Ab): bII43
		var v43 = new FourPartVoicing(88, 81, 76, 68);
		var r43 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v43, null);
		Assert.True(r43.Success);
		Assert.Equal("bII43", r43.RomanText);

		// 3rd inversion (seventh in bass: B(Cb)): bII42
		var v42 = new FourPartVoicing(83, 78, 71, 71); // B in bass
		var r42 = HarmonyAnalyzer.AnalyzeTriad(pcs, key, v42, null);
		Assert.True(r42.Success);
		Assert.Equal("bII42", r42.RomanText);
	}
}

