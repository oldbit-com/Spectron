namespace OldBit.Spectron.Emulation.Devices.DivMmc.SD;

internal static class Token
{
    internal const byte DataError = 0x01;
    internal const byte StartBlock = 0xFE;
    internal const byte DataAccepted = 0x05;
}