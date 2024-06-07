namespace OldBit.ZXSpectrum.Emulator.Screen;

internal record BorderState
{
    public Color Color { get; set; } = Colors.White;
    public int ClockCycle { get; set; }
    public int Index { get; set; }
}