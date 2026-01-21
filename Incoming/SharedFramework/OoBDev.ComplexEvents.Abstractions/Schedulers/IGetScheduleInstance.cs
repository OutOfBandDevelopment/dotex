using System;

namespace OoBDev.ComplexEvents.Abstractions.Schedulers
{
    /// <summary>
    /// These will be returned by the GetPendingSchedulesAsync on the registered ISchedulePersistenceProvider.  These values
    /// should be selected based on requestTime &gt;= NextStart. 
    /// </summary>
    public interface IGetScheduleInstance
    {
        /// <summary>
        /// map this as your reference key
        /// </summary>
        string ReferenceKey { get; }
        /// <summary>
        /// Resolve your scheduler type and bind here
        /// </summary>
        Type Scheduler { get; }
        /// <summary>
        /// If this is provided and valid it will be used to calculate the NextStart sent to ISchedulePersistenceProvider
        /// UpdateScheduleAsync function.  
        /// </summary>
        string[] Schedules { get; }
    }
}
