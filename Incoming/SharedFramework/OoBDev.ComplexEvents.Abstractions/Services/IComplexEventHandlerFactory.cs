using System.Collections.Generic;

namespace OoBDev.ComplexEvents.Abstractions.Services
{
    public interface IComplexEventHandlerFactory
    {
        IEnumerable<IComplexEventHandler> GetHandlers(string target);
    }
}
