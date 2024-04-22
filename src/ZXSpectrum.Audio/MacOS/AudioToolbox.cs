using System.Runtime.InteropServices;

namespace ZXSpectrum.Audio.MacOS;

using OSStatus = int;

/// <summary>
/// Native methods imported from AudioToolbox MacOS library.
/// </summary>
internal static partial class AudioToolbox
{
    private const string AudioToolboxLibrary = "/System/Library/Frameworks/AudioToolbox.framework/AudioToolbox";

    [LibraryImport(AudioToolboxLibrary)]
    internal static unsafe partial OSStatus AudioQueueNewOutput(ref AudioStreamBasicDescription format,
        delegate* unmanaged<IntPtr, IntPtr, IntPtr, void> callback, IntPtr userData, IntPtr callbackRunLoop, IntPtr callbackRunLoopMode,
        uint flags, out IntPtr audioQueue);

    [LibraryImport(AudioToolboxLibrary)]
    internal static partial OSStatus AudioQueueDispose(IntPtr audioQueue, [MarshalAs(UnmanagedType.Bool)] bool immediate);

    [LibraryImport(AudioToolboxLibrary)]
    internal static unsafe partial OSStatus AudioQueueFreeBuffer(IntPtr audioQueue, IntPtr audioQueueBuffer);

    [LibraryImport(AudioToolboxLibrary)]
    internal static unsafe partial OSStatus AudioQueueStart(IntPtr audioQueue, AudioTimeStamp* startTime);

    [LibraryImport(AudioToolboxLibrary)]
    internal static unsafe partial OSStatus AudioQueueStop(IntPtr audioQueue, [MarshalAs(UnmanagedType.Bool)] bool immediate);

    [LibraryImport(AudioToolboxLibrary)]
    internal static unsafe partial OSStatus AudioQueueAllocateBuffer(IntPtr audioQueue, uint bufferSize, out IntPtr audioQueueBuffer);

    [LibraryImport(AudioToolboxLibrary)]
    internal static unsafe partial OSStatus AudioQueueEnqueueBuffer(IntPtr audioQueue, AudioQueueBuffer* audioQueueBuffer, int packets,
        AudioStreamPacketDescription* packetDescriptions);
}
