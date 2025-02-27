using MemoryPack;

namespace OldBit.Spectron.Emulation.State.Components;

[MemoryPackable]
public sealed partial record MemoryState
{
    public byte PagingMode { get; set; }

    public byte[][] Banks { get; set; } = new byte[8][];

    public void SetBank(byte[] memory, int pageNumber) => Banks[pageNumber] = memory;

    internal MemoryState()
    {
        for (var i = 0; i < 8; i++)
        {
            Banks[i] = [];
        }
    }
}