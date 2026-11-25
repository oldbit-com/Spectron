using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Articletype
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();
}
