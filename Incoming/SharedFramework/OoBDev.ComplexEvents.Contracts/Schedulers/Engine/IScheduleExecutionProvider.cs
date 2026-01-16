using System;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Contracts.Schedulers.Engine
{
    /// <summary>
    /// This is the runtime to support the scheduler process
    /// </summary>
    public interface IScheduleExecutionProvider
    {
        /// <summary>
        /// this is the entry point for the scheduler runtime which will retrieve the execute the events to be processed.  
        /// </summary>
        /// <returns></returns>
        Task ExecuteAsync();

        /// <summary>
        /// Function to call to register all configured schedules
        /// </summary>
        /// <returns></returns>
        Task RegisterAllAsync();
    }
}
