using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Contenttype
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual ICollection<Content> Contents { get; set; } = new List<Content>();
}
