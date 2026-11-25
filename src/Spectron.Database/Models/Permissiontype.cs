using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Permissiontype
{
    public string Id { get; set; } = null!;

    public string Text { get; set; } = null!;

    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();
}
