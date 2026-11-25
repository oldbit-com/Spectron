using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class ZxsrScore
{
    public int Id { get; set; }

    public int MagrefId { get; set; }

    public byte? ScoreSeq { get; set; }

    public string Category { get; set; } = null!;

    public byte IsOverall { get; set; }

    public string? Score { get; set; }

    public string? Comments { get; set; }

    public virtual Magref Magref { get; set; } = null!;
}
