using System.Text;
using OldBit.Spectron.Emulation.Devices.Beta128;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;
using OldBit.Spectron.Emulation.Devices.Beta128.Floppy;
using OldBit.Spectron.ViewModels;

namespace OldBit.Spectron.Tests.ViewModels;

public class DiskViewModelTests
{
    private readonly DiskDriveManager _manager;
    private readonly DiskDrive _drive;
    private FloppyDisk Floppy => _drive.DiskFile!.Floppy;

    public DiskViewModelTests()
    {
        _manager = new DiskDriveManager();
        _drive = _manager.Drives[DriveId.DriveA];
    }

    [Fact]
    public void WhenNoDiskInserted_PropertiesAreEmpty()
    {
        var vm = new DiskViewModel(_manager, DriveId.DriveA);

        vm.Name.ShouldBeEmpty();
        vm.NumberOfTracks.ShouldBeEmpty();
        vm.NumberOfSides.ShouldBeEmpty();
        vm.NumberOfFiles.ShouldBeEmpty();
        vm.NumberOfDeletedFiles.ShouldBeEmpty();
        vm.Files.ShouldBeEmpty();
    }

    [Fact]
    public void WhenDiskInserted_PopulatesLabel()
    {
        _drive.NewDisk();

        var vm = new DiskViewModel(_manager, DriveId.DriveA);

        vm.Name.ShouldBe("New Disk");
    }

    [Fact]
    public void WhenDiskInserted_PopulatesGeometry()
    {
        _drive.NewDisk();

        var vm = new DiskViewModel(_manager, DriveId.DriveA);

        vm.NumberOfTracks.ShouldBe("80");
        vm.NumberOfSides.ShouldBe("2");
    }

    [Fact]
    public void WithEmptyDirectory_ReportsZeroFileCounts()
    {
        _drive.NewDisk();

        var vm = new DiskViewModel(_manager, DriveId.DriveA);

        vm.NumberOfFiles.ShouldBe("0");
        vm.NumberOfDeletedFiles.ShouldBe("0");
        vm.Files.ShouldBeEmpty();
    }

    [Fact]
    public void WithOneActiveFile_AddsFileToCollection()
    {
        _drive.NewDisk();

        WriteDirectoryEntry(Floppy, directorySector: 1, entryIndex: 0, fileType: (byte)'B', fileName: "HELLO");

        var vm = new DiskViewModel(_manager, DriveId.DriveA);

        vm.NumberOfFiles.ShouldBe("1");
        vm.Files.Count.ShouldBe(1);
        vm.Files[0].FileName.ShouldBe("HELLO   ");
        vm.Files[0].FileType.ShouldBe("B");
    }

    [Fact]
    public void WithDeletedFile_CountsItSeparatelyFromActiveFiles()
    {
        _drive.NewDisk();

        WriteDeletedEntry(Floppy, directorySector: 1, entryIndex: 0);

        var vm = new DiskViewModel(_manager, DriveId.DriveA);

        vm.NumberOfFiles.ShouldBe("0");
        vm.NumberOfDeletedFiles.ShouldBe("1");
        vm.Files.ShouldBeEmpty();
    }

    [Fact]
    public void WithMixedDirectory_ReportsCorrectCounts()
    {
        _drive.NewDisk();

        WriteDirectoryEntry(Floppy, directorySector: 1, entryIndex: 0, fileType: (byte)'C', fileName: "FILE1");
        WriteDeletedEntry(Floppy, directorySector: 1, entryIndex: 1);
        WriteDirectoryEntry(Floppy, directorySector: 1, entryIndex: 2, fileType: (byte)'B', fileName: "FILE2");
        WriteDeletedEntry(Floppy, directorySector: 1, entryIndex: 3);

        var vm = new DiskViewModel(_manager, DriveId.DriveA);

        vm.NumberOfFiles.ShouldBe("2");
        vm.NumberOfDeletedFiles.ShouldBe("2");
        vm.Files.Count.ShouldBe(2);
    }

    [Fact]
    public void WithEndMarkerInDirectory_StopsParsingFurtherEntries()
    {
        _drive.NewDisk();

        WriteDirectoryEntry(Floppy, directorySector: 1, entryIndex: 0, fileType: (byte)'B', fileName: "BEFORE");
        WriteEndMarker(Floppy, directorySector: 1, entryIndex: 1);
        WriteDirectoryEntry(Floppy, directorySector: 1, entryIndex: 2, fileType: (byte)'B', fileName: "AFTER");

        var vm = new DiskViewModel(_manager, DriveId.DriveA);

        vm.NumberOfFiles.ShouldBe("1");
        vm.Files.Count.ShouldBe(1);
        vm.Files[0].FileName.ShouldBe("BEFORE  ");
    }

