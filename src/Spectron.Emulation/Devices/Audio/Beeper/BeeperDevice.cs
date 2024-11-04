using OldBit.Spectron.Emulation.Tape;

namespace OldBit.Spectron.Emulation.Devices.Audio.Beeper;

internal class BeeperDevice : IDevice
{
    private readonly CassettePlayer? _cassettePlayer;

    internal bool IsEnabled { get; set; }

    internal Action<byte> OnUpdateBeeper { get; init; } = _ => { };

    internal BeeperDevice(CassettePlayer? cassettePlayer) => _cassettePlayer = cassettePlayer;

    public void WritePort(Word address, byte value)
    {
        if (!IsEnabled || !Ula.IsUlaPort(address))
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