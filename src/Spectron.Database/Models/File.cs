using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class File
{
    public int Id { get; set; }

    public int? LabelId { get; set; }

    public int? IssueId { get; set; }

    public int? ToolId { get; set; }

    public string FileLink { get; set; } = null!;

    public string? FileDate { get; set; }

    public int? FileSize { get; set; }

    public string? FileMd5 { get; set; }

    public byte FiletypeId { get; set; }

    public string? Comments { get; set; }

    public virtual Filetype Filetype { get; set; } = null!;

    public virtual Issue? Issue { get; set; }

    public virtual Label? Label { get; set; }

    public virtual Tool? Tool { get; set; }
}
