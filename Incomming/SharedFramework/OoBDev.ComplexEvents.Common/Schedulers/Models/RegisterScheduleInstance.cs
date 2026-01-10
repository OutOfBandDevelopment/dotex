using OoBDev.ComplexEvents.Contracts.Schedulers;
using System;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.ComplexEvents.Common.Schedulers.Models
{
    [ExcludeFromCodeCoverage]
    public class RegisterScheduleInstance : IRegisterScheduleInstance
    {
        public Type Scheduler { get; set; } = typeof(object);
        public string[] Schedules { get; set; } = Array.Empty<string>();
        public DateTimeOffset? NextStart { get; set; }
    }
}

