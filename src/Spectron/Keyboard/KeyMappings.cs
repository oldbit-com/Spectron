using System.Collections.Generic;
using Avalonia.Input;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Keyboard;

namespace OldBit.Spectron.Keyboard;

public static class KeyMappings
{
    public static List<SpectrumKey> ToSpectrumKey(KeyEventArgs e, JoystickInput joystickInput)
    {
        if ((e.KeyModifiers & KeyModifiers.Alt) != 0)
        {
            return [];
        }

        return e.KeySymbol switch
        {
            ":" => [SpectrumKey.SymbolShift, SpectrumKey.Z],
            "\"" => [SpectrumKey.SymbolShift, SpectrumKey.P],
            "'" => [SpectrumKey.SymbolShift, SpectrumKey.D7],
            // This needs different handling because it needs Extended mode
            "[" => [SpectrumKey.SymbolShift, SpectrumKey.CapsShift, SpectrumKey.Y],

            _ => e.PhysicalKey switch
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
                PhysicalKey.Enter => [SpectrumKey.Enter],
                PhysicalKey.ControlLeft => [SpectrumKey.SymbolShift],
                PhysicalKey.ControlRight => [SpectrumKey.SymbolShift],
                PhysicalKey.Space => [SpectrumKey.Space],

                // Extra keys on standard keyboard mapped to ZX Spectrum keys
                PhysicalKey.Backspace => [SpectrumKey.CapsShift, SpectrumKey.D0],
                PhysicalKey.Comma => [SpectrumKey.SymbolShift, SpectrumKey.N],
                PhysicalKey.Period => [SpectrumKey.SymbolShift, SpectrumKey.M],
                PhysicalKey.Semicolon => [SpectrumKey.SymbolShift, SpectrumKey.O],
                PhysicalKey.ArrowLeft => joystickInput == JoystickInput.Left ? [] : [SpectrumKey.CapsShift, SpectrumKey.D5],
                PhysicalKey.ArrowDown => joystickInput == JoystickInput.Down ? [] : [SpectrumKey.CapsShift, SpectrumKey.D6],
                PhysicalKey.ArrowUp => joystickInput == JoystickInput.Up ? [] : [SpectrumKey.CapsShift, SpectrumKey.D7],
                PhysicalKey.ArrowRight => joystickInput == JoystickInput.Right ? [] : [SpectrumKey.CapsShift, SpectrumKey.D8],
                PhysicalKey.Minus => [SpectrumKey.SymbolShift, SpectrumKey.J],
                _ => []
            }
        };
    }

    public static JoystickInput ToJoystickAction(PhysicalKey key, PhysicalKey fireKey) => key switch
    {
        PhysicalKey.ArrowLeft => JoystickInput.Left,
        PhysicalKey.ArrowRight => JoystickInput.Right,
        PhysicalKey.ArrowDown => JoystickInput.Down,
        PhysicalKey.ArrowUp => JoystickInput.Up,
        _ => key == fireKey ? JoystickInput.Fire : JoystickInput.None
    };
}