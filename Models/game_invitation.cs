using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class game_invitation
{
    public int invitation_id { get; set; }

    public int from_user_id { get; set; }

    public int to_user_id { get; set; }

    public int? time_control_minutes { get; set; }

    public string? message { get; set; }

    public string? status { get; set; }

    public DateTime? created_at { get; set; }

    public DateTime? expires_at { get; set; }

    public DateTime? responded_at { get; set; }

    public virtual user from_user { get; set; } = null!;

    public virtual user to_user { get; set; } = null!;
}
