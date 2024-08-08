namespace OldBit.Spectral.Emulation.Computers;

internal record HardwareSettings(
    float ClockMhz,
    int TicksPerLine,
    int TicksPerFrame,
    int InterruptFrequency,
    int FirstPixelTick);

internal static class Hardware
{
    internal static HardwareSettings Spectrum48K { get; } = new(
        ClockMhz: 3.5f,
        TicksPerLine: 224,
        TicksPerFrame: 69888,
        InterruptFrequency: 50,
        FirstPixelTick: 14335);

    internal static HardwareSettings Spectrum128K { get; } = new(
        ClockMhz: 3.5469f,
        TicksPerLine: 228,
        TicksPerFrame: 70908,
        InterruptFrequency: 50,
        FirstPixelTick: 14361);

    internal static HardwareSettings Timex2048 { get; } = new HardwareSettings(
        ClockMhz: 3.5f,
        TicksPerLine: 224,
        TicksPerFrame: 69888,
        InterruptFrequency: 50,
        FirstPixelTick: 14335);
}