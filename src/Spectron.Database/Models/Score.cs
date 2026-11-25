using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Score
{
    public byte WebsiteId { get; set; }

    public int EntryId { get; set; }

    public int Score1 { get; set; }

    public int Votes { get; set; }

    public virtual Entry Entry { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
