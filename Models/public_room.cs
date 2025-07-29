using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class public_room
{
    public int room_id { get; set; }

    public string room_code { get; set; } = null!;

    public string room_name { get; set; } = null!;

    public string? room_description { get; set; }

    public string host_username { get; set; } = null!;

    public int? current_participants { get; set; }

    public int? max_participants { get; set; }

    public string? room_status { get; set; }

    public int? time_control_minutes { get; set; }

    public bool? is_rated { get; set; }

    public bool? allow_spectators { get; set; }

    public int? min_rating { get; set; }

    public int? max_rating { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? last_activity { get; set; }

    public string availability_status { get; set; } = null!;
}
