using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class game
{
    public int game_id { get; set; }

    public int? room_id { get; set; }

    public string game_type { get; set; } = null!;

    public int? white_player_id { get; set; }

    public int? black_player_id { get; set; }

    public string white_player_type { get; set; } = null!;

    public string black_player_type { get; set; } = null!;

    public string? game_status { get; set; }

    public string? result { get; set; }

    public string? result_reason { get; set; }

    public string? current_turn { get; set; }

    public int? move_count { get; set; }

    public int? white_rating_before { get; set; }

    public int? black_rating_before { get; set; }

    public int? white_rating_after { get; set; }

    public int? black_rating_after { get; set; }

    public int? time_control_minutes { get; set; }

    public bool? is_rated { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? started_at { get; set; }

    public DateTime? finished_at { get; set; }

    public DateTime? last_move_at { get; set; }

    public virtual ICollection<ai_performance> ai_performances { get; set; } = new List<ai_performance>();

    public virtual user? black_player { get; set; }

    public virtual ICollection<game_state> game_states { get; set; } = new List<game_state>();

    public virtual ICollection<move> moves { get; set; } = new List<move>();

    public virtual game_room? room { get; set; }

    public virtual user? white_player { get; set; }
}
