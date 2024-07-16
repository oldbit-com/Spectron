namespace OldBit.ZXSpectrum.Emulator.Hardware;

public class Keyboard : IInputDevice
{
    private readonly Dictionary<byte, byte> _keyStates = new()
    {
        { 0xFE, 0xFF },     // Shift, Z, X, C, V
        { 0xFD, 0xFF },     // A, S, D, F, G
        { 0xFB, 0xFF },     // Q, W, E, R, T
        { 0xF7, 0xFF },     // 1, 2, 3, 4, 5
        { 0xEF, 0xFF },     // 0, 9, 8, 7, 6
        { 0xDF, 0xFF },     // P, O, I, U, Y
        { 0xBF, 0xFF },     // Enter, L, K, J, H
        { 0x7F, 0xFF }      // Space, Sym, M, N, B
    };

    private readonly Dictionary<SpectrumKey, (byte Bit, byte Port)> _keyPorts = new()
    {
        { SpectrumKey.CapsShift, (0b00001, 0xFE) },
        { SpectrumKey.Z, (0b00010, 0xFE) },
        { SpectrumKey.X, (0b00100, 0xFE) },
        { SpectrumKey.C, (0b01000, 0xFE) },
        { SpectrumKey.V, (0b10000, 0xFE) },

        { SpectrumKey.A, (0b00001, 0xFD) },
        { SpectrumKey.S, (0b00010, 0xFD) },
        { SpectrumKey.D, (0b00100, 0xFD) },
        { SpectrumKey.F, (0b01000, 0xFD) },
        { SpectrumKey.G, (0b10000, 0xFD) },

        { SpectrumKey.Q, (0b00001, 0xFB) },
        { SpectrumKey.W, (0b00010, 0xFB) },
        { SpectrumKey.E, (0b00100, 0xFB) },
        { SpectrumKey.R, (0b01000, 0xFB) },
        { SpectrumKey.T, (0b10000, 0xFB) },

        { SpectrumKey.D1, (0b00001, 0xF7) },
        { SpectrumKey.D2, (0b00010, 0xF7) },
        { SpectrumKey.D3, (0b00100, 0xF7) },
        { SpectrumKey.D4, (0b01000, 0xF7) },
        { SpectrumKey.D5, (0b10000, 0xF7) },

        { SpectrumKey.D0, (0b00001, 0xEF) },
        { SpectrumKey.D9, (0b00010, 0xEF) },
        { SpectrumKey.D8, (0b00100, 0xEF) },
        { SpectrumKey.D7, (0b01000, 0xEF) },
        { SpectrumKey.D6, (0b10000, 0xEF) },

        { SpectrumKey.P, (0b00001, 0xDF) },
        { SpectrumKey.O, (0b00010, 0xDF) },
        { SpectrumKey.I, (0b00100, 0xDF) },
        { SpectrumKey.U, (0b01000, 0xDF) },
        { SpectrumKey.Y, (0b10000, 0xDF) },

        { SpectrumKey.Enter, (0b00001, 0xBF) },
        { SpectrumKey.L, (0b00010, 0xBF) },
        { SpectrumKey.K, (0b00100, 0xBF) },
        { SpectrumKey.J, (0b01000, 0xBF) },
        { SpectrumKey.H, (0b10000, 0xBF) },

        { SpectrumKey.Space, (0b00001, 0x7F) },
        { SpectrumKey.SymbolShift, (0b00010, 0x7F) },
        { SpectrumKey.M, (0b00100, 0x7F) },
        { SpectrumKey.N, (0b01000, 0x7F) },
        { SpectrumKey.B, (0b10000, 0x7F) }
    };

    public byte? Read(Word address)
    {
        if (!IsKeyPort(address))
        {
            return null;
        }

        return GetKeyState((byte)(address >> 8));
    }

    public void HandleKeyDown(IEnumerable<SpectrumKey> keys)
    {
        foreach (var key in keys)
        {
            if (_keyPorts.TryGetValue(key, out var mapping))
            {
                _keyStates[mapping.Port] &= (byte)~mapping.Bit;
            }
        }
    }

    public void HandleKeyUp(IEnumerable<SpectrumKey> keys)
    {
        foreach (var key in keys)
        {
            if (_keyPorts.TryGetValue(key, out var mapping))
            {
                _keyStates[mapping.Port] |= mapping.Bit;
            }
        }
    }

    private byte GetKeyState(byte port)
    {
        if (_keyStates.TryGetValue(port, out var state))
        {
            return state;
        }

        state = 0xFF;

        // Special case, for example if port is 0x02, it means check any key except row A-G (0xFD) etc.
        foreach (var (keyPort, keyState) in _keyStates)
        {
            if ((keyPort & port) == port)
            {
                state &= keyState;
            }
        }

        return state;
    }

    private static bool IsKeyPort(Word address) => (address & 0x01) == 0;
}