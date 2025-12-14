using System;
using System.Collections.Generic;

namespace BBltZen;

public partial class TriggerLogs
{
    public int LogId { get; set; }

    public string? TriggerName { get; set; }

    public DateTime? ExecutionTime { get; set; }

    public int? OrdersUpdated { get; set; }

    public int? ExecutionDurationMs { get; set; }

    public string? Details { get; set; }
}
