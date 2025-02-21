using OoBDev.Search.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Search;

/// <summary>
/// Interface for storing and querying vectors.
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// Stores the specified embeddings and metadata.
    /// </summary>
    /// <param name="embeddings">The embeddings to store.</param>
    /// <param name="metadata">The metadata to store.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<string[]> StoreVectorsAsync(IEnumerable<ReadOnlyMemory<float>> embeddings, Dictionary<string, object> metadata);

    /// <summary>
    /// Stores the specified embeddings and metadata.
    /// </summary>
    /// <param name="items">The embeddings to store.</param>
    /// <param name="metadata">The metadata to store.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<string[]> StoreVectorsAsync(IEnumerable<(ReadOnlyMemory<float> embedding, Dictionary<string, object> metadata)> items, Dictionary<string, object> metadata);

    /// <summary>
    /// Lists the stored vectors.
    /// </summary>
    /// <returns>A sequence of search result models.</returns>
    IAsyncEnumerable<SearchResultModel> ListAsync();

    /// <summary>
    /// Finds the neighbors of the specified vector.
    /// </summary>
    /// <param name="find">The vector to find neighbors for.</param>
    /// <returns>A sequence of search result models.</returns>
    IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find);

    /// <summary>
    /// Finds the neighbors of the specified vector, grouped by the specified key.
    /// </summary>
    /// <param name="find">The vector to find neighbors for.</param>
    /// <param name="groupBy">The key to group the results by.</param>
    /// <returns>A sequence of search result models.</returns>
    IAsyncEnumerable<SearchResultModel> FindNeighborsAsync(ReadOnlyMemory<float> find, string groupBy);
}

/// <summary>
/// Interface for storing and querying vectors.
/// </summary>
public interface IVectorStore<T> : IVectorStore
{
}

