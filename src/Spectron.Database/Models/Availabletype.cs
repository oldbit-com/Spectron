using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Availabletype
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual ICollection<Entry> Entries { get; set; } = new List<Entry>();
}
