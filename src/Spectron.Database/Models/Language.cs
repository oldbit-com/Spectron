using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Language
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual ICollection<Alias> Aliases { get; set; } = new List<Alias>();

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

    public virtual ICollection<Download> Downloads { get; set; } = new List<Download>();

    public virtual ICollection<Entry> Entries { get; set; } = new List<Entry>();

    public virtual ICollection<Magazine> Magazines { get; set; } = new List<Magazine>();

    public virtual ICollection<Scrap> Scraps { get; set; } = new List<Scrap>();

    public virtual ICollection<Webref> Webrefs { get; set; } = new List<Webref>();
}
