using CmdScale.EntityFrameworkCore.TimescaleDB.Operations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace CmdScale.EntityFrameworkCore.TimescaleDB.Internals.Features.CompressionPolicies
{
    public class CompressionPolicyDiffer : IFeatureDiffer
    {
        public IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel? source, IRelationalModel? target)
        {
            List<MigrationOperation> operations = [];

            // Extract policies from metadata via the ModelExtractor
            List<AddCompressionPolicyOperation> sourcePolicies = [.. CompressionPolicyModelExtractor.GetCompressionPolicies(source)];
            List<AddCompressionPolicyOperation> targetPolicies = [.. CompressionPolicyModelExtractor.GetCompressionPolicies(target)];

            // 1. Identify new compression policies
            IEnumerable<AddCompressionPolicyOperation> newPolicies = targetPolicies
                .Where(t => !sourcePolicies.Any(s => s.TableName == t.TableName && s.Schema == t.Schema));
            operations.AddRange(newPolicies);

            // 2. Identify updated compression policies (Alter)
            var updatedPolicies = targetPolicies
                .Join(
                    sourcePolicies,
                    t => (t.Schema, t.TableName),
                    s => (s.Schema, s.TableName),
                    (targetPolicy, sourcePolicy) => new { Target = targetPolicy, Source = sourcePolicy }
                )
                .Where(x =>
                    x.Target.CompressAfter != x.Source.CompressAfter ||
                    x.Target.CompressCreatedBefore != x.Source.CompressCreatedBefore ||
                    x.Target.InitialStart != x.Source.InitialStart ||
                    x.Target.ScheduleInterval != x.Source.ScheduleInterval ||
                    x.Target.Timezone != x.Source.Timezone
                );

            foreach (var policy in updatedPolicies)
            {
                operations.Add(new AlterCompressionPolicyOperation
                {
                    TableName = policy.Target.TableName,
                    Schema = policy.Target.Schema,

                    CompressAfter = policy.Target.CompressAfter,
                    CompressCreatedBefore = policy.Target.CompressCreatedBefore,
                    InitialStart = policy.Target.InitialStart,
                    ScheduleInterval = policy.Target.ScheduleInterval,
                    Timezone = policy.Target.Timezone,

                    OldCompressAfter = policy.Source.CompressAfter,
                    OldCompressCreatedBefore = policy.Source.CompressCreatedBefore,
                    OldInitialStart = policy.Source.InitialStart,
                    OldScheduleInterval = policy.Source.ScheduleInterval,
                    OldTimezone = policy.Source.Timezone
                });
            }

            // 3. Identify removed compression policies (Drop)
            IEnumerable<DropCompressionPolicyOperation> removedPolicies = sourcePolicies
                .Where(s => !targetPolicies.Any(t => t.TableName == s.TableName && t.Schema == s.Schema))
                .Select(p => new DropCompressionPolicyOperation { TableName = p.TableName, Schema = p.Schema });

            operations.AddRange(removedPolicies);

            return operations;
        }
    }
}
