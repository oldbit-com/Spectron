using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Emulation;

internal sealed record HardwareSettings(
    float ClockMhz,
    ComputerType ComputerType,
    int TicksPerLine,
    int TicksPerFrame,
    int InterruptFrequency,
    int FirstPixelTicks,    // Number of T-states passed since the interrupt generation to the first display byte is being sent to screen (early timing).
    int LastPixelTicks,     // Number of T-states passed since the interrupt generation to the last display byte is being sent to screen.
    int LineTicks,          // Number of ticks for the whole screen line, e.g. including borders, content and retrace (224T).
    bool HasAyChip = false);

internal static class Hardware
{
    private const int ContentLineTicks = 128;   // Number of ticks for the screen line content (128T).

    internal static HardwareSettings Spectrum48K { get; } = new(
        ClockMhz: 3.5f,
        ComputerType: ComputerType.Spectrum48K,
        TicksPerLine: 224,
        TicksPerFrame: 69888,
        InterruptFrequency: 50,
        FirstPixelTicks: 14335,
        LastPixelTicks: 14335 + (ScreenSize.ContentHeight - 1) * 224 + ContentLineTicks,
        LineTicks: 224);

    internal static HardwareSettings Spectrum128K { get; } = new(
        ClockMhz: 3.5469f,
        ComputerType: ComputerType.Spectrum128K,
        TicksPerLine: 228,
        TicksPerFrame: 70908,
        InterruptFrequency: 50,
        FirstPixelTicks: 14361,
        LastPixelTicks: 14361 + (ScreenSize.ContentHeight - 1) * 228 + ContentLineTicks,
        LineTicks: 228,
        HasAyChip: true);

    internal static HardwareSettings Timex2048 { get; } = new HardwareSettings(
        ClockMhz: 3.5f,
        ComputerType: ComputerType.Timex2048,
        TicksPerLine: 224,
        TicksPerFrame: 69888,
        InterruptFrequency: 50,
        FirstPixelTicks: 14335,
        LastPixelTicks: 14335 + (ScreenSize.ContentHeight - 1) * 224 + ContentLineTicks,
        LineTicks: 224);
}