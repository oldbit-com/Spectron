using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Country
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual ICollection<Label> LabelCountries { get; set; } = new List<Label>();

    public virtual ICollection<Label> LabelCountry2s { get; set; } = new List<Label>();

    public virtual ICollection<Magazine> Magazines { get; set; } = new List<Magazine>();
}
