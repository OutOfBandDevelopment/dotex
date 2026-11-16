using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Ocr;
using OoBDev.TestUtilities;

namespace OoBDev.Docling.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class DoclingOcrTests
{
    private string pdf = "";

    [TestInitialize]
    public void Init()
    {
        pdf = Path.Combine(Path.GetTempPath(), "docling-" + Path.GetRandomFileName() + ".pdf");
        File.WriteAllText(pdf, "%PDF-1.4");
    }

    [TestCleanup]
    public void Clean() => File.Delete(pdf);

    private sealed class StepClock : TimeProvider
    {
        private DateTimeOffset now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow()
        {
            now = now.AddSeconds(100);
            return now;
        }
    }

    private static DoclingOcr Client(StubHandler handler, int timeoutSeconds = 1800, TimeProvider? time = null) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("http://docling/") },
            Options.Create(new DoclingOptions { BaseUrl = "http://docling", TimeoutSeconds = timeoutSeconds, PollWaitSeconds = 7 }), time);

    private static (HttpStatusCode, string) Ok(string json) => (HttpStatusCode.OK, json);

    [TestMethod]
    public async Task Recognize_PostPollTwiceFetch_ReturnsMarkdown()
    {
        var polls = 0;
        var handler = new StubHandler((req, _) => req.RequestUri!.AbsolutePath switch
        {
            "/v1/convert/file/async" => Ok("{\"task_id\":\"t 1\"}"),
            var p when p.StartsWith("/v1/status/poll/", StringComparison.Ordinal) => Ok(++polls < 2 ? "{\"task_status\":\"started\"}" : "{\"task_status\":\"success\"}"),
            _ => Ok("{\"status\":\"success\",\"document\":{\"md_content\":\"# hi\"}}"),
        });

        var text = await Client(handler).RecognizeAsync(pdf, CancellationToken.None);

        Assert.AreEqual("# hi", text);
        Assert.AreEqual(4, handler.Calls.Count);
        StringAssert.Contains(handler.Calls[0].Body, "images_scale");
        StringAssert.Contains(handler.Calls[0].Body, "<<<PAGE>>>");
        StringAssert.Contains(handler.Calls[1].Request.RequestUri!.AbsoluteUri, "t%201?wait=7");
    }

    [TestMethod]
    public async Task Recognize_OcrLanguagesSet_SendsOneFieldPerLanguage()
    {
        var handler = new StubHandler((req, _) => req.RequestUri!.AbsolutePath switch
        {
            "/v1/convert/file/async" => Ok("{\"task_id\":\"t\"}"),
            var p when p.StartsWith("/v1/status/poll/", StringComparison.Ordinal) => Ok("{\"task_status\":\"success\"}"),
            _ => Ok("{\"status\":\"success\",\"document\":{\"md_content\":\"x\"}}"),
        });
        var client = new DoclingOcr(new HttpClient(handler) { BaseAddress = new Uri("http://docling/") },
            Options.Create(new DoclingOptions { BaseUrl = "http://docling", OcrLanguages = "eng, deu" }));

        await client.RecognizeAsync(pdf, CancellationToken.None);

        var body = handler.Calls[0].Body;
        StringAssert.Contains(body, "name=ocr_lang");
        StringAssert.Contains(body, "eng");
        StringAssert.Contains(body, "deu");
    }

    [TestMethod]
    public async Task Recognize_FailureThenSuccess_RunsLightOptions()
    {
        var posts = new List<string>();
        var taskNumber = 0;
        var handler = new StubHandler((req, body) =>
        {
            var path = req.RequestUri!.AbsolutePath;
            if (path.EndsWith("async", StringComparison.Ordinal))
            {
                posts.Add(body);
                return Ok($"{{\"task_id\":\"t{++taskNumber}\"}}");
            }

            if (path.Contains("status", StringComparison.Ordinal))
            {
                return Ok(path.EndsWith("t1", StringComparison.Ordinal) ? "{\"task_status\":\"failure\"}" : "{\"task_status\":\"success\"}");
            }

            return Ok("{\"status\":\"success\",\"document\":{\"md_content\":\"light\"}}");
        });

        var text = await Client(handler).RecognizeAsync(pdf, CancellationToken.None);

        Assert.AreEqual("light", text);
        Assert.AreEqual(2, posts.Count);
        Assert.IsTrue(posts[1].Contains("fast", StringComparison.Ordinal) && !posts[0].Contains("fast", StringComparison.Ordinal));
    }

    [TestMethod]
    public async Task Recognize_FailsBothTimes_OcrFailed()
    {
        var handler = new StubHandler((req, _) => req.RequestUri!.AbsolutePath switch
        {
            "/v1/convert/file/async" => Ok("{\"task_id\":\"t\"}"),
            var p when p.Contains("status", StringComparison.Ordinal) => Ok("{\"task_status\":\"revoked\"}"),
            _ => Ok("{}"),
        });

        var e = await Assert.ThrowsExactlyAsync<OcrFailedException>(() => Client(handler).RecognizeAsync(pdf, CancellationToken.None));

        StringAssert.Contains(e.Message, "revoked");
    }

    [TestMethod]
    public async Task Recognize_ResultNotSuccess_OcrFailedWithErrors()
    {
        var handler = new StubHandler((req, _) => req.RequestUri!.AbsolutePath switch
        {
            "/v1/convert/file/async" => Ok("{\"task_id\":\"t\"}"),
            var p when p.Contains("status", StringComparison.Ordinal) => Ok("{\"task_status\":\"success\"}"),
            _ => Ok("{\"status\":\"failure\",\"errors\":[\"bad\"]}"),
        });

        var e = await Assert.ThrowsExactlyAsync<OcrFailedException>(() => Client(handler).RecognizeAsync(pdf, CancellationToken.None));

        StringAssert.Contains(e.Message, "bad");
    }

    [TestMethod]
    public async Task Recognize_NeverFinishes_TimesOut()
    {
        var handler = new StubHandler((req, _) => req.RequestUri!.AbsolutePath switch
        {
            "/v1/convert/file/async" => Ok("{\"task_id\":\"t\"}"),
            _ => Ok("{\"task_status\":\"pending\"}"),
        });

        await Assert.ThrowsExactlyAsync<TimeoutException>(() => Client(handler, timeoutSeconds: 250, time: new StepClock()).RecognizeAsync(pdf, CancellationToken.None));
    }

    [TestMethod]
    public async Task Recognize_ServerDown_HttpRequestException()
    {
        var handler = new StubHandler((_, _) => (HttpStatusCode.BadGateway, "{}"));

        await Assert.ThrowsExactlyAsync<HttpRequestException>(() => Client(handler).RecognizeAsync(pdf, CancellationToken.None));
    }

    [TestMethod]
    public async Task Recognize_NoTaskId_HttpRequestException()
    {
        var handler = new StubHandler((_, _) => Ok("{}"));

        await Assert.ThrowsExactlyAsync<HttpRequestException>(() => Client(handler).RecognizeAsync(pdf, CancellationToken.None));
    }

    [TestMethod]
    public async Task Recognize_NotJson_HttpRequestException()
    {
        var handler = new StubHandler((_, _) => Ok("<html>"));

        await Assert.ThrowsExactlyAsync<HttpRequestException>(() => Client(handler).RecognizeAsync(pdf, CancellationToken.None));
    }

    [TestMethod]
    public async Task Recognize_ResultWithoutDocument_ReturnsEmpty()
    {
        var handler = new StubHandler((req, _) => req.RequestUri!.AbsolutePath switch
        {
            "/v1/convert/file/async" => Ok("{\"task_id\":\"t\"}"),
            var p when p.Contains("status", StringComparison.Ordinal) => Ok("{\"task_status\":\"success\"}"),
            _ => Ok("{\"status\":\"success\"}"),
        });

        Assert.AreEqual("", await Client(handler).RecognizeAsync(pdf, CancellationToken.None));
    }

    [TestMethod]
    public void Extractor_NamesTheEngine() =>
        Assert.AreEqual("docling-serve tesseract (OCR)", Client(new StubHandler((_, _) => Ok("{}"))).Extractor);
}
