using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Emulation;

internal sealed record HardwareSettings(
    float ClockMhz,
    ComputerType ComputerType,
    int TicksPerFrame,
    int InterruptFrequency,
    int InterruptDuration,
    int FirstPixelTicks,    // Number of T-states passed since the interrupt generation to the first display byte is being sent to screen (early timing).
    int LastPixelTicks,     // Number of T-states passed since the interrupt generation to the last display byte is being sent to screen.
    int TicksPerLine,       // Number of ticks for the whole screen line, e.g. including borders, content and retrace (224T).
    bool HasAyChip = false);

internal static class Hardware
{
    private const int ContentLineTicks = 128;   // Number of ticks for the screen line content (128T).

    internal static HardwareSettings Spectrum48K { get; } = new(
        ClockMhz: 3.5f,
        ComputerType: ComputerType.Spectrum48K,
        TicksPerFrame: 69888,
        InterruptFrequency: 50,
        InterruptDuration: 32,
        FirstPixelTicks: 14335,
        LastPixelTicks: 14335 + (ScreenSize.ContentHeight - 1) * 224 + ContentLineTicks,
        TicksPerLine: 224);

    internal static HardwareSettings Spectrum128K { get; } = new(
        ClockMhz: 3.5469f,
        ComputerType: ComputerType.Spectrum128K,
        TicksPerFrame: 70908,
        InterruptFrequency: 50,
        InterruptDuration: 36,
        FirstPixelTicks: 14361,
        LastPixelTicks: 14361 + (ScreenSize.ContentHeight - 1) * 228 + ContentLineTicks,
        TicksPerLine: 228,
        HasAyChip: true);

    internal static HardwareSettings Timex2048 { get; } = new(
        ClockMhz: 3.5f,
        ComputerType: ComputerType.Timex2048,
        TicksPerFrame: 69888,
        InterruptFrequency: 50,
        InterruptDuration: 32,
        FirstPixelTicks: 14335,
        LastPixelTicks: 14335 + (ScreenSize.ContentHeight - 1) * 224 + ContentLineTicks,
        TicksPerLine: 224);
}