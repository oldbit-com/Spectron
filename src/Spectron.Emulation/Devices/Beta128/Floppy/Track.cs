using System.Collections;
using OldBit.Spectron.Emulation.Devices.Beta128.Controller;

namespace OldBit.Spectron.Emulation.Devices.Beta128.Floppy;

internal sealed class Track(int totalSectors)
{
    internal const int MaxLength = 6250;

    internal const byte StartIdDataMarker = 0xF5;
    internal const byte StartIndexMarker = 0xF6;
    internal const byte WriteCrcMarker = 0xF7;

    private readonly Sector[] _sectors = new Sector[totalSectors];
    private readonly BitArray _markers = new(MaxLength);

    internal byte[] Data { get; } = new byte[MaxLength];

    internal Sector this[int sectorNo]
    {
        get
        {
            ValidateSectorIndex(sectorNo);
            return _sectors[sectorNo - 1];
        }
        set
        {
            ValidateSectorIndex(sectorNo);
            _sectors[sectorNo - 1] = value;
        }
    }

    internal void Write(int position, byte value, bool isMarker = false)
    {
        Data[position] = value;
        _markers[position] = isMarker;
    }

    internal void UpdateSectors()
    {
        for (var position = 0; position < _markers.Length - 1; position++)
        {
            if (!IsIdMarkerNext(position))
            {
                continue;
            }

            var idPosition = position + 2;
            var sectorNo = Data[position + 4];

            while (++position < _markers.Length - 1)
            {
                if (!IsDataMarkerNext(position))
                {
                    continue;
                }

                var dataPosition = position + 2;
                this[sectorNo] = new Sector(this, idPosition, dataPosition, FloppyDisk.BytesPerSector);

                break;
            }
        }
    }

    private bool IsIdMarkerNext(int position) =>
        _markers[position] && Data[position] == 0xA1 && Data[position + 1] == AddressMark.Id;

    private bool IsDataMarkerNext(int position) =>
        _markers[position] && Data[position] == 0xA1 && (Data[position + 1] == AddressMark.Normal || Data[position + 1] == AddressMark.Deleted);

    private static void ValidateSectorIndex(int sectorNo)
    {
        if (sectorNo is < 1 or > 16)
        {
            throw new ArgumentException("Sector index must be between 1 and 16");
        }
    }
}