using Microsoft.Extensions.Logging;
using System;

namespace OoBDev.System.Linq;

internal class ConsoleLogger<T> : ILogger<T>
{
    private class Scope<TState> : IDisposable
    {
        public void Dispose() { }
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => new Scope<TState>();
    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = $"{this}:{logLevel}:>{Environment.NewLine}\t{formatter(state, exception)}";
        if (logLevel >= LogLevel.Warning)
        {
            Console.Error.WriteLine(message);
        }
        else
        {
            Console.WriteLine(message);
        }
    }
}
