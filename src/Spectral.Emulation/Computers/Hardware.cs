namespace OldBit.Spectral.Emulation.Computers;

internal record ComputerSettings(
    float ClockMhz,
    int TicksPerLine,
    int TicksPerFrame,
    int FirstPixelTick);

internal static class Hardware
{
    internal static ComputerSettings Spectrum48K { get; } = new(
        ClockMhz: 3.5f,
        TicksPerLine: 224,
        TicksPerFrame: 69888,
        FirstPixelTick: 14335);

    internal static ComputerSettings Spectrum16K { get; } = Spectrum48K;

    internal static ComputerSettings Spectrum128K { get; } = new(
        ClockMhz: 3.5469f,
        TicksPerLine: 228,
        TicksPerFrame: 70908,
        FirstPixelTick: 14361);

    internal static ComputerSettings Timex2048 { get; } = new ComputerSettings(
        ClockMhz: 3.5f,
        TicksPerLine: 224,
        TicksPerFrame: 69888,
        FirstPixelTick: 14335);
}