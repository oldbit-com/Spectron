using System.Runtime.InteropServices;

namespace ZXSpectrum.Audio.MacOS;

[StructLayout(LayoutKind.Sequential)]
internal struct AudioStreamBasicDescription
{
    internal double SampleRate;
    internal AudioFormatType Format;
    internal AudioFormatFlags FormatFlags;
    internal uint BytesPerPacket;
    internal uint FramesPerPacket;
    internal uint BytesPerFrame;
    internal uint ChannelsPerFrame;
    internal uint BitsPerChannel;
    internal uint Reserved;
}

internal enum AudioFormatType : uint
{
    LinearPCM = 0x6c70636d,
}

[Flags]
internal enum AudioFormatFlags : uint
{
    None = 0,
    AudioFormatFlagIsFloat = 1,
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
internal unsafe delegate void AudioQueueOutputCallback(IntPtr userData, IntPtr audioQueue, AudioQueueBuffer* buffer);

[StructLayout(LayoutKind.Sequential)]
internal struct AudioTimeStamp
{
    public double SampleTime;
    public uint HostTime;
    public double RateScalar;
    public uint WordClockTime;
    public uint SMPTETime;
    public uint Flags;
    public uint Reserved;
}

[StructLayout(LayoutKind.Sequential)]
internal struct AudioQueueBuffer
{
    public uint AudioDataBytesCapacity;
    public IntPtr AudioData;
    public uint AudioDataByteSize;
    public IntPtr UserData;
    public uint PacketDescriptionCapacity;
    public IntPtr PacketDescriptions;
    public uint PacketDescriptionCount;
}

[StructLayout(LayoutKind.Sequential)]
internal struct AudioStreamPacketDescription
{
    public long StartOffset;
    public uint VariableFramesInPacket;
    public uint DataByteSize;
}