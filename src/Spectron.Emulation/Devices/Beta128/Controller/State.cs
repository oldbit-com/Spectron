namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal enum State
{
    Idle,
    Wait,
    DelayBeforeCommand,
    CMD_RW,
    FOUND_NEXT_ID,
    ReadSector,
    Read,
    WriteSector,
    Write,
    WriteTrack,
    WriteTrackData,
    CommandType1,
    Step,
    SEEKSTART,
    Seek,
    Verify,
    Reset
}