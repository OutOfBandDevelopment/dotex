using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.DataLoader;
using OoBDev.WebApi.Models;

namespace OoBDev.WebApi.Controllers;

/// <summary>Loads a small CSV or JSON text into a throwaway in-memory table with <see cref="Loader"/> and returns the report and the stored rows. Nothing is kept between requests.</summary>
/// <param name="readers">The registered row readers.</param>
[ApiController]
[Route("api/[controller]")]
public class DataLoadController(IEnumerable<IRowReader> readers) : ControllerBase
{
    /// <summary>The largest accepted text, in characters.</summary>
    public const int MaxTextLength = 100_000;

    /// <summary>Loads the text and returns the preview.</summary>
    /// <param name="request">The file and the key column.</param>
    /// <param name="cancellationToken">The request cancellation token.</param>
    /// <returns>The load report and the stored rows, or a problem when the request is not valid.</returns>
    [HttpPost("preview")]
    [ApplicationRight("data.load")]
    [ProducesResponseType<DataLoadResponseModel>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DataLoadResponseModel>> Preview([FromBody] DataLoadRequestModel request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return Refuse("Paste some CSV or JSON text.");
        }

        if (request.Text.Length > MaxTextLength)
        {
            return Refuse($"The text is longer than {MaxTextLength} characters.");
        }

        if (string.IsNullOrWhiteSpace(request.KeyColumn))
        {
            return Refuse("Name the key column.");
        }

        if (!Enum.TryParse<DuplicatePolicy>(request.Duplicates, ignoreCase: true, out var policy))
        {
            return Refuse("Duplicates must be Skip, Replace or Fail.");
        }

        if (!readers.Any(r => string.Equals(r.Format, request.Format, StringComparison.OrdinalIgnoreCase)))
        {
            return Refuse($"The format must be one of: {string.Join(", ", readers.Select(r => r.Format))}.");
        }

        var key = request.KeyColumn.Trim();
        var sink = new InMemoryRowSink(key);
        var loader = new Loader(readers, [], sink);
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(request.Text));
        var report = await loader.LoadAsync(stream, request.Format, new LoadOptions { Duplicates = policy }, cancellationToken);

        var columns = sink.Rows.Values.SelectMany(r => r.Keys).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var rows = sink.Rows.Values
            .Select(r => (IReadOnlyList<string?>)[.. columns.Select(c => r.TryGetValue(c, out var v) ? v : null)])
            .ToList();
        return new DataLoadResponseModel(report.Read, report.Written, report.Replaced, report.Skipped, [.. report.Errors.Select(e => new DataLoadErrorModel(e.Line, e.Message))], columns, rows);
    }

    private ObjectResult Refuse(string reason) => Problem(reason, statusCode: StatusCodes.Status400BadRequest, title: "Load refused");
}
