using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.Documents.Ocr;
using OoBDev.Documents.Pdf;

namespace OoBDev.Documents.Markdown.Tests;

internal sealed class FakePdfText(PdfText text) : IPdfTextReader
{
    public PdfText Read(string path, CancellationToken cancellationToken) => text;
}

internal sealed class FakePdfTools(PdfOutline? outline, bool throws = false) : IPdfPageTools
{
    public List<(int From, int To)> Written { get; } = [];

    public PdfOutline? Inspect(string path) => throws ? throw new InvalidOperationException("bad pdf") : outline;

    public void WritePages(string path, int from, int to, string outputPath)
    {
        Written.Add((from, to));
        File.WriteAllText(outputPath, $"{from}-{to}");
    }
}

internal sealed class FakeOcr(Func<string, string> answer) : IDocumentOcr
{
    public List<string> Calls { get; } = [];

    public string Extractor => "fake";

    public Task<string> RecognizeAsync(string pdfPath, CancellationToken cancellationToken)
    {
        Calls.Add(Path.GetFileName(pdfPath));
        return Task.FromResult(answer(File.ReadAllText(pdfPath)));
    }
}
