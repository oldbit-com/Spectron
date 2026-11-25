using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Relationtype
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public string Reciprocal { get; set; } = null!;

    public string? Comments { get; set; }

    public virtual ICollection<Relation> Relations { get; set; } = new List<Relation>();
}
