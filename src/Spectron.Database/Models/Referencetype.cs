using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Referencetype
{
    public byte Id { get; set; }

    public string Text { get; set; } = null!;

    public virtual ICollection<Magref> Magrefs { get; set; } = new List<Magref>();
}
