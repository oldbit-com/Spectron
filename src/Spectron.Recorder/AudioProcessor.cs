using System.IO.Compression;
using OldBit.Spectron.Emulation.Devices.Audio;

namespace OldBit.Spectron.Recorder;

internal class AudioProcessor(StereoMode stereoMode, string outputFilePath, string rawRecordingFilePath)
{
    internal void Process()
    {
        var rawRecodingFile = File.OpenRead(rawRecordingFilePath);
        using var rawFileStream = new GZipStream(rawRecodingFile, CompressionMode.Decompress);

        using var writer = new WaveFileWriter(
            outputFilePath,
            AudioManager.PlayerSampleRate,
            stereoMode == StereoMode.None ? 1 : 2);

        var buffer = new byte[16384];

        while (true)
        {
            var count = rawFileStream.ReadAtLeast(buffer, buffer.Length, throwOnEndOfStream: false);

            if (count < 2)
            {
                break;
            }

            writer.Write(buffer.Take(count));
        }

        writer.UpdateHeader();

        writer.Close();
    }
}