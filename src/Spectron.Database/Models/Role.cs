using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Role
{
    public int EntryId { get; set; }

    public int LabelId { get; set; }

    public string RoletypeId { get; set; } = null!;

    public virtual Author Author { get; set; } = null!;

    public virtual Entry Entry { get; set; } = null!;

    public virtual Label Label { get; set; } = null!;

    public virtual Roletype Roletype { get; set; } = null!;
}
