using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Nvg
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int? EntryId { get; set; }

    public string? FileLink { get; set; }

    public string? FileDate { get; set; }

    public int? FileSize { get; set; }

    public byte? FiletypeId { get; set; }

    public byte IsDemo { get; set; }

    public byte? MachinetypeId { get; set; }

    public string? Comments { get; set; }

    public string? Url { get; set; }

    public virtual Entry? Entry { get; set; }

    public virtual Filetype? Filetype { get; set; }

    public virtual Machinetype? Machinetype { get; set; }
}
