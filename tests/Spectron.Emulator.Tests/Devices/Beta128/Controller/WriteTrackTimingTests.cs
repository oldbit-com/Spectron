using OldBit.Spectron.Emulation.Devices.Beta128.Controller;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;
using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;
using OldBit.Spectron.Emulator.Tests.Fixtures;
using Xunit.Abstractions;

namespace OldBit.Spectron.Emulator.Tests.Devices.Beta128.Controller;

public class WriteTrackTimingTests
{
    private readonly ITestOutputHelper _output;

    public WriteTrackTimingTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void FormatTrack_ShouldCompleteAndConsumeExpectedData()
    {
        var provider = new TestDiskDriveProvider();
        var controller = new DiskController(3.5f, provider);
        var drive = provider.Drives[DriveId.DriveA];
        drive.NewDisk();

        // Select drive A, side 0 and enable the head load.
        controller.ControlRegister = 0x08;

        // Issue RESTORE to spin the motor before the write track command.
        var now = 0L;
        controller.ProcessCommand(now, 0x00);
        AdvanceUntilIdle(controller, ref now);

        var formatData = BuildFormatTrackData();
        var byteIndex = 0;

        controller.ProcessCommand(now, 0xF4);

        var safetyCounter = 0;
        while (!controller.IsIdle)
        {
            controller.ProcessState(now);

            var status = controller.StatusRegister;
            if ((status & (byte)ControllerStatus.DataRequest) != 0)
            {
                var value = byteIndex < formatData.Length
                    ? formatData[byteIndex++]
                    : (byte)0x4E;

                controller.DataRegister = value;
            }

            now += 1;
            if (++safetyCounter > 1_000_000)
            {
                break;
            }
        }

        _output.WriteLine($"Ticks elapsed: {now}");
        _output.WriteLine($"Bytes transferred: {byteIndex}");

        Assert.True(controller.IsIdle);
        Assert.Equal(formatData.Length, byteIndex);
    }

    private static void AdvanceUntilIdle(DiskController controller, ref long now)
    {
        var safetyCounter = 0;
        while (!controller.IsIdle)
        {
            controller.ProcessState(now);
            now += 1;
            if (++safetyCounter > 200_000)
            {
                break;
            }
        }
    }

    private static byte[] BuildFormatTrackData()
    {
        var buffer = new byte[Track.MaxLength];
        var position = 0;

        void Write(byte value, int count = 1)
        {
            for (var i = 0; i < count; i++)
            {
                buffer[position++] = value;
            }
        }

        Write(0x4E, 80);
        Write(0x00, 12);
        Write(Track.StartIndexMarker, 3);
        Write(0xFC);
        Write(0x4E, 50);

        var size = (byte)Math.Log2(FloppyDisk.BytesPerSector / 128.0);

        for (byte sector = 0; sector < FloppyDisk.TotalSectors; sector++)
        {
            Write(0x00, 12);
            Write(Track.StartIdDataMarker, 3);
            Write(AddressMark.Id);
            Write(0);              // Cylinder
            Write(0);              // Side
            Write((byte)(sector + 1));
            Write(size);
            Write(Track.WriteCrcMarker);
            Write(0x4E, 22);
            Write(0x00, 12);
            Write(Track.StartIdDataMarker, 3);
            Write(AddressMark.Normal);
            Write(0x00, FloppyDisk.BytesPerSector);
            Write(Track.WriteCrcMarker);
            Write(0x4E, 54);
        }

        while (position < buffer.Length)
        {
            buffer[position++] = 0x4E;
        }

        return buffer;
    }
}
