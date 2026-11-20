using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Feature
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public byte Version { get; set; }

    public int? LabelId { get; set; }

    public int? Label2Id { get; set; }

    public virtual Label? Label { get; set; }

    public virtual Label? Label2 { get; set; }

    public virtual ICollection<Magref> Magrefs { get; set; } = new List<Magref>();
}
