using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Topictype
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
