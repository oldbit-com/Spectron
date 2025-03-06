using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Emulation;

internal sealed record HardwareSettings(
    float ClockMhz,
    ComputerType ComputerType,
    int TicksPerFrame,          // Total number of T-states per frame
    int InterruptFrequency,
    int InterruptDuration,
    int RetraceTicks,
    int FloatingBusStartTicks,
    int FirstPixelTicks,        // Number of T-states passed since the interrupt generation to the first display byte is being sent to screen (early timing).
    int LastPixelTicks,         // Number of T-states passed since the interrupt generation to the last display byte is being sent to screen.
    int TicksPerLine,           // Number of ticks for the whole screen line, e.g. including borders, content and retrace (224T).
    bool HasAyChip = false);

internal static class Hardware
{
    private const int ContentLineTicks = 128;   // Number of ticks for the screen line content (128T).

    internal static HardwareSettings Spectrum48K { get; } = new(
        ClockMhz: 3.5f,
        ComputerType: ComputerType.Spectrum48K,
        TicksPerFrame: 69888,
        InterruptFrequency: 50,
        RetraceTicks: 48,
        FloatingBusStartTicks: 14338,
        InterruptDuration: 32,
        FirstPixelTicks: 14336,
        LastPixelTicks: 14336 + (ScreenSize.ContentHeight - 1) * 224 + ContentLineTicks,
        TicksPerLine: 224);

    internal static HardwareSettings Spectrum128K { get; } = new(
        ClockMhz: 3.5469f,
        ComputerType: ComputerType.Spectrum128K,
        TicksPerFrame: 70908,
        InterruptFrequency: 50,
        RetraceTicks: 52,
        FloatingBusStartTicks: 14364,
        InterruptDuration: 36,
        FirstPixelTicks: 14362,
        LastPixelTicks: 14362 + (ScreenSize.ContentHeight - 1) * 228 + ContentLineTicks,
        TicksPerLine: 228,
        HasAyChip: true);

    internal static HardwareSettings Timex2048 { get; } = new(
        ClockMhz: 3.5f,
        ComputerType: ComputerType.Timex2048,
        TicksPerFrame: 69888,
        InterruptFrequency: 50,
        RetraceTicks: 48,
        FloatingBusStartTicks: 14338,
        InterruptDuration: 32,
        FirstPixelTicks: 14336,
        LastPixelTicks: 14336 + (ScreenSize.ContentHeight - 1) * 224 + ContentLineTicks,
        TicksPerLine: 224);
}