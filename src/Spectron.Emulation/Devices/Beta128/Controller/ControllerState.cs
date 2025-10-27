namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

internal enum ControllerState
{
    Idle,
    Wait,
    Type1Command,
    DelayBeforeCommand,
    ReadWriteCommand,
    FoundNextId,
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