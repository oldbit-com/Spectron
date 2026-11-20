using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class ZxsrCaption
{
    public int MagrefId { get; set; }

    public short CaptionSeq { get; set; }

    public string Text { get; set; } = null!;

    public byte IsBanner { get; set; }

    public virtual Magref Magref { get; set; } = null!;
}
