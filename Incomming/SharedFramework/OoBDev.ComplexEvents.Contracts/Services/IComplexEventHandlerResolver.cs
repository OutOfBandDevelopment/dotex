
using System;

namespace OoBDev.ComplexEvents.Contracts.Services
{
    public interface IComplexEventHandlerResolver
    {
        public bool CheckTarget(string target, Type handler);
    }
}
