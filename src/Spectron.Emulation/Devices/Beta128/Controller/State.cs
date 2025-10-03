namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal enum State
{
    Idle,
    Wait,
    DelayBeforeCommand,
    CommandReadWrite,
    FOUND_NEXT_ID,
    ReadSector,
    Read,
    WriteSector,
    Write,
    WriteTrack,
    WriteTrackData,
    CommandType1,
    Step,
    SeekStart,
    Seek,
    Verify,
    Reset
}