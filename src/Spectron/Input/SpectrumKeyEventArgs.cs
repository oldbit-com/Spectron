using System;
using System.Collections.Generic;
using Avalonia.Input;
using OldBit.Spectron.Emulation.Devices.Keyboard;

namespace OldBit.Spectron.Input;

public class SpectrumKeyEventArgs(List<SpectrumKey> keys, Key key, bool isKeyPressed, bool isSimulated = false) : EventArgs
{
    public List<SpectrumKey> Keys { get; } = keys;

    public Key Key { get; } = key;

    public bool IsKeyPressed { get; } = isKeyPressed;

    public bool IsSimulated { get; } = isSimulated;
}
