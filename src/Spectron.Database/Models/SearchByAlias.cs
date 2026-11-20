using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class SearchByAlias
{
    public int EntryId { get; set; }

    public string Title { get; set; } = null!;

    public string LibraryTitle { get; set; } = null!;
}
