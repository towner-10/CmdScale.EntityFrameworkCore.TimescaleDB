using System;

namespace CmdScale.EntityFrameworkCore.TimescaleDB.Configuration.CompressionPolicy
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CompressionPolicyAttribute : Attribute
    {
        /// <summary>
        /// The age after which the policy job compresses chunks, specified as an interval string (e.g., "30 days") or integer.
        /// Mutually exclusive with <see cref="CompressCreatedBefore"/>.
        /// </summary>
        public string? CompressAfter { get; set; }

        /// <summary>
        /// Chunks with creation time older than this cut-off point are compressed. Specified as an interval string (e.g., "30 days").
        /// Mutually exclusive with <see cref="CompressAfter"/>.
        /// </summary>
        public string? CompressCreatedBefore { get; set; }

        /// <summary>
        /// The interval between the finish time of the last execution and the next start.
        /// </summary>
        public string? ScheduleInterval { get; set; }

        /// <summary>
        /// Time the policy is first run. If omitted, then the schedule interval is the interval from the finish time of the last execution to the next start. If provided, it serves as the origin with respect to which the next_start is calculated.
        /// </summary>
        public string? InitialStart { get; set; }

        /// <summary>
        /// A valid time zone. If <see cref="InitialStart"/> is provided, the time zone is used to calculate the next start time. If <see cref="InitialStart"/> is not provided, the time zone is used to determine the current time for calculating the next start time based on the schedule interval.
        /// </summary>
        public string? Timezone { get; set; }

        /// <summary>
        /// Initializes the attribute. 
        /// Ensures the mutual exclusivity of CompressAfter and CompressCreatedBefore.
        /// </summary>
        public CompressionPolicyAttribute(string? compressAfter = null, string? compressCreatedBefore = null)
        {
            if (compressAfter != null && compressCreatedBefore != null)
                throw new ArgumentException($"{nameof(CompressAfter)} and {nameof(CompressCreatedBefore)} are mutually exclusive.");

            if (compressAfter == null && compressCreatedBefore == null)
                throw new ArgumentException($"Either {nameof(CompressAfter)} or {nameof(CompressCreatedBefore)} must be provided.");

            CompressAfter = compressAfter;
            CompressCreatedBefore = compressCreatedBefore;
        }
    }
}
