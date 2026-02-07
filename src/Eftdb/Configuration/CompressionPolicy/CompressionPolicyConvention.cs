using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Reflection;

namespace CmdScale.EntityFrameworkCore.TimescaleDB.Configuration.CompressionPolicy
{
    /// <summary>
    /// A convention that configures the compression policy for a hypertable based on the presence of
    /// the [CompressionPolicy] attribute.
    /// </summary>
    public class CompressionPolicyConvention : IEntityTypeAddedConvention
    {
        /// <summary>
        /// Called when an entity type is added to the model.
        /// </summary>
        /// <param name="entityTypeBuilder">The builder for the entity type.</param>
        /// <param name="context">Additional information available during convention execution.</param>
        public void ProcessEntityTypeAdded(IConventionEntityTypeBuilder entityTypeBuilder, IConventionContext<IConventionEntityTypeBuilder> context)
        {
            IConventionEntityType entityType = entityTypeBuilder.Metadata;
            CompressionPolicyAttribute? attribute = entityType.ClrType?.GetCustomAttribute<CompressionPolicyAttribute>();

            if (attribute != null)
            {
                // Core flag to indicate this hypertable has a compression policy
                entityTypeBuilder.HasAnnotation(CompressionPolicyAnnotations.HasCompressionPolicy, true);

                // Handle the mutual exclusivity logic
                if (!string.IsNullOrWhiteSpace(attribute.CompressAfter))
                {
                    entityTypeBuilder.HasAnnotation(CompressionPolicyAnnotations.CompressAfter, attribute.CompressAfter);
                }
                else if (!string.IsNullOrWhiteSpace(attribute.CompressCreatedBefore))
                {
                    entityTypeBuilder.HasAnnotation(CompressionPolicyAnnotations.CompressCreatedBefore, attribute.CompressCreatedBefore);
                }

                // Handle common scheduling annotations
                if (!string.IsNullOrWhiteSpace(attribute.ScheduleInterval))
                    entityTypeBuilder.HasAnnotation(CompressionPolicyAnnotations.ScheduleInterval, attribute.ScheduleInterval);

                if (!string.IsNullOrWhiteSpace(attribute.InitialStart))
                {
                    if (DateTime.TryParse(attribute.InitialStart, out DateTime parsedDateTimeOffset))
                    {
                        entityTypeBuilder.HasAnnotation(CompressionPolicyAnnotations.InitialStart, parsedDateTimeOffset);
                    }
                    else
                    {
                        throw new InvalidOperationException($"InitialStart '{attribute.InitialStart}' is not a valid DateTime format.");
                    }
                }

                if (!string.IsNullOrWhiteSpace(attribute.Timezone))
                    entityTypeBuilder.HasAnnotation(CompressionPolicyAnnotations.Timezone, attribute.Timezone);
            }
        }
    }
}
