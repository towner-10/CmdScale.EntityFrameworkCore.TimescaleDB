using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace CmdScale.EntityFrameworkCore.TimescaleDB.Operations
{
    public class AddCompressionPolicyOperation : MigrationOperation
    {
        public string TableName { get; set; } = string.Empty;
        public string Schema { get; set; } = string.Empty;

        /// <summary>
        /// The age of data after which it should be compressed.
        /// </summary>
        public string? CompressAfter { get; set; }

        /// <summary>
        /// The specific point in time before which data should be compressed.
        /// </summary>
        public string? CompressCreatedBefore { get; set; }

        public DateTime? InitialStart { get; set; }
        public string? ScheduleInterval { get; set; }
        public string? Timezone { get; set; }
    }
}
