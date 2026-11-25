using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class ZxsrReview
{
    public int Id { get; set; }

    public string? IntroText { get; set; }

    public string? ReviewText { get; set; }

    public string? ReviewRating { get; set; }

    public virtual ICollection<Magref> Magrefs { get; set; } = new List<Magref>();
}
