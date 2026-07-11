using OldBit.Spectron.Files.Serialization;
using OldBit.Spectron.Files.Sna;

namespace OldBit.Spectron.Files.Tests.Sna;

public class SnaFileTests
{
    [Fact]
    public void SnaFile_48K_ShouldDeserializeFromStream()
    {
        var data = GetSna48Data();

        using var stream = new MemoryStream(data);
        var snaFile = SnaFile.Load(stream);

        snaFile.Header.I.ShouldBe(0x01);
        snaFile.Header.HLPrime.ShouldBe(0x0302);
        snaFile.Header.DEPrime.ShouldBe(0x0504);
        snaFile.Header.BCPrime.ShouldBe(0x0706);
        snaFile.Header.AFPrime.ShouldBe(0x0908);
        snaFile.Header.HL.ShouldBe(0x0B0A);
        snaFile.Header.DE.ShouldBe(0x0D0C);
        snaFile.Header.BC.ShouldBe(0x0F0E);
        snaFile.Header.IY.ShouldBe(0x1110);
        snaFile.Header.IX.ShouldBe(0x1312);
        snaFile.Header.Interrupt.ShouldBe(0x14);
        snaFile.Header.R.ShouldBe(0x15);
        snaFile.Header.AF.ShouldBe(0x1716);
        snaFile.Header.SP.ShouldBe(0x1918);
        snaFile.Header.InterruptMode.ShouldBe(0x1A);
        snaFile.Header.BorderColor.ShouldBe(0x1B);
        snaFile.Ram48.ShouldBeEquivalentTo(data[27..]);
        snaFile.Header128.ShouldBeNull();
        snaFile.RamBanks.ShouldBeNull();
    }

    [Fact]
    public void SnaFile_128K_ShouldDeserializeFromStream()
    {
        var data = GetSna128();
        using var stream = new MemoryStream(data);
        var snaFile = SnaFile.Load(stream);

        snaFile.Header.I.ShouldBe(0x01);
        snaFile.Header.HLPrime.ShouldBe(0x0302);
        snaFile.Header.DEPrime.ShouldBe(0x0504);
        snaFile.Header.BCPrime.ShouldBe(0x0706);
        snaFile.Header.AFPrime.ShouldBe(0x0908);
        snaFile.Header.HL.ShouldBe(0x0B0A);
        snaFile.Header.DE.ShouldBe(0x0D0C);
        snaFile.Header.BC.ShouldBe(0x0F0E);
        snaFile.Header.IY.ShouldBe(0x1110);
        snaFile.Header.IX.ShouldBe(0x1312);
        snaFile.Header.Interrupt.ShouldBe(0x14);
        snaFile.Header.R.ShouldBe(0x15);
        snaFile.Header.AF.ShouldBe(0x1716);
        snaFile.Header.SP.ShouldBe(0x1918);
        snaFile.Header.InterruptMode.ShouldBe(0x1A);
        snaFile.Header.BorderColor.ShouldBe(0x1B);
        snaFile.Ram48.ShouldBeEquivalentTo(data[27..49179]);
        snaFile.Header128!.PC.ShouldBe(0x1D1C);
        snaFile.Header128!.PageMode.ShouldBe(0x05);
        snaFile.Header128!.TrDosRom.ShouldBe(0x00);
        snaFile.RamBanks!.Count.ShouldBe(6);
        snaFile.RamBanks![0].ShouldBeEquivalentTo(data.Skip(49183).Take(16384).ToArray());
        snaFile.RamBanks![1].ShouldBeEquivalentTo(data.Skip(65567).Take(16384).ToArray());
        snaFile.RamBanks![2].ShouldBeEquivalentTo(data.Skip(81951).Take(16384).ToArray());
        snaFile.RamBanks![3].ShouldBeEquivalentTo(data.Skip(98335).Take(16384).ToArray());
        snaFile.RamBanks![4].ShouldBeEquivalentTo(data.Skip(114719).Take(16384).ToArray());
        snaFile.RamBanks![5].ShouldBeEquivalentTo(data.Skip(131103).Take(16384).ToArray());
    }

