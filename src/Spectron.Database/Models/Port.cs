using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Port
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public int EntryId { get; set; }

    public byte PlatformId { get; set; }

    public byte IsOfficial { get; set; }

    public string? LinkSystem { get; set; }

    public virtual Entry Entry { get; set; } = null!;

    public virtual Platform Platform { get; set; } = null!;
}
