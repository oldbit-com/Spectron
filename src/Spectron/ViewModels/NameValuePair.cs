namespace OldBit.Spectron.ViewModels;

public class NameValuePair<T>(string name, T value)
{
    public string Name { get; init; } = name;

    public T Value { get; init; } = value;
}