using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class friendship
{
    public int friendship_id { get; set; }

    public int user1_id { get; set; }

    public int user2_id { get; set; }

    public string? status { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual user user1 { get; set; } = null!;

    public virtual user user2 { get; set; } = null!;
}
