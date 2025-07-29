using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class ai_performance
{
    public int performance_id { get; set; }

    public int game_id { get; set; }

    public string ai_level { get; set; } = null!;

    public string algorithm_type { get; set; } = null!;

    public decimal? average_depth { get; set; }

    public int? average_nodes_evaluated { get; set; }

    public int? average_move_time_ms { get; set; }

    public int? total_moves { get; set; }

    public int? opening_book_moves { get; set; }

    public int? endgame_tablebase_moves { get; set; }

    public DateTime? created_at { get; set; }

    public virtual game game { get; set; } = null!;
}
