using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Relation
{
    public int EntryId { get; set; }

    public int OriginalId { get; set; }

    public string RelationtypeId { get; set; } = null!;

    public virtual Entry Entry { get; set; } = null!;

    public virtual Entry Original { get; set; } = null!;

    public virtual Relationtype Relationtype { get; set; } = null!;
}
