using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Webref
{
    public int EntryId { get; set; }

    public string Link { get; set; } = null!;

    public byte WebsiteId { get; set; }

    public string LanguageId { get; set; } = null!;

    public virtual Entry Entry { get; set; } = null!;

    public virtual Language Language { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
