
namespace OoBDev.ComplexEvents.EntityFrameworkCore
{
    public static class SchedulePersistenceProviderCommands
    {
        public const string GetRecord = "[_Scheduler].[GetRecord]";
        public const string LockRecord = "[_Scheduler].[LockRecord]";
        public const string PendingCount = "[_Scheduler].[PendingCount]";
        public const string ReleaseLock = "[_Scheduler].[ReleaseLock]";
        public const string RegisterSchedulers = "[_Scheduler].[RegisterSchedulers]";
    }
}
