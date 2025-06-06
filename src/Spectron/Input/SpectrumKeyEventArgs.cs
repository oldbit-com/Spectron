using System;
using System.Collections.Generic;
using OldBit.Spectron.Emulation.Devices.Keyboard;
using SharpHook.Data;

namespace OldBit.Spectron.Input;

public class SpectrumKeyEventArgs(List<SpectrumKey> keys, KeyCode keyCode, bool isKeyPressed) : EventArgs
{
    public List<SpectrumKey> Keys { get; } = keys;

    public KeyCode KeyCode { get; } = keyCode;

    public bool IsKeyPressed { get; } = isKeyPressed;
}
