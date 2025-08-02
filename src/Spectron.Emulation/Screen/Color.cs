using System.Runtime.InteropServices;

namespace OldBit.Spectron.Emulation.Screen;

[StructLayout(LayoutKind.Sequential)]
public readonly record struct Color
{
    public readonly byte Red;
    public readonly byte Green;
    public readonly byte Blue;
    public readonly byte Alpha = 0xFF;

    public uint Abgr => (uint)(0xFF << 24 | Blue << 16 | Green << 8 | Red);
    public uint Argb => (uint)(0xFF << 24 | Red << 16 | Green << 8 | Blue);

    public Color(int Red, int Green, int Blue) : this((byte)Red, (byte)Green, (byte)Blue) { }

    private Color(byte Red, byte Green, byte Blue)
    {
        this.Red = Red;
        this.Green = Green;
        this.Blue = Blue;
    }
}
