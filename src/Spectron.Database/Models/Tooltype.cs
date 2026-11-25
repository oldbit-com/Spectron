using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Tooltype
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual ICollection<Tool> Tools { get; set; } = new List<Tool>();
}
