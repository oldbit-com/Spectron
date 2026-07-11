using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Tzx.Blocks;

namespace OldBit.Spectron.Files.Tzx;

/// <summary>
/// Reads a block data from a stream.
/// </summary>
internal sealed class BlockReader
{
    private readonly ByteStreamReader _reader;
    private int _blockCounter;

    /// <summary>
    /// Creates a new instance of the block reader.
    /// </summary>
    /// <param name="stream">The stream to read data from.</param>
    internal BlockReader(Stream stream)
    {
        _reader = new ByteStreamReader(stream);
    }

    /// <summary>
    /// Reads the next block from the stream.
    /// </summary>
    /// <returns>An instance of the concrete block or null if there is no more data.</returns>
    /// <exception cref="ArgumentException">Thrown if block is not recognized.</exception>
    internal IBlock? ReadNextBlock()
    {
        _blockCounter += 1;
        if (_blockCounter == 1)
        {
            return new HeaderBlock(_reader);
        }

        // Try to read ID of the next block
        if (!_reader.TryReadByte(out var id))
        {
            return null;
        }

        return id switch
        {
            BlockCode.StandardSpeedData => new StandardSpeedDataBlock(_reader),
            BlockCode.TurboSpeedData => new TurboSpeedDataBlock(_reader),
            BlockCode.PureTone => new PureToneBlock(_reader),
            BlockCode.PulseSequence => new PulseSequenceBlock(_reader),
            BlockCode.PureData => new PureDataBlock(_reader),
            BlockCode.DirectRecording => new DirectRecordingBlock(_reader),
            BlockCode.CswRecording => new CswRecordingBlock(_reader),
            BlockCode.GeneralizedData => new GeneralizedDataBlock(_reader),
            BlockCode.Pause => new PauseBlock(_reader),
            BlockCode.GroupStart => new GroupStartBlock(_reader),
            BlockCode.GroupEnd => new GroupEndBlock(),
            BlockCode.JumpToBlock => new JumpToBlock(_reader),
            BlockCode.LoopStart => new LoopStartBlock(_reader),
            BlockCode.LoopEnd => new LoopEndBlock(),
            BlockCode.CallSequence => new CallSequenceBlock(_reader),
            BlockCode.ReturnFromSequence => new ReturnFromSequenceBlock(),
            BlockCode.Select => new SelectBlock(_reader),
            BlockCode.StopTheTape48 => new StopTheTape48Block(_reader),
            BlockCode.SetSignalLevel => new SetSignalLevelBlock(_reader),
            BlockCode.TextDescription => new TextDescriptionBlock(_reader),
            BlockCode.Message => new MessageBlock(_reader),
            BlockCode.ArchiveInfo => new ArchiveInfoBlock(_reader),
            BlockCode.HardwareType => new HardwareTypeBlock(_reader),
            BlockCode.CustomInfo => new CustomInfoBlock(_reader),
            BlockCode.Glue => new GlueBlock(_reader),
            BlockCode.C64RomTypeData => new C64RomTypeDataBlock(_reader),
            BlockCode.KansasCityStandard => new KansasCityStandardBlock(_reader),
            _ => throw new ArgumentException($"Error reading TZX file. Unrecognized block id={id}.")
        };
    }
}