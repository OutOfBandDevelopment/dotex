using System;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.Search.Providers;

/// <summary>
/// Provides search functionality combining semantic, lexical, and hybrid search approaches.
/// </summary>
[Obsolete]
[ExcludeFromCodeCoverage]
public class SearchProvider
{
    //private readonly ISearchContent<SearchResultModel> _semantic;
    //private readonly ISearchContent<SearchResultModel> _lexical;
    //private readonly ISearchContent<SearchResultModel> _hybrid;
    //private readonly IEmbeddingProvider _embedding;
    //private readonly ISearchContent<SearchResultModel> _contentStore;

    ///// <summary>
    ///// Initializes a new instance of the <see cref="SearchProvider"/> class.
    ///// </summary>
    ///// <param name="semantic">The semantic search content.</param>
    ///// <param name="lexical">The lexical search content.</param>
    ///// <param name="hybrid">The hybrid search content.</param>
    ///// <param name="embedding">The embedding provider.</param>
    ///// <param name="contentStore">The content store.</param>
    //public SearchProvider(
    //    [FromKeyedServices(SearchTypes.Semantic)] ISearchContent<SearchResultModel> semantic,
    //    [FromKeyedServices(SearchTypes.Lexical)] ISearchContent<SearchResultModel> lexical,
    //    [FromKeyedServices(SearchTypes.Hybrid)] ISearchContent<SearchResultModel> hybrid,
    //    IEmbeddingProvider embedding,
    //    [FromKeyedServices(SearchTypes.None)] ISearchContent<SearchResultModel> contentStore
    //    )
    //{
    //    _semantic = semantic;
    //    _lexical = lexical;
    //    _hybrid = hybrid;
    //    _embedding = embedding;
    //    _contentStore = contentStore;
    //}

    ///// <summary>
    ///// Lists all available search results.
    ///// </summary>
    ///// <returns>An enumerable collection of search results.</returns>
    //public async Task<IEnumerable<SearchResultModel>> ListAsync() =>
    //    await _contentStore.QueryAsync("").ToReadOnlyCollectionAsync();

    ///// <summary>
    ///// Generates an embedding for the given text.
    ///// </summary>
    ///// <param name="text">The text to embed.</param>
    ///// <returns>An array representing the embedding.</returns>
    //public async Task<float[]> EmbedAsync(string text) => await _embedding.GetEmbeddingAsync(text);

    ///// <summary>
    ///// Performs a semantic search with the given query.
    ///// </summary>
    ///// <param name="query">The search query.</param>
    ///// <param name="limit">The maximum number of results to return.</param>
    ///// <returns>An enumerable collection of search results.</returns>
    //public async Task<IEnumerable<SearchResultModel>> SemanticSearchAsync(string? query, int limit) =>
    //    await _semantic.QueryAsync(query, limit).ToReadOnlyCollectionAsync();

    ///// <summary>
    ///// Performs a lexical search with the given query.
    ///// </summary>
    ///// <param name="query">The search query.</param>
    ///// <param name="limit">The maximum number of results to return.</param>
    ///// <returns>An enumerable collection of search results.</returns>
    //public async Task<IEnumerable<SearchResultModel>> LexicalSearchAsync(string? query, int limit) =>
    //   await _lexical.QueryAsync(query, limit).ToReadOnlyCollectionAsync();

    ///// <summary>
    ///// Performs a lexical search with the given query.
    ///// </summary>
    ///// <param name="query">The search query.</param>
    ///// <param name="limit">The maximum number of results to return.</param>
    ///// <returns>An enumerable collection of search results.</returns>
    //public async Task<IEnumerable<SearchResultModel>> HybridSearchAsync(string? query, int limit) =>
    //    await _hybrid.QueryAsync(query, limit).ToReadOnlyCollectionAsync();

}
