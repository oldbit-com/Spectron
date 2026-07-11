using OldBit.Spectron.Files.IO;

namespace OldBit.Spectron.Files.Z80;

internal static class Parser
{
    internal static Z80File Parse(Stream stream)
    {
        var reader = new ByteStreamReader(stream);

        var snapshot = new Z80File(new Z80Header(reader));

        if (snapshot.Header.Version == 1)
        {
            var data = reader.ReadToEnd();

            snapshot.Memory = snapshot.Header.Flags1.IsDataCompressed ?
                DataCompressor.Decompress(data, hasEndMarker: true) :
                data;
        }
        else
        {
            while (true)
            {
                var memoryBlock = MemoryBlock.Load(reader);
                if (memoryBlock == null)
                {
                    break;
                }

                snapshot.MemoryBlocks.Add(memoryBlock);
            }
        }

        return snapshot;
    }
}