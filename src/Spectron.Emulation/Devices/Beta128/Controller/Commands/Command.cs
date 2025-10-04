namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller.Commands;

internal readonly struct Command
{
    private readonly byte _command;

    internal CommandType Type { get; private init; }

    internal Command(byte command)
    {
        _command = command;
        Type = GetType(command);
    }

    private static CommandType GetType(byte value)
    {
        // Type I (Restore, Seek, Step)
        if ((value & 0x80) == 0)
        {
            return CommandType.Type1;
        }

        // Type II (Read / Write Sector)
        if ((value & 0x40) == 0)
        {
            return CommandType.Type2;
        }

        // Type IV (Force Interrupt)
        if ((value & 0xF0) == 0xD0)
        {
            return CommandType.Type4;
        }

        // Type III (Read Address, Read Track, Write Track)]
        return CommandType.Type3;
    }


    // Bit E - 15 ms delay (0: no 15ms delay, 1: 15 ms delay)
    internal bool ShouldDelay => (_command & 0x04) != 0;

    // 0  0  0  0  h  V r1 r0
    internal bool IsRestore => (_command & 0xF0) == 0xF0;

    // 0  0  0  1  h  V r1 r0
    internal bool IsSeek => (_command & 0xF0) == 0x10;

    // 0  0  1  T  h  V r1 r0
    internal bool IsStep => (_command & 0xE0) == 0x20;

    // 0  1  0  T  h  V r1 r0
    internal bool IsStepIn => (_command & 0xE0) == 0x40;

    // 0  1  1  T  h  V r1 r0
    internal bool IsStepOut => (_command & 0xE0) == 0x60;

    // 1  0  0  m  S  E  C  0
    internal bool IsReadSector => (_command & 0xE0) == 0x80;

    // 1  0  1  m  S  E  C a0
    internal bool IsWriteSector => (_command & 0xE0) == 0xA0;

    // 1  1  0  0  0  E  0  0
    internal bool IsReadAddress => (_command & 0xFB) == 0xC0;

    // 1  1  1  0  0  E  0  0
    internal bool IsReadTrack => (_command & 0xFB) == 0xE0;

    // 1  1  1  1  0  E  0  0
    internal bool IsWriteTrack => (_command & 0xFB) == 0xF0;

    // C flag value for ReadSector/WriteSector
    internal bool HasSideCompareFlagSet => (_command & 0x02) == 0x02;

    // S flag value for ReadSector/WriteSector
    internal int SideSelectFlagSet => (_command & 0x08) >> 3;

    // m flag set for ReadSector/WriteSector
    internal bool HasMultipleFlagSet => (_command & 0x10) == 0x10;

    // T flag set for ReadSector/WriteSector
    internal bool HasTrackUpdateFlagSet => (_command & 0x10) == 0x10;

    // V flag set for Restore/Seek/Step/StepIn/StepOut
    internal bool HasVerifyFlagSet => (_command & 0x04) == 0x04;

    // r0 r1 for Restore/Seek/Step/StepIn/StepOut
    internal int SteppingRate => _command & 0x03;
}