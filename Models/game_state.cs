using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Models;

[Index("game_id", Name = "game_id")]
public partial class game_state
{
    [Key]
    public int state_id { get; set; }

    public int game_id { get; set; }

    public int move_number { get; set; }

    [StringLength(100)]
    public string fen_position { get; set; } = null!;

    [Column(TypeName = "json")]
    public string board_state { get; set; } = null!;

    public bool? is_check { get; set; }

    public bool? is_checkmate { get; set; }

    public bool? is_stalemate { get; set; }

    public ulong? zobrist_hash { get; set; }

    [Column(TypeName = "text")]
    public string? pv_line { get; set; }

    public float? position_score { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? created_at { get; set; }

    [ForeignKey("game_id")]
    [InverseProperty("game_states")]
    public virtual game game { get; set; } = null!;
}
