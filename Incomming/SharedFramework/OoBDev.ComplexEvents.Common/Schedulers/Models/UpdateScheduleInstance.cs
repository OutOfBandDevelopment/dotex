using OoBDev.ComplexEvents.Contracts.Schedulers;
using System;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.ComplexEvents.Common.Schedulers.Models
{
    [ExcludeFromCodeCoverage]
    public class UpdateScheduleInstance : IReleaseScheduleInstance
    {
        public UpdateScheduleInstance(
            string referenceKey,
            Type scheduler, 
            DateTimeOffset? nextStart, 
            string? errorMessage
            )
        {
            ReferenceKey = referenceKey;
            Scheduler = scheduler;
            NextStart = nextStart;
            ErrorMessage = errorMessage;
        }

        public string ReferenceKey { get; }
        public Type Scheduler { get;  }
        public DateTimeOffset? NextStart { get;  }
        public string? ErrorMessage { get;  }
    }
}

