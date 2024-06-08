using System.Collections.Generic;
using Avalonia.Input;
using OldBit.ZXSpectrum.Emulator.Hardware;

namespace OldBit.ZXSpectrum.Helpers;

public static class KeyMappings
{
    public static List<SpectrumKey> ToSpectrumKey(KeyEventArgs e)
    {
        return e.PhysicalKey switch
        {
            PhysicalKey.A => [SpectrumKey.A],
            PhysicalKey.B => [SpectrumKey.B],
            PhysicalKey.C => [SpectrumKey.C],
            PhysicalKey.D => [SpectrumKey.D],
            PhysicalKey.E => [SpectrumKey.E],
            PhysicalKey.F => [SpectrumKey.F],
            PhysicalKey.G => [SpectrumKey.G],
            PhysicalKey.H => [SpectrumKey.H],
            PhysicalKey.I => [SpectrumKey.I],
            PhysicalKey.J => [SpectrumKey.J],
            PhysicalKey.K => [SpectrumKey.K],
            PhysicalKey.L => [SpectrumKey.L],
            PhysicalKey.M => [SpectrumKey.M],
            PhysicalKey.N => [SpectrumKey.N],
            PhysicalKey.O => [SpectrumKey.O],
            PhysicalKey.P => [SpectrumKey.P],
            PhysicalKey.Q => [SpectrumKey.Q],
            PhysicalKey.R => [SpectrumKey.R],
            PhysicalKey.S => [SpectrumKey.S],
            PhysicalKey.T => [SpectrumKey.T],
            PhysicalKey.U => [SpectrumKey.U],
            PhysicalKey.V => [SpectrumKey.V],
            PhysicalKey.W => [SpectrumKey.W],
            PhysicalKey.X => [SpectrumKey.X],
            PhysicalKey.Y => [SpectrumKey.Y],
            PhysicalKey.Z => [SpectrumKey.Z],
            PhysicalKey.Digit0 => [SpectrumKey.D0],
            PhysicalKey.Digit1 => [SpectrumKey.D1],
            PhysicalKey.Digit2 => [SpectrumKey.D2],
            PhysicalKey.Digit3 => [SpectrumKey.D3],
            PhysicalKey.Digit4 => [SpectrumKey.D4],
            PhysicalKey.Digit5 => [SpectrumKey.D5],
            PhysicalKey.Digit6 => [SpectrumKey.D6],
            PhysicalKey.Digit7 => [SpectrumKey.D7],
            PhysicalKey.Digit8 => [SpectrumKey.D8],
            PhysicalKey.Digit9 => [SpectrumKey.D9],
            PhysicalKey.ShiftLeft => [SpectrumKey.CapsShift],
            PhysicalKey.ShiftRight => [SpectrumKey.CapsShift],
            PhysicalKey.Enter => [SpectrumKey.Enter],
            PhysicalKey.AltLeft => [SpectrumKey.SymbolShift],
            PhysicalKey.AltRight => [SpectrumKey.SymbolShift],
            PhysicalKey.Space => [SpectrumKey.Space],

            // Extra keys on standard keyboard mapped to ZX Spectrum keys
            PhysicalKey.Backspace => [SpectrumKey.CapsShift, SpectrumKey.D0],
            PhysicalKey.Comma => [SpectrumKey.SymbolShift, SpectrumKey.N],
            PhysicalKey.Period => [SpectrumKey.SymbolShift, SpectrumKey.M],
            PhysicalKey.Semicolon => [SpectrumKey.SymbolShift, SpectrumKey.O],
            PhysicalKey.ArrowLeft => [SpectrumKey.CapsShift, SpectrumKey.D5],
            PhysicalKey.ArrowDown => [SpectrumKey.CapsShift, SpectrumKey.D6],
            PhysicalKey.ArrowUp => [SpectrumKey.CapsShift, SpectrumKey.D7],
            PhysicalKey.ArrowRight => [SpectrumKey.CapsShift, SpectrumKey.D8],
            _ => []
        };
    }
}