using System;
using System.Collections.Generic;

namespace Session_5_Dennis_Hilfinger;

public partial class Role
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
