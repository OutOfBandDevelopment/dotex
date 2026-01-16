using OoBDev.ComplexEvents.Contracts;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Common.Tests.Examples
{
    [ComplexEventHandler(TargetType = typeof(ExampleEventData))]
    public class ExampleEventHandler : IComplexEventHandler
    {
        public Task HandleEvent(object message) =>
            Task.FromResult(0);
    }
}
