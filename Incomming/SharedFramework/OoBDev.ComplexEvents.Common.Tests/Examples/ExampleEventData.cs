using OoBDev.ComplexEvents.Contracts;

namespace OoBDev.ComplexEvents.Common.Tests.Examples
{
    public class ExampleEventData : IEventData
    {
        public ExampleEventData(string message)
        {
            Message = message;
        }

        public string Message { get;  }
    }
}
