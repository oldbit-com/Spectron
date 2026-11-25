using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Topic
{
    public int Id { get; set; }

    public string TopictypeId { get; set; } = null!;

    public int? LabelId { get; set; }

    public short? MagazineId { get; set; }

    public string Name { get; set; } = null!;

    public string? Comments { get; set; }

    public virtual Label? Label { get; set; }

    public virtual Magazine? Magazine { get; set; }

    public virtual ICollection<Magazine> Magazines { get; set; } = new List<Magazine>();

    public virtual ICollection<Magref> Magrefs { get; set; } = new List<Magref>();

    public virtual Topictype Topictype { get; set; } = null!;
}
