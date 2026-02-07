using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CmdScale.EntityFrameworkCore.TimescaleDB.Configuration.CompressionPolicy
{
    /// <summary>
    /// Provides extension methods for configuring TimescaleDB hypertable compression policies using the EF Core Fluent API.
    /// </summary>
    public static class CompressionPolicyTypeBuilder
    {
        /// <summary>
        /// Configures a TimescaleDB compression policy for the entity using a fluent API.
        /// </summary>
        /// <remarks>
        /// A compression policy automatically compresses chunks based on age or a specific creation date.
        /// This method applies the necessary annotations to the entity's metadata for migration generation.
        /// </remarks>
        /// <example>
        /// <code>
        /// modelBuilder.Entity&lt;DeviceReading&gt;()
        ///     .WithCompressionPolicy(
        ///         compressAfter: "30 days",
        ///         scheduleInterval: "1 day",
        ///         timezone: "UTC");
        /// </code>
        /// </example>
        /// <typeparam name="TEntity">The type of the entity being configured.</typeparam>
        /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
        /// <param name="compressAfter">The age of data after which it should be compressed (e.g., '7 days'). Mutually exclusive with <paramref name="compressCreatedBefore"/>.</param>
        /// <param name="compressCreatedBefore">Compresses chunks created before this absolute time. Mutually exclusive with <paramref name="compressAfter"/>.</param>
        /// <param name="initialStart">The first time the policy job is scheduled to run.</param>
        /// <param name="scheduleInterval">The interval at which the compression policy job runs.</param>
        /// <param name="timezone">The timezone used when evaluating the policy schedule.</param>
        /// <returns>The same builder instance so that multiple calls can be chained.</returns>
        public static EntityTypeBuilder<TEntity> WithCompressionPolicy<TEntity>(
            this EntityTypeBuilder<TEntity> entityTypeBuilder,
            string? compressAfter = null,
            string? compressCreatedBefore = null,
            DateTime? initialStart = null,
            string? scheduleInterval = null,
            string? timezone = null) where TEntity : class
        {
            if (compressAfter != null && compressCreatedBefore != null)
                throw new ArgumentException("Cannot specify both compressAfter and compressCreatedBefore.");

            if (compressAfter == null && compressCreatedBefore == null)
                throw new ArgumentException("Either compressAfter or compressCreatedBefore must be provided.");

            entityTypeBuilder.HasAnnotation(CompressionPolicyAnnotations.HasCompressionPolicy, true);

            if (!string.IsNullOrWhiteSpace(compressAfter))
                entityTypeBuilder.HasAnnotation(CompressionPolicyAnnotations.CompressAfter, compressAfter);

            if (!string.IsNullOrWhiteSpace(compressCreatedBefore))
                entityTypeBuilder.HasAnnotation(CompressionPolicyAnnotations.CompressCreatedBefore, compressCreatedBefore);

            if (initialStart.HasValue)
                entityTypeBuilder.HasAnnotation(CompressionPolicyAnnotations.InitialStart, initialStart.Value);

            if (!string.IsNullOrWhiteSpace(scheduleInterval))
                entityTypeBuilder.HasAnnotation(CompressionPolicyAnnotations.ScheduleInterval, scheduleInterval);

            if (!string.IsNullOrWhiteSpace(timezone))
                entityTypeBuilder.HasAnnotation(CompressionPolicyAnnotations.Timezone, timezone);

            return entityTypeBuilder;
        }
    }
}
