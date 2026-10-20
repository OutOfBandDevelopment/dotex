using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>A document and an XPath query to run over it.</summary>
public class XPathQueryRequestModel
{
    /// <summary>Gets or sets the format: json, yaml, html or xml.</summary>
    public required string Format { get; set; }

    /// <summary>Gets or sets the document text.</summary>
    public required string Content { get; set; }

    /// <summary>Gets or sets the XPath query.</summary>
    public required string XPath { get; set; }
}

/// <summary>The matches of an XPath query.</summary>
public class XPathQueryResponseModel
{
    /// <summary>Gets or sets the number of matches.</summary>
    public int Count { get; set; }

    /// <summary>Gets or sets the text of each match.</summary>
    public required List<string> Matches { get; set; }

    /// <summary>Gets or sets the document as XML, the shape XPath actually sees.</summary>
    public required string Xml { get; set; }
}

/// <summary>Markdown to render.</summary>
public class MarkdownRequestModel
{
    /// <summary>Gets or sets the Markdown text.</summary>
    public required string Markdown { get; set; }
}

/// <summary>Rendered Markdown.</summary>
public class MarkdownResponseModel
{
    /// <summary>Gets or sets the HTML.</summary>
    public required string Html { get; set; }
}
