using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Alias
{
    public int EntryId { get; set; }

    public short ReleaseSeq { get; set; }

    public string LanguageId { get; set; } = null!;

    public string Title { get; set; } = null!;

    public virtual Entry Entry { get; set; } = null!;

    public virtual Language Language { get; set; } = null!;

    public virtual Release Release { get; set; } = null!;
}
