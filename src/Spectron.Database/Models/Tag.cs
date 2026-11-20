using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Link { get; set; }

    public string? Comments { get; set; }

    public string TagtypeId { get; set; } = null!;

    public int? ToolId { get; set; }

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual Tagtype Tagtype { get; set; } = null!;

    public virtual Tool? Tool { get; set; }
}
