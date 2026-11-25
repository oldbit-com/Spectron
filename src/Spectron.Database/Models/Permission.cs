using System;
using System.Collections.Generic;

namespace OldBit.Spectron.Database.Models;

public partial class Permission
{
    public byte WebsiteId { get; set; }

    public int LabelId { get; set; }

    public string PermissiontypeId { get; set; } = null!;

    public string? Text { get; set; }

    public virtual Label Label { get; set; } = null!;

    public virtual Permissiontype Permissiontype { get; set; } = null!;

    public virtual Website Website { get; set; } = null!;
}
