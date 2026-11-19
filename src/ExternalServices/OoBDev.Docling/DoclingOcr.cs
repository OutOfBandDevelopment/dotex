using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.Documents.Ocr;

namespace OoBDev.Docling;

/// <summary>
/// OCR through docling-serve's asynchronous API: post the PDF, poll the task until it succeeds, fetch the Markdown. Tesseract at 3x scale reads fax-style scans best, so those options come first;
/// a failed task is tried once more with lighter options (2x scale, fast tables, no formula enrichment), which large or odd PDFs most often need.
/// </summary>
/// <param name="http">The client with <see cref="HttpClient.BaseAddress"/> set to the server.</param>
/// <param name="options">The settings.</param>
/// <param name="time">The clock used for the deadline.</param>
public sealed class DoclingOcr(HttpClient http, IOptions<DoclingOptions> options, TimeProvider? time = null) : IDocumentLayoutOcr
{
    private static readonly (string Key, string Value)[] HeavyOptions =
    [
        ("to_formats", "md"), ("do_ocr", "true"), ("force_ocr", "true"), ("ocr_engine", "tesseract"), ("images_scale", "2"),
        ("do_table_structure", "true"), ("table_mode", "accurate"), ("do_formula_enrichment", "true"),
        ("image_export_mode", "placeholder"), ("md_page_break_placeholder", IDocumentOcr.PageMark),
    ];

    private static readonly (string Key, string Value)[] LightOptions =
    [
        ("to_formats", "md"), ("do_ocr", "true"), ("force_ocr", "true"), ("ocr_engine", "tesseract"), ("images_scale", "2"),
        ("do_table_structure", "true"), ("table_mode", "fast"), ("do_formula_enrichment", "false"),
        ("image_export_mode", "placeholder"), ("md_page_break_placeholder", IDocumentOcr.PageMark),
    ];

    private readonly TimeProvider clock = time ?? TimeProvider.System;

    /// <inheritdoc />
    public string Extractor => "docling-serve tesseract (OCR)";

    /// <inheritdoc />
    public async Task<string> RecognizeAsync(string pdfPath, CancellationToken cancellationToken) =>
        Markdown(await ConvertWithRetryAsync(pdfPath, false, cancellationToken).ConfigureAwait(false));

    /// <inheritdoc />
    public async Task<OcrLayout> RecognizeLayoutAsync(string pdfPath, CancellationToken cancellationToken)
    {
        var document = await ConvertWithRetryAsync(pdfPath, true, cancellationToken).ConfigureAwait(false);
        var regions = document.ValueKind == JsonValueKind.Object && document.TryGetProperty("json_content", out var json) && json.ValueKind == JsonValueKind.Object
            ? DoclingLayout.Parse(json)
            : [];
        return new OcrLayout(Markdown(document), regions);
    }

    private static string Markdown(JsonElement document) =>
        document.ValueKind == JsonValueKind.Object && document.TryGetProperty("md_content", out var md) ? md.GetString() ?? "" : "";

    private async Task<JsonElement> ConvertWithRetryAsync(string pdfPath, bool layout, CancellationToken ct)
    {
        try
        {
            return await ConvertAsync(pdfPath, HeavyOptions, layout, ct).ConfigureAwait(false);
        }
        catch (OcrFailedException)
        {
            return await ConvertAsync(pdfPath, LightOptions, layout, ct).ConfigureAwait(false);
        }
    }

    private async Task<JsonElement> ConvertAsync(string pdfPath, (string Key, string Value)[] settings, bool layout, CancellationToken ct)
    {
        var timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds);
        using var form = new MultipartFormDataContent();
        foreach (var (key, value) in settings)
        {
            form.Add(new StringContent(value), key);
        }

        if (layout)
        {
            form.Add(new StringContent("json"), "to_formats");
        }

        foreach (var lang in options.Value.OcrLanguages.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            form.Add(new StringContent(lang), "ocr_lang");
        }

        using var file = File.OpenRead(pdfPath);
        using var part = new StreamContent(file);
        part.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(part, "files", "doc.pdf");

        string task;
        using (var response = await http.PostAsync(new Uri("v1/convert/file/async", UriKind.Relative), form, ct).ConfigureAwait(false))
        {
            response.EnsureSuccessStatusCode();
            var json = await ReadJsonAsync(response, ct).ConfigureAwait(false);
            task = json.TryGetProperty("task_id", out var id) ? id.GetString() ?? "" : "";
            if (task.Length == 0)
            {
                throw new HttpRequestException("docling returned no task id");
            }
        }

        var deadline = clock.GetUtcNow() + timeout;
        var wait = options.Value.PollWaitSeconds.ToString(CultureInfo.InvariantCulture);
        while (true)
        {
            using var poll = await http.GetAsync(new Uri($"v1/status/poll/{Uri.EscapeDataString(task)}?wait={wait}", UriKind.Relative), ct).ConfigureAwait(false);
            poll.EnsureSuccessStatusCode();
            var pollJson = await ReadJsonAsync(poll, ct).ConfigureAwait(false);
            var status = pollJson.TryGetProperty("task_status", out var s) ? s.GetString() : null;
            if (string.Equals(status, "success", StringComparison.Ordinal))
            {
                break;
            }

            if (status is "failure" or "revoked")
            {
                throw new OcrFailedException($"docling: task {status}");
            }

            if (clock.GetUtcNow() > deadline)
            {
                throw new TimeoutException($"docling: no result after {timeout.TotalSeconds:F0}s");
            }
        }

        using var result = await http.GetAsync(new Uri($"v1/result/{Uri.EscapeDataString(task)}", UriKind.Relative), ct).ConfigureAwait(false);
        result.EnsureSuccessStatusCode();
        var body = await ReadJsonAsync(result, ct).ConfigureAwait(false);
        var outcome = body.TryGetProperty("status", out var st) ? st.GetString() : null;
        if (!string.Equals(outcome, "success", StringComparison.Ordinal))
        {
            var errors = body.TryGetProperty("errors", out var e) ? e.ToString() : "";
            throw new OcrFailedException($"docling: {outcome} {errors}".TrimEnd());
        }

        return body.TryGetProperty("document", out var document) ? document : default;
    }

    private static async Task<JsonElement> ReadJsonAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var stream = await response.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);
            return document.RootElement.Clone();
        }
        catch (JsonException e)
        {
            throw new HttpRequestException(e.Message, e);
        }
    }
}
