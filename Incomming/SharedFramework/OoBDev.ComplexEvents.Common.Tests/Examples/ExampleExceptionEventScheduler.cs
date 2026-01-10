using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Schedulers;
using System;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Common.Tests.Examples
{
    [ScheduleAt("0 */2 * * * *")]
    [ScheduleAt("0 */15 * * * *")]
    public class ExampleExceptionEventScheduler : IComplexEventScheduler
    {
        public Task<IEventData> RequestAsync(DateTimeOffset requestTime) => Task.FromException<IEventData>(new Exception("Test Error"));
    }
}
