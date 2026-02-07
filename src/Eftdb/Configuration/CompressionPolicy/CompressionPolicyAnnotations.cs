namespace CmdScale.EntityFrameworkCore.TimescaleDB.Configuration.CompressionPolicy
{
    /// <summary>
    /// Contains constants for annotations used by the TimescaleDB provider extension.
    /// </summary>
    public static class CompressionPolicyAnnotations
    {
        public const string HasCompressionPolicy = "TimescaleDB:HasCompressionPolicy";
        
        public const string CompressAfter = "TimescaleDB:CompressionPolicy:CompressAfter";
        public const string CompressCreatedBefore = "TimescaleDB:CompressionPolicy:CompressCreatedBefore";
        public const string ScheduleInterval = "TimescaleDB:CompressionPolicy:ScheduleInterval";
        public const string InitialStart = "TimescaleDB:CompressionPolicy:InitialStart";
        public const string Timezone =  "TimescaleDB:CompressionPolicy:Timezone";
    }
}