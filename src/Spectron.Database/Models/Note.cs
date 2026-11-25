using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Note
{
    public int Id { get; set; }

    public int? EntryId { get; set; }

    public int? LabelId { get; set; }

    public string NotetypeId { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual Entry? Entry { get; set; }

    public virtual Label? Label { get; set; }

    public virtual Notetype Notetype { get; set; } = null!;
}
