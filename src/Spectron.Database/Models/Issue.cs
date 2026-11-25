using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Issue
{
    public int Id { get; set; }

    public short MagazineId { get; set; }

    public int? LabelId { get; set; }

    public int? Label2Id { get; set; }

    public short? DateYear { get; set; }

    public short? DateMonth { get; set; }

    public short? DateDay { get; set; }

    public short? Volume { get; set; }

    public short? Number { get; set; }

    public string? Special { get; set; }

    public string? Supplement { get; set; }

    public short CoverPage { get; set; }

    public int? ParentId { get; set; }

    public string? CurrencyId { get; set; }

    public int? Price { get; set; }

    public string? LinkMask { get; set; }

    public string? ArchiveMask { get; set; }

    public string? TranslationMask { get; set; }

    public virtual Currency? Currency { get; set; }

    public virtual ICollection<Entry> Entries { get; set; } = new List<Entry>();

    public virtual ICollection<File> Files { get; set; } = new List<File>();

    public virtual ICollection<Issue> InverseParent { get; set; } = new List<Issue>();

    public virtual Label? Label { get; set; }

    public virtual Label? Label2 { get; set; }

    public virtual Magazine Magazine { get; set; } = null!;

    public virtual ICollection<Magref> Magrefs { get; set; } = new List<Magref>();

    public virtual Issue? Parent { get; set; }
}
