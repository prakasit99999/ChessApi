using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class active_game
{
    public int game_id { get; set; }

    public int? room_id { get; set; }

    public string game_type { get; set; } = null!;

    public string white_player_type { get; set; } = null!;

    public string black_player_type { get; set; } = null!;

    public string? white_username { get; set; }

    public string? black_username { get; set; }

    public string? current_turn { get; set; }

    public int? move_count { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? last_move_at { get; set; }

    public string? room_name { get; set; }

    public string? room_code { get; set; }
}
