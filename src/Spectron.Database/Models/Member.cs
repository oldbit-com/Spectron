using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Member
{
    public int TagId { get; set; }

    public int EntryId { get; set; }

    public short CategoryId { get; set; }

    public short? MemberSeq { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Entry Entry { get; set; } = null!;

    public virtual Tag Tag { get; set; } = null!;
}
