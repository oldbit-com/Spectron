using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Platform
{
    public byte Id { get; set; }

    public string Text { get; set; } = null!;

    public virtual ICollection<Port> Ports { get; set; } = new List<Port>();

    public virtual ICollection<Tool> Tools { get; set; } = new List<Tool>();
}
