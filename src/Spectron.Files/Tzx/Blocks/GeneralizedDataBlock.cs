using OldBit.Spectron.Files.IO;
using OldBit.Spectron.Files.Serialization;

namespace OldBit.Spectron.Files.Tzx.Blocks;

/// <summary>
/// Represents the 'Generalized Data' block.
/// </summary>
public class GeneralizedDataBlock : IBlock
{
    /// <summary>
    /// Gets the block ID.
    /// </summary>
    [FileData(Order = 0)]
    public byte BlockId => BlockCode.GeneralizedData;

    /// <summary>
    /// Gets the block length.
    /// </summary>
    [FileData(Order = 1)]
    public int Length =>
        14 +
        PilotSymbols.Count * (1 + 2 * PilotSymbols.Count) +
        PilotDataStream.Count * 3 +
        DataSymbols.Count * (1 + 2 * DataSymbols.Count)+
        DataStream.Count;

    /// <summary>
    /// Gets or sets the pause value (in millisecond) that should be applied after this block.
    /// </summary>
    [FileData(Order = 2)]
    public Word PauseDuration { get; set; }

    /// <summary>
    /// Helper property needed by the serialization.
    /// The total number of symbols in pilot/sync block (can be 0) [TOTP].
    /// </summary>
    [FileData(Order = 3)]
    private int PilotTotalSymbols => PilotDataStream.Count;

    /// <summary>
    /// Helper property needed by the serialization.
    /// Gets or sets the maximum number of pulses per pilot/sync symbol [NPP].
    /// </summary>
    [FileData(Order = 4)]
    private byte PilotMaxPulses => PilotSymbols.Count > 0 ? (byte)PilotSymbols[0].PulseLengths.Count : (byte)0;

    /// <summary>
    /// Helper property needed by the serialization.
    /// Gets or sets the number of pilot/sync symbols in the alphabet table (0=256) [ASP].
    /// </summary>
    [FileData(Order = 5)]
    private byte PilotSymbolsCountAlpha => (byte)PilotSymbols.Count;

    /// <summary>
    /// Helper property needed by the serialization.
    /// Gets or sets the total number of symbols in data stream (can be 0) [TOTD].
    /// </summary>
    [FileData(Order = 6)]
    private int DataTotalSymbols => DataStream.Count * 8;

    /// <summary>
    /// Helper property needed by the serialization.
    /// Gets or sets the maximum number of pulses per data symbol [NPD].
    /// </summary>
    [FileData(Order = 7)]
    private byte DataMaxPulses => DataSymbols.Count > 0 ? (byte)DataSymbols[0].PulseLengths.Count : (byte)0;

    /// <summary>
    /// Helper property needed by the serialization.
    /// Gets or sets the Number of data symbols in the alphabet table (0=256) [ASD].
    /// </summary>
    [FileData(Order = 8)]
    private byte DataSymbolsCountAlpha => (byte)DataSymbols.Count;

    /// <summary>
    /// Gets or sets the pilot and sync symbols definition table.
    /// </summary>
    [FileData(Order = 9)]
    public List<SymbolDef> PilotSymbols { get; set; } = [];

    /// <summary>
    /// Gets or sets the pilot and sync data stream.
    /// </summary>
    [FileData(Order = 10)]
    public List<Prle> PilotDataStream { get; set; } = [];

    /// <summary>
    /// Gets or sets the data symbols definition table.
    /// </summary>
    [FileData(Order = 11)]
    public List<SymbolDef> DataSymbols { get; set; } = [];

    /// <summary>
    /// Gets or sets the data stream.
    /// </summary>
    [FileData(Order = 12)]
    public List<byte> DataStream { get; set; } = [];

    /// <summary>
    /// Creates a new instance of the 'Generalized Data' block.
    /// </summary>
    public GeneralizedDataBlock()
    {
    }

    /// <summary>
    /// Creates a new instance of the 'Generalized Data' block using the byte reader.
    /// </summary>
    /// <param name="reader">The ByteStreamReader used to initialize the GeneralizedDataBlock properties.</param>
    internal GeneralizedDataBlock(ByteStreamReader reader) : this()
    {
        reader.ReadDWord();
        PauseDuration = reader.ReadWord();
        var pilotTotalSymbols = reader.ReadDWord();
        var pilotMaxPulses = reader.ReadByte();
        var pilotSymbolsCountAlpha = reader.ReadByte();
        var dataTotalSymbols = reader.ReadDWord();
        var dataMaxPulses = reader.ReadByte();
        var dataSymbolsCountAlpha = reader.ReadByte();
        if (pilotTotalSymbols > 0)
        {
            for (var i = 0; i < pilotSymbolsCountAlpha; i++)
            {
                PilotSymbols.Add(new SymbolDef
                {
                    Flags = reader.ReadByte(),
                    PulseLengths = new List<ushort>(reader.ReadWords(pilotMaxPulses))
                });
            }
            for (var i = 0; i < pilotTotalSymbols; i++)
            {
                PilotDataStream.Add(new Prle
                {
                    Symbol = reader.ReadByte(),
                    RepeatCount = reader.ReadWord()
                });
            }
        }
        if (dataTotalSymbols > 0)
        {
            for (var i = 0; i < dataSymbolsCountAlpha; i++)
            {
                DataSymbols.Add(new SymbolDef
                {
                    Flags = reader.ReadByte(),
                    PulseLengths = new List<ushort>(reader.ReadWords(dataMaxPulses))
                });
            }
            // DataTotalSymbols is in bits, divide by 8
            DataStream = new List<byte>(reader.ReadBytes((int)dataTotalSymbols / 8));
        }
    }

    /// <summary>
    /// Represents the SYMDEF structure.
    /// </summary>
    public class SymbolDef
    {
        /// <summary>
        /// Gets or sets the symbol flags.
        /// b0-b1: starting symbol polarity
        ///     00: opposite to the current level (make an edge, as usual) - default
        ///     01: same as the current level (no edge - prolongs the previous pulse)
        ///     10: force low level
        ///     11: force high level
        /// </summary>
        [FileData(Order = 0)]
        public byte Flags { get; set; }

        /// <summary>
        /// Gets or sets array of pulse lengths.
        /// </summary>
        [FileData(Order = 1)]
        public List<ushort> PulseLengths { get; set; } = new();
    }

    /// <summary>
    /// Represents the PRLE structure.
    /// </summary>
    public class Prle
    {
        /// <summary>
        /// Gets or sets the symbol to be represented.
        /// </summary>
        [FileData(Order = 0)]
        public byte Symbol { get; set; }

        /// <summary>
        /// Gets or sets the number of repetitions.
        /// </summary>
        [FileData(Order = 1)]
        public ushort RepeatCount { get; set; }
    }
}