using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Models;

[Index("game_id", Name = "game_id")]
public partial class move
{
    [Key]
    public int move_id { get; set; }

    public int game_id { get; set; }

    public int move_number { get; set; }

    public sbyte start_x { get; set; }

    public sbyte start_y { get; set; }

    public sbyte end_x { get; set; }

    public sbyte end_y { get; set; }

    public sbyte? captured_x { get; set; }

    public sbyte? captured_y { get; set; }

    public sbyte? promoted_x { get; set; }

    public sbyte? promoted_y { get; set; }

    public sbyte? enpassant_x { get; set; }

    public sbyte? enpassant_y { get; set; }

    [Column(TypeName = "enum('pawn','rook','knight','bishop','queen','king')")]
    public string piece_type { get; set; } = null!;

    [Column(TypeName = "enum('white','black')")]
    public string team { get; set; } = null!;

    [Column(TypeName = "enum('pawn','rook','knight','bishop','queen','king')")]
    public string? captured_piece_type { get; set; }

    [Column(TypeName = "enum('white','black')")]
    public string? captured_piece_team { get; set; }

    [Column(TypeName = "enum('pawn','rook','knight','bishop','queen','king')")]
    public string? promoted_from { get; set; }

    [Column(TypeName = "enum('pawn','rook','knight','bishop','queen','king')")]
    public string? promoted_to { get; set; }

    public bool? is_castling { get; set; }

    public bool? is_en_passant { get; set; }

    public bool? is_capture { get; set; }

    public bool? is_check { get; set; }

    public bool? is_pawn_two_step { get; set; }

    public bool? piece_has_moved_before { get; set; }

    [Column(TypeName = "enum('minimax','alpha_beta')")]
    public string? algorithm_type { get; set; }

    [Precision(8, 3)]
    public decimal? ai_evaluation_score { get; set; }

    public int? ai_depth_searched { get; set; }

    public int? ai_nodes_evaluated { get; set; }

    public int? move_time_ms { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? created_at { get; set; }

    [ForeignKey("game_id")]
    [InverseProperty("moves")]
    public virtual game game { get; set; } = null!;
}
