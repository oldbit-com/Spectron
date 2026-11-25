using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class License
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string LicensetypeId { get; set; } = null!;

    public string? LinkWikipedia { get; set; }

    public string? LinkSite { get; set; }

    public string? Comments { get; set; }

    public virtual Licensetype Licensetype { get; set; } = null!;

    public virtual ICollection<Relatedlicense> Relatedlicenses { get; set; } = new List<Relatedlicense>();

    public virtual ICollection<Label> Labels { get; set; } = new List<Label>();
}
