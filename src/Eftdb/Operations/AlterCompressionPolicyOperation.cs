using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace CmdScale.EntityFrameworkCore.TimescaleDB.Operations
{
    public class AlterCompressionPolicyOperation : MigrationOperation
    {
        public string TableName { get; set; } = string.Empty;
        public string Schema { get; set; } = string.Empty;

        // Current Values
        public string? CompressAfter { get; set; }
        public string? CompressCreatedBefore { get; set; }
        public DateTime? InitialStart { get; set; }
        public string? ScheduleInterval { get; set; }
        public string? Timezone { get; set; }

        // Old Values (for diffing)
        public string? OldCompressAfter { get; set; }
        public string? OldCompressCreatedBefore { get; set; }
        public DateTime? OldInitialStart { get; set; }
        public string? OldScheduleInterval { get; set; }
        public string? OldTimezone { get; set; }
    }
}
