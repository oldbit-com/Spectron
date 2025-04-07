namespace OldBit.Spectron.Emulation.Devices.DivMmc.RTC;

internal sealed class RtcDevice(DivMmcDevice divMmc) : IDevice
{
    private const byte RtcYear = 0x01;
    private const byte RtcMonth = 0x02;
    private const byte RtcDay = 0x03;
    private const byte RtcHour = 0x04;
    private const byte RtcMinute = 0x05;
    private const byte RtcSecond = 0x06;

    private byte _register;

    public void WritePort(Word address, byte value)
    {
        if (!divMmc.IsEnabled || address != 0x3B11)
        {
            return;
        }

        _register = value;
    }

    public byte? ReadPort(Word address)
    {
        if (!divMmc.IsEnabled || address != 0x3B11)
        {
            return null;
        }

        return _register switch
        {
            RtcYear => (byte)(DateTime.Now.Year - 1980),
            RtcMonth => (byte)DateTime.Now.Month,
            RtcDay => (byte)DateTime.Now.Day,
            RtcHour => (byte)DateTime.Now.Hour,
            RtcMinute => (byte)DateTime.Now.Minute,
            RtcSecond => (byte)(DateTime.Now.Second / 2),
            _ => null
        };
    }
}