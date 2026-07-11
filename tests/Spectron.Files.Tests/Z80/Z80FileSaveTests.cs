using OldBit.Spectron.Files.Z80;
using OldBit.Spectron.Files.Z80.Types;

namespace OldBit.Spectron.Files.Tests.Z80;

public class Z80FileSaveTests
{
    [Fact]
    public void Z80Snapshot_ShouldSaveSpectrum48Version3()
    {
        var snapshot = new Z80File(new Z80Header
        {
            A = 0x01,
            F = 0x02,
            BC = 0x1721,
            HL = 0x19B6,
            PC = 0x1153,
            SP = 0xFF48,
            I = 0x3F,
            R = 0x89,
            DE = 0x5CB9,
            BCPrime = 0x4566,
            DEPrime = 0x4294,
            HLPrime = 0xABCF,
            APrime = 0x04,
            FPrime = 0x05,
            IY = 0x1110,
            IX = 0x1312,
            IFF1 = 1,
            IFF2 = 0,
            Flags1 =
            {
                IsDataCompressed = true,
            },
            Flags2 =
            {
                InterruptMode = 1,
                Issue2Emulation = true,
                DoubleInterruptFrequency = true,
                VideoSynchronization = 2,
                JoystickType = JoystickType.Sinclair2
            },
            HardwareMode = HardwareMode.Spectrum48 | HardwareMode.Interface1,
            Port7FFD = 0xA5,
            IsInterface1RomPage = true,
            PortFFFD = 0x23,
            AyRegisters = Enumerable.Range(1, 16).Select(x => (byte)x).ToArray(),
        }, new byte[0xC000]);

        snapshot.Header.Flags1.BorderColor = 7;
        snapshot.Header.Flags3!.EmulateRegisterR = true;
        snapshot.Header.Flags3.EmulateLdirInstruction = true;
        snapshot.Header.Flags3.UseAySound = true;
        snapshot.Header.Flags3.EmulateFullerAudioBox = true;
        snapshot.Header.Flags3.ModifyHardware = true;

        var stream = new MemoryStream();
        snapshot.Save(stream);
        var data = stream.ToArray();

        data[0].ShouldBe(0x01); // A
        data[1].ShouldBe(0x02); // F
        data[2].ShouldBe(0x21); // BC
        data[3].ShouldBe(0x17); // BC
        data[4].ShouldBe(0xB6); // HL
        data[5].ShouldBe(0x19); // HL
        data[6].ShouldBe(0); // PC == 0 indicates version 3
        data[7].ShouldBe(0); // PC == 0 indicates version 3
        data[8].ShouldBe(0x48); // SP
        data[9].ShouldBe(0xFF); // SP
        data[10].ShouldBe(0x3F); // I
        data[11].ShouldBe(0x89); // R
        data[12].ShouldBe(0x2F); // Flags1
        data[13].ShouldBe(0xB9); // DE
        data[14].ShouldBe(0x5C); // DE
        data[15].ShouldBe(0x66); // BC'
        data[16].ShouldBe(0x45); // BC'
        data[17].ShouldBe(0x94); // DE'
        data[18].ShouldBe(0x42); // DE'
        data[19].ShouldBe(0xCF); // HL'
        data[20].ShouldBe(0xAB); // HL'
        data[21].ShouldBe(0x04); // A'
        data[22].ShouldBe(0x05); // F'
        data[23].ShouldBe(0x10); // IY
        data[24].ShouldBe(0x11); // IY
        data[25].ShouldBe(0x12); // IX
        data[26].ShouldBe(0x13); // IX
        data[27].ShouldBe(1); // IFF1
        data[28].ShouldBe(0); // IFF2
        data[29].ShouldBe(0xED); // Flags2
        data[30].ShouldBe(0x36); // Extra header length
        data[31].ShouldBe(0); // Extra header length
        data[32].ShouldBe(0x53); // PC
        data[33].ShouldBe(0x11); // PC
        data[34].ShouldBe(0x01); // Hardware mode
        data[35].ShouldBe(0xA5); // Port 7FFD
        data[36].ShouldBe(0xFF); // Interface 1 ROM paged
        data[37].ShouldBe(0xC7); // Flags3
        data[38].ShouldBe(0x23); // Port FFFD
        data[39..55].ShouldBeEquivalentTo(Enumerable.Range(1, 16).Select(i => (byte)i).ToArray()); // AY registers

        data.Length.ShouldBe(86 + 789);
    }

