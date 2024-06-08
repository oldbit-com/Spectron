using OldBit.Z80Cpu;
using OldBit.ZXSpectrum.Emulator.Hardware;
using OldBit.ZXSpectrum.Emulator.Hardware.Audio;
using OldBit.ZXSpectrum.Emulator.Screen;
using OldBit.ZXSpectrum.Emulator.Tape;

namespace OldBit.ZXSpectrum.Emulator.Computers;

public class Spectrum48 : ISpectrum
{
    private const float ClockMHz = 3.5f;
    private const int CyclesPerFrame = (64 + 192 + 56) * 224; // 69888
    private const float InterruptFrequencyHz = ClockMHz * 1000000 / CyclesPerFrame;

    private readonly Border _border = new();
    private readonly ScreenRenderer _screenRenderer;
    private readonly Memory48 _memory;
    private readonly Beeper _beeper;
    private readonly Z80 _z80;
    private readonly TapeLoader _tapeLoader;

    private Timer? _timer;
    private readonly object _timerLock = new();
    private bool _isPaused;
    private bool _isPausedRequested;

    public Spectrum48()
    {
        _screenRenderer = new ScreenRenderer(_border);
        _memory = new Memory48();
        _beeper = new Beeper(ClockMHz);

        _z80 = new Z80(_memory)
        {
            Trap = Trap
        };

        var bus = new Bus(Keyboard, _beeper, _border, _z80.Cycles);
        _z80.AddBus(bus);

        _tapeLoader = new TapeLoader(_z80, _memory);
    }

    private void Trap()
    {
        if (_z80.Registers.PC == RomRoutines.LoadBytes)
        {

        }
    }

    private void InterruptHandler(object? data)
    {
        lock (_timerLock)
        {
            if (_isPausedRequested)
            {
                _isPaused = true;
                _isPausedRequested = false;
            }

            if (!_isPaused)
            {
                _z80.Run(CyclesPerFrame);
                _z80.Int(0xFF);
            }

            var pixels = _screenRenderer.Render(_memory.Screen);
            OnScreenUpdate(pixels);
        }
    }

    public void Start()
    {
        var timerPeriod = TimeSpan.FromMicroseconds(1 / InterruptFrequencyHz * 1000000);
       _timer = new Timer(InterruptHandler, null, TimeSpan.FromSeconds(2), timerPeriod);
    }

    public void Stop()
    {
        _beeper.Stop();
        _timer?.Dispose();
    }

    public void Pause()
    {
        _isPausedRequested = true;
        while (!_isPaused) { }
    }

    public void Resume()
    {
        _isPaused = false;
    }

    public void LoadFile(string fileName)
    {

        _tapeLoader.LoadFile(fileName);
    }

    public Action<byte[]> OnScreenUpdate { get; init; } = _ => { };

    public Keyboard Keyboard { get; } = new();
}