using System;

namespace OoBDev.ComplexEvents.Abstractions.Schedulers
{
    /// <summary>
    /// When the schedule provider in instantiated it will scan the IOC container for registered IComplexEventHandlers with
    /// ScheduleAtAttributes on the implementation. These values will be sent to the ISchedulePersistenceProvider on the 
    /// RegisterIfNotExistsAsync method.  These values should be created with the application data if a matched value for 
    /// the related AssemblyQualifiedName does not already exist.  
    /// </summary>
    public interface IRegisterScheduleInstance
    {
        Type Scheduler { get; }
        string[] Schedules { get; }
        DateTimeOffset? NextStart { get; }
    }
}
