namespace OldBit.Spectron.Emulation.Devices.Beta128.Drive;

internal sealed class FloppyDiskOld
{
    private const int NumberOfSectors = 16;
    private const int NumberOfBytesPerSector = 256;

    private readonly int _tracksCount;
    private readonly int _sidesCount;

    private readonly byte[][][][] _tracks;  // 40|80 tracks; each track 1|2 sides, each track 16 sectors, each sector 256 bytes

    internal int TracksCount => _tracksCount;

    internal FloppyDiskOld(byte[] data)
    {
        var diskType = data[0x8E3];

        switch (diskType)
        {
            case DiskType.EightyTracksDoubleSided:
                _tracksCount = 80;
                _sidesCount = 2;
                break;

            case DiskType.EightyTracksSingleSided:
                _tracksCount = 80;
                _sidesCount = 1;
                break;

            case DiskType.FortyTracksDoubleSided:
                _tracksCount = 40;
                _sidesCount = 2;
                break;

            case DiskType.FortyTracksSingleSided:
                _tracksCount = 40;
                _sidesCount = 1;
                break;

            default:
                throw new ArgumentException($"Unknown TRD disk type: 0x{diskType:x2}.");
        }

        _tracks = new byte[_tracksCount][][][];

        for (var track = 0; track < _tracksCount; track++)
        {
            _tracks[track] = [];
        }

        ParseData(data);
    }


    internal byte GetData(int track, int side, int sector, int offset) =>
        _tracks[track][side][sector][offset];

    private void ParseData(byte[] data)
    {
        var track = 0;
        var side = 0;
        var sector = 0;

        foreach (var chunk in data.Chunk(256))
        {
            if (chunk.Length < 256)
            {
                break;
            }

            InitializeTrack(_tracks, track);

            _tracks[track][side][sector] = chunk;
            sector += 1;

            if (sector < NumberOfSectors)
            {
                continue;
            }

            sector = 0;
            side += 1;

            if (side < _sidesCount)
            {
                continue;
            }

            side = 0;
            track += 1;

            if (track >= _tracksCount)
            {
                break;
            }
        }
    }

    private void InitializeTrack(byte[][][][] tracks, int track)
    {
        if (tracks[track].Length != 0)
        {
            return;
        }

        // Add 1 or 2 sides to the track
        tracks[track] = new byte[_sidesCount][][];

        // Add 16 sectors to all sides
        for (var side = 0; side < _sidesCount; side++)
        {
            tracks[track][side] = new byte[NumberOfSectors][];

            // Add 256 bytes of data to each sector
            for (var sector = 0; sector < NumberOfSectors; sector++)
            {
                tracks[track][side][sector] = new byte[NumberOfBytesPerSector];
            }
        }
    }
}