using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Host
{
    public byte Id { get; set; }

    public string Title { get; set; } = null!;

    public string Link { get; set; } = null!;

    public string Admin { get; set; } = null!;

    public short? MagazineId { get; set; }

    public virtual Magazine? Magazine { get; set; }

    public virtual ICollection<Magreflink> Magreflinks { get; set; } = new List<Magreflink>();
}
