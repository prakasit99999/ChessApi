using System;
using System.Collections.Generic;

namespace ChessApi.Models;

public partial class system_setting
{
    public int setting_id { get; set; }

    public string setting_key { get; set; } = null!;

    public string setting_value { get; set; } = null!;

    public string? description { get; set; }

    public DateTime? updated_at { get; set; }
}
