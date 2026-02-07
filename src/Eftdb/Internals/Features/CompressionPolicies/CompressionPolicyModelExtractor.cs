using CmdScale.EntityFrameworkCore.TimescaleDB.Configuration.CompressionPolicy;
using CmdScale.EntityFrameworkCore.TimescaleDB.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace CmdScale.EntityFrameworkCore.TimescaleDB.Internals.Features.CompressionPolicies
{
    public static class CompressionPolicyModelExtractor
    {
        public static IEnumerable<AddCompressionPolicyOperation> GetCompressionPolicies(IRelationalModel? relationalModel)
        {
            if (relationalModel == null)
            {
                yield break;
            }

            foreach (IEntityType entityType in relationalModel.Model.GetEntityTypes())
            {
                // Check if the entity has been marked for a compression policy
                bool hasCompressionPolicy = entityType.FindAnnotation(CompressionPolicyAnnotations.HasCompressionPolicy)?.Value as bool? ?? false;
                if (!hasCompressionPolicy)
                {
                    continue;
                }

                // Retrieve mutually exclusive compression triggers
                string? compressAfter = entityType.FindAnnotation(CompressionPolicyAnnotations.CompressAfter)?.Value as string;
                string? compressCreatedBefore = entityType.FindAnnotation(CompressionPolicyAnnotations.CompressCreatedBefore)?.Value as string;

                // Validation: At least one must be present (though the Convention/Fluent API should have enforced this)
                if (string.IsNullOrWhiteSpace(compressAfter) && string.IsNullOrWhiteSpace(compressCreatedBefore))
                {
                    continue;
                }

                DateTime? initialStart = entityType.FindAnnotation(CompressionPolicyAnnotations.InitialStart)?.Value as DateTime?;

                yield return new AddCompressionPolicyOperation
                {
                    TableName = entityType.GetTableName()!,
                    // Fallback to your provider's default schema if not explicitly set
                    Schema = entityType.GetSchema() ?? DefaultValues.DefaultSchema,

                    CompressAfter = compressAfter,
                    CompressCreatedBefore = compressCreatedBefore,
                    InitialStart = initialStart,

                    // Scheduling and Timezone
                    ScheduleInterval = entityType.FindAnnotation(CompressionPolicyAnnotations.ScheduleInterval)?.Value as string
                                       ?? DefaultValues.CompressionPolicyScheduleInterval,

                    Timezone = entityType.FindAnnotation(CompressionPolicyAnnotations.Timezone)?.Value as string
                               ?? DefaultValues.CompressionPolicyTimezone
                };
            }
        }
    }
}
