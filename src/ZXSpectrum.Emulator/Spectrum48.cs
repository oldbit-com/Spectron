using System.Reflection;
using OldBit.ZXSpectrum.Emulator.Hardware;
using OldBit.ZXSpectrum.Emulator.Hardware.Audio;
using OldBit.ZXSpectrum.Emulator.Screen;

namespace OldBit.ZXSpectrum.Emulator;

public class Spectrum48 : ISpectrum
{
    private const float ClockMHz = 3.5f;
    private readonly Border _border = new();
    private readonly ScreenRenderer _screenRenderer;
    private readonly Memory48 _memory;
    private readonly Beeper _beeper;
    private readonly Z80.Net.Z80 _z80;
    private Timer? _timer;
    private readonly object _timerLock = new();

    public Spectrum48()
    {
        _screenRenderer = new ScreenRenderer(_border);
        var rom = ReadRom();
        _memory = new Memory48(rom);

        _z80 = new Z80.Net.Z80(_memory);
        _beeper = new Beeper(ClockMHz);
        var bus = new Bus(Keyboard, _beeper, _border, _z80.Cycles);
        _z80.AddBus(bus);
        _z80.Trap = Trap;
    }

    private void Trap()
    {
        if (_z80.Registers.PC == RomRoutines.LoadBytes)
        {

        }
    }

    private static byte[] ReadRom()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("OldBit.ZXSpectrum.Emulator.Rom.48.rom")!;
        using var reader = new BinaryReader(stream);

        return reader.ReadBytes((int)stream.Length);
    }

    private void InterruptHandler(object? data)
    {
        lock (_timerLock)
        {
            _z80.Run(69888);
            _z80.Int(0xFF);

            var pixels = _screenRenderer.Render(_memory.Screen);
            OnScreenUpdate(pixels);
        }
    }

    public void Start()
    {
        const double frequency = 3.5 * 1000000 / 69888;
        var period = TimeSpan.FromMicroseconds(1 / frequency * 1000000);
       _timer = new Timer(InterruptHandler, null, TimeSpan.FromSeconds(2), period);
    }

    public void Stop()
    {
        _beeper.Stop();
        _timer?.Dispose();
    }

    public void LoadFile(string fileName)
    {
        throw new NotImplementedException();
    }

    public Action<byte[]> OnScreenUpdate { get; init; } = _ => { };

    public Keyboard Keyboard { get; } = new();
}