    [Fact]
    public void SnaFile_48K_ShouldSerializeToBytes()
    {
        var snaFile = new SnaFile()
        {
            Header = new SnaHeader
            {
                I = 0x01,
                HLPrime = 0x0302,
                DEPrime = 0x0504,
                BCPrime = 0x0706,
                AFPrime = 0x0908,
                HL = 0x0B0A,
                DE = 0x0D0C,
                BC = 0x0F0E,
                IY = 0x1110,
                IX = 0x1312,
                Interrupt = 0x14,
                R = 0x15,
                AF = 0x1716,
                SP = 0x1918,
                InterruptMode = 0x1A,
                BorderColor = 0x1B
            },
            Ram48 = Enumerable.Repeat((byte)0xFF, 49152).ToArray()
        };

        var result = FileDataSerializer.Serialize(snaFile);

        result.ShouldBeEquivalentTo(GetSna48Data());
    }

    [Fact]
    public void SnaFile_128K_ShouldSerializeToBytes()
    {
        var snaFile = new SnaFile
        {
            Header = new SnaHeader
            {
                I = 0x01,
                HLPrime = 0x0302,
                DEPrime = 0x0504,
                BCPrime = 0x0706,
                AFPrime = 0x0908,
                HL = 0x0B0A,
                DE = 0x0D0C,
                BC = 0x0F0E,
                IY = 0x1110,
                IX = 0x1312,
                Interrupt = 0x14,
                R = 0x15,
                AF = 0x1716,
                SP = 0x1918,
                InterruptMode = 0x1A,
                BorderColor = 0x1B
            },
            Ram48 = Enumerable.Repeat((byte)0x40, 49152).ToArray(),
            Header128 = new SnaHeader128
            {
                PC = 0x1D1C,
                PageMode = 0x05,
                TrDosRom = 0x00
            },
            RamBanks = new List<byte[]>
            {
                Enumerable.Repeat((byte)0x50, 16384).ToArray(),
                Enumerable.Repeat((byte)0x60, 16384).ToArray(),
                Enumerable.Repeat((byte)0x70, 16384).ToArray(),
                Enumerable.Repeat((byte)0x70, 16384).ToArray(),
                Enumerable.Repeat((byte)0x80, 16384).ToArray(),
                Enumerable.Repeat((byte)0x90, 16384).ToArray()
            }
        };

        var result = FileDataSerializer.Serialize(snaFile);

        result.ShouldBeEquivalentTo(GetSna128());
    }

    private static byte[] GetSna48Data()
    {
        var data = new byte[]
        {
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A,
            0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14,
            0x15, 0x16, 0x17, 0x18, 0x19, 0x1A, 0x1B
        };
        Array.Resize(ref data, 49179);
        Array.Fill<byte>(data, 0xFF, 27, 0xC000);

        return data;
    }

    private static byte[] GetSna128()
    {
        var data = GetSna48Data();
        Array.Resize(ref data, 147487);
        Array.Fill<byte>(data, 0x40, 27, 0xC000);        // Blocks 2, 3, 5
        data[49179] = 0x1C; // PC
        data[49180] = 0x1D; // PC
        data[49181] = 0x05; // Page mode
        data[49182] = 0x00; // TR-DOS ROM paged
        Array.Fill<byte>(data, 0x50, 49183, 0x4000);     // Bank 0
        Array.Fill<byte>(data, 0x60, 65567, 0x4000);     // Bank 1
        Array.Fill<byte>(data, 0x70, 81951, 0x4000);     // Bank 4
        Array.Fill<byte>(data, 0x70, 98335, 0x4000);     // Bank 5 saved twice due to page setting
        Array.Fill<byte>(data, 0x80, 114719, 0x4000);    // Bank 6
        Array.Fill<byte>(data, 0x90, 131103, 0x4000);    // Bank 7

        return data;
    }
}