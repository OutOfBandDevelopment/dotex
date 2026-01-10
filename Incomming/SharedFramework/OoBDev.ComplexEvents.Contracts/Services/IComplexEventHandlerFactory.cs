using System.Collections.Generic;

namespace OoBDev.ComplexEvents.Contracts.Services
{
    public interface IComplexEventHandlerFactory
    {
        IEnumerable<IComplexEventHandler> GetHandlers(string target);
    }
}
