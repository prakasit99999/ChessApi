using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Models;

[Index("email", Name = "email", IsUnique = true)]
[Index("username", Name = "username", IsUnique = true)]
public partial class user
{
    [Key]
    public int user_id { get; set; }

    [StringLength(50)]
    public string username { get; set; } = null!;

    [StringLength(100)]
    public string? email { get; set; }

    [Column(TypeName = "text")]
    public string password_hash { get; set; } = null!;

    public int? rating { get; set; }

    public int? games_played { get; set; }

    public int? games_won { get; set; }

    public int? games_lost { get; set; }

    public int? games_drawn { get; set; }

    [Column(TypeName = "enum('offline','online','playing')")]
    public string? status { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? created_at { get; set; }

    [InverseProperty("black_player")]
    public virtual ICollection<game> gameblack_players { get; set; } = new List<game>();

    [InverseProperty("white_player")]
    public virtual ICollection<game> gamewhite_players { get; set; } = new List<game>();

    [InverseProperty("user")]
    public virtual ICollection<matchmaking_queue> matchmaking_queues { get; set; } = new List<matchmaking_queue>();
}
