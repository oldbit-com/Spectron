namespace OldBit.Spectron.Emulation.Screen;

public readonly record struct Color
{
    public int Abgr { get; }
    public int Argb { get; }
    public byte Red { get; init; }
    public byte Green { get; init; }
    public byte Blue { get; init; }

    internal Color(int Red, int Green, int Blue) : this((byte)Red, (byte)Green, (byte)Blue) { }

    private Color(byte Red, byte Green, byte Blue)
    {
        this.Red = Red;
        this.Green = Green;
        this.Blue = Blue;

        Abgr = 0xFF << 24 | Blue << 16 | Green << 8 | Red;
        Argb = 0xFF << 24 | Red << 16 | Green << 8 | Blue;
    }
}
