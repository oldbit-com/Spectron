using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class SpexAuthor
{
    public int Id { get; set; }

    public int EntryId { get; set; }

    public string Name { get; set; } = null!;

    public int? LabelId { get; set; }

    public virtual Entry Entry { get; set; } = null!;

    public virtual Label? Label { get; set; }
}
