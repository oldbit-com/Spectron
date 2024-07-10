using FluentAssertions;
using OldBit.ZXSpectrum.Emulator.Hardware;

namespace OldBit.ZXSpectrum.Emulator.UnitTests;

public class KeyboardTests
{
    [Theory]
    [InlineData(SpectrumKey.CapsShift, 0xFE, 0b11111110)]
    [InlineData(SpectrumKey.Z, 0xFE, 0b11111101)]
    [InlineData(SpectrumKey.X, 0xFE, 0b11111011)]
    [InlineData(SpectrumKey.C, 0xFE, 0b11110111)]
    [InlineData(SpectrumKey.V, 0xFE, 0b11101111)]
    [InlineData(SpectrumKey.A, 0xFD, 0b11111110)]
    [InlineData(SpectrumKey.S, 0xFD, 0b11111101)]
    [InlineData(SpectrumKey.D, 0xFD, 0b11111011)]
    [InlineData(SpectrumKey.F, 0xFD, 0b11110111)]
    [InlineData(SpectrumKey.G, 0xFD, 0b11101111)]
    [InlineData(SpectrumKey.Q, 0xFB, 0b11111110)]
    [InlineData(SpectrumKey.W, 0xFB, 0b11111101)]
    [InlineData(SpectrumKey.E, 0xFB, 0b11111011)]
    [InlineData(SpectrumKey.R, 0xFB, 0b11110111)]
    [InlineData(SpectrumKey.T, 0xFB, 0b11101111)]
    [InlineData(SpectrumKey.D1, 0xF7, 0b11111110)]
    [InlineData(SpectrumKey.D2, 0xF7, 0b11111101)]
    [InlineData(SpectrumKey.D3, 0xF7, 0b11111011)]
    [InlineData(SpectrumKey.D4, 0xF7, 0b11110111)]
    [InlineData(SpectrumKey.D5, 0xF7, 0b11101111)]
    [InlineData(SpectrumKey.D0, 0xEF, 0b11111110)]
    [InlineData(SpectrumKey.D9, 0xEF, 0b11111101)]
    [InlineData(SpectrumKey.D8, 0xEF, 0b11111011)]
    [InlineData(SpectrumKey.D7, 0xEF, 0b11110111)]
    [InlineData(SpectrumKey.D6, 0xEF, 0b11101111)]
    [InlineData(SpectrumKey.P, 0xDF, 0b11111110)]
    [InlineData(SpectrumKey.O, 0xDF, 0b11111101)]
    [InlineData(SpectrumKey.I, 0xDF, 0b11111011)]
    [InlineData(SpectrumKey.U, 0xDF, 0b11110111)]
    [InlineData(SpectrumKey.Y, 0xDF, 0b11101111)]
    [InlineData(SpectrumKey.Enter, 0xBF, 0b11111110)]
    [InlineData(SpectrumKey.L, 0xBF, 0b11111101)]
    [InlineData(SpectrumKey.K, 0xBF, 0b11111011)]
    [InlineData(SpectrumKey.J, 0xBF, 0b11110111)]
    [InlineData(SpectrumKey.H, 0xBF, 0b11101111)]
    [InlineData(SpectrumKey.Space, 0x7F, 0b11111110)]
    [InlineData(SpectrumKey.SymbolShift, 0x7F, 0b11111101)]
    [InlineData(SpectrumKey.M, 0x7F, 0b11111011)]
    [InlineData(SpectrumKey.N, 0x7F, 0b11110111)]
    [InlineData(SpectrumKey.B, 0x7F, 0b11101111)]
    public void GivenKeyIsPressed_WhenReadPort_ThenReturnsState(SpectrumKey key, byte port, byte expectedState)
    {
        var keyboard = new Keyboard();
        keyboard.KeyDown(new[] { key });

        var state = keyboard.GetKeyState(port);

        state.Should().Be(expectedState);
    }

    [Theory]
    [InlineData(new[] { SpectrumKey.Z }, 0x02, 0b11111101)]
    [InlineData(new[] { SpectrumKey.E }, 0x02, 0b11111011)]
    [InlineData(new[] { SpectrumKey.B }, 0x02, 0b11101111)]
    [InlineData(new[] { SpectrumKey.Z, SpectrumKey.E, SpectrumKey.B }, 0x02, 0b11101001)]
    [InlineData(new[] { SpectrumKey.P }, 0x7E, 0b11111111)]
    [InlineData(new[] { SpectrumKey.A }, 0x02, 0b11111111)]
    [InlineData(new[] { SpectrumKey.S }, 0x02, 0b11111111)]
    [InlineData(new[] { SpectrumKey.D }, 0x02, 0b11111111)]
    [InlineData(new[] { SpectrumKey.F }, 0x02, 0b11111111)]
    [InlineData(new[] { SpectrumKey.G }, 0x02, 0b11111111)]
    public void GivenKeyIsPressed_WhenReadSeveralPorts_ThenReturnsCompositeState(IEnumerable<SpectrumKey> keys, byte port, byte expectedState)
    {
        var keyboard = new Keyboard();
        keyboard.KeyDown(keys);

        var state = keyboard.GetKeyState(port);

        state.Should().Be(expectedState);
    }
}