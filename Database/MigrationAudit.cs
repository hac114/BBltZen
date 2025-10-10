using System;
using System.Collections.Generic;

namespace Database;

public partial class MigrationAudit
{
    public int AuditId { get; set; }

    public string OperationType { get; set; } = null!;

    public string ObjectName { get; set; } = null!;

    public string ObjectType { get; set; } = null!;

    public string MigrationPhase { get; set; } = null!;

    public string? ExecutedBy { get; set; }

    public DateTime? ExecutedAt { get; set; }

    public string? Notes { get; set; }
}
