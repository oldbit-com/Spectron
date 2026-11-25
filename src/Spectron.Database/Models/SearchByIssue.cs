using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class SearchByIssue
{
    public int IssueId { get; set; }

    public string Name { get; set; } = null!;
}
