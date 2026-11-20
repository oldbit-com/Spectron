using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Relatedlicense
{
    public int EntryId { get; set; }

    public int LicenseId { get; set; }

    public byte IsOfficial { get; set; }

    public virtual Entry Entry { get; set; } = null!;

    public virtual License License { get; set; } = null!;
}
