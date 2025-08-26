using OldBit.Spectron.Emulation.Devices.Interface1;

namespace OldBit.Spectron.Emulator.Tests.Devices.Interface1;

public class CartridgeTests
{
    [Fact]
    public void CartridgeShouldLoad()
    {
        var data = CreateEmptyCartridge();

        var cartridge = new Cartridge(data);
    }

    private static byte[] CreateEmptyCartridge(bool includeWriteProtected = true)
    {
        var data = new List<byte>();

        for (byte i = 0xFE; i > 0; i--)
        {
            // Header, 15 bytes
            data.AddRange([0x01, i, 0x00, 0x00, 0x44, 0x41, 0x54, 0x41, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x0F]);

            // Data, 528 bytes
            data.AddRange(Enumerable.Repeat((byte)0, 528));
        }

        // Write protect
        data.Add(0x01);

        return data.ToArray();
    }
}