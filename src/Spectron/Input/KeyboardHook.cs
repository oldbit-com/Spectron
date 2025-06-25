using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OldBit.Spectron.Emulation.Devices.Joystick;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using SharpHook;
using SharpHook.Data;
using SharpHook.Reactive;

namespace OldBit.Spectron.Input;

public sealed class KeyboardHook : IDisposable
{
    private readonly SimpleReactiveGlobalHook _hook = new();
    private readonly EventSimulator _simulator = new();

    private KeyCode _capsShift = KeyCode.VcLeftShift;
    private KeyCode _symbolShift = KeyCode.VcRightAlt;

    private bool _isShiftPressed;

    public event EventHandler<SpectrumKeyEventArgs>? SpectrumKeyPressed;
    public event EventHandler<SpectrumKeyEventArgs>? SpectrumKeyReleased;

    public KeyboardHook()
    {
        _hook.KeyPressed.Subscribe(KeyPressed);
        _hook.KeyReleased.Subscribe(KeyReleased);
    }

    public void Run() => _hook.RunAsync();

    private void KeyPressed(KeyboardHookEventArgs e)
    {
        HandleShiftState(e, isPressed: true);

        var spectrumKeys = ToSpectrumKeySequence(e.Data.KeyCode);

        if (spectrumKeys.Count > 0)
        {
            SpectrumKeyPressed?.Invoke(this, new SpectrumKeyEventArgs(spectrumKeys, e.Data.KeyCode, isKeyPressed: true));
        }
        else
        {
            switch (e.Data.KeyCode)
            {
                case KeyCode.VcOpenBracket:
                    SendExtendedModeKey(!_isShiftPressed ? KeyCode.VcY : KeyCode.VcF);
                    break;

                case KeyCode.VcCloseBracket:
                    SendExtendedModeKey(!_isShiftPressed ? KeyCode.VcU : KeyCode.VcG);
                    break;

                case KeyCode.VcBackQuote:
                    SendExtendedModeKey(KeyCode.VcA);
                    break;

                case KeyCode.VcBackslash:
                    SendExtendedModeKey(!_isShiftPressed ? KeyCode.VcD : KeyCode.VcS);
                    break;
            }
        }
    }

    private void KeyReleased(KeyboardHookEventArgs e)
    {
        HandleShiftState(e, isPressed: false);

        var spectrumKeys = ToSpectrumKeySequence(e.Data.KeyCode);

        if (spectrumKeys.Count > 0)
        {
            SpectrumKeyReleased?.Invoke(this, new SpectrumKeyEventArgs(spectrumKeys, e.Data.KeyCode, isKeyPressed: false));
        }
        else
        {
            switch (e.Data.KeyCode)
            {
                case KeyCode.VcOpenBracket:
                    _simulator.SimulateKeyRelease(KeyCode.VcY);
                    _simulator.SimulateKeyRelease(KeyCode.VcF);
                    break;

                case KeyCode.VcCloseBracket:
                    _simulator.SimulateKeyRelease(KeyCode.VcU);
                    _simulator.SimulateKeyRelease(KeyCode.VcG);
                    break;

                case KeyCode.VcBackQuote:
                    _simulator.SimulateKeyRelease(KeyCode.VcA);
                    break;

                case KeyCode.VcBackslash:
                    _simulator.SimulateKeyRelease(KeyCode.VcD);
                    _simulator.SimulateKeyRelease(KeyCode.VcS);
                    break;
            }
        }
    }

    private void SendExtendedModeKey(KeyCode keyCode)
    {
        // Enable Extended mode
        _simulator.SimulateKeyPress(_capsShift);
        _simulator.SimulateKeyPress(_symbolShift);

        Task.Delay(30).ContinueWith(_ =>
        {
            _simulator.SimulateKeyRelease(_capsShift);

            Task.Delay(5).ContinueWith(_ =>
                _simulator.SimulateKeyPress(keyCode));
        });
    }

    private void HandleShiftState(KeyboardHookEventArgs e, bool isPressed)
    {
        if (e is { IsEventSimulated: false, Data.KeyCode: KeyCode.VcLeftShift or KeyCode.VcRightShift })
        {
            _isShiftPressed = isPressed;
        }
    }

