using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class game_state
{
    public int state_id { get; set; }

    public int game_id { get; set; }

    public int move_number { get; set; }

    public string fen_position { get; set; } = null!;

    public string board_state { get; set; } = null!;

    public bool? is_check { get; set; }

    public bool? is_checkmate { get; set; }

    public bool? is_stalemate { get; set; }

    public string? castle_rights { get; set; }

    public string? en_passant_square { get; set; }

    public int? halfmove_clock { get; set; }

    public int? fullmove_number { get; set; }

    public DateTime? created_at { get; set; }

    public virtual game game { get; set; } = null!;
}
