using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Extensions.Linq;

/// <summary>
/// Extensions to add async support to existing IEnumerable{T}
/// </summary>
public static class AsyncEnumerableExtensions
{
    /// <summary>
    /// Converts an asynchronous enumerable sequence to a set (IEnumerable{T}) asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="items">The asynchronous enumerable sequence to convert.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the set of elements in the sequence.</returns>
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> items)
    {
        var result = new List<T>();
        await foreach (var item in items)
            result.Add(item);
        return result;
    }

    /// <summary>
    /// Process IQueryable{T} to a List{T} as async
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="source"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<List<TModel>> ToListAsync<TModel>(
        this IQueryable<TModel> source,
        CancellationToken cancellationToken = default
        )
    {
        // copied from EF Core 
        // https://github.com/dotnet/efcore/blob/main/src/EFCore/Extensions/EntityFrameworkQueryableExtensions.cs
        var list = new List<TModel>();
        await foreach (var element in source.AsAsyncEnumerable(cancellationToken))
        {
            list.Add(element);
        }

        return list;
    }

    /// <summary>
    /// Convert IAsyncEnumerable{TModel} to Task{IEnumerable{TModel}}
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="items"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async Task<IEnumerable<TModel>> AsEnumerableAsync<TModel>(
        this IAsyncEnumerable<TModel> items,
        CancellationToken token = default
        )
    {
        var list = new List<TModel>();
        await foreach (var item in items)
        {
            if (token.IsCancellationRequested) break;
            list.Add(item);
        }
        return list.AsReadOnly();
    }

    /// <summary>
    /// Convert Task{IEnumerable{TModel}} to IAsyncEnumerable{TModel}
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="items"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<TModel> AsAsyncEnumerable<TModel>(
        this Task<IEnumerable<TModel>> items,
        [EnumeratorCancellation] CancellationToken cancellationToken = default
        )
    {
        foreach (var item in await items)
        {
            if (cancellationToken.IsCancellationRequested) break;
            yield return item;
            await Task.Yield();
        }
    }

    /// <summary>
    /// Convert IEnumerable{TModel} to IAsyncEnumerable{TModel}
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <param name="items"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async IAsyncEnumerable<TModel> AsAsyncEnumerable<TModel>(
        this IEnumerable<TModel> items,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Modified from EF Core to also enable Async iteration over enumerable that are not natively async.  
        if (items is IAsyncEnumerable<TModel> asyncItems)
        {
            await foreach (var item in asyncItems)
            {
                if (cancellationToken.IsCancellationRequested) break;
                yield return item;
            }
        }
        else
        {
            foreach (var item in items)
            {
                if (cancellationToken.IsCancellationRequested) break;
                yield return item;
                await Task.Yield();
            }
        }
    }

    /// <summary>
    /// Project each element of the source async enumerable sequence into a new form asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
    /// <param name="items">The source async enumerable sequence to project.</param>
    /// <param name="map">A transform function to apply to each element.</param>
    /// <returns>An async enumerable sequence whose elements are the result of invoking the transform function on each element of the source.</returns>
    public static async IAsyncEnumerable<TResult> Select<T, TResult>(
        this IAsyncEnumerable<T> items,
        Func<T, TResult> map
        )
    {
        await foreach (var item in items)
            yield return map(item);
    }

    /// <summary>
    /// Project each element of the source async enumerable sequence into a new form asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
    /// <param name="items">The source async enumerable sequence to project.</param>
    /// <param name="map">A transform function to apply to each element asynchronously.</param>
    /// <returns>An async enumerable sequence whose elements are the result of invoking the transform function on each element of the source.</returns>
    public static async IAsyncEnumerable<TResult> Select<T, TResult>(
        this IAsyncEnumerable<T> items,
        Func<T, Task<TResult>> map
        )
    {
        await foreach (var item in items)
            yield return await map(item);
    }

    /// <summary>
    /// Converts an async enumerable sequence to a read-only collection asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the source sequence.</typeparam>
    /// <param name="items">The source async enumerable sequence to convert.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains a read-only collection of elements.</returns>
    public static async Task<IReadOnlyCollection<T>> ToReadOnlyCollectionAsync<T>(
        this IAsyncEnumerable<T> items,
        CancellationToken cancellationToken = default
        )
    {
        var collection = new Collection<T>();
        await foreach (var item in items)
            collection.Add(item);
        return collection;
    }

    /// <summary>
    /// Converts an asynchronous enumerable sequence to a set (IEnumerable{T}) asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of elements in the sequence.</typeparam>
    /// <param name="items">The asynchronous enumerable sequence to convert.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the set of elements in the sequence.</returns>
    [Obsolete($"change to .{nameof(ToListAsync)}()", error: true)]
    public static async Task<IEnumerable<T>> ToSetAsync<T>(this IAsyncEnumerable<T> items) =>
        await items.ToListAsync();
}