    private List<SpectrumKey> ToSpectrumKeySequence(KeyCode keyCode)
    {
        if (keyCode == _capsShift)
        {
            return [SpectrumKey.CapsShift];
        }

        if (keyCode == _symbolShift)
        {
            return [SpectrumKey.SymbolShift];
        }

        return keyCode switch
        {
            KeyCode.Vc0 => [SpectrumKey.D0],
            KeyCode.Vc1 => [SpectrumKey.D1],
            KeyCode.Vc2 => [SpectrumKey.D2],
            KeyCode.Vc3 => [SpectrumKey.D3],
            KeyCode.Vc4 => [SpectrumKey.D4],
            KeyCode.Vc5 => [SpectrumKey.D5],
            KeyCode.Vc6 => [SpectrumKey.D6],
            KeyCode.Vc7 => [SpectrumKey.D7],
            KeyCode.Vc8 => [SpectrumKey.D8],
            KeyCode.Vc9 => [SpectrumKey.D9],
            KeyCode.VcA => [SpectrumKey.A],
            KeyCode.VcB => [SpectrumKey.B],
            KeyCode.VcC => [SpectrumKey.C],
            KeyCode.VcD => [SpectrumKey.D],
            KeyCode.VcE => [SpectrumKey.E],
            KeyCode.VcF => [SpectrumKey.F],
            KeyCode.VcG => [SpectrumKey.G],
            KeyCode.VcH => [SpectrumKey.H],
            KeyCode.VcI => [SpectrumKey.I],
            KeyCode.VcJ => [SpectrumKey.J],
            KeyCode.VcK => [SpectrumKey.K],
            KeyCode.VcL => [SpectrumKey.L],
            KeyCode.VcM => [SpectrumKey.M],
            KeyCode.VcN => [SpectrumKey.N],
            KeyCode.VcO => [SpectrumKey.O],
            KeyCode.VcP => [SpectrumKey.P],
            KeyCode.VcQ => [SpectrumKey.Q],
            KeyCode.VcR => [SpectrumKey.R],
            KeyCode.VcS => [SpectrumKey.S],
            KeyCode.VcT => [SpectrumKey.T],
            KeyCode.VcU => [SpectrumKey.U],
            KeyCode.VcV => [SpectrumKey.V],
            KeyCode.VcW => [SpectrumKey.W],
            KeyCode.VcX => [SpectrumKey.X],
            KeyCode.VcY => [SpectrumKey.Y],
            KeyCode.VcZ => [SpectrumKey.Z],
            KeyCode.VcEnter => [SpectrumKey.Enter],
            KeyCode.VcSpace => [SpectrumKey.Space],
            KeyCode.VcBackspace => [SpectrumKey.None, SpectrumKey.CapsShift, SpectrumKey.D0],
            KeyCode.VcEscape => [SpectrumKey.None],
            KeyCode.VcComma => !_isShiftPressed
                ? [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.N]
                : [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.R],
            KeyCode.VcPeriod => !_isShiftPressed
                ? [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.M]
                : [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.T],
            KeyCode.VcSemicolon => !_isShiftPressed
                ? [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.O]
                : [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.Z],
            KeyCode.VcSlash => !_isShiftPressed
                ? [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.V]
                : [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.C],
            KeyCode.VcQuote => !_isShiftPressed
                ? [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.P]
                : [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.D7],
            KeyCode.VcEquals => !_isShiftPressed
                ? [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.L]
                : [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.K],
            KeyCode.VcMinus => !_isShiftPressed
                ? [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.J]
                : [SpectrumKey.None, SpectrumKey.SymbolShift, SpectrumKey.D0],
            KeyCode.VcLeft => [SpectrumKey.CapsShift, SpectrumKey.D5],
            KeyCode.VcDown => [SpectrumKey.CapsShift, SpectrumKey.D6],
            KeyCode.VcUp => [SpectrumKey.CapsShift, SpectrumKey.D7],
            KeyCode.VcRight => [SpectrumKey.CapsShift, SpectrumKey.D8],
            _ => []
        };
    }

    public void UpdateShiftKeys(KeyCode capsShift, KeyCode symbolShift)
    {
        _capsShift = capsShift;
        _symbolShift = symbolShift;
    }

    public static JoystickInput ToJoystickAction(KeyCode keyCode, KeyCode fireKey) => keyCode switch
    {
        KeyCode.VcLeft => JoystickInput.Left,
        KeyCode.VcRight => JoystickInput.Right,
        KeyCode.VcDown => JoystickInput.Down,
        KeyCode.VcUp => JoystickInput.Up,
        _ => keyCode == fireKey ? JoystickInput.Fire : JoystickInput.None
    };

    public void Dispose() => _hook.Dispose();
}