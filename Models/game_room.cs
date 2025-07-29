using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class game_room
{
    public int room_id { get; set; }

    public string room_code { get; set; } = null!;

    public string room_name { get; set; } = null!;

    public string? room_description { get; set; }

    public int created_by_user_id { get; set; }

    public string? room_type { get; set; }

    public string? password_hash { get; set; }

    public int? max_participants { get; set; }

    public int? current_participants { get; set; }

    public string? room_status { get; set; }

    public string? game_settings { get; set; }

    public int? time_control_minutes { get; set; }

    public bool? is_rated { get; set; }

    public bool? allow_spectators { get; set; }

    public int? min_rating { get; set; }

    public int? max_rating { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? started_at { get; set; }

    public DateTime? expires_at { get; set; }

    public DateTime? last_activity { get; set; }

    public virtual user created_by_user { get; set; } = null!;

    public virtual ICollection<game> games { get; set; } = new List<game>();

    public virtual ICollection<room_chat_message> room_chat_messages { get; set; } = new List<room_chat_message>();

    public virtual ICollection<room_participant> room_participants { get; set; } = new List<room_participant>();
}
