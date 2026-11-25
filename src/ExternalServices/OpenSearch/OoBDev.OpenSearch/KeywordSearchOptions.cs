using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.OpenSearch;

/// <summary>Settings of the keyword search provider, bound from <see cref="SectionName"/>.</summary>
public sealed class KeywordSearchOptions
{
    /// <summary>The configuration section.</summary>
    public const string SectionName = "OoBDev:OpenSearch:Keyword";

    /// <summary>The fields the free text is matched against; <c>*</c> matches every text field.</summary>
    [MinLength(1)]
    public IList<string> TextFields { get; set; } = ["*"];

    /// <summary>The field names a request may filter on; a filter on any other name is refused with <c>UnsupportedFilterException</c>.</summary>
    public IList<string> FilterFields { get; set; } = [];
}
