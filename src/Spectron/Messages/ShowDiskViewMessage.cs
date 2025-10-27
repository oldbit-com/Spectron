using OldBit.Spectron.Emulation.Devices.Beta128;
using OldBit.Spectron.Emulation.Devices.Beta128.Drive;

namespace OldBit.Spectron.Messages;

public record ShowDiskViewMessage(DiskDriveManager DiskDriveManager, DriveId DriveId);