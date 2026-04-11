using OldBit.Spectron.Emulation.Tape;

namespace OldBit.Spectron.Emulation.Devices.Audio.Beeper;

internal class BeeperDevice : IDevice
{
    private readonly CassettePlayer? _cassettePlayer;
    private readonly Func<Word, bool> _isUlaPort;

    internal bool IsEnabled { get; set; }

    internal Action<byte> OnUpdateBeeper { get; init; } = _ => { };

    internal BeeperDevice(CassettePlayer? cassettePlayer, Func<Word, bool> isUlaPort)
    {
        _cassettePlayer = cassettePlayer;
        _isUlaPort = isUlaPort;
    }

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled || !_isUlaPort(address))
        {
            return;
        }

        if (_cassettePlayer?.IsPlaying == true)
        {
            value = _cassettePlayer.EarBit ? (byte)(value | 0x10) : (byte)(value & 0xEF);
        }

        OnUpdateBeeper(value);
    }
}