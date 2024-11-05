using FluentAssertions;
using OldBit.Spectron.Emulation.Devices.Keyboard;

namespace OldBit.Spectron.Emulator.Tests.Devices.Keyboard;

public class KeyboardHandlerTests
{
    [Theory]
    [InlineData(SpectrumKey.CapsShift, 0xFE, 0b11111110)]
    [InlineData(SpectrumKey.Z, 0xFEFE, 0b11111101)]
    [InlineData(SpectrumKey.X, 0xFEFE, 0b11111011)]
    [InlineData(SpectrumKey.C, 0xFEFE, 0b11110111)]
    [InlineData(SpectrumKey.V, 0xFEFE, 0b11101111)]
    [InlineData(SpectrumKey.A, 0xFDFE, 0b11111110)]
    [InlineData(SpectrumKey.S, 0xFDFE, 0b11111101)]
    [InlineData(SpectrumKey.D, 0xFDFE, 0b11111011)]
    [InlineData(SpectrumKey.F, 0xFDFE, 0b11110111)]
    [InlineData(SpectrumKey.G, 0xFDFE, 0b11101111)]
    [InlineData(SpectrumKey.Q, 0xFBFE, 0b11111110)]
    [InlineData(SpectrumKey.W, 0xFBFE, 0b11111101)]
    [InlineData(SpectrumKey.E, 0xFBFE, 0b11111011)]
    [InlineData(SpectrumKey.R, 0xFBFE, 0b11110111)]
    [InlineData(SpectrumKey.T, 0xFBFE, 0b11101111)]
    [InlineData(SpectrumKey.D1, 0xF7FE, 0b11111110)]
    [InlineData(SpectrumKey.D2, 0xF7FE, 0b11111101)]
    [InlineData(SpectrumKey.D3, 0xF7FE, 0b11111011)]
    [InlineData(SpectrumKey.D4, 0xF7FE, 0b11110111)]
    [InlineData(SpectrumKey.D5, 0xF7FE, 0b11101111)]
    [InlineData(SpectrumKey.D0, 0xEFFE, 0b11111110)]
    [InlineData(SpectrumKey.D9, 0xEFFE, 0b11111101)]
    [InlineData(SpectrumKey.D8, 0xEFFE, 0b11111011)]
    [InlineData(SpectrumKey.D7, 0xEFFE, 0b11110111)]
    [InlineData(SpectrumKey.D6, 0xEFFE, 0b11101111)]
    [InlineData(SpectrumKey.P, 0xDFFE, 0b11111110)]
    [InlineData(SpectrumKey.O, 0xDFFE, 0b11111101)]
    [InlineData(SpectrumKey.I, 0xDFFE, 0b11111011)]
    [InlineData(SpectrumKey.U, 0xDFFE, 0b11110111)]
    [InlineData(SpectrumKey.Y, 0xDFFE, 0b11101111)]
    [InlineData(SpectrumKey.Enter, 0xBFFE, 0b11111110)]
    [InlineData(SpectrumKey.L, 0xBFFE, 0b11111101)]
    [InlineData(SpectrumKey.K, 0xBFFE, 0b11111011)]
    [InlineData(SpectrumKey.J, 0xBFFE, 0b11110111)]
    [InlineData(SpectrumKey.H, 0xBFFE, 0b11101111)]
    [InlineData(SpectrumKey.Space, 0x7FFE, 0b11111110)]
    [InlineData(SpectrumKey.SymbolShift, 0x7FFE, 0b11111101)]
    [InlineData(SpectrumKey.M, 0x7FFE, 0b11111011)]
    [InlineData(SpectrumKey.N, 0x7FFE, 0b11110111)]
    [InlineData(SpectrumKey.B, 0x7FFE, 0b11101111)]
    public void GivenKeyIsPressed_WhenReadPort_ThenReturnsState(SpectrumKey key, Word port, byte expectedState)
    {
        var keyboard = new KeyboardHandler();
        keyboard.HandleKeyDown(new[] { key });

        var state = keyboard.Read(port);

        state.Should().Be(expectedState);
    }

    [Theory]
    [InlineData(new[] { SpectrumKey.Z }, 0x02FE, 0b11111101)]
    [InlineData(new[] { SpectrumKey.E }, 0x02FE, 0b11111011)]
    [InlineData(new[] { SpectrumKey.B }, 0x02FE, 0b11101111)]
    [InlineData(new[] { SpectrumKey.Z, SpectrumKey.E, SpectrumKey.B }, 0x02FE, 0b11101001)]
    [InlineData(new[] { SpectrumKey.P }, 0x7EFE, 0b11111111)]
    [InlineData(new[] { SpectrumKey.A }, 0x02FE, 0b11111111)]
    [InlineData(new[] { SpectrumKey.S }, 0x02FE, 0b11111111)]
    [InlineData(new[] { SpectrumKey.D }, 0x02FE, 0b11111111)]
    [InlineData(new[] { SpectrumKey.F }, 0x02FE, 0b11111111)]
    [InlineData(new[] { SpectrumKey.G }, 0x02FE, 0b11111111)]
    public void GivenKeyIsPressed_WhenReadSeveralPorts_ThenReturnsCompositeState(IEnumerable<SpectrumKey> keys, Word port, byte expectedState)
    {
        var keyboard = new KeyboardHandler();
        keyboard.HandleKeyDown(keys);

        var state = keyboard.Read(port);

        state.Should().Be(expectedState);
    }
}