using System.Diagnostics.CodeAnalysis;

namespace OldBit.Spectron.Emulation.Devices.Storage.SD;

internal sealed class Command
{
    private byte _crc;

    internal int Id { get; }

    internal int Length { get; private set; }

    internal byte[] Arguments { get; } = new byte[4];

    private Command(byte id)
    {
        Id = (id & 0x3F);
        Length = 1;
    }

    internal static bool TryCreateCommand(byte value, [NotNullWhen(true)]out Command? command)
    {
        if ((value & 0xC0) != 0x40)
        {
            command = null;

            return false;
        }

        command = new Command(value);

        return true;
    }

    internal void ProcessNextByte(byte value)
    {
        if (Length == 5)
        {
            _crc = value;
        }
        else
        {
            Arguments[Length - 1] = value;
        }

        Length += 1;
    }
}