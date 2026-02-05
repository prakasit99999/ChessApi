using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Models;

[Index("black_player_id", Name = "black_player_id")]
[Index("white_player_id", Name = "white_player_id")]
public partial class game
{
    [Key]
    public int game_id { get; set; }

    [Column(TypeName = "enum('single_player','ai_vs_ai','local_multiplayer','online_multiplayer')")]
    public string game_type { get; set; } = null!;

    [Column(TypeName = "enum('ranked','nomal')")]
    public string? match_mode { get; set; }

    public int? white_player_id { get; set; }

    public int? black_player_id { get; set; }

    [Column(TypeName = "enum('human','ai_easy','ai_medium','ai_hard')")]
    public string white_player_type { get; set; } = null!;

    [Column(TypeName = "enum('human','ai_easy','ai_medium','ai_hard')")]
    public string black_player_type { get; set; } = null!;

    [Column(TypeName = "enum('waiting','in_progress','paused','finished','abandoned','aborted','disconnected','active')")]
    public string? game_status { get; set; }

    [Column(TypeName = "enum('white_wins','black_wins','draw')")]
    public string? result { get; set; }

    [Column(TypeName = "enum('Checkmate','Stalemate','DrawAgreement','Timeout','InsufficientMaterial','FiftyMoveRule','ThreefoldRepitition','Resignation','Abandoned','GameAbortedEarly','Disconnected','SystemTerminated')")]
    public string? result_reason { get; set; }

    public int? move_count { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? created_at { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? started_at { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? finished_at { get; set; }

    [InverseProperty("game")]
    public virtual ICollection<ai_performance> ai_performances { get; set; } = new List<ai_performance>();

    [ForeignKey("black_player_id")]
    [InverseProperty("gameblack_players")]
    public virtual user? black_player { get; set; }

    [InverseProperty("game")]
    public virtual ICollection<game_state> game_states { get; set; } = new List<game_state>();

    [InverseProperty("game")]
    public virtual ICollection<move> moves { get; set; } = new List<move>();

    [ForeignKey("white_player_id")]
    [InverseProperty("gamewhite_players")]
    public virtual user? white_player { get; set; }
}
