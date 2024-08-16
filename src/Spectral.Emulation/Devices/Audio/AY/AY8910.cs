namespace OldBit.Spectral.Emulation.Devices.Audio.AY;

internal class AY8910 : IDevice
{
    private const Word RegisterPort = 0xFFFD;
    private const Word DataPort = 0xBFFD;

    private int _registerIndex;
    private int[] _registers = new int[16];

    public void WritePort(Word address, byte value)
    {
        if (IsRegisterPort(address))
        {
            _registerIndex = value & 0x0F;
        }
        else if (IsDataPort(address))
        {

        }
    }

    public byte? ReadPort(Word address)
    {
        if (IsRegisterPort(address))
        {

        }
        if (address != RegisterPort)
        {
            return null;
        }
        return null;
    }

    // Register port 0xFFFD is decoded as: A15=1,A14=1 & A1=0
    private static bool IsRegisterPort(Word address) => (address & 0xC002) == 0xC000;

    // Data port 0xBFFD is decoded as: A15=1 & A1=0
    private static bool IsDataPort(Word address) => (address & 0x8002) == 0x8000;
}