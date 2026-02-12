using System.Collections.Generic;

namespace OldBit.Spectron.Settings;

public class FavoriteProgram
{
    public string FilePath { get; set; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public bool IsFolder { get; init; }
    public List<FavoriteProgram> Favorites { get; } = [];
}

public class FavoritePrograms
{
    public List<FavoriteProgram> Favorites { get; } = [];
}