using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Entry
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public byte IsXrated { get; set; }

    public byte? MachinetypeId { get; set; }

    public byte MaxPlayers { get; set; }

    public byte? GenretypeId { get; set; }

    public byte? SpotGenretypeId { get; set; }

    public string? AvailabletypeId { get; set; }

    public byte WithoutLoadScreen { get; set; }

    public byte WithoutInlay { get; set; }

    public string? LanguageId { get; set; }

    public int? IssueId { get; set; }

    public virtual ICollection<Alias> Aliases { get; set; } = new List<Alias>();

    public virtual ICollection<Author> Authors { get; set; } = new List<Author>();

    public virtual Availabletype? Availabletype { get; set; }

    public virtual ICollection<Booktypein> BooktypeinBooks { get; set; } = new List<Booktypein>();

    public virtual ICollection<Booktypein> BooktypeinEntries { get; set; } = new List<Booktypein>();

    public virtual ICollection<Content> ContentContainers { get; set; } = new List<Content>();

    public virtual ICollection<Content> ContentEntries { get; set; } = new List<Content>();

    public virtual ICollection<Download> Downloads { get; set; } = new List<Download>();

    public virtual Genretype? Genretype { get; set; }

    public virtual Issue? Issue { get; set; }

    public virtual Language? Language { get; set; }

    public virtual Machinetype? Machinetype { get; set; }

    public virtual ICollection<Magref> Magrefs { get; set; } = new List<Magref>();

    public virtual ICollection<Member> Members { get; set; } = new List<Member>();

    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();

    public virtual ICollection<Nvg> Nvgs { get; set; } = new List<Nvg>();

    public virtual ICollection<Port> Ports { get; set; } = new List<Port>();

    public virtual ICollection<Publisher> Publishers { get; set; } = new List<Publisher>();

    public virtual ICollection<Relatedlicense> Relatedlicenses { get; set; } = new List<Relatedlicense>();

    public virtual ICollection<Relation> RelationEntries { get; set; } = new List<Relation>();

    public virtual ICollection<Relation> RelationOriginals { get; set; } = new List<Relation>();

    public virtual ICollection<Release> Releases { get; set; } = new List<Release>();

    public virtual ICollection<Remake> Remakes { get; set; } = new List<Remake>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();

    public virtual ICollection<Scrap> Scraps { get; set; } = new List<Scrap>();

    public virtual ICollection<SpexAuthor> SpexAuthors { get; set; } = new List<SpexAuthor>();

    public virtual ICollection<SpexEntry> SpexEntries { get; set; } = new List<SpexEntry>();

    public virtual Genretype? SpotGenretype { get; set; }

    public virtual ICollection<Webref> Webrefs { get; set; } = new List<Webref>();
}
