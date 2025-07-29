using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class user_statistic
{
    public int stat_id { get; set; }

    public int user_id { get; set; }

    public string period_type { get; set; } = null!;

    public DateOnly period_date { get; set; }

    public int? games_played { get; set; }

    public int? games_won { get; set; }

    public int? games_lost { get; set; }

    public int? games_drawn { get; set; }

    public int? rating_change { get; set; }

    public decimal? average_game_length_moves { get; set; }

    public string? favorite_opening { get; set; }

    public decimal? win_rate_as_white { get; set; }

    public decimal? win_rate_as_black { get; set; }

    public DateTime? updated_at { get; set; }

    public virtual user user { get; set; } = null!;
}
