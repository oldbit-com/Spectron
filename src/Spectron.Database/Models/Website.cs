using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Website
{
    public byte Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Comments { get; set; }

    public string? Link { get; set; }

    public string? LinkMask { get; set; }

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    public virtual ICollection<Score> Scores { get; set; } = new List<Score>();

    public virtual ICollection<Webref> Webrefs { get; set; } = new List<Webref>();
}
