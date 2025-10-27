namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller.Commands;

internal enum CommandType
{
    /// <summary>
    /// No command has been supplied.
    /// </summary>
    None,

    /// <summary>
    /// Restore:     0  0  0  0  h  V r1 r0
    /// Seek:        0  0  0  1  h  V r1 r0
    /// Step:        0  0  1  T  h  V r1 r0
    /// StepIn:      0  1  0  T  h  V r1 r0
    /// StepOut:     0  1  1  T  h  V r1 r0
    /// </summary>
    Type1,

    /// <summary>
    /// ReadSector:  1  0  0  m  S  E  C  0
    /// WriteSector: 1  0  1  m  S  E  C a0
    /// </summary>
    Type2,

    /// <summary>
    /// ReadAddress: 1  1  0  0  0  E  0  0
    /// ReadTrack:   1  1  1  0  0  E  0  0
    /// WriteTrack:  1  1  1  1  0  E  0  0
    /// </summary>
    Type3,

    /// <summary>
    /// Interrupt:   1  1  0  1 i3 i2 i1 i0
    /// </summary>
    Type4,
}