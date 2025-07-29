using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class opening_statistic
{
    public int opening_id { get; set; }

    public string opening_name { get; set; } = null!;

    public string? eco_code { get; set; }

    public string move_sequence { get; set; } = null!;

    public int? games_played { get; set; }

    public int? white_wins { get; set; }

    public int? black_wins { get; set; }

    public int? draws { get; set; }

    public int? average_rating { get; set; }

    public DateTime? last_updated { get; set; }
}
