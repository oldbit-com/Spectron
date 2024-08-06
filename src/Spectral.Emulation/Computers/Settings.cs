namespace OldBit.Spectral.Emulation.Computers;

internal record ComputerSettings(
    float ClockMhz,
    int TicksPerScanLine,
    int TicksPerFrame);

internal static class Settings
{
    internal static Dictionary<Computer, ComputerSettings> Computers { get; } = new()
    {
        [Computer.Spectrum16K] = new ComputerSettings(
            ClockMhz: 3.5f,
            TicksPerScanLine: 224,
            TicksPerFrame: 69888),

        [Computer.Spectrum48K] = new ComputerSettings(
            ClockMhz: 3.5f,
            TicksPerScanLine: 224,
            TicksPerFrame: 69888),

        [Computer.Spectrum128K] = new ComputerSettings(
            ClockMhz: 3.5469f,
            TicksPerScanLine: 228,
            TicksPerFrame: 70908),

        [Computer.Timex2048] = new ComputerSettings(
            ClockMhz: 3.5f,
            TicksPerScanLine: 224,
            TicksPerFrame: 69888),
    };
}