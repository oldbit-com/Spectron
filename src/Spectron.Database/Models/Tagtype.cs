using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Tagtype
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
