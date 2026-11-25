using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Magreflink
{
    public int MagrefId { get; set; }

    public string Link { get; set; } = null!;

    public byte HostId { get; set; }

    public virtual Host Host { get; set; } = null!;

    public virtual Magref Magref { get; set; } = null!;
}
