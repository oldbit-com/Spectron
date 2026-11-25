using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Licensetype
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual ICollection<License> Licenses { get; set; } = new List<License>();
}
