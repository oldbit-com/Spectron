namespace OldBit.Spectron.Emulation.Devices.Interface1.Microdrive;

internal sealed class Block(byte[] data, bool isPreambleValid = false)
{
    private const int PreambleLength = 12;

    private int _preamblePosition;
    private int _preambleCounter;

    public byte[] Data { get; } = data;
    public bool IsPreambleValid { get; private set; } = isPreambleValid;

    internal bool ProcessPreamble(byte value)
    {
        // Check preamble; it consists of 12 bytes: 10×00 + 2×FF and
        // it is not saved to MDR, we just check if a sequence is valid
        switch (_preamblePosition)
        {
            case 0 when value == 0x00:
                IsPreambleValid = false;
                _preambleCounter = 1;
                break;

            case > 0 and < 10 when value == 0x00:
            case 10 when value == 0xFF:
                _preambleCounter += 1;
                break;

            case 11 when value == 0xFF && _preambleCounter == 11:
                _preambleCounter += 1;
                IsPreambleValid = true;
                break;
        }

        _preamblePosition += 1;

        return _preamblePosition <= PreambleLength;
    }

    public void Synchronize() => _preamblePosition = 0;
}