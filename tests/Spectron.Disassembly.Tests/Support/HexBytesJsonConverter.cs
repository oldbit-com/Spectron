using System.Text.Json;
using System.Text.Json.Serialization;

namespace OldBit.Spectron.Disassembly.Tests.Support;

public class HexBytesJsonConverter : JsonConverter<byte[]>
{
    public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var hexStrings = JsonSerializer.Deserialize<List<string>>(ref reader, options);

        if (hexStrings == null)
        {
            return null;
        }

        return hexStrings
            .Select(hex => Convert.ToByte(hex, 16))
            .ToArray();
    }

    public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options) =>
        throw new NotImplementedException();
}