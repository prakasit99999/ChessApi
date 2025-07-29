using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class room_participant
{
    public int participant_id { get; set; }

    public int room_id { get; set; }

    public int user_id { get; set; }

    public string role { get; set; } = null!;

    public string? player_color { get; set; }

    public bool? is_ready { get; set; }

    public DateTime? joined_at { get; set; }

    public DateTime? left_at { get; set; }

    public string? status { get; set; }

    public virtual game_room room { get; set; } = null!;

    public virtual user user { get; set; } = null!;
}
