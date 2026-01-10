using System;

namespace OoBDev.ComplexEvents.Contracts.Schedulers
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class ScheduleAtAttribute : Attribute
    {
        public ScheduleAtAttribute(string defaultSchedule) => DefaultSchedule = defaultSchedule;

        public string DefaultSchedule { get; }
    }
}
