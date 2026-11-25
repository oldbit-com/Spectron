using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Filetype
{
    public byte Id { get; set; }

    public string Text { get; set; } = null!;

    public virtual ICollection<Download> Downloads { get; set; } = new List<Download>();

    public virtual ICollection<File> Files { get; set; } = new List<File>();

    public virtual ICollection<Nvg> Nvgs { get; set; } = new List<Nvg>();

    public virtual ICollection<Scrap> Scraps { get; set; } = new List<Scrap>();
}
