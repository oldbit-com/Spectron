using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Booktypein
{
    public int EntryId { get; set; }

    public int BookId { get; set; }

    public short Installment { get; set; }

    public short Volume { get; set; }

    public short Page { get; set; }

    public byte IsOriginal { get; set; }

    public virtual Entry Book { get; set; } = null!;

    public virtual Entry Entry { get; set; } = null!;
}
