using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Models;

[Table("matchmaking_queue")]
[Index("user_id", Name = "user_id")]
public partial class matchmaking_queue
{
    [Key]
    public int queue_id { get; set; }

    public int user_id { get; set; }

    public int? min_rating { get; set; }

    public int? max_rating { get; set; }

    [Column(TypeName = "enum('waiting','matched','cancelled')")]
    public string? status { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? joined_at { get; set; }

    [ForeignKey("user_id")]
    [InverseProperty("matchmaking_queues")]
    public virtual user user { get; set; } = null!;
}
