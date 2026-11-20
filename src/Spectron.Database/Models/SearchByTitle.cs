using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class SearchByTitle
{
    public string EntryTitle { get; set; } = null!;

    public int EntryId { get; set; }
}
