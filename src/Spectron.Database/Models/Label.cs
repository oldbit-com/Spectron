using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Label
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? CountryId { get; set; }

    public string? Country2Id { get; set; }

    public int? FromId { get; set; }

    public int? OwnerId { get; set; }

    public byte WasRenamed { get; set; }

    public string? Deceased { get; set; }

    public string? LinkWikipedia { get; set; }

    public string? LinkSite { get; set; }

    public string? LabeltypeId { get; set; }

    public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

    public virtual ICollection<Author> AuthorLabels { get; set; } = new List<Author>();

    public virtual ICollection<Author> AuthorTeams { get; set; } = new List<Author>();

    public virtual ICollection<Content> Contents { get; set; } = new List<Content>();

    public virtual Country? Country { get; set; }

    public virtual Country? Country2 { get; set; }

    public virtual ICollection<Feature> FeatureLabel2s { get; set; } = new List<Feature>();

    public virtual ICollection<Feature> FeatureLabels { get; set; } = new List<Feature>();

    public virtual ICollection<File> Files { get; set; } = new List<File>();

    public virtual Label? From { get; set; }

    public virtual ICollection<Label> InverseFrom { get; set; } = new List<Label>();

    public virtual ICollection<Label> InverseOwner { get; set; } = new List<Label>();

    public virtual ICollection<Issue> IssueLabel2s { get; set; } = new List<Issue>();

    public virtual ICollection<Issue> IssueLabels { get; set; } = new List<Issue>();

    public virtual Labeltype? Labeltype { get; set; }

    public virtual ICollection<Magref> Magrefs { get; set; } = new List<Magref>();

    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();

    public virtual Label? Owner { get; set; }

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    public virtual ICollection<Publisher> Publishers { get; set; } = new List<Publisher>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<SpexAuthor> SpexAuthors { get; set; } = new List<SpexAuthor>();

    public virtual ICollection<SpexEntry> SpexEntryPub1Labels { get; set; } = new List<SpexEntry>();

    public virtual ICollection<SpexEntry> SpexEntryPub2Labels { get; set; } = new List<SpexEntry>();

    public virtual ICollection<SpexEntry> SpexEntryPub3Labels { get; set; } = new List<SpexEntry>();

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();

    public virtual ICollection<License> Licenses { get; set; } = new List<License>();
}
