using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class user_ranking
{
    public int user_id { get; set; }

    public string username { get; set; } = null!;

    public int? rating { get; set; }

    public int? games_played { get; set; }

    public int? games_won { get; set; }

    public int? games_lost { get; set; }

    public int? games_drawn { get; set; }

    public decimal? win_percentage { get; set; }

    public string? status { get; set; }

    public ulong ranking { get; set; }
}
