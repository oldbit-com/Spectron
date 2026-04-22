using System.Runtime.InteropServices;
using System.Security.Cryptography;
using OldBit.Spectron.Emulation.Screen;

namespace OldBit.Spectron.Integration.Tests.Fixtures;

public static class FrameBufferExtensions
{
    public static string CalculateHash(this FrameBuffer frameBuffer)
    {
        var pixels = frameBuffer.Pixels.Select(x => x.Abgr).ToArray();
        var bytes = MemoryMarshal.AsBytes(pixels);
        var hash =  SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}