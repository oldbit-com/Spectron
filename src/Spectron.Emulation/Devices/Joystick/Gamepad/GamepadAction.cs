using static OldBit.Spectron.Emulation.Devices.Joystick.Gamepad.GamepadAction;

namespace OldBit.Spectron.Emulation.Devices.Joystick.Gamepad;

public enum GamepadAction
{
    None,

    // Joystick controls
    JoystickLeft,
    JoystickRight,
    JoystickUp,
    JoystickDown,
    JoystickFire,

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

        >= D0 and <= D9 => action.ToString()[1..],
        >= A and <= Z => action.ToString(),

        _ => string.Empty
    };
}