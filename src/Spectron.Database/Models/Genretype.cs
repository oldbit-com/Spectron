using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Genretype
{
    public byte Id { get; set; }

    public string Text { get; set; } = null!;

    public virtual ICollection<Entry> EntryGenretypes { get; set; } = new List<Entry>();

    public virtual ICollection<Entry> EntrySpotGenretypes { get; set; } = new List<Entry>();

    public virtual ICollection<SpexEntry> SpexEntries { get; set; } = new List<SpexEntry>();
}
