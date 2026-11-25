using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Content
{
    public int ContainerId { get; set; }

    public short MediaSeq { get; set; }

    public string MediaSide { get; set; } = null!;

    public short ProgSeq { get; set; }

    public int? EntryId { get; set; }

    public int? LabelId { get; set; }

    public string? Alias { get; set; }

    public byte IsOriginal { get; set; }

    public string ContenttypeId { get; set; } = null!;

    public virtual Entry Container { get; set; } = null!;

    public virtual Contenttype Contenttype { get; set; } = null!;

    public virtual Entry? Entry { get; set; }

    public virtual Label? Label { get; set; }
}
