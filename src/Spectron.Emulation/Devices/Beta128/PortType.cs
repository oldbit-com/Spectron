using System.Diagnostics.CodeAnalysis;

namespace OldBit.Spectron.Emulation.Devices.Beta128;

internal static class PortType
{
    internal const byte None = 0;
    internal const byte Command = 0x1F;
    internal const byte Status = 0x1F;
    internal const byte Track = 0x3F;
    internal const byte Sector = 0x5F;
    internal const byte Data = 0x7F;
    internal const byte Control = 0xFF;
}