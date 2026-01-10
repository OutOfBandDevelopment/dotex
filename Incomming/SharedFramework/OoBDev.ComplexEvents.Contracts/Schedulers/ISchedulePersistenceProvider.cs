using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Contracts.Schedulers
{
    /// <summary>
    /// This providers must be implemented within any application that chooses to use the Scheduling Engine for ComplexEvents
    /// </summary>
    public interface ISchedulePersistenceProvider
    {
        /* Assembly-Class, Schedule, NextStart, LastComplete, LastException, Disable */

        /// <summary>
        /// This will be called the in Schedule engine is initialized.  The values send in here will be based on IComplexEventHandlers
        /// registered in the IOC container with a ScheduleAt attribute defined.
        /// </summary>
        /// <param name="schedules"></param>
        /// <returns></returns>
        Task RegisterIfNotExistAsync(IEnumerable<IRegisterScheduleInstance> schedules);

        /// <summary>
        /// The application should return the count of pending events
        /// </summary>
        /// <returns></returns>
        Task<int> GetPendingCountAsync();

        /// <summary>
        /// The application should return a scheduled event that is past due 
        /// </summary>
        /// <returns></returns>
        Task<IGetScheduleInstance?> GetAndLockAsync();

        /// <summary>
        /// The application should  store the provided NextStart.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        Task ReleaseAsync(IReleaseScheduleInstance instance);
    }
}
