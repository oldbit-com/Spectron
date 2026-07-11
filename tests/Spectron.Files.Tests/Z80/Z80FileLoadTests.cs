using System.Reflection;
using OldBit.Spectron.Files.Z80;
using OldBit.Spectron.Files.Z80.Types;

namespace OldBit.Spectron.Files.Tests.Z80;

public class Z80FileLoadTests
{
    [Fact]
    public void ShouldLoad48KSnapshotVersion3()
    {
        using var file = LoadTestFile("Test48.z80");
        var snapshot = Z80File.Load(file);

        snapshot.Header.Version.ShouldBe(3);
        snapshot.Header.ExtraHeaderLength.ShouldBe(55);
        snapshot.Header.HardwareMode.ShouldBe(HardwareMode.Spectrum48);

        snapshot.Header.A.ShouldBe(0x01);
        snapshot.Header.F.ShouldBe(0x5C);

        snapshot.Header.APrime.ShouldBe(0x02);
        snapshot.Header.FPrime.ShouldBe(0x44);

        snapshot.Header.BC.ShouldBe(0x1721);
        snapshot.Header.DE.ShouldBe(0x5CB9);
        snapshot.Header.HL.ShouldBe(0x5CB6);
        snapshot.Header.IX.ShouldBe(0x03D4);
        snapshot.Header.IY.ShouldBe(0x5C3A);

        snapshot.Header.BCPrime.ShouldBe(0x1720);
        snapshot.Header.DEPrime.ShouldBe(0x0097);
        snapshot.Header.HLPrime.ShouldBe(0x0017);

        snapshot.Header[6].ShouldBe(0x00);       // Version 1 PC
        snapshot.Header[7].ShouldBe(0x00);       // Version 1 PC
        snapshot.Header.SP.ShouldBe(0xFF48);
        snapshot.Header.I.ShouldBe(0x3F);
        snapshot.Header.R.ShouldBe(0x21);
        snapshot.Header.IFF1.ShouldBe(0x00);
        snapshot.Header.IFF2.ShouldBe(0x00);

        // Flags1
        snapshot.Header[12].ShouldBe(0x04);
        snapshot.Header.Flags1.Bit7R.ShouldBe(0);
        snapshot.Header.Flags1.BorderColor.ShouldBe(2);
        snapshot.Header.Flags1.IsSamRam.ShouldBeFalse();
        snapshot.Header.Flags1.IsDataCompressed.ShouldBeFalse();

        // Flags2
        snapshot.Header[29].ShouldBe(0x41);
        snapshot.Header.Flags2.InterruptMode.ShouldBe(1);
        snapshot.Header.Flags2.Issue2Emulation.ShouldBeFalse();
        snapshot.Header.Flags2.DoubleInterruptFrequency.ShouldBeFalse();
        snapshot.Header.Flags2.VideoSynchronization.ShouldBe(0);
        snapshot.Header.Flags2.JoystickType.ShouldBe(JoystickType.Kempston);

        // Flags3
        snapshot.Header[37].ShouldBe(0);

        snapshot.Header.HighTStateCounter.ShouldBe(0);
        snapshot.Header.LowTStateCounter.ShouldBe(0);

        snapshot.MemoryBlocks.Count.ShouldBe(3);
    }

    [Fact]
    public void ShouldLoad128KSnapshotVersion3()
    {
        using var file = LoadTestFile("Test128.z80");
        var snapshot = Z80File.Load(file);

        snapshot.Header.Version.ShouldBe(3);
        snapshot.Header.ExtraHeaderLength.ShouldBe(55);
        snapshot.Header.HardwareMode.ShouldBe(HardwareMode.Spectrum128);

        snapshot.Header.A.ShouldBe(0xDD);
        snapshot.Header.F.ShouldBe(0x74);

        snapshot.Header.APrime.ShouldBe(0x00);
        snapshot.Header.FPrime.ShouldBe(0x44);

        snapshot.Header.BC.ShouldBe(0x1300);
        snapshot.Header.DE.ShouldBe(0x1213);
        snapshot.Header.HL.ShouldBe(0x5C3B);
        snapshot.Header.IX.ShouldBe(0xFD6C);
        snapshot.Header.IY.ShouldBe(0x5C3A);

        snapshot.Header.BCPrime.ShouldBe(0x1801);
        snapshot.Header.DEPrime.ShouldBe(0x0068);
        snapshot.Header.HLPrime.ShouldBe(0x0038);

        snapshot.Header[6].ShouldBe(0x00);       // Version 1 PC
        snapshot.Header[7].ShouldBe(0x00);       // Version 1 PC
        snapshot.Header.SP.ShouldBe(0x5BF9);
        snapshot.Header.I.ShouldBe(0x00);
        snapshot.Header.R.ShouldBe(0x37);
        snapshot.Header.IFF1.ShouldBe(0x00);
        snapshot.Header.IFF2.ShouldBe(0x00);

        // Flags1
        snapshot.Header[12].ShouldBe(0x0C);
        snapshot.Header.Flags1.Bit7R.ShouldBe(0);
        snapshot.Header.Flags1.BorderColor.ShouldBe(6);
        snapshot.Header.Flags1.IsSamRam.ShouldBeFalse();
        snapshot.Header.Flags1.IsDataCompressed.ShouldBeFalse();

        // Flags2
        snapshot.Header[29].ShouldBe(0x01);
        snapshot.Header.Flags2.InterruptMode.ShouldBe(1);
        snapshot.Header.Flags2.Issue2Emulation.ShouldBeFalse();
        snapshot.Header.Flags2.DoubleInterruptFrequency.ShouldBeFalse();
        snapshot.Header.Flags2.VideoSynchronization.ShouldBe(0);
        snapshot.Header.Flags2.JoystickType.ShouldBe(JoystickType.Cursor);

        // Flags3
        snapshot.Header[37].ShouldBe(0x04);
        snapshot.Header.Flags3?.UseAySound.ShouldBeTrue();

        snapshot.Header.HighTStateCounter.ShouldBe(0);
        snapshot.Header.LowTStateCounter.ShouldBe(0);

        snapshot.MemoryBlocks.Count.ShouldBe(8);
    }

    private static FileStream LoadTestFile(string fileName)
    {
        var location = typeof(Z80FileLoadTests).GetTypeInfo().Assembly.Location;
        var dir = Path.GetDirectoryName(location) ?? throw new InvalidOperationException();
        var path =  Path.Combine(dir, "TestFiles", fileName);

        return File.OpenRead(path);
    }
}