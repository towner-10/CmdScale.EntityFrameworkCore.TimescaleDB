using System.Data;
using System.Data.Common;

namespace CmdScale.EntityFrameworkCore.TimescaleDB.Design.Scaffolding
{
    /// <summary>
    /// Extracts compression policy metadata from a TimescaleDB database for scaffolding.
    /// </summary>
    public sealed class CompressionPolicyScaffoldingExtractor : ITimescaleFeatureExtractor
    {
        public sealed record CompressionPolicyInfo(
            string? CompressAfter,
            string? CompressCreatedBefore,
            DateTime? InitialStart,
            string? ScheduleInterval,
            string? Timezone
        );

        public Dictionary<(string Schema, string TableName), object> Extract(DbConnection connection)
        {
            bool wasOpen = connection.State == ConnectionState.Open;
            if (!wasOpen)
            {
                connection.Open();
            }

            try
            {
                Dictionary<(string, string), CompressionPolicyInfo> compressionPolicies = [];
                using (DbCommand command = connection.CreateCommand())
                {
                    // Note: 'compress_after' and 'compress_created_before' are inside the config jsonb.
                    // 'timezone' is also an optional parameter in the compression policy config.
                    command.CommandText = @"
                        SELECT
                            j.hypertable_schema,
                            j.hypertable_name,
                            j.config ->> 'compress_after' AS compress_after,
                            j.config ->> 'compress_created_before' AS compress_created_before,
                            j.initial_start,
                            j.schedule_interval::text,
                            j.config ->> 'timezone' AS timezone
                        FROM timescaledb_information.jobs AS j
                        WHERE j.proc_name = 'policy_compression';";

                    using DbDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        string schema = reader.GetString(0);
                        string name = reader.GetString(1);

                        string? compressAfter = reader.IsDBNull(2) ? null : reader.GetString(2);
                        string? compressCreatedBefore = reader.IsDBNull(3) ? null : reader.GetString(3);
                        DateTime? initialStart = reader.IsDBNull(4) ? null : reader.GetDateTime(4);
                        string? scheduleInterval = reader.IsDBNull(5) ? null : reader.GetString(5);
                        string? timezone = reader.IsDBNull(6) ? null : reader.GetString(6);

                        // A valid policy must have at least one of these two
                        if (!string.IsNullOrEmpty(compressAfter) || !string.IsNullOrEmpty(compressCreatedBefore))
                        {
                            compressionPolicies[(schema, name)] = new CompressionPolicyInfo(
                                compressAfter,
                                compressCreatedBefore,
                                initialStart,
                                scheduleInterval,
                                timezone
                            );
                        }
                    }
                }

                return compressionPolicies.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (object)kvp.Value
                );
            }
            finally
            {
                if (!wasOpen)
                {
                    connection.Close();
                }
            }
        }
    }
}
