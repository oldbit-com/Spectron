using System.Diagnostics;
using OldBit.Spectral.Emulator.Hardware;
using OldBit.Spectral.Emulator.Hardware.Audio;
using OldBit.Spectral.Emulator.Rom;
using OldBit.Spectral.Emulator.Screen;
using OldBit.Spectral.Emulator.Tape;
using OldBit.Z80Cpu;

namespace OldBit.Spectral.Emulator.Computers;

public class Spectrum48K : IEmulator
{
    private const float ClockMHz = 3.5f;
    private const float InterruptFrequencyHz = ClockMHz * 1000000 / DefaultTimings.FrameTicks;

    private readonly ScreenRenderer _screenRenderer;
    private readonly Memory48K _memory;
    private readonly Beeper _beeper;
    private readonly Z80 _z80;
    private readonly TapeLoader _tapeLoader;
    private readonly TapePlayer _tapePlayer;
    private readonly Stopwatch _stopwatch = new();

    private Timer? _timer;
    private readonly object _timerLock = new();
    private bool _isPausedRequested;

    public Keyboard Keyboard { get; } = new();
    public bool IsPaused { get; private set; }

    public delegate void RenderScreenEvent(FrameBuffer frameBuffer);
    public event RenderScreenEvent? RenderScreen;

    public Spectrum48K(RomType romType)
    {
        var rom = RomReader.ReadRom(romType);
        _memory = new Memory48K(rom);
        _memory.ScreenMemoryUpdated += ScreenMemoryUpdated;

        _screenRenderer = new ScreenRenderer(_memory);
        _beeper = new Beeper(ClockMHz);

        var contendedMemory = new ContendedMemory();
        _z80 = new Z80(_memory, contendedMemory);

        _z80.BeforeFetch += BeforeInstructionFetch;
        _z80.Clock.TicksAdded += ClockTicksAdded;

        _tapePlayer = new TapePlayer(_z80.Clock);

        var ula = new Ula(_memory, Keyboard, _beeper, _screenRenderer, _z80.Clock, _tapePlayer);
        var bus = new Bus();

        bus.AddInputDevice(ula);
        bus.AddOutputDevice(ula);

        _z80.AddBus(bus);

        _tapeLoader = new TapeLoader(_z80, _memory, _screenRenderer, _tapePlayer);
    }

    private void BeforeInstructionFetch(Word pc)
    {
        if (pc == RomRoutines.LoadBytes)
        {
            if (!_tapePlayer.IsPlaying)
            {
                _tapePlayer.Start();
            }
        }
    }

    private void ClockTicksAdded(int addedTicks, int previousFrameTicks, int currentFrameTicks) => _screenRenderer.UpdateContent(currentFrameTicks);

    private void ProcessInterrupt(object? data)
    {
        lock (_timerLock)
        {
            if (_isPausedRequested)
            {
                IsPaused = true;
                _isPausedRequested = false;
            }

            if (!IsPaused)
            {
                RunFrame();
            }

            RenderScreen?.Invoke(_screenRenderer.FrameBuffer);
        }
    }

    private void RunFrame()
    {
        StartFrame();

        _z80.Run(DefaultTimings.FrameTicks);

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

        _z80.INT(0xFF);
    }

    public void Start()
    {
        var timerPeriod = TimeSpan.FromMicroseconds(1 / InterruptFrequencyHz * 1000000);
       _timer = new Timer(ProcessInterrupt, null, TimeSpan.FromSeconds(2), timerPeriod);

       _stopwatch.Start();
    }

    public void Stop()
    {
        _beeper.Stop();
        _timer?.Dispose();
    }

    public void Pause()
    {
        _isPausedRequested = true;
        while (!IsPaused) { }
    }

    public void Resume()
    {
        IsPaused = false;
    }

    public void Reset()
    {
        _z80.Reset();
        _screenRenderer.Reset();
    }

    public void LoadFile(string fileName)
    {
        _tapeLoader.LoadFile(fileName);
    }

    private void ScreenMemoryUpdated(Word address) => _screenRenderer.ScreenMemoryUpdated(address);
}