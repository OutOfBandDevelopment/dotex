using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Schedulers;
using System;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Common.Tests.Examples
{
    [ScheduleAt("0 */2 * * * *")]
    public class ExampleEventScheduler : IComplexEventScheduler
    {
        public Task<IEventData> RequestAsync(DateTimeOffset requestTime) =>
            Task.FromResult<IEventData>(new ExampleEventData(nameof(ExampleEventScheduler)));
    }
}
