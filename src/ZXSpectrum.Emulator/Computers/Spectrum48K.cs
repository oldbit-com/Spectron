using OldBit.Z80Cpu;
using OldBit.ZXSpectrum.Emulator.Hardware;
using OldBit.ZXSpectrum.Emulator.Hardware.Audio;
using OldBit.ZXSpectrum.Emulator.Screen;
using OldBit.ZXSpectrum.Emulator.Tape;

namespace OldBit.ZXSpectrum.Emulator.Computers;

public class Spectrum48K : ISpectrum
{
    private const float ClockMHz = 3.5f;
    private const float InterruptFrequencyHz = ClockMHz * 1000000 / Constants.FrameTicks;

    private readonly ScreenRenderer _screenRenderer;
    private readonly Memory48K _memory;
    private readonly Beeper _beeper;
    private readonly Z80 _z80;
    private readonly TapeLoader _tapeLoader;

    private Timer? _timer;
    private readonly object _timerLock = new();
    private bool _isPaused;
    private bool _isPausedRequested;

    public Action<ScreenBuffer> OnScreenRender { get; init; } = _ => { };
    public Keyboard Keyboard { get; } = new();

    public Spectrum48K()
    {
        _memory = new Memory48K();
        _memory.ScreenMemoryUpdated += ScreenMemoryUpdated;

        _screenRenderer = new ScreenRenderer(_memory);
        _beeper = new Beeper(ClockMHz);

        var contendedMemoryProvider = new ContendedMemoryProvider();
        _z80 = new Z80(_memory, contendedMemoryProvider)
        {
            //Trap = Trap
        };

        var bus = new Bus(Keyboard, _beeper, _screenRenderer, _z80.Clock);
        _z80.AddBus(bus);

        _tapeLoader = new TapeLoader(_z80, _memory);
    }

    private void ProcessInterrupt(object? data)
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
                RunFrame();
            }

            OnScreenRender(_screenRenderer.ScreenBuffer);
        }
    }

    private void RunFrame()
    {
        StartFrame();

        foreach (var ticks in FastLookup.ContentTicks)
        {
            _z80.Run(ticks, RunMode.Incremental);
            _screenRenderer.UpdateContent(_z80.Clock.FrameTicks);
        }

        _z80.Run(Constants.FrameTicks, RunMode.Incremental);

        EndFrame();
    }

    private void StartFrame()
    {
        _z80.Clock.NewFrame();
        _screenRenderer.NewFrame();
    }

    private void EndFrame()
    {
        _screenRenderer.UpdateBorder(_z80.Clock.FrameTicks);
        _z80.Int(0xFF);
    }

    public void Start()
    {
        var timerPeriod = TimeSpan.FromMicroseconds(1 / InterruptFrequencyHz * 1000000);
       _timer = new Timer(ProcessInterrupt, null, TimeSpan.FromSeconds(2), timerPeriod);
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

    private void ScreenMemoryUpdated(ScreenMemoryUpdatedEventArgs e) => _screenRenderer.ScreenMemoryUpdated(e.Address);
}