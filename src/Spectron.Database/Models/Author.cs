using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Author
{
    public int EntryId { get; set; }

    public int LabelId { get; set; }

    public int? TeamId { get; set; }

    public short AuthorSeq { get; set; }

    public virtual Entry Entry { get; set; } = null!;

    public virtual Label Label { get; set; } = null!;

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual Label? Team { get; set; }
}
