using System.Reflection;
using OldBit.Spectron.Emulation;
using OldBit.Spectron.Emulation.Devices.Beta128;
using OldBit.Spectron.Emulation.Devices.Beta128.Controller;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;
using OldBit.Spectron.Emulation.Devices.Memory;
using OldBit.Spectron.Emulation.Rom;
using OldBit.Z80Cpu;

namespace OldBit.Spectron.Emulator.Tests.Devices.Beta128.Controller;

public class DiskControllerTests
{
    private readonly Beta128Device _beta128;
    private readonly TestDiskDriveProvider _diskDriveProvider = new();
    private readonly DiskController _controller;

    public DiskControllerTests()
    {
        var memory = new Memory48K(RomReader.ReadRom(RomType.Original48));
        var z80 = new Z80(memory);

        _beta128 = new Beta128Device(z80, Hardware.Spectrum48K.ClockMhz, memory, ComputerType.Spectrum48K, _diskDriveProvider);
        _controller = _beta128.Controller;

        _beta128.Enable();

        // Insert test disk
        var binFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var testFilePath = Path.Combine(binFolder, "TestData", "Test Disk.trd");
        _diskDriveProvider.Drives[DriveId.DriveA].InsertDisk(testFilePath);

        // Page in ROM
        z80.Registers.PC = 0x3D00;
        z80.Step();
    }

    [Fact]
    public void DiskController_WhenNoDisk_ShouldHaveStatusNoDisk()
    {
        _beta128.WritePort(0x1F, 0xF4); // Write Track with 15 ms delay

        _controller.ProcessState(0);
        // Wait before the command (15 ms equals 52504)
        _controller.ProcessState(52504);
        _controller.ProcessState(52505);

        _beta128.WritePort(0x7F, 0xA1);
    }
}