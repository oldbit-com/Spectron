using System.Collections.Generic;
using OldBit.Spectron.Emulation;

namespace OldBit.Spectron.Settings;

public class FavoriteProgram
{
    public string Path { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public bool IsFolder { get; init; }
    public List<FavoriteProgram> Items { get; init; } = [];
    public ComputerType? ComputerType { get; init; }
    public bool? IsUlaPlusEnabled { get; init; }
}

public class FavoritePrograms
{
    public List<FavoriteProgram> Items { get; init; } = [];
}