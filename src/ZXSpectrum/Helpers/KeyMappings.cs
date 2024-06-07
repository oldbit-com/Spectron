using System.Collections.Generic;
using Avalonia.Input;
using OldBit.ZXSpectrum.Emulator.Hardware;

namespace OldBit.ZXSpectrum.Helpers;

public static class KeyMappings
{
    public static List<SpectrumKey> ToSpectrumKey(KeyEventArgs e)
    {
        return e.Key switch
        {
            Key.A => [SpectrumKey.A],
            Key.B => [SpectrumKey.B],
            Key.C => [SpectrumKey.C],
            Key.D => [SpectrumKey.D],
            Key.E => [SpectrumKey.E],
            Key.F => [SpectrumKey.F],
            Key.G => [SpectrumKey.G],
            Key.H => [SpectrumKey.H],
            Key.I => [SpectrumKey.I],
            Key.J => [SpectrumKey.J],
            Key.K => [SpectrumKey.K],
            Key.L => [SpectrumKey.L],
            Key.M => [SpectrumKey.M],
            Key.N => [SpectrumKey.N],
            Key.O => [SpectrumKey.O],
            Key.P => [SpectrumKey.P],
            Key.Q => [SpectrumKey.Q],
            Key.R => [SpectrumKey.R],
            Key.S => [SpectrumKey.S],
            Key.T => [SpectrumKey.T],
            Key.U => [SpectrumKey.U],
            Key.V => [SpectrumKey.V],
            Key.W => [SpectrumKey.W],
            Key.X => [SpectrumKey.X],
            Key.Y => [SpectrumKey.Y],
            Key.Z => [SpectrumKey.Z],
            Key.D0 => [SpectrumKey.D0],
            Key.D1 => [SpectrumKey.D1],
            Key.D2 => [SpectrumKey.D2],
            Key.D3 => [SpectrumKey.D3],
            Key.D4 => [SpectrumKey.D4],
            Key.D5 => [SpectrumKey.D5],
            Key.D6 => [SpectrumKey.D6],
            Key.D7 => [SpectrumKey.D7],
            Key.D8 => [SpectrumKey.D8],
            Key.D9 => [SpectrumKey.D9],
            Key.LeftShift => [SpectrumKey.CapsShift],
            Key.RightShift => [SpectrumKey.CapsShift],
            Key.Enter => [SpectrumKey.Enter],
            Key.RightAlt => [SpectrumKey.SymbolShift],
            Key.LeftAlt => [SpectrumKey.SymbolShift],
            Key.Space => [SpectrumKey.Space],
            Key.Back => [SpectrumKey.CapsShift, SpectrumKey.D0],
            _ => []
        };
    }
}