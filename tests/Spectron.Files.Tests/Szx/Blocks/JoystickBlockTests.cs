using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx.Blocks;

namespace OldBit.Spectron.Files.Tests.Szx.Blocks;

public class JoystickBlockTests
{
    [Fact]
    public void Joystick_ShouldConvertToBytes()
    {
        var joysstick = GetJoystickBlock();
        using var writer = new MemoryStream();

        joysstick.Write(writer);

        var data = writer.ToArray();
        data.Length.ShouldBe(8 + 6);

        // Header
        BitConverter.ToUInt32(data[..4].ToArray()).ShouldBe((DWord)0x00594F4A);
        BitConverter.ToUInt32(data[4..8].ToArray()).ShouldBe((DWord)6);

        // Data
        BitConverter.ToUInt32(data[8..12].ToArray()).ShouldBe((DWord)0);
        data[12].ShouldBe(JoystickBlock.JoystickSinclair1);
        data[13].ShouldBe(JoystickBlock.JoystickSinclair2);
    }

    [Fact]
    public void Joystick_ShouldConvertFromBytes()
    {
        var joystickData = GetJoystickBlockData();
        using var memoryStream = new MemoryStream(joystickData);
        var reader = new ByteStreamReader(memoryStream);

        var joystick = JoystickBlock.Read(reader, joystickData.Length);

        joystick.Flags.ShouldBe((DWord)0);
        joystick.JoystickTypePlayer1.ShouldBe(JoystickBlock.JoystickSinclair1);
        joystick.JoystickTypePlayer2.ShouldBe(JoystickBlock.JoystickSinclair2);
    }

    private static byte[] GetJoystickBlockData()
    {
        var joystick = GetJoystickBlock();
        using var writer = new MemoryStream();

        joystick.Write(writer);

        return writer.ToArray()[8..].ToArray();
    }

    private static JoystickBlock GetJoystickBlock() => new()
    {
        Flags = 0,
        JoystickTypePlayer1 = JoystickBlock.JoystickSinclair1,
        JoystickTypePlayer2 = JoystickBlock.JoystickSinclair2
    };
}