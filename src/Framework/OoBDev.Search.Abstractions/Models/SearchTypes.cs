using System;

namespace OoBDev.Search.Models;

/// <summary>
/// Specifies the types of search.
/// </summary>
[Flags]
public enum SearchTypes
{
    /// <summary>
    /// Indicates no specific search type.
    /// </summary>
    None,

    /// <summary>
    /// Indicates a semantic search type.
    /// </summary>
    Semantic = 1,

    /// <summary>
    /// Indicates a lexical search type.
    /// </summary>
    Lexical = 2,

    /// <summary>
    /// Indicates a hybrid search type combining semantic and lexical searches.
    /// </summary>
    Hybrid = 3,
}
