using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Input;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using OldBit.Spectron.Settings;

namespace OldBit.Spectron.Input;

public sealed class KeyboardHandler
{
    private Key _capsShift = Key.LeftShift;
    private Key _symbolShift = Key.RightAlt;
    private bool _shouldHandleExtendedKeys;

    public event EventHandler<SpectrumKeyEventArgs>? SpectrumKeyPressed;
    public event EventHandler<SpectrumKeyEventArgs>? SpectrumKeyReleased;

    internal void KeyPressed(KeyEventArgs e)
    {
        if ((e.KeyModifiers & KeyModifiers.Meta) == KeyModifiers.Meta)
        {
            return;
        }

        HandleKeyEvent(e, isPressed: true);
    }

    internal void KeyReleased(KeyEventArgs e)
    {
        if ((e.KeyModifiers & KeyModifiers.Meta) == KeyModifiers.Meta)
        {
            return;
        }

        HandleKeyEvent(e, isPressed: false);
    }

    internal void UpdateSettings(KeyboardSettings keyboardSettings)
    {
        _capsShift = keyboardSettings.CapsShiftKey;
        _symbolShift = keyboardSettings.SymbolShiftKey;
        _shouldHandleExtendedKeys = keyboardSettings.ShouldHandleExtendedKeys;
    }

    internal void SimulatePressKey(SpectrumKey key) =>
        SpectrumKeyPressed?.Invoke(this, new SpectrumKeyEventArgs([key], Key.None, isKeyPressed: true, isSimulated: true));

    internal void SimulateReleaseKey(SpectrumKey key) =>
        SpectrumKeyReleased?.Invoke(this, new SpectrumKeyEventArgs([key], Key.None, isKeyPressed: false, isSimulated: true));

    internal static JoystickInput ToJoystickAction(Key key, Key fireKey) => key switch
    {
        Key.Left => JoystickInput.Left,
        Key.Right => JoystickInput.Right,
        Key.Down => JoystickInput.Down,
        Key.Up => JoystickInput.Up,
        _ => key == fireKey ? JoystickInput.Fire : JoystickInput.None
    };

    private void HandleKeyEvent(KeyEventArgs e, bool isPressed)
    {
        var spectrumKeys = ToSpectrumKeySequence(e);

        if (spectrumKeys.Count > 0)
        {
            var eventArgs = new SpectrumKeyEventArgs(spectrumKeys, e.Key, isPressed);

            if (isPressed)
            {
                SpectrumKeyPressed?.Invoke(this, eventArgs);
            }
            else
            {
                SpectrumKeyReleased?.Invoke(this, eventArgs);
            }
        }
        else if (_shouldHandleExtendedKeys)
        {
            HandleExtendedKey(e, isPressed);
        }
        else
        {
            var eventArgs = new SpectrumKeyEventArgs([], e.Key, isPressed);

            if (isPressed)
            {
                SpectrumKeyPressed?.Invoke(this, eventArgs);
            }
            else
            {
                SpectrumKeyReleased?.Invoke(this, eventArgs);
            }
        }
    }

    private void HandleExtendedKey(KeyEventArgs e, bool isPressed)
    {
        if (isPressed)
        {
            switch (e.KeySymbol)
            {
                case "[":
                    SendExtendedModeKey(Key.Y);
                    break;

                case "]":
                    SendExtendedModeKey(Key.U);
                    break;

                case "{":
                    SendExtendedModeKey(Key.F);
                    break;

                case "}":
                    SendExtendedModeKey(Key.G);
                    break;

                case "`":
                    SendExtendedModeKey(Key.A);
                    break;

                case @"\":
                    SendExtendedModeKey(Key.D);
                    break;

                case "|":
                    SendExtendedModeKey(Key.S);
                    break;

                default:
                    SpectrumKeyPressed?.Invoke(this, new SpectrumKeyEventArgs([], e.Key, isPressed));
                    break;
            }
        }
        else
        {
            switch (e.KeySymbol)
            {
                case "[":
                    SimulateKeyRelease(new KeyEventArgs { Key = Key.Y });
                    break;

                case "]":
                    SimulateKeyRelease(new KeyEventArgs { Key = Key.U });
                    break;

                case "{":
                    SimulateKeyRelease(new KeyEventArgs { Key = Key.F });
                    break;

                case "}":
                    SimulateKeyRelease(new KeyEventArgs { Key = Key.G });
                    break;

                case "`":
                    SimulateKeyRelease(new KeyEventArgs { Key = Key.A });
                    break;

                case @"\":
                    SimulateKeyRelease(new KeyEventArgs { Key = Key.D });
                    break;

                case "|":
                    SimulateKeyRelease(new KeyEventArgs { Key = Key.S });
                    break;

                default:
                    SpectrumKeyReleased?.Invoke(this, new SpectrumKeyEventArgs([], e.Key, isPressed));
                    break;
            }
        }
    }

    private void SendExtendedModeKey(Key key)
    {
        // Enable Extended mode
        SimulateKeyPress(new KeyEventArgs { Key = _capsShift });
        SimulateKeyPress(new KeyEventArgs { Key = _symbolShift });

        Task.Delay(30).ContinueWith(_ =>
        {
            SimulateKeyRelease(new KeyEventArgs { Key = _capsShift });

            Task.Delay(5).ContinueWith(_ =>
            {
                SimulateKeyPress(new KeyEventArgs { Key = key });
                Task.Delay(30).ContinueWith(_ => SimulateKeyRelease(new KeyEventArgs { Key = _symbolShift }));
            });
        });
    }

    private List<SpectrumKey> ToSpectrumKeySequence(KeyEventArgs e)
    {
        if (e.Key == _capsShift)
        {
            return [SpectrumKey.CapsShift];
        }

        if (e.Key == _symbolShift)
        {
            return [SpectrumKey.SymbolShift];
        }

        List<SpectrumKey> keys = e.Key switch
        {
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
            Key.Enter => [SpectrumKey.Enter],
            Key.Space => [SpectrumKey.Space],
            _ => []
        };

        if (keys.Count > 0 || !_shouldHandleExtendedKeys)
        {
            return keys;
        }

        keys = e.KeySymbol switch
        {
            "," => [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.N],
            "<" => [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.R],
            "." => [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.M],
            ">" => [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.T],
            ";" => [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.O],
            ":" => [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.Z],
            "/" => [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.V],
            "?" => [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.C],
            "'" => [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.D7],
            "\"" => [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.P],
            "=" => [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.L],
            "+" => [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.K],
            "-" => [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.J],
            "_" => [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.D0],
            _ => []
        };

        if (keys.Count > 0)
        {
            return keys;
        }

        return e.Key switch
        {
            Key.Back => [SpectrumKey.None, SpectrumKey.CapsShift, SpectrumKey.D0],
            Key.Escape => [SpectrumKey.None],
            Key.Left => [SpectrumKey.CapsShift, SpectrumKey.D5],
            Key.Down => [SpectrumKey.CapsShift, SpectrumKey.D6],
            Key.Up => [SpectrumKey.CapsShift, SpectrumKey.D7],
            Key.Right => [SpectrumKey.CapsShift, SpectrumKey.D8],
            _ => []
        };
    }

    private void SimulateKeyPress(KeyEventArgs e) => KeyPressed(e);

    private void SimulateKeyRelease(KeyEventArgs e) => KeyReleased(e);
}