namespace OldBit.Spectron.Emulation.Devices.Beta128.Controller;

[Flags]
public enum ControllerStatus
{
    /// <summary>
    /// Busy.
    /// Type I: When set command is in progress. When reset, no command is in progress.
    /// Type II, III: When set, the command is under execution. When reset, no command is under execution.
    /// </summary>
    Busy = 0x01,

    /// <summary>
    /// Index.
    /// Type I: When set, indicates index mark detected from drive. This bit is an inverted copy of the /IP input.
    /// </summary>
    Index = 0x02,

    /// <summary>
    /// Data Request.
    /// Type II, III: This bit is a copy of the DRQ output. When set, it indicates the DR is full on a Read operation or
    /// the DR is empty on a Write operation. This bit is reset to zero when updated.
    /// </summary>
    DataRequest = 0x02,

    /// <summary>
    /// Track 0.
    /// Type I: When set, indicates Read/Write head is positioned to Track 0. This bit is an inverted copy of the /TR00 input.
    /// </summary>
    TrackZero = 0x04,

    /// <summary>
    /// Lost Data.
    /// Type II, III: When set, it indicates the computer did not respond to DRQ in one byte time. This bit is reset to zero when updated.
    /// </summary>
    Lost = 0x04,

    /// <summary>
    /// CRC Error.
    /// Type I: CRC encountered in ID field.
    /// Type II, III: If b4 is set, an error is found in one or more ID fields otherwise it indicates error in data field. This bit is reset when updated.
    /// </summary>
    CrcError = 0x08,

    /// <summary>
    /// Seek Error
    /// Type I: When set, the desired track was not verified. This bit is reset 0 when updated.
    /// </summary>
    SeekError = 0x10,

    /// <summary>
    /// Record Not Found.
    /// Type II, III: When set, it indicates the desired track, sector, or side were not found. This bit is reset when updated.
    /// </summary>
    NotFound = 0x10,

    /// <summary>
    /// Head Loaded.
    /// Type I: When set, it indicates the head is loaded an engaged. This bit is a logical "and" of HLD and HLT signals.
    /// </summary>
    HeadLoaded = 0x20,

    /// <summary>
    /// Record Type
    /// Type II, III: On Read Record: it indicates the record-type code from data field address mark (1: Deleted Data Mark, 0: Data Mark).
    /// </summary>
    RecordType = 0x20,

    /// <summary>
    /// Write Fault.
    /// Type II, III: On any write: it indicates a Write Fault. This bit is reset when updated.
    /// </summary>
    WriteFault = 0x20,

    /// <summary>
    /// Protected.
    /// Type I: When set, indicates Write Protect is activated. This bit is an inverted copy of /WRPT input.
    /// Type II, III: On Read Record or Read Track, not used. On any write: it indicates a Write Protect. This bit is reset when updated.
    /// </summary>
    WriteProtect = 0x40,

    /// <summary>
    /// Not Ready.
    /// Type I, II, III: This bit when set indicates the drive is not ready. When reset it indicates that the drive is ready. This bit is an inverted copy of the
    /// Ready input and logically 'ored' with MR.
    /// </summary>
    NotReady = 0x80,
}