using System.Security.Cryptography;
using OldBit.Spectron.Emulation.Rom;

namespace OldBit.Spectron.Emulator.Tests.Rom;

public class RomReaderTests
{
    [Theory]
    [InlineData(RomType.Original48, "5ea7c2b824672e914525d1d5c419d71b84a426a2")]
    [InlineData(RomType.Original128Bank0, "4f4b11ec22326280bdb96e3baf9db4b4cb1d02c5")]
    [InlineData(RomType.Original128Bank1, "80080644289ed93d71a1103992a154cc9802b2fa")]
    [InlineData(RomType.Retroleum, "077df85fb84e9cb466ed9e6fd93f9046ce0c6815")]
    [InlineData(RomType.BrendanAlford, "f699b73abfb1ab53c063ac02ac6283705864c734")]
    [InlineData(RomType.BusySoft, "2ee2dbe6ab96b60d7af1d6cb763b299374c21776")]
    [InlineData(RomType.GoshWonderful, "a701c3d4b698f7d2be537dc6f79e06e4dbc95929")]
    [InlineData(RomType.Harston, "6b373a7a56d9f9353d0f2652b89fe4a5b202bced")]
    [InlineData(RomType.Interface1V1, "4ffd9ed9c00cdc6f92ce69fdd8b618ef1203f48e")]
    [InlineData(RomType.Interface1V2, "5cfb6bca4177c45fefd571734576b55e3a127c08")]
    [InlineData(RomType.DivMmc, "b6f3f5f381d67eed24ab214698cb2b3b7d6091da")]
    [InlineData(RomType.TrDos, "c9d69cf3a0219f6e37e7eb5046961fa8fa8eb2c6")]
    public void Rom_ShouldHaveCorrectChecksum(RomType romType, string expectedSha1)
    {
        var rom = RomReader.ReadRom(romType);

        var hash = SHA1.HashData(rom);
        var sha1 = Convert.ToHexString(hash).ToLowerInvariant();

        sha1.ShouldBe(expectedSha1);
    }
}