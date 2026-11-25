using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Magtype
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual ICollection<Magazine> Magazines { get; set; } = new List<Magazine>();
}
