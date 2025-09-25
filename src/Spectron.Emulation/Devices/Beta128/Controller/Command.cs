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
}