using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class TriggerLogsDTO
    {
        public int LogId { get; set; }
        public string? TriggerName { get; set; }
        public DateTime? ExecutionTime { get; set; }
        public int? OrdersUpdated { get; set; }
        public int? ExecutionDurationMs { get; set; }
        public string? Details { get; set; }
    }
}
