using CmdScale.EntityFrameworkCore.TimescaleDB.Configuration.CompressionPolicy;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using static CmdScale.EntityFrameworkCore.TimescaleDB.Design.Scaffolding.CompressionPolicyScaffoldingExtractor;

namespace CmdScale.EntityFrameworkCore.TimescaleDB.Design.Scaffolding
{
    /// <summary>
    /// Applies compression policy annotations to scaffolded database tables.
    /// </summary>
    public sealed class CompressionPolicyAnnotationApplier : IAnnotationApplier
    {
        public void ApplyAnnotations(DatabaseTable table, object featureInfo)
        {
            if (featureInfo is not CompressionPolicyInfo policyInfo)
            {
                throw new ArgumentException($"Expected {nameof(CompressionPolicyInfo)}, got {featureInfo.GetType().Name}", nameof(featureInfo));
            }

            table[CompressionPolicyAnnotations.HasCompressionPolicy] = true;

            // Apply the primary logic (After vs CreatedBefore)
            if (!string.IsNullOrWhiteSpace(policyInfo.CompressAfter))
            {
                table[CompressionPolicyAnnotations.CompressAfter] = policyInfo.CompressAfter;
            }
            else if (!string.IsNullOrWhiteSpace(policyInfo.CompressCreatedBefore))
            {
                table[CompressionPolicyAnnotations.CompressCreatedBefore] = policyInfo.CompressCreatedBefore;
            }

            if (policyInfo.InitialStart.HasValue)
            {
                table[CompressionPolicyAnnotations.InitialStart] = policyInfo.InitialStart.Value;
            }

            // Only annotate if values deviate from TimescaleDB/Provider defaults to keep the code clean
            if (policyInfo.ScheduleInterval != DefaultValues.CompressionPolicyScheduleInterval)
            {
                table[CompressionPolicyAnnotations.ScheduleInterval] = policyInfo.ScheduleInterval;
            }

            if (policyInfo.Timezone != DefaultValues.CompressionPolicyTimezone)
            {
                table[CompressionPolicyAnnotations.Timezone] = policyInfo.Timezone;
            }
        }
    }
}
