using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class SpexEntry
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int EntryId { get; set; }

    public short? ReleaseSeq { get; set; }

    public int? Pub1LabelId { get; set; }

    public int? Pub2LabelId { get; set; }

    public int? Pub3LabelId { get; set; }

    public byte? GenretypeId { get; set; }

    public decimal Orgprice { get; set; }

    public int Repub2price { get; set; }

    public int Repub3price { get; set; }

    public int Diskprice { get; set; }

    public string Fgtkey { get; set; } = null!;

    public virtual Entry Entry { get; set; } = null!;

    public virtual Genretype? Genretype { get; set; }

    public virtual Label? Pub1Label { get; set; }

    public virtual Label? Pub2Label { get; set; }

    public virtual Label? Pub3Label { get; set; }

    public virtual Release? Release { get; set; }
}
