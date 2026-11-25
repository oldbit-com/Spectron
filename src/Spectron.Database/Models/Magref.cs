using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Magref
{
    public int Id { get; set; }

    public byte ReferencetypeId { get; set; }

    public int? EntryId { get; set; }

    public int? LabelId { get; set; }

    public int? TopicId { get; set; }

    public int IssueId { get; set; }

    public short Page { get; set; }

    public byte IsOriginal { get; set; }

    public string ScoreGroup { get; set; } = null!;

    public int? ReviewId { get; set; }

    public byte? AwardId { get; set; }

    public virtual ZxsrAward? Award { get; set; }

    public virtual Entry? Entry { get; set; }

    public virtual Issue Issue { get; set; } = null!;

    public virtual Label? Label { get; set; }

    public virtual ICollection<Magreflink> Magreflinks { get; set; } = new List<Magreflink>();

    public virtual Referencetype Referencetype { get; set; } = null!;

    public virtual ZxsrReview? Review { get; set; }

    public virtual Topic? Topic { get; set; }

    public virtual ICollection<ZxsrCaption> ZxsrCaptions { get; set; } = new List<ZxsrCaption>();

    public virtual ICollection<ZxsrScore> ZxsrScores { get; set; } = new List<ZxsrScore>();

    public virtual ICollection<Feature> Features { get; set; } = new List<Feature>();
}
