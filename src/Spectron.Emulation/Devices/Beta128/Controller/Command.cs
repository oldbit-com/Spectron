namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal static class Command
{
    internal static CommandType GetType(byte value)
    {
        // Type I (Restore, Seek, Step)
        if ((value & 0x80) == 0)
        {
            return CommandType.Type1;
        }

        // Type II (Read / Write Sector)
        if ((value & 0x40) == 0)
        {
            return CommandType.Type2;
        }

        // Type IV (Force Interrupt)
        if ((value & 0xF0) == 0xD0)
        {
            return CommandType.Type4;
        }

        // Type III (Read Address, Read Track, Write Track)]
        return CommandType.Type3;
    }

    // Bit E - 15 ms delay (0: no 15ms delay, 1: 15 ms delay)
    internal static bool ShouldDelay(byte command) => (command & 0x04) != 0;

    // 1  0  0  m  S  E  C  0
    internal static bool IsReadSector(byte command) => (command & 0xE0) == 0x80;

    // 1  0  1  m  S  E  C a0
    internal static bool IsWriteSector(byte command) => (command & 0xE0) == 0xA0;

    // 1  1  0  0  0  E  0  0
    internal static bool IsReadAddress(byte command) => (command & 0xFB) == 0xC0;

    // 1  1  1  0  0  E  0  0
    internal static bool IsReadTrack(byte command) => (command & 0xFB) == 0xE0;

    // 1  1  1  1  0  E  0  0
    internal static bool IsWriteTrack(byte command) => (command & 0xFB) == 0xF0;

    // C flag value for ReadSector or WriteSector
    internal static bool IsSideCompareFlagSet(byte command) => (command & 0x02) == 0x02;

    // S flag value for ReadSector or WriteSector
    internal static int GetSideSelectFlag(byte command) => (command & 0x08) >> 3;
}