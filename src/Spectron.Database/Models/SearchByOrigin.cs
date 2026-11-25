using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class SearchByOrigin
{
    public int EntryId { get; set; }

    public string LibraryTitle { get; set; } = null!;

    public string OrigintypeId { get; set; } = null!;

    public int? ContainerId { get; set; }

    public int? IssueId { get; set; }

    public int? DateYear { get; set; }

    public int? DateMonth { get; set; }

    public int? DateDay { get; set; }

    public string? Publication { get; set; }
}
