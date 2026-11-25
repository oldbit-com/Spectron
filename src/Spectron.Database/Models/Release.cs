using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Release
{
    public int EntryId { get; set; }

    public short ReleaseSeq { get; set; }

    public short? ReleaseYear { get; set; }

    public short? ReleaseMonth { get; set; }

    public short? ReleaseDay { get; set; }

    public string? CurrencyId { get; set; }

    public int? ReleasePrice { get; set; }

    public int? BudgetPrice { get; set; }

    public int? MicrodrivePrice { get; set; }

    public int? DiskPrice { get; set; }

    public decimal? CartridgePrice { get; set; }

    public string? BookIsbn { get; set; }

    public short? BookPages { get; set; }

    public virtual ICollection<Alias> Aliases { get; set; } = new List<Alias>();

    public virtual Currency? Currency { get; set; }

    public virtual ICollection<Download> Downloads { get; set; } = new List<Download>();

    public virtual Entry Entry { get; set; } = null!;

    public virtual ICollection<Publisher> Publishers { get; set; } = new List<Publisher>();

    public virtual ICollection<Scrap> Scraps { get; set; } = new List<Scrap>();

    public virtual ICollection<SpexEntry> SpexEntries { get; set; } = new List<SpexEntry>();
}
