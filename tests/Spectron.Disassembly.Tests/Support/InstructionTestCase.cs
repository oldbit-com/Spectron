using System.Text.Json.Serialization;

namespace OldBit.Spectron.Disassembly.Tests.Support;

public class InstructionTestCase
{
    [JsonPropertyName("bytes")]
    [JsonConverter(typeof(HexBytesJsonConverter))]
    public byte[] Bytes { get; init; } = [];

    [JsonPropertyName("instruction")]
    public string Instruction { get; init; } = string.Empty;

    [JsonPropertyName("startAddress")]
    public int StartAddress { get; init; }

    [JsonPropertyName("isUndocumented")]
    public bool IsUndocumented { get; init; }

    [JsonPropertyName("count")]
    public int Count { get; init; } = 1;
}