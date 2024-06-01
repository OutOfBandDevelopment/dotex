using System;

namespace OoBDev.TestUtilities.Logging;

internal class LoggerScope<TState>(TState state) : IDisposable
{
    public void Dispose() { }
}