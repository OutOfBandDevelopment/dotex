using System;
using OoBDev.TestUtilities.Containers;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.Docling.Tests;

[TestClass]
[TestCategory(TestCategories.Integration)]
public class DoclingIntegrationTests
{
    public required TestContext TestContext { get; set; }

    private static string Pdf()
    {
        var stream = "BT /F1 48 Tf 72 700 Td (Hello Docling) Tj ET";
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R /Resources << /Font << /F1 5 0 R >> >> >>",
            $"<< /Length {stream.Length} >>\nstream\n{stream}\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
        };
        var pdf = new StringBuilder("%PDF-1.4\n");
        var offsets = new int[objects.Length];
        for (var i = 0; i < objects.Length; i++)
        {
            offsets[i] = pdf.Length;
            pdf.Append(i + 1).Append(" 0 obj\n").Append(objects[i]).Append("\nendobj\n");
        }

        var xref = pdf.Length;
        pdf.Append("xref\n0 ").Append(objects.Length + 1).Append("\n0000000000 65535 f \n");
        foreach (var offset in offsets)
        {
            pdf.Append(offset.ToString("D10", global::System.Globalization.CultureInfo.InvariantCulture)).Append(" 00000 n \n");
        }

        pdf.Append("trailer\n<< /Size ").Append(objects.Length + 1).Append(" /Root 1 0 R >>\nstartxref\n").Append(xref).Append("\n%%EOF\n");
        return pdf.ToString();
    }

    [IntegrationTestMethod("oobd-test-docling")]
    public async Task Recognize_RealDoclingServe_ReturnsTheText()
    {
        var url = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("DOCLING_URL"));
        using var http = new HttpClient { BaseAddress = new Uri(url.TrimEnd('/') + "/") };
        var ocr = new DoclingOcr(http, Options.Create(new DoclingOptions { BaseUrl = url, TimeoutSeconds = 600, OcrLanguages = "eng" }));
        await using var services = new ServiceCollection().TryAddProviders().BuildServiceProvider();
        using var temp = services.GetRequiredService<ITempFileFactory>().GetTempFile();
        var path = Path.ChangeExtension(temp.FilePath, ".pdf");
        await File.WriteAllTextAsync(path, Pdf(), Encoding.ASCII, TestContext.CancellationToken);
        try
        {
            var text = await ocr.RecognizeAsync(path, CancellationToken.None);

            StringAssert.Contains(text, "Hello", StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
