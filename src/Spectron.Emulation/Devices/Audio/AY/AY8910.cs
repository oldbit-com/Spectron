using OldBit.Z80Cpu;
using static OldBit.Spectron.Emulation.Devices.Audio.AY.Registers;

namespace OldBit.Spectron.Emulation.Devices.Audio.AY;

/// <summary>
/// AY audion chip emulation.
/// Implementation based on JSpeccy code, e.g. copied the logic how AY audio is generated.
/// https://github.com/jsanchezv/JSpeccy/blob/master/src/main/java/machine/AY8912.java
/// </summary>
internal class AY8910(Clock clock, double statesPerSample) : IDevice
{
    private const Word RegisterPort = 0xFFFD;
    private const Word DataPort = 0xBFFD;

    private const int MaxAmplitude = 10900;

    // https://groups.google.com/g/comp.sys.sinclair/c/-zCR2kxMryY
    private static readonly double[] SignalLevels = [
        0.0000, 0.0137, 0.0205, 0.0291, 0.0423, 0.0618, 0.0847, 0.1369,
        0.1691, 0.2647, 0.3527, 0.4499, 0.5704, 0.6873, 0.8482, 1.0000];

    private readonly int[] _volumeLevels = SignalLevels.Select(x => (int)(x * MaxAmplitude)).ToArray();

    private readonly Channel _channelA = new();
    private readonly Channel _channelB = new();
    private readonly Channel _channelC = new();
    private readonly Envelope _envelope = new();
    private readonly Noise _noise = new();

    private int _audioTicks;

    private int _selectedRegister;
    private readonly byte[] _registers = new byte[16];

    internal bool IsEnabled { get; set; }

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled)
        {
            return;
        }

        if (IsSelectRegisterPort(address))
        {
            _selectedRegister = value & 0x0F;
        }
        else if (IsDataPort(address))
        {
            Update(clock.FrameTicks);

            SetRegister(_selectedRegister, value);

            switch (_selectedRegister)
            {
                case FineTuneA:
                case CoarseTuneA:
                    _channelA.SetPeriod(_registers[FineTuneA], _registers[CoarseTuneA]);
                    break;

                case FineTuneB:
                case CoarseTuneB:
                    _channelB.SetPeriod(_registers[FineTuneB], _registers[CoarseTuneB]);
                    break;

                case FineTuneC:
                case CoarseTuneC:
                    _channelC.SetPeriod(_registers[FineTuneC], _registers[CoarseTuneC]);
                    break;

                case NoisePeriod:
                    _noise.SetPeriod(value);
                    break;

                case Mixer:
                    _channelA.IsToneDisabled = (value & 0x01) != 0;
                    _channelB.IsToneDisabled = (value & 0x02) != 0;
                    _channelC.IsToneDisabled = (value & 0x04) != 0;

                    _channelA.IsNoiseDisabled = (value & 0x08) != 0;
                    _channelB.IsNoiseDisabled = (value & 0x10) != 0;
                    _channelC.IsNoiseDisabled = (value & 0x20) != 0;
                    break;

                case AmplitudeA:
                    (_channelA.AmplitudeMode, _channelA.Amplitude) = GetAmplitude(value);
                    break;

                case AmplitudeB:
                    (_channelB.AmplitudeMode, _channelB.Amplitude) = GetAmplitude(value);
                    break;

                case AmplitudeC:
                    (_channelC.AmplitudeMode, _channelC.Amplitude) = GetAmplitude(value);
                    break;

                case FineTuneEnvelope:
                case CoarseTuneEnvelope:
                    _envelope.SetPeriod(_registers[FineTuneEnvelope], _registers[CoarseTuneEnvelope]);
                    break;

                case EnvelopeShape:
                    _envelope.SetShape(value);

                    if (_channelA.AmplitudeMode == AmplitudeMode.VariableLevel)
                    {
                       _channelA.Amplitude = _volumeLevels[_envelope.Amplitude];
                    }

                    if (_channelB.AmplitudeMode == AmplitudeMode.VariableLevel)
                    {
                        _channelB.Amplitude = _volumeLevels[_envelope.Amplitude];
                    }

                    if (_channelC.AmplitudeMode == AmplitudeMode.VariableLevel)
                    {
                        _channelC.Amplitude = _volumeLevels[_envelope.Amplitude];
                    }

                    break;
            }
        }
    }

    public byte? ReadPort(Word address)
    {
        if (!IsEnabled)
        {
            return null;
        }

        if (IsSelectRegisterPort(address))
        {

        }
        if (address != RegisterPort)
        {
            return null;
        }
        return null;
    }

    internal void Reset()
    {
        _selectedRegister = 0;

        _channelA.Reset();
        _channelB.Reset();
        _channelC.Reset();
        _noise.Reset();
        _envelope.Reset();
    }

    internal void EndFrame()
    {
        Update(clock.FrameTicks);

        _audioTicks -= clock.FrameTicks;
    }

    private void Update(int ticks)
    {
        while (_audioTicks < ticks)
        {
            _audioTicks += 16;

            _channelA.Update();
            _channelB.Update();
            _channelC.Update();
            _noise.Update();
            _envelope.Update();

            if (_channelA.AmplitudeMode == AmplitudeMode.VariableLevel)
            {
                _channelA.Amplitude = _volumeLevels[_envelope.Amplitude];
            }

            if (_channelB.AmplitudeMode == AmplitudeMode.VariableLevel)
            {
                _channelB.Amplitude = _volumeLevels[_envelope.Amplitude];
            }

            if (_channelC.AmplitudeMode == AmplitudeMode.VariableLevel)
            {
                _channelC.Amplitude = _volumeLevels[_envelope.Amplitude];
            }
        }
    }

    private (AmplitudeMode Mode, int Amplitude) GetAmplitude(int value)
    {
        var mode = (AmplitudeMode)((value >> 4) & 0x01);
        var amplitude = mode switch
        {
            AmplitudeMode.FixedLevel => _volumeLevels[value & 0x0F],
            _ => _volumeLevels[_envelope.Amplitude],
        };

        return (mode, amplitude);
    }

    private void SetRegister(int register, byte value) => _registers[register] = value;

    // Register port 0xFFFD is decoded as: A15=1,A14=1 & A1=0
    private static bool IsSelectRegisterPort(Word address) => (address & 0xC002) == 0xC000;

    // Data port 0xBFFD is decoded as: A15=1 & A1=0
    private static bool IsDataPort(Word address) => (address & 0x8002) == 0x8000;
}