using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Publisher
{
    public int EntryId { get; set; }

    public short ReleaseSeq { get; set; }

    public int LabelId { get; set; }

    public short PublisherSeq { get; set; }

    public virtual Entry Entry { get; set; } = null!;

    public virtual Label Label { get; set; } = null!;

    public virtual Release Release { get; set; } = null!;
}
