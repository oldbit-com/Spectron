using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Download
{
    public int Id { get; set; }

    public int EntryId { get; set; }

    public short ReleaseSeq { get; set; }

    public string FileLink { get; set; } = null!;

    public string? FileDate { get; set; }

    public int? FileSize { get; set; }

    public string? FileMd5 { get; set; }

    public byte FiletypeId { get; set; }

    public byte ScrBorder { get; set; }

    public string? LanguageId { get; set; }

    public byte IsDemo { get; set; }

    public string? SchemetypeId { get; set; }

    public byte? MachinetypeId { get; set; }

    public string? FileCode { get; set; }

    public string? FileBarcode { get; set; }

    public string? FileDl { get; set; }

    public string? CasetypeId { get; set; }

    public string? SourcetypeId { get; set; }

    public short? ReleaseYear { get; set; }

    public string? Comments { get; set; }

    public virtual Casetype? Casetype { get; set; }

    public virtual Entry Entry { get; set; } = null!;

    public virtual Filetype Filetype { get; set; } = null!;

    public virtual Language? Language { get; set; }

    public virtual Machinetype? Machinetype { get; set; }

    public virtual Release Release { get; set; } = null!;

    public virtual Schemetype? Schemetype { get; set; }

    public virtual Sourcetype? Sourcetype { get; set; }
}
