using OldBit.Spectron.Emulation.Devices.Keyboard;
using static OldBit.Spectron.Emulation.Devices.Gamepad.GamepadAction;

namespace OldBit.Spectron.Emulation.Devices.Gamepad;

public enum GamepadAction
{
    None,

    // Joystick controls
    JoystickLeft,
    JoystickRight,
    JoystickUp,
    JoystickDown,
    JoystickFire,

    // Emulator actions
    Pause,
    Rewind,
    QuickSave,
    QuickLoad,
    NMI,

    // Keyboard (numbers)
    D0,
    D1,
    D2,
    D3,
    D4,
    D5,
    D6,
    D7,
    D8,
    D9,

    // Keyboard (special)
    Space,
    CapsShift,
    SymbolShift,

    // Keyboard (letters)
    A,
    B,
    C,
    D,
    E,
    F,
    G,
    H,
    I,
    J,
    K,
    L,
    M,
    N,
    O,
    P,
    Q,
    R,
    S,
    T,
    U,
    V,
    W,
    X,
    Y,
    Z
}

public static class GamepadActionExtensions
{
    public static string GetName(this GamepadAction action) => action switch
    {
        None => "None",

        JoystickLeft => "Left",
        JoystickRight => "Right",
        JoystickUp => "Up",
        JoystickDown => "Down",
        JoystickFire => "Fire",

        Space => "Space",
        CapsShift => "Caps Shift",
        SymbolShift => "Symbol Shift",

        Pause => "Pause",
        Rewind => "Rewind",
        QuickSave => "Quick Save",
        QuickLoad => "Quick Load",
        NMI => "NMI",

        >= D0 and <= D9 => action.ToString()[1..],
        >= A and <= Z => action.ToString(),

        _ => string.Empty
    };

    public static SpectrumKey? ToSpectrumKey(this GamepadAction action) => action switch
    {
        D0 => SpectrumKey.D0,
        D1 => SpectrumKey.D1,
        D2 => SpectrumKey.D2,
        D3 => SpectrumKey.D3,
        D4 => SpectrumKey.D4,
        D5 => SpectrumKey.D5,
        D6 => SpectrumKey.D6,
        D7 => SpectrumKey.D7,
        D8 => SpectrumKey.D8,
        D9 => SpectrumKey.D9,

        Space => SpectrumKey.Space,
        CapsShift => SpectrumKey.CapsShift,
        SymbolShift => SpectrumKey.SymbolShift,

        A => SpectrumKey.A,
        B => SpectrumKey.B,
        C => SpectrumKey.C,
        D => SpectrumKey.D,
        E => SpectrumKey.E,
        F => SpectrumKey.F,
        G => SpectrumKey.G,
        H => SpectrumKey.H,
        I => SpectrumKey.I,
        J => SpectrumKey.J,
        K => SpectrumKey.K,
        L => SpectrumKey.L,
        M => SpectrumKey.M,
        N => SpectrumKey.N,
        O => SpectrumKey.O,
        P => SpectrumKey.P,
        Q => SpectrumKey.Q,
        R => SpectrumKey.R,
        S => SpectrumKey.S,
        T => SpectrumKey.T,
        U => SpectrumKey.U,
        V => SpectrumKey.V,
        W => SpectrumKey.W,
        X => SpectrumKey.X,
        Y => SpectrumKey.Y,
        Z => SpectrumKey.Z,

        _ => null
    };
}