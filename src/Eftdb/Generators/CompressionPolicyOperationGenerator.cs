using CmdScale.EntityFrameworkCore.TimescaleDB.Operations;
using System.Globalization;

namespace CmdScale.EntityFrameworkCore.TimescaleDB.Generators
{
    public class CompressionPolicyOperationGenerator
    {
        private readonly string quoteString = "\"";
        private readonly SqlBuilderHelper sqlHelper;

        public CompressionPolicyOperationGenerator(bool isDesignTime = false)
        {
            if (isDesignTime)
            {
                quoteString = "\"\"";
            }

            sqlHelper = new SqlBuilderHelper(quoteString);
        }

        public List<string> Generate(AddCompressionPolicyOperation operation)
        {
            List<string> statements =
            [
                BuildAddCompressionPolicySql(operation)
            ];

            List<string> alterJobClauses = BuildAlterJobClauses(operation);
            if (alterJobClauses.Count != 0)
            {
                statements.Add(BuildAlterJobSql(operation.TableName, operation.Schema, alterJobClauses));
            }

            return statements;
        }

        public List<string> Generate(AlterCompressionPolicyOperation operation)
        {
            string qualifiedTableName = sqlHelper.Regclass(operation.TableName, operation.Schema);
            List<string> statements = [];

            // Compression policies require recreation if the fundamental logic (after vs before) or the target time changes
            bool needsRecreation = operation.CompressAfter != operation.OldCompressAfter ||
                                   operation.CompressCreatedBefore != operation.OldCompressCreatedBefore ||
                                   operation.InitialStart != operation.OldInitialStart ||
                                   operation.Timezone != operation.OldTimezone;

            if (needsRecreation)
            {
                statements.Add($"SELECT remove_compression_policy({qualifiedTableName}, if_exists => true);");
                statements.Add(BuildAddCompressionPolicySql(operation));

                // Re-apply scheduling configurations that aren't part of the 'add' function
                List<string> finalStateClauses = BuildAlterJobClauses(operation);
                if (finalStateClauses.Count != 0)
                {
                    statements.Add(BuildAlterJobSql(operation.TableName, operation.Schema, finalStateClauses));
                }
            }
            else
            {
                List<string> changedClauses = BuildAlterJobClauses(operation);
                if (changedClauses.Count != 0)
                {
                    statements.Add(BuildAlterJobSql(operation.TableName, operation.Schema, changedClauses));
                }
            }

            return statements;
        }

        public List<string> Generate(DropCompressionPolicyOperation operation)
        {
            string qualifiedTableName = sqlHelper.Regclass(operation.TableName, operation.Schema);
            return [$"SELECT remove_compression_policy({qualifiedTableName}, if_exists => true);"];
        }

        private string BuildAddCompressionPolicySql(AddCompressionPolicyOperation operation)
        {
            string qualifiedTableName = sqlHelper.Regclass(operation.TableName, operation.Schema);
            List<string> args = [];

            // Handle Mutually Exclusive Arguments
            if (!string.IsNullOrWhiteSpace(operation.CompressAfter))
            {
                // Logic to determine if it's an interval or integer based on context could be added here
                args.Add($"compress_after => INTERVAL '{operation.CompressAfter}'");
            }
            else if (!string.IsNullOrWhiteSpace(operation.CompressCreatedBefore))
            {
                args.Add($"compress_created_before => '{operation.CompressCreatedBefore}'");
            }

            if (operation.InitialStart.HasValue)
            {
                string timestamp = operation.InitialStart.Value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
                args.Add($"initial_start => '{timestamp}'");
            }

            if (!string.IsNullOrWhiteSpace(operation.Timezone))
            {
                args.Add($"timezone => '{operation.Timezone}'");
            }

            return $"SELECT add_compression_policy({qualifiedTableName}, {string.Join(", ", args)});";
        }

        private string BuildAddCompressionPolicySql(AlterCompressionPolicyOperation operation)
        {
            string qualifiedTableName = sqlHelper.Regclass(operation.TableName, operation.Schema);
            List<string> args = [];

            // Handle Mutually Exclusive Arguments
            if (!string.IsNullOrWhiteSpace(operation.CompressAfter))
            {
                // Logic to determine if it's an interval or integer based on context could be added here
                args.Add($"compress_after => INTERVAL '{operation.CompressAfter}'");
            }
            else if (!string.IsNullOrWhiteSpace(operation.CompressCreatedBefore))
            {
                args.Add($"compress_created_before => '{operation.CompressCreatedBefore}'");
            }

            if (operation.InitialStart.HasValue)
            {
                string timestamp = operation.InitialStart.Value.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
                args.Add($"initial_start => '{timestamp}'");
            }

            if (!string.IsNullOrWhiteSpace(operation.Timezone))
            {
                args.Add($"timezone => '{operation.Timezone}'");
            }

            return $"SELECT add_compression_policy({qualifiedTableName}, {string.Join(", ", args)});";
        }

        private static List<string> BuildAlterJobClauses(AddCompressionPolicyOperation operation)
        {
            List<string> clauses = [];

            if (!string.IsNullOrWhiteSpace(operation.ScheduleInterval))
                clauses.Add($"schedule_interval => INTERVAL '{operation.ScheduleInterval}'");

            return clauses;
        }

        private static List<string> BuildAlterJobClauses(AlterCompressionPolicyOperation operation)
        {
            List<string> clauses = [];

            if (operation.ScheduleInterval != operation.OldScheduleInterval)
                clauses.Add($"schedule_interval => INTERVAL '{operation.ScheduleInterval}'");

            // Standard job parameter diffing logic...
            return clauses;
        }

        private static string BuildAlterJobSql(string tableName, string schema, IEnumerable<string> clauses)
        {
            return $@"
                SELECT alter_job(job_id, {string.Join(", ", clauses)})
                FROM timescaledb_information.jobs
                WHERE proc_name = 'policy_compression' AND hypertable_schema = '{schema}' AND hypertable_name = '{tableName}';".Trim();
        }
    }
}
