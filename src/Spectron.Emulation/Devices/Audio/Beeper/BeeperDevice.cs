using OldBit.Spectron.Emulation.Tape;

namespace OldBit.Spectron.Emulation.Devices.Audio.Beeper;

internal class BeeperDevice : IDevice
{
    private readonly CassettePlayer? _tapePlayer;

    internal Action<byte> BeeperUpdated { get; init; } = _ => { };

    internal BeeperDevice(CassettePlayer? tapePlayer) => _tapePlayer = tapePlayer;

    public void WritePort(Word address, byte value)
    {
        if (!Ula.IsUlaPort(address))
        {
            return;
        }

        if (_tapePlayer?.IsPlaying == true)
        {
            value = _tapePlayer.EarBit ? (byte)(value | 0x10) : (byte)(value & 0xEF);
        }

        BeeperUpdated(value);
    }
}