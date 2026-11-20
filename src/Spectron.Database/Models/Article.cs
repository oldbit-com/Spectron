using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Article
{
    public int LabelId { get; set; }

    public string Link { get; set; } = null!;

    public string ArticletypeId { get; set; } = null!;

    public string? Title { get; set; }

    public string LanguageId { get; set; } = null!;

    public string? Writer { get; set; }

    public short? DateYear { get; set; }

    public virtual Articletype Articletype { get; set; } = null!;

    public virtual Label Label { get; set; } = null!;

    public virtual Language Language { get; set; } = null!;
}
