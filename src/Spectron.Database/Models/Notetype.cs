using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Notetype
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
}
