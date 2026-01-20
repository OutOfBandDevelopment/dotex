
using System;

namespace OoBDev.ComplexEvents.Abstractions.Services
{
    public interface IComplexEventHandlerResolver
    {
        public bool CheckTarget(string target, Type handler);
    }
}
