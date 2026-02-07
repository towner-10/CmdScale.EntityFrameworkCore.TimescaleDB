using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace CmdScale.EntityFrameworkCore.TimescaleDB.Operations
{
    public class DropCompressionPolicyOperation : MigrationOperation
    {
        public string TableName { get; set; } = string.Empty;
        public string Schema { get; set; } = string.Empty;
    }
}
