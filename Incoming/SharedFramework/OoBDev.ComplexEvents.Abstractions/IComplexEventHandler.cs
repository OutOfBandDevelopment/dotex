using System;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Abstractions
{
    public interface IComplexEventHandler
    {
        Task HandleEvent(object message);
    }
}
