namespace OldBit.Spectron.Debugger.Extensions;

public static class ByteArrayExtensions
{
    public static List<byte[]> ToChunks(this byte[] byteArray, int chunkSize)
    {
        var chunks = new List<byte[]>();
        var arrayLength = byteArray.Length;
        var numChunks = (int)Math.Ceiling((double)arrayLength / chunkSize);

        for (var i = 0; i < numChunks; i++)
        {
            var startIdx = i * chunkSize;
            var endIdx = Math.Min(startIdx + chunkSize, arrayLength);
            var chunkLength = endIdx - startIdx;

            var chunk = new byte[chunkLength];
            Array.Copy(byteArray, startIdx, chunk, 0, chunkLength);
            chunks.Add(chunk);
        }

        return chunks;
    }
}