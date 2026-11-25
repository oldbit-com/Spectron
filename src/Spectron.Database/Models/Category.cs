using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Category
{
    public short Id { get; set; }

    public string Text { get; set; } = null!;

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();
}
