using System;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Contracts
{
    public interface IComplexEventHandler
    {
        Task HandleEvent(object message);
    }
}
