using System.Collections.Generic;
using System.Threading;

namespace OoBDev.Documents.Markdown.Tests;

internal sealed class FakeFileReader(string text, params string[] extensions) : IFileMarkdownReader
{
    public IReadOnlyCollection<string> Extensions { get; } = extensions;

    public string Extractor => "fake reader";

    public string Read(string path, CancellationToken cancellationToken) => text;
}
