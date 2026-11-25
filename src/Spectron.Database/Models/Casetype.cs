using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Casetype
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual ICollection<Download> Downloads { get; set; } = new List<Download>();

    public virtual ICollection<Scrap> Scraps { get; set; } = new List<Scrap>();
}