    [Fact]
    public void Z80Snapshot_ShouldSaveSpectrum128Version3()
    {
        var memory128 = Enumerable.Range(0, 8).Select(value =>
            Enumerable.Repeat((byte)value, 0x4000).ToArray());

        var snapshot = new Z80File(new Z80Header
        {
            A = 0x01,
            F = 0x02,
            BC = 0x1721,
            HL = 0x19B6,
            PC = 0x1153,
            SP = 0xFF48,
            I = 0x3F,
            R = 0x89,
            DE = 0x5CB9,
            BCPrime = 0x4566,
            DEPrime = 0x4294,
            HLPrime = 0xABCF,
            APrime = 0x04,
            FPrime = 0x05,
            IY = 0x1110,
            IX = 0x1312,
            IFF1 = 1,
            IFF2 = 0,
            Flags1 =
            {
                IsDataCompressed = true,
            },
            Flags2 =
            {
                InterruptMode = 1,
                Issue2Emulation = true,
                DoubleInterruptFrequency = true,
                VideoSynchronization = 2,
                JoystickType = JoystickType.Sinclair2
            },
            HardwareMode = HardwareMode.Spectrum128,
            Port7FFD = 0xA5,
            IsInterface1RomPage = true,
            PortFFFD = 0x23,
            AyRegisters = Enumerable.Range(1, 16).Select(x => (byte)x).ToArray(),
        }, memory128);

        snapshot.Header.Flags1.BorderColor = 7;
        snapshot.Header.Flags3!.EmulateRegisterR = true;
        snapshot.Header.Flags3.EmulateLdirInstruction = true;
        snapshot.Header.Flags3.UseAySound = true;
        snapshot.Header.Flags3.EmulateFullerAudioBox = true;
        snapshot.Header.Flags3.ModifyHardware = true;

        var stream = new MemoryStream();
        snapshot.Save(stream);
        var data = stream.ToArray();

        data[0].ShouldBe(0x01); // A
        data[1].ShouldBe(0x02); // F
        data[2].ShouldBe(0x21); // BC
        data[3].ShouldBe(0x17); // BC
        data[4].ShouldBe(0xB6); // HL
        data[5].ShouldBe(0x19); // HL
        data[6].ShouldBe(0); // PC == 0 indicates version 3
        data[7].ShouldBe(0); // PC == 0 indicates version 3
        data[8].ShouldBe(0x48); // SP
        data[9].ShouldBe(0xFF); // SP
        data[10].ShouldBe(0x3F); // I
        data[11].ShouldBe(0x89); // R
        data[12].ShouldBe(0x2F); // Flags1
        data[13].ShouldBe(0xB9); // DE
        data[14].ShouldBe(0x5C); // DE
        data[15].ShouldBe(0x66); // BC'
        data[16].ShouldBe(0x45); // BC'
        data[17].ShouldBe(0x94); // DE'
        data[18].ShouldBe(0x42); // DE'
        data[19].ShouldBe(0xCF); // HL'
        data[20].ShouldBe(0xAB); // HL'
        data[21].ShouldBe(0x04); // A'
        data[22].ShouldBe(0x05); // F'
        data[23].ShouldBe(0x10); // IY
        data[24].ShouldBe(0x11); // IY
        data[25].ShouldBe(0x12); // IX
        data[26].ShouldBe(0x13); // IX
        data[27].ShouldBe(1); // IFF1
        data[28].ShouldBe(0); // IFF2
        data[29].ShouldBe(0xED); // Flags2
        data[30].ShouldBe(0x36); // Extra header length
        data[31].ShouldBe(0); // Extra header length
        data[32].ShouldBe(0x53); // PC
        data[33].ShouldBe(0x11); // PC
        data[34].ShouldBe(4); // Hardware mode
        data[35].ShouldBe(0xA5); // Port 7FFD
        data[36].ShouldBe(0xFF); // Interface 1 ROM paged
        data[37].ShouldBe(0xC7); // Flags3
        data[38].ShouldBe(0x23); // Port FFFD
        data[39..55].ShouldBeEquivalentTo(Enumerable.Range(1, 16).Select(i => (byte)i).ToArray()); // AY registers

        data.Length.ShouldBe(86 + 263 * 8);
    }
}