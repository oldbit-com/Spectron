namespace OldBit.Spectron.Files.Tzx;

/// <summary>
/// Contains constant values representing different types of TZX block codes used in the system.
/// </summary>
/// <remarks>
/// These block codes correspond to various data types and operations defined in the TZX file format.
/// They are used to identify the type of block in TZX-related operations and processes.
/// </remarks>
public static class BlockCode
{
    /// <summary>
    ///  Standard Speed Data (ID 0x10).
    /// </summary>
    public const byte StandardSpeedData = 0x10;

    /// <summary>
    /// Turbo Speed Data (ID 0x11).
    /// </summary>
    public const byte TurboSpeedData = 0x11;

    /// <summary>
    /// Pure Tone (ID 0x12).
    /// </summary>
    public const byte PureTone = 0x12;

    /// <summary>
    /// Pulse Sequence (ID 0x13).
    /// </summary>
    public const byte PulseSequence = 0x13;

    /// <summary>
    /// Pure Data (ID 0x14).
    /// </summary>
    public const byte PureData = 0x14;

    /// <summary>
    /// Direct Recording (ID 0x15).
    /// </summary>
    public const byte DirectRecording = 0x15;

    /// <summary>
    /// C64 ROM Type Data (ID 0x16).
    /// </summary>
    public const byte C64RomTypeData = 0x16;

    /// <summary>
    /// CSW Recording (ID 0x18).
    /// </summary>
    public const byte CswRecording = 0x18;

    /// <summary>
    /// Generalized Data (ID 0x19).
    /// </summary>
    public const byte GeneralizedData = 0x19;

    /// <summary>
    /// Pause (Silence) or 'Stop the Tape' Command (ID 0x20).
    /// </summary>
    public const byte Pause = 0x20;

    /// <summary>
    /// Group Start (ID 0x21).
    /// </summary>
    public const byte GroupStart = 0x21;

    /// <summary>
    /// Group End (ID 0x22).
    /// </summary>
    public const byte GroupEnd = 0x22;

    /// <summary>
    /// Jump to Block (ID 0x23).
    /// </summary>
    public const byte JumpToBlock = 0x23;

    /// <summary>
    /// Loop Start (ID 0x24).
    /// </summary>
    public const byte LoopStart = 0x24;

    /// <summary>
    /// Loop End (ID 0x25).
    /// </summary>
    public const byte LoopEnd = 0x25;

    /// <summary>
    /// Call Sequence (ID 0x26).
    /// </summary>
    public const byte CallSequence = 0x26;

    /// <summary>
    ///  Return from Sequence (ID 0x27).
    /// </summary>
    public const byte ReturnFromSequence = 0x27;

    /// <summary>
    /// Select (ID 0x28).
    /// </summary>
    public const byte Select = 0x28;

    /// <summary>
    /// Stop the Tape if in 48K mode (ID 0x2A).
    /// </summary>
    public const byte StopTheTape48 = 0x2A;

    /// <summary>
    /// Set Signal Level (ID 0x2B).
    /// </summary>
    public const byte SetSignalLevel = 0x2B;

    /// <summary>
    /// Text Description (ID 0x30).
    /// </summary>
    public const byte TextDescription = 0x30;

    /// <summary>
    /// Message (ID 0x31).
    /// </summary>
    public const byte Message = 0x31;

    /// <summary>
    /// Archive Info (ID 0x32).
    /// </summary>
    public const byte ArchiveInfo = 0x32;

    /// <summary>
    /// Hardware Type (ID 0x33).
    /// </summary>
    public const byte HardwareType = 0x33;

    /// <summary>
    /// Custom Info (ID 0x35).
    /// </summary>
    public const byte CustomInfo = 0x35;

    /// <summary>
    /// TSX format specific block used by MSX computers.
    /// </summary>
    public const byte KansasCityStandard = 0x4B;

    /// <summary>
    /// "Glue" block (ID 0x5A).
    /// </summary>
    public const byte Glue = 0x5A;
}