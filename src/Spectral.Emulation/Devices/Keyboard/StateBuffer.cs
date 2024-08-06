namespace OldBit.Spectral.Emulation.Devices.Keyboard;

internal class StateBuffer
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


}