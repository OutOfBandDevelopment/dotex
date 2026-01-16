using System;
using System.Threading.Tasks;

namespace OoBDev.Caching.Common.Factories
{
    public static class ResultAwaiter<T>
    {
#pragma warning disable CS8603 // Possible null reference return.
        public static async Task<object> Unwrap(object task) => await ((Task<T>)task).ConfigureAwait(false);
#pragma warning restore CS8603 // Possible null reference return.
        public static object Wrap(object input) => Task.FromResult((T)input);
    }

    public static class ResultAwaiter
    {
        public static object Unwrap(Type type, object sourceTask)
        {
            var awaiterType = typeof(ResultAwaiter<>);
            var genericAwaiterType = awaiterType.MakeGenericType(type) ?? throw new NotSupportedException();
            var task = (Task<object>)(genericAwaiterType.GetMethod("Unwrap")?.Invoke(null, new object[] { sourceTask }) ?? throw new NotSupportedException());
            var result = task?.GetAwaiter().GetResult() ?? throw new NotSupportedException();
            return result;
        }
        public static object Wrap(object input)
        {
            var awaiterType = typeof(ResultAwaiter<>);
            var genericAwaiterType = awaiterType.MakeGenericType(input.GetType()) ?? throw new NotSupportedException();
            var result = genericAwaiterType.GetMethod("Wrap")?.Invoke(null, new object[] { input }) ?? throw new NotSupportedException();
            return result;
        }
    }
}
