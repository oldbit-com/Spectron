using Avalonia.Input;
using OldBit.Spectron.Emulation.Devices.Keyboard;

namespace OldBit.Spectron.Tests.Input;

public static class KeyTestData
{
    public static TheoryData<Key, SpectrumKey> StandardKeyData => new()
    {
        { Key.D0, SpectrumKey.D0 },
        { Key.D1, SpectrumKey.D1 },
        { Key.D2, SpectrumKey.D2 },
        { Key.D3, SpectrumKey.D3 },
        { Key.D4, SpectrumKey.D4 },
        { Key.D5, SpectrumKey.D5 },
        { Key.D6, SpectrumKey.D6 },
        { Key.D7, SpectrumKey.D7 },
        { Key.D8, SpectrumKey.D8 },
        { Key.D9, SpectrumKey.D9 },
        { Key.A, SpectrumKey.A },
        { Key.B, SpectrumKey.B },
        { Key.C, SpectrumKey.C },
        { Key.D, SpectrumKey.D },
        { Key.E, SpectrumKey.E },
        { Key.F, SpectrumKey.F },
        { Key.G, SpectrumKey.G },
        { Key.H, SpectrumKey.H },
        { Key.I, SpectrumKey.I },
        { Key.J, SpectrumKey.J },
        { Key.K, SpectrumKey.K },
        { Key.L, SpectrumKey.L },
        { Key.M, SpectrumKey.M },
        { Key.N, SpectrumKey.N },
        { Key.O, SpectrumKey.O },
        { Key.P, SpectrumKey.P },
        { Key.Q, SpectrumKey.Q },
        { Key.R, SpectrumKey.R },
        { Key.S, SpectrumKey.S },
        { Key.T, SpectrumKey.T },
        { Key.U, SpectrumKey.U },
        { Key.V, SpectrumKey.V },
        { Key.W, SpectrumKey.W },
        { Key.X, SpectrumKey.X },
        { Key.Y, SpectrumKey.Y },
        { Key.Z, SpectrumKey.Z },
        { Key.Enter, SpectrumKey.Enter },
        { Key.Space, SpectrumKey.Space },
    };

    public static TheoryData<Key, List<SpectrumKey>> ExtraKeyData => new()
    {
        { Key.Back, [SpectrumKey.None, SpectrumKey.CapsShift, SpectrumKey.D0] },
        { Key.Escape, [SpectrumKey.None] },
        { Key.Left, [SpectrumKey.CapsShift, SpectrumKey.D5] },
        { Key.Down, [SpectrumKey.CapsShift, SpectrumKey.D6] },
        { Key.Up, [SpectrumKey.CapsShift, SpectrumKey.D7] },
        { Key.Right, [SpectrumKey.CapsShift, SpectrumKey.D8] },
    };

    public static TheoryData<string, List<SpectrumKey>> MoreExtraKeyData => new()
    {
        { ",", [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.N] },
        { "<", [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.R] },
        { ".", [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.M] },
        { ">", [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.T] },
        { ";", [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.O] },
        { ":", [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.Z] },
        { "/", [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.V] },
        { "?", [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.C] },
        { "'", [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.D7] },
        { "\"", [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.P] },
        { "=", [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.L] },
        { "+", [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.K] },
        { "-", [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.J] },
        { "_", [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.D0] },
    };
}