using OoBDev.ComplexEvents.Contracts.Schedulers;
using System;

namespace OoBDev.ComplexEvents.EntityFrameworkCore
{
    public class GetScheduleInstance : IGetScheduleInstance
    {
        public GetScheduleInstance(
            string referenceKey,
            Type scheduler,
            string[] schedules
            )
        {
            ReferenceKey = referenceKey;
            Scheduler = scheduler;
            Schedules = schedules;
        }
        public string ReferenceKey { get; }
        public Type Scheduler { get; }
        public string[] Schedules { get; }
    }
}
