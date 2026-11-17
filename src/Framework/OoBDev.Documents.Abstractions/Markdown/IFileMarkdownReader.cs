using System.Collections.Generic;
using System.Threading;

namespace OoBDev.Documents.Markdown;

/// <summary>Turns one file format into Markdown text. Implemented by an adapter that carries the format library, so the framework itself stays free of it.</summary>
public interface IFileMarkdownReader
{
    /// <summary>The file extensions handled, with the dot and in lower case (for example <c>.html</c>).</summary>
    IReadOnlyCollection<string> Extensions { get; }

    /// <summary>The extractor name written to the front matter.</summary>
    string Extractor { get; }

    /// <summary>Reads the file as Markdown.</summary>
    /// <param name="path">The source file; never changed.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    string Read(string path, CancellationToken cancellationToken);
}
