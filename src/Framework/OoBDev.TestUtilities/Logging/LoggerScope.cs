using System;

namespace OoBDev.TestUtilities.Logging;

internal class LoggerScope<TState>(TState state) : IDisposable
{
    public TState State => state;

    public void Dispose() { }
}
