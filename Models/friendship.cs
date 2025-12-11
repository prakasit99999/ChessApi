using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ChessApi.Models;

[Index("user1_id", Name = "user1_id")]
[Index("user2_id", Name = "user2_id")]
public partial class friendship
{
    [Key]
    public int friendship_id { get; set; }

    public int user1_id { get; set; }

    public int user2_id { get; set; }

    [Column(TypeName = "enum('pending','accepted','blocked')")]
    public string? status { get; set; }

    [Column(TypeName = "timestamp")]
    public DateTime? created_at { get; set; }

    [ForeignKey("user1_id")]
    [InverseProperty("friendshipuser1s")]
    public virtual user user1 { get; set; } = null!;

    [ForeignKey("user2_id")]
    [InverseProperty("friendshipuser2s")]
    public virtual user user2 { get; set; } = null!;
}
