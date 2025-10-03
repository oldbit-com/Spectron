namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal enum ControllerState
{
    Idle,
    Wait,
    CommandType1,
    DelayBeforeCommand,
    CommandReadWrite,
    FOUND_NEXT_ID,
    Read,
    ReadSector,
    Write,
    WriteSector,
    WriteTrack,
    WriteTrackData,
    Step,
    SeekStart,
    Seek,
    Verify,
    Reset
}