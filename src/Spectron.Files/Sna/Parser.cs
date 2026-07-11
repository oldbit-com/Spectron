using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Sna;

internal static class Parser
{
    internal static SnaFile Parse(Stream stream)
    {
        var reader = new ByteStreamReader(stream);
        var snapshot = new SnaFile
        {
            Header = new SnaHeader(reader),
            Ram48 = reader.ReadBytes(0xC000)
        };

        var sna128HeaderData = new byte[4];
        var readCount = reader.ReadAtLeast(sna128HeaderData, sna128HeaderData.Length);

        if (readCount == 0)
        {
            return snapshot;    // No 128K data available, treat as 48K SNA file
        }

        if (readCount != sna128HeaderData.Length)
        {
            throw new EndOfStreamException("Not enough data to read the 128K SNA file format.");
        }

        snapshot.Header128 = new SnaHeader128
        {
            PC = (Word)(sna128HeaderData[0] | (sna128HeaderData[1] << 8)),
            PageMode = sna128HeaderData[2],
            TrDosRom = sna128HeaderData[3]
        };

        snapshot.RamBanks = new List<byte[]>();
        for (var bank = 0; bank < 8; bank++)
        {
            if (bank == 2 || bank == 5 || bank == (snapshot.Header128.PageMode & 0x07))
            {
                continue;   // These banks are included in the 48K SNA file format, skip them
            }

            var bankData = reader.ReadBytes(0x4000);
            snapshot.RamBanks.Add(bankData);
        }

        return snapshot;
    }
}