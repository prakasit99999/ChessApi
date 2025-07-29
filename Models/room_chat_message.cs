using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class room_chat_message
{
    public int message_id { get; set; }

    public int room_id { get; set; }

    public int user_id { get; set; }

    public string? message_type { get; set; }

    public string message_content { get; set; } = null!;

    public DateTime? created_at { get; set; }

    public DateTime? expires_at { get; set; }

    public virtual game_room room { get; set; } = null!;

    public virtual user user { get; set; } = null!;
}
