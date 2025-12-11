using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Models;

[Table("ai_performance")]
[Index("game_id", Name = "game_id")]
public partial class ai_performance
{
    [Key]
    public int performance_id { get; set; }

    public int game_id { get; set; }

    [Column(TypeName = "enum('easy','medium','hard')")]
    public string? ai_level { get; set; }

    [Column(TypeName = "enum('minimax','alpha_beta')")]
    public string? algorithm_type { get; set; }

    [Precision(5, 2)]
    public decimal? average_depth { get; set; }

    public int? average_nodes_evaluated { get; set; }

    public int? average_move_time_ms { get; set; }

    public int? total_moves { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? created_at { get; set; }

    [ForeignKey("game_id")]
    [InverseProperty("ai_performances")]
    public virtual game game { get; set; } = null!;
}
