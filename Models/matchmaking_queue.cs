using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class matchmaking_queue
{
    public int queue_id { get; set; }

    public int user_id { get; set; }

    public int? preferred_time_control { get; set; }

    public int? min_rating { get; set; }

    public int? max_rating { get; set; }

    public DateTime? joined_at { get; set; }

    public string? status { get; set; }

    public virtual user user { get; set; } = null!;
}
