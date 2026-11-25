using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Remake
{
    public int Id { get; set; }

    public int EntryId { get; set; }

    public string Title { get; set; } = null!;

    public string FileLink { get; set; } = null!;

    public string? FileDate { get; set; }

    public int? FileSize { get; set; }

    public string? Authors { get; set; }

    public string? Platforms { get; set; }

    public string? RemakeYears { get; set; }

    public string? RemakeStatus { get; set; }

    public virtual Entry Entry { get; set; } = null!;
}