    [Fact]
    public void WithFilesAcrossMultipleSectors_ParsesAllFiles()
    {
        _drive.NewDisk();

        // Fill all 16 entries in sector 1
        for (var i = 0; i < 16; i++)
        {
            WriteDirectoryEntry(Floppy, directorySector: 1, entryIndex: i,
                fileType: (byte)'B', fileName: $"FILE{i:D4}");
        }

        // Write one more file in sector 2
        WriteDirectoryEntry(Floppy, directorySector: 2, entryIndex: 0, fileType: (byte)'C', fileName: "NEXTSECT");

        var vm = new DiskViewModel(_manager, DriveId.DriveA);

        vm.NumberOfFiles.ShouldBe("17");
        vm.Files.Count.ShouldBe(17);
        vm.Files[16].FileName.ShouldBe("NEXTSECT");
    }

    [Theory]
    [InlineData((byte)'B', "B")]
    [InlineData((byte)'C', "C")]
    [InlineData((byte)0x21, "!")]  // just above 0x20 — printable
    [InlineData((byte)0x7E, "~")] // just below 0x7F — printable
    [InlineData((byte)' ', "?")] // 0x20 is not > 0x20 — not printable
    [InlineData((byte)0x7F, "?")] // 0x7F is not < 0x7F — not printable
    [InlineData((byte)0x01, "?")] // control character — not printable
    public void FileType_ReturnsCorrectRepresentation(byte fileTypeByte, string expectedType)
    {
        _drive.NewDisk();

        WriteDirectoryEntry(Floppy, directorySector: 1, entryIndex: 0, fileType: fileTypeByte, fileName: "FILE    ");

        var vm = new DiskViewModel(_manager, DriveId.DriveA);

        vm.Files[0].FileType.ShouldBe(expectedType);
    }

    [Theory]
    [InlineData(5, 0b00110100, "T26H0S5")]  // trackNo = 0b00110100>>1 = 26, sideNo = 0
    [InlineData(8, 0b00110101, "T26H1S8")]  // trackNo = 26, sideNo = 1
    [InlineData(1, 0b00000010, "T1H0S1")]   // trackNo = 1, sideNo = 0
    [InlineData(16, 0b00000001, "T0H1S16")] // trackNo = 0, sideNo = 1
    public void FileAddress_IsEncodedAsTrackHeadSector(byte startSector, byte trackSidePacked, string expectedAddress)
    {
        _drive.NewDisk();

        WriteDirectoryEntry(Floppy, directorySector: 1, entryIndex: 0, fileType: (byte)'B', fileName: "FILE", startSector: startSector, trackSidePacked: trackSidePacked);

        var vm = new DiskViewModel(_manager, DriveId.DriveA);

        vm.Files[0].Address.ShouldBe(expectedAddress);
    }

    [Fact]
    public void FileLength_ReflectsLengthSectorsByte()
    {
        _drive.NewDisk();

        WriteDirectoryEntry(Floppy, directorySector: 1, entryIndex: 0, fileType: (byte)'B', fileName: "FILE", lengthSectors: 42);

        var vm = new DiskViewModel(_manager, DriveId.DriveA);

        vm.Files[0].Length.ShouldBe("42");
    }

    private static void WriteDirectoryEntry(
        FloppyDisk floppy,
        int directorySector,
        int entryIndex,
        byte fileType,
        string fileName,
        byte lengthSectors = 0,
        byte startSector = 0,
        byte trackSidePacked = 0)
    {
        var sector = floppy.GetSector(0, 0, directorySector);
        var offset = entryIndex * 16;

        var nameBytes = Encoding.ASCII.GetBytes(fileName.PadRight(8)[..8]);

        for (var i = 0; i < 8; i++)
        {
            sector[offset + i] = nameBytes[i];
        }

        sector[offset + 0x08] = fileType;
        sector[offset + 0x0D] = lengthSectors;
        sector[offset + 0x0E] = startSector;
        sector[offset + 0x0F] = trackSidePacked;
    }

    private static void WriteDeletedEntry(FloppyDisk floppy, int directorySector, int entryIndex)
    {
        var sector = floppy.GetSector(0, 0, directorySector);
        var offset = entryIndex * 16;

        sector[offset] = 0x01;
        for (var i = 1; i < 16; i++)
        {
            sector[offset + i] = 0x20;
        }
    }

    private static void WriteEndMarker(FloppyDisk floppy, int directorySector, int entryIndex)
    {
        var sector = floppy.GetSector(0, 0, directorySector);
        sector[entryIndex * 16] = 0x00;
    }
}
