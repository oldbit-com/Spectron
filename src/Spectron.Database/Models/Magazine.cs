using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Magazine
{
    public short Id { get; set; }

    public string Name { get; set; } = null!;

    public string CountryId { get; set; } = null!;

    public string LanguageId { get; set; } = null!;

    public string? LinkSite { get; set; }

    public string MagtypeId { get; set; } = null!;

    public int? TopicId { get; set; }

    public string? LinkMask { get; set; }

    public string? ArchiveMask { get; set; }

    public string? TranslationMask { get; set; }

    public virtual Country Country { get; set; } = null!;

    public virtual ICollection<Host> Hosts { get; set; } = new List<Host>();

    public virtual ICollection<Issue> Issues { get; set; } = new List<Issue>();

    public virtual Language Language { get; set; } = null!;

    public virtual Magtype Magtype { get; set; } = null!;

    public virtual Topic? Topic { get; set; }

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();

    public virtual ICollection<ZxsrAward> ZxsrAwards { get; set; } = new List<ZxsrAward>();
}
