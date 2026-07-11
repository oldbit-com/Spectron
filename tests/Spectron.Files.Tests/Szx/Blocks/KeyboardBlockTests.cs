using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Szx.Blocks;

namespace OldBit.Spectron.Files.Tests.Szx.Blocks;

public class KeyboardBlockTests
{
    [Fact]
    public void Keyboard_ShouldConvertToBytes()
    {
        var keyboard = GetKeyboardBlock();
        using var writer = new MemoryStream();

        keyboard.Write(writer);

        var data = writer.ToArray();
        data.Length.ShouldBe(8 + 5);

        // Header
        BitConverter.ToUInt32(data[..4].ToArray()).ShouldBe((DWord)0x4259454B);
        BitConverter.ToUInt32(data[4..8].ToArray()).ShouldBe((DWord)5);

        // Data
        BitConverter.ToUInt32(data[8..12].ToArray()).ShouldBe(KeyboardBlock.FlagsIssue2);
        data[12].ShouldBe(KeyboardBlock.JoystickKempston);
    }

    [Fact]
    public void Keyboard_ShouldConvertFromBytes()
    {
        var keyboardData = GetKeyboardBlockData();
        using var memoryStream = new MemoryStream(keyboardData);
        var reader = new ByteStreamReader(memoryStream);

        var keyboard = KeyboardBlock.Read(reader, keyboardData.Length);

        keyboard.Flags.ShouldBe(KeyboardBlock.FlagsIssue2);
        keyboard.Joystick.ShouldBe(KeyboardBlock.JoystickKempston);
    }

    private static byte[] GetKeyboardBlockData()
    {
        var keyboard = GetKeyboardBlock();
        using var writer = new MemoryStream();

        keyboard.Write(writer);

        return writer.ToArray()[8..].ToArray();
    }

    private static KeyboardBlock GetKeyboardBlock() => new()
    {
        Flags = KeyboardBlock.FlagsIssue2,
        Joystick = KeyboardBlock.JoystickKempston
    };
}