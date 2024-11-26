using static OldBit.Spectron.Emulation.Devices.Joystick.GamePad.GamePadAction;

namespace OldBit.Spectron.Emulation.Devices.Joystick.GamePad;

public enum GamePadAction
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

public static class GamePadActionExtensions
{
    public static string GetName(this GamePadAction action) => action switch
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