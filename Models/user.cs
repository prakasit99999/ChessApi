using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class user
{
    public int user_id { get; set; }

    public string username { get; set; } = null!;

    public string? email { get; set; }

    public string password_hash { get; set; } = null!;

    public int? rating { get; set; }

    public int? games_played { get; set; }

    public int? games_won { get; set; }

    public int? games_lost { get; set; }

    public int? games_drawn { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? last_login { get; set; }

    public string? status { get; set; }

    public virtual ICollection<friendship> friendshipuser1s { get; set; } = new List<friendship>();

    public virtual ICollection<friendship> friendshipuser2s { get; set; } = new List<friendship>();

    public virtual ICollection<game_invitation> game_invitationfrom_users { get; set; } = new List<game_invitation>();

    public virtual ICollection<game_invitation> game_invitationto_users { get; set; } = new List<game_invitation>();

    public virtual ICollection<game_room> game_rooms { get; set; } = new List<game_room>();

    public virtual ICollection<game> gameblack_players { get; set; } = new List<game>();

    public virtual ICollection<game> gamewhite_players { get; set; } = new List<game>();

    public virtual ICollection<matchmaking_queue> matchmaking_queues { get; set; } = new List<matchmaking_queue>();

    public virtual ICollection<room_chat_message> room_chat_messages { get; set; } = new List<room_chat_message>();

    public virtual ICollection<room_participant> room_participants { get; set; } = new List<room_participant>();

    public virtual ICollection<user_statistic> user_statistics { get; set; } = new List<user_statistic>();
}
