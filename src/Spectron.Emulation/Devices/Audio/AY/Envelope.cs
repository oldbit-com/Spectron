namespace OldBit.Spectron.Emulation.Devices.Audio.AY;

/// <summary>
///     \_________    00xx
///     /|________    01xx
///     \|\|\|\|\|    1000
///     \_________    1001
///     \|‾‾‾‾‾‾‾‾    1011
///     /|/|/|/|/|    1100
///     /‾‾‾‾‾‾‾‾     1101
///     /\/\/\/\/\    1110
///     /|________    1111
/// </summary>
internal sealed class Envelope
{
    private int _counter;
    private bool _continue;
    private bool _attack;

    internal int Period { get; private set; }
    internal int Amplitude { get; private set; }

    internal bool IsHoldBitSet { get; private set; }
    internal bool IsAlternateBitSet { get; private set; }
    internal bool IsAttackBitSet { get; private set; }
    internal bool IsContinueBitSet { get; private set; }

    internal void SetPeriod(byte fineTune, byte coarseTune) =>
        Period = (coarseTune << 8) | fineTune;

    internal void SetShape(byte shape)
    {
        IsHoldBitSet = (shape & 0x01) != 0;
        IsAlternateBitSet = (shape & 0x02) != 0;
        IsAttackBitSet = (shape & 0x04) != 0;
        IsContinueBitSet = (shape & 0x08) != 0;

        _counter = 0;
        _attack = IsAttackBitSet;
        _continue = true;
    }

    internal void Update()
    {
        _counter += 1;

        if (_continue && _counter >= Period)
        {
            _counter = 0;

            Amplitude = _attack ? Amplitude + 1 : Amplitude - 1;

            if ((Amplitude & 0x10) != 0)
            {
                if (IsContinueBitSet)
                {
                    if (IsAlternateBitSet)
                    {
                        _attack = !_attack;
                    }

                    if (IsHoldBitSet)
                    {
                        Amplitude = _attack ? 15 : 0;
                        _continue = false;
                    }
                    else
                    {
                        Amplitude = _attack ? 0 : 15;
                    }
                }
                else
                {
                    Amplitude = 0;
                    _continue = false;
                }
            }
        }
    }

    internal void Reset()
    {
        _continue = false;
        IsAttackBitSet = false;
    }
}

