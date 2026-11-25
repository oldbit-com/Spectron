using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Currency
{
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Symbol { get; set; }

    public byte Prefix { get; set; }

    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();

    public virtual ICollection<Release> Releases { get; set; } = new List<Release>();
}
