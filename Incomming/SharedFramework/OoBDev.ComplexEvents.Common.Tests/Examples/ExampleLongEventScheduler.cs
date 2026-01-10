using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Schedulers;
using System;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Common.Tests.Examples
{
    [ScheduleAt("0 */2 * * * *")]
    public class ExampleLongEventScheduler : IComplexEventScheduler
    {
        public async Task<IEventData> RequestAsync(DateTimeOffset requestTime)
        {
            await Task.Delay(15 * 1000);
            return new ExampleEventData(nameof(ExampleLongEventScheduler));
        }
    }
}
