using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class SearchByName
{
    public string LabelName { get; set; } = null!;

    public int LabelId { get; set; }
}
