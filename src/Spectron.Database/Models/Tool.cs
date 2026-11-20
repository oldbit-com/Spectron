using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Tool
{
    public int Id { get; set; }

    public string TooltypeId { get; set; } = null!;

    public byte PlatformId { get; set; }

    public string Title { get; set; } = null!;

    public string? Authors { get; set; }

    public string? Link { get; set; }

    public string? LatestDate { get; set; }

    public string? Comments { get; set; }

    public virtual ICollection<File> Files { get; set; } = new List<File>();

    public virtual Platform Platform { get; set; } = null!;

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();

    public virtual Tooltype Tooltype { get; set; } = null!;
}
