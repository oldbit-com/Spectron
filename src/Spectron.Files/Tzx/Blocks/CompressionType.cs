namespace OldBit.Spectron.Files.Tzx.Blocks
{
    /// <summary>
    /// Defines types of compression used in CSW recording blocks.
    /// </summary>
    public enum CompressionType
    {
        /// <summary>
        /// Represents an unknown compression type.
        /// This value is used as a default or fallback when the compression type is not recognized
        /// or explicitly specified in the context of CSW recording blocks.
        /// </summary>
        Unknown = 0x00,

        /// <summary>
        /// Represents the RLE (Run-Length Encoding) compression type.
        /// This type of compression is used to minimize the size of CSW recording blocks
        /// by encoding sequences of repeated data values as a single value and run length.
        /// </summary>
        Rle = 0x01,

        /// <summary>
        /// Represents the ZRle compression type.
        /// This compression type is used in CSW recording blocks to optimize data storage
        /// through zero-run length encoding, reducing the size of sections with continuous zeroes.
        /// </summary>
        ZRle = 0x02
    }
}
