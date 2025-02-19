using System;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.OpenSearch;

/// <summary>
/// Provides functionality for storing and searching lexical content using OpenSearch.
/// </summary>
[Obsolete]
[ExcludeFromCodeCoverage]
public class LexicalProvider
{
    //private readonly IOpenSearchLowLevelClient _client;
    //private readonly IOptions<OpenSearchOptions> _config;

    ///// <summary>
    ///// Initializes a new instance of the <see cref="LexicalProvider"/> class.
    ///// </summary>
    ///// <param name="config">The configuration options for OpenSearch.</param>
    ///// <param name="client">The low-level OpenSearch client.</param>
    //public LexicalProvider(
    //    IOptions<OpenSearchOptions> config,
    //    IOpenSearchLowLevelClient client
    //    )
    //{
    //    _config = config;
    //    _client = client;
    //}

    ///// <summary>
    ///// Queries the OpenSearch index for lexical content matching the specified query string.
    ///// </summary>
    ///// <param name="queryString">The query string to search for.</param>
    ///// <param name="limit">The maximum number of results to return.</param>
    ///// <param name="page">The page number of results to retrieve.</param>
    ///// <returns>An asynchronous enumerable of SearchResultModel objects representing the search results.</returns>
    //public async IAsyncEnumerable<SearchResultModel> QueryAsync(string? queryString, int limit = 25, int page = 0)
    //{
    //    await foreach (var item in ((ISearchContent<JsonNode>)this).QueryAsync(queryString, 25, 0))
    //        yield return new()
    //        {
    //            Score = (float?)item["_score"] ?? 0,
    //            PathHash = (item["_source"]?[nameof(SearchResultModel.PathHash)]?.ToString()) ?? "",
    //            Content = (item["_source"]?[nameof(SearchResultModel.Content)]?.ToString()) ?? "",
    //            File = (item["_source"]?[nameof(SearchResultModel.File)]?.ToString()) ?? "",
    //            Type = SearchTypes.Lexical,
    //        };
    //}

    ///// <summary>
    ///// Queries the OpenSearch index for lexical content matching the specified query string.
    ///// </summary>
    ///// <param name="queryString">The query string to search for.</param>
    ///// <param name="limit">The maximum number of results to return.</param>
    ///// <param name="page">The page number of results to retrieve.</param>
    ///// <returns>An asynchronous enumerable of JsonNode objects representing the search results.</returns>
    //async IAsyncEnumerable<JsonNode> ISearchContent<JsonNode>.QueryAsync(string? queryString, int limit, int page)
    //{
    //    if (string.IsNullOrWhiteSpace(queryString))
    //        yield break;

    //    // https://opensearch.org/docs/latest/query-dsl/full-text/index/
    //    var lookupResult = await _client.SearchAsync<StringResponse>(_config.Value.IndexName,
    //       PostData.Serializable(new
    //       {
    //           query = new
    //           {
    //               match = new
    //               {
    //                   Content = queryString
    //               }
    //           }
    //       }));

    //    var lookupJson = JsonNode.Parse(lookupResult.Body);
    //    if (lookupJson?["hits"]?["hits"] is JsonArray arr)
    //    {
    //        foreach (var item in arr)
    //            if (item != null)
    //                yield return item;
    //    }
    //}

    ///// <summary>
    ///// Stores the content in the OpenSearch index if it doesn't already exist.
    ///// </summary>
    ///// <param name="full">The full path of the content to store.</param>
    ///// <param name="file">The name of the file to store.</param>
    ///// <param name="pathHash">The hash value of the file path.</param>
    ///// <returns>A boolean value indicating whether the content was successfully stored.</returns>
    //public async Task<bool> TryStoreAsync(string full, string file, string pathHash)
    //{
    //    var lookupResult = await _client.SearchAsync<StringResponse>(_config.Value.IndexName,
    //       PostData.Serializable(new
    //       {
    //           query = new
    //           {
    //               match = new
    //               {
    //                   PathHash = new
    //                   {
    //                       query = pathHash
    //                   }
    //               }
    //           }
    //       }));

    //    var lookupJson = JsonNode.Parse(lookupResult.Body);
    //    if (lookupJson?["hits"]?["hits"] is JsonArray arr && arr.Count > 0)
    //        return false;

    //    var id = Guid.NewGuid().ToString();
    //    _ = await _client.IndexAsync<StringResponse>(_config.Value.IndexName, id, PostData.Serializable(new
    //    {
    //        Id = id,

    //        File = file,
    //        OriginalFile = full,
    //        PathHash = pathHash,

    //        FileName = Path.GetFileNameWithoutExtension(file),
    //        Directory = Path.GetDirectoryName(file) ?? "",
    //        Extensions = Path.GetExtension(file),

    //        Content = File.ReadAllText(full),
    //    }));

    //    return true;
    //}
}
