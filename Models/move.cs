using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class move
{
    public int move_id { get; set; }

    public int game_id { get; set; }

    public int move_number { get; set; }

    public string player_color { get; set; } = null!;

    public string piece_type { get; set; } = null!;

    public string from_square { get; set; } = null!;

    public string to_square { get; set; } = null!;

    public string move_notation { get; set; } = null!;

    public string? captured_piece { get; set; }

    public string? promotion_piece { get; set; }

    public bool? is_castle { get; set; }

    public string? castle_type { get; set; }

    public bool? is_en_passant { get; set; }

    public bool? is_check { get; set; }

    public bool? is_checkmate { get; set; }

    public int? move_time_seconds { get; set; }

    public decimal? ai_evaluation_score { get; set; }

    public int? ai_depth_searched { get; set; }

    public int? ai_nodes_evaluated { get; set; }

    public DateTime? created_at { get; set; }

    public virtual game game { get; set; } = null!;
}
