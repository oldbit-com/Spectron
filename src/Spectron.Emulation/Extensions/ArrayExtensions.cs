using System.Diagnostics.CodeAnalysis;

namespace OldBit.Spectron.Emulation.Extensions;

public static class ArrayExtensions
{
    [return: NotNullIfNotNull(nameof(first))]
    [return: NotNullIfNotNull(nameof(second))]
    public static T[]? Concatenate<T>(this T[]? first, T[]? second)
    {
        if (first is null)
        {
            return second;
        }

        if (second is null)
        {
            return first;
        }

        var result = new T[first.Length + second.Length];

        Array.Copy(first, 0, result, 0, first.Length);
        Array.Copy(second, 0, result, first.Length, second.Length);

        return result;
    }
}