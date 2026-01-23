using System;
using System.Threading.Tasks;

namespace OoBDev.Caching.Factories;

/// <summary>
/// Generic helper for unwrapping and wrapping Task results.
/// </summary>
/// <typeparam name="T">The result type of the task.</typeparam>
public static class ResultAwaiter<T>
{
#pragma warning disable CS8603 // Possible null reference return.
    /// <summary>
    /// Unwraps a Task and returns its result as an object.
    /// </summary>
    /// <param name="task">The task to unwrap.</param>
    /// <returns>The task result as an object.</returns>
    public static async Task<object> Unwrap(object task) => await ((Task<T>)task).ConfigureAwait(false);
#pragma warning restore CS8603 // Possible null reference return.

    /// <summary>
    /// Wraps an object in a completed Task.
    /// </summary>
    /// <param name="input">The value to wrap.</param>
    /// <returns>A completed task containing the input value.</returns>
    public static object Wrap(object input) => Task.FromResult((T)input);
}

/// <summary>
/// Non-generic helper for unwrapping and wrapping Task results using reflection.
/// </summary>
public static class ResultAwaiter
{
    /// <summary>
    /// Unwraps a Task using reflection and returns its result.
    /// </summary>
    /// <param name="type">The result type of the task.</param>
    /// <param name="sourceTask">The task to unwrap.</param>
    /// <returns>The task result as an object.</returns>
    /// <exception cref="NotSupportedException">Thrown when unable to unwrap the task.</exception>
    public static object Unwrap(Type type, object sourceTask)
    {
        var awaiterType = typeof(ResultAwaiter<>);
        var genericAwaiterType = awaiterType.MakeGenericType(type) ?? throw new NotSupportedException();
        var task = (Task<object>)(genericAwaiterType.GetMethod("Unwrap")?.Invoke(null, new object[] { sourceTask }) ?? throw new NotSupportedException());
        var result = task?.GetAwaiter().GetResult() ?? throw new NotSupportedException();
        return result;
    }

    /// <summary>
    /// Wraps an object in a completed Task using reflection.
    /// </summary>
    /// <param name="input">The value to wrap.</param>
    /// <returns>A completed task containing the input value.</returns>
    /// <exception cref="NotSupportedException">Thrown when unable to wrap the value.</exception>
    public static object Wrap(object input)
    {
        var awaiterType = typeof(ResultAwaiter<>);
        var genericAwaiterType = awaiterType.MakeGenericType(input.GetType()) ?? throw new NotSupportedException();
        var result = genericAwaiterType.GetMethod("Wrap")?.Invoke(null, new object[] { input }) ?? throw new NotSupportedException();
        return result;
    }
}
