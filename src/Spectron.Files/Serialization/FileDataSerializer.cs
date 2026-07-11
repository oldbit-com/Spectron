using System.Collections;
using System.Reflection;
using OldBit.Spectron.Files.Extensions;

namespace OldBit.Spectron.Files.Serialization;

/// <summary>
/// Implements generic data serializer.
/// </summary>
internal static class FileDataSerializer
{
    /// <summary>
    /// Serializes the data as an array of bytes.
    /// </summary>
    /// <param name="data">The data to serialize.</param>
    /// <returns>An array of bytes representing the serialized data.</returns>
    internal static byte[] Serialize(object? data)
    {
        if (data == null)
        {
            return [];
        }

        var propsAndAttrs =
            data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(p => (Property: p, Atttribute: p.GetCustomAttributes(typeof(FileDataAttribute)).FirstOrDefault() as FileDataAttribute))
            .Where(p => p.Atttribute != null)
            .OrderBy(p => p.Atttribute!.Order);

        var result = new List<byte>();
        foreach (var propAttr in propsAndAttrs)
        {
            if (propAttr.Property.PropertyType == typeof(byte))
            {
                var value = (byte)propAttr.Property.GetValue(data)!;
                result.Add(value);
            }
            else if (propAttr.Property.PropertyType == typeof(Word))
            {
                var value = (Word)propAttr.Property.GetValue(data)!;
                result.Add((byte)value);
                result.Add((byte)(value >> 8));
            }
            else if (propAttr.Property.PropertyType == typeof(short))
            {
                var value = (short)propAttr.Property.GetValue(data)!;
                result.Add((byte)value);
                result.Add((byte)(value >> 8));
            }
            else if (propAttr.Property.PropertyType == typeof(string))
            {
                var value = (string)propAttr.Property.GetValue(data)!;
                if (propAttr.Atttribute!.Size != 0)
                {
                    if (value.Length > propAttr.Atttribute.Size)
                    {
                        value = value[..propAttr.Atttribute.Size];
                    }
                    else if (value.Length < propAttr.Atttribute.Size)
                    {
                        value = value.PadRight(propAttr.Atttribute.Size);
                    }
                }

                result.AddRange(value.ToAsciiBytes());
            }
            else if (propAttr.Property.PropertyType == typeof(int))
            {
                var value = (int)propAttr.Property.GetValue(data)!;
                for (var i = 0; i < 4; i++)
                {
                    result.Add((byte)(value >> (i * 8)));
                    if (propAttr.Atttribute!.Size != 0 && i == propAttr.Atttribute!.Size - 1)
                    {
                        break;
                    }
                }
            }
            else if (propAttr.Property.PropertyType == typeof(DWord))
            {
                var value = (DWord)propAttr.Property.GetValue(data)!;
                for (var i = 0; i < 4; i++)
                {
                    result.Add((byte)(value >> (i * 8)));
                    if (propAttr.Atttribute!.Size != 0 && i == propAttr.Atttribute!.Size - 1)
                    {
                        break;
                    }
                }
            }
            else if (propAttr.Property.PropertyType.IsEnum)
            {
                var value = (Enum)propAttr.Property.GetValue(data)!;
                result.Add(Convert.ToByte(value));
            }
            else if (propAttr.Property.PropertyType.IsEnumerable())
            {
                var values = (IEnumerable?)propAttr.Property.GetValue(data);
                if (values != null)
                {
                    result.AddRange(SerializeEnumerable(values));
                }
            }
            else
            {
                result.AddRange(Serialize(propAttr.Property.GetValue(data)!));
            }
        }

        return result.ToArray();
    }

    internal static IEnumerable<byte> SerializePrimitiveType(object value)
    {
        var type = value.GetType();

        if (type == typeof(byte))
        {
            yield return Convert.ToByte(value);
        }
        else if (type == typeof(short))
        {
            yield return (byte)(short)value;
            yield return (byte)((short)value >> 8);
        }
        else if (type == typeof(Word))
        {
            yield return (byte)(Word)value;
            yield return (byte)((Word)value >> 8);
        }
        else
        {
            throw new NotSupportedException($"The type '{type.Name}' is not supported by the serializer.");
        }
    }

    private static List<byte> SerializeEnumerable(IEnumerable values)
    {
        var items = new List<byte>();

        foreach (var value in values)
        {
            if (value.GetType().IsPrimitive)
            {
                items.AddRange(SerializePrimitiveType(value));
            }
            else if (value.GetType().IsEnumerable())
            {
                items.AddRange(SerializeEnumerable((IEnumerable)value));
            }
            else
            {
                items.AddRange(Serialize(value));
            }
        }

        return items;
    }
}