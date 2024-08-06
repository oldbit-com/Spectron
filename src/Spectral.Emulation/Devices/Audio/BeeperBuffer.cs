using System.Collections;

namespace OldBit.Spectral.Emulation.Devices.Audio;

/// <summary>
/// Represents an infinite circular buffer that provides beeper data.
/// </summary>
/// <param name="capacity">The capacity of the buffer.</param>
/// <param name="defaultValue">The default value to return when buffer is empty.</param>
internal class BeeperBuffer(int capacity, Func<byte> defaultValue) : IEnumerable<byte>
{
    private readonly BeeperDataEnumerator _enumerator = new(capacity, defaultValue);

    public void Write(byte[] data) => _enumerator.Write(data);

    public IEnumerator<byte> GetEnumerator() => _enumerator;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}