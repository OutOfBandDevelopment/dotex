using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Docling.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class DoclingLayoutTests
{
    public required TestContext TestContext { get; set; }

    private const string Json = """
        {"pages":{"1":{"size":{"width":200,"height":400},"page_no":1},"2":{"size":{"width":200,"height":400},"page_no":2}},
         "texts":[
           {"label":"text","text":"low","prov":[{"page_no":1,"bbox":{"l":20,"t":100,"r":100,"b":80,"coord_origin":"BOTTOMLEFT"}}]},
           {"label":"section_header","text":"Title","prov":[{"page_no":1,"bbox":{"l":20,"t":390,"r":180,"b":360,"coord_origin":"BOTTOMLEFT"}}]},
           {"label":"text","text":"second page","prov":[{"page_no":2,"bbox":{"l":0,"t":40,"r":100,"b":20,"coord_origin":"TOPLEFT"}}]},
           {"label":"text","text":"no box","prov":[{"page_no":1}]},
           {"label":"text","text":"unknown page","prov":[{"page_no":9,"bbox":{"l":0,"t":0,"r":1,"b":1}}]}
         ],
         "tables":[{"label":"table","prov":[{"page_no":1,"bbox":{"l":-10,"t":260,"r":300,"b":200,"coord_origin":"BOTTOMLEFT"}}]}]}
        """;

    [TestMethod]
    public void Parse_BottomLeftAndTopLeftBoxes_BecomePageFractionsInReadingOrder()
    {
        using var doc = JsonDocument.Parse(Json);

        var regions = DoclingLayout.Parse(doc.RootElement);

        foreach (var r in regions)
        {
            TestContext.WriteLine("{0} p{1} {2} ({3:F3},{4:F3})-({5:F3},{6:F3})", r.Label, r.Page, r.Text, r.Left, r.Top, r.Right, r.Bottom);
        }

        Assert.HasCount(4, regions, "the item without a box and the one on an unknown page are skipped");
        Assert.AreEqual("Title", regions[0].Text);
        Assert.AreEqual(0.1, regions[0].Left, 1e-9);
        Assert.AreEqual(0.025, regions[0].Top, 1e-9);
        Assert.AreEqual(0.9, regions[0].Right, 1e-9);
        Assert.AreEqual(0.1, regions[0].Bottom, 1e-9);
        Assert.AreEqual("table", regions[1].Label);
        Assert.AreEqual(0.35, regions[1].Top, 1e-9);
        Assert.AreEqual(0.0, regions[1].Left, "a box outside the page is clamped");
        Assert.AreEqual(1.0, regions[1].Right);
        Assert.AreEqual("low", regions[2].Text);
        Assert.AreEqual(0.75, regions[2].Top, 1e-9);
        Assert.AreEqual(2, regions[3].Page);
        Assert.AreEqual(0.05, regions[3].Top, 1e-9);
    }

    [TestMethod]
    public async Task RecognizeLayout_AsksForJsonAndReturnsMarkdownWithRegions()
    {
        var pdf = Path.Combine(Path.GetTempPath(), "docling-" + Path.GetRandomFileName() + ".pdf");
        await File.WriteAllTextAsync(pdf, "%PDF-1.4");
        try
        {
            var handler = new StubHandler((req, _) => req.RequestUri!.AbsolutePath switch
            {
                "/v1/convert/file/async" => (HttpStatusCode.OK, "{\"task_id\":\"t\"}"),
                var p when p.StartsWith("/v1/status/poll/", StringComparison.Ordinal) => (HttpStatusCode.OK, "{\"task_status\":\"success\"}"),
                _ => (HttpStatusCode.OK, "{\"status\":\"success\",\"document\":{\"md_content\":\"# Title\",\"json_content\":" + Json + "}}"),
            });
            var client = new DoclingOcr(new HttpClient(handler) { BaseAddress = new Uri("http://docling/") }, Options.Create(new DoclingOptions { BaseUrl = "http://docling" }));

            var layout = await client.RecognizeLayoutAsync(pdf, CancellationToken.None);

            TestContext.WriteLine("{0} regions", layout.Regions.Count);
            Assert.AreEqual("# Title", layout.Markdown);
            Assert.HasCount(4, layout.Regions);
            StringAssert.Contains(handler.Calls[0].Body, "name=to_formats");
            StringAssert.Contains(handler.Calls[0].Body, "json");
        }
        finally
        {
            File.Delete(pdf);
        }
    }

    [TestMethod]
    public void Parse_DocumentWithoutPages_HasNoRegions()
    {
        using var doc = JsonDocument.Parse("{\"texts\":[{\"text\":\"x\",\"prov\":[{\"page_no\":1,\"bbox\":{\"l\":0,\"t\":0,\"r\":1,\"b\":1}}]}]}");

        Assert.IsEmpty(DoclingLayout.Parse(doc.RootElement));
    }
}
