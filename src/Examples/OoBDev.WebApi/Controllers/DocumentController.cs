using OoBDev.Documents;
using OoBDev.Documents.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Controller for handling document-related operations.
/// </summary>
[AllowAnonymous]
[Route("[Controller]/[Action]")]
[BlobContainer(ContainerName = "docs")]
public class DocumentController : Controller
{
    /// <summary>
    /// Represents a container for documents.
    /// </summary>
    [BlobContainer(ContainerName = "docs")]
    public class Documents { }

    /// <summary>
    /// Represents a container for documents.
    /// </summary>
    [BlobContainer(ContainerName = "summary")]
    public class Summaries { }

    private readonly IDocumentConversion _converter;
    private readonly IDocumentTypeTools _documentTypes;
    private readonly ILogger _logger;
    private readonly IBlobContainer<Documents> _docs;
    private readonly IBlobContainer<Summaries> _summary;

    /// <summary>
    /// Initializes a new instance of the <see cref="DocumentController"/> class.
    /// </summary>
    /// <param name="converter">The document conversion service.</param>
    /// <param name="documentTypes">The document type tools.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="docs">The blob container for documents.</param>
    /// <param name="summary">The blob container for summaries.</param>
    public DocumentController(
        IDocumentConversion converter,
        IDocumentTypeTools documentTypes,
        ILogger<DocumentController> logger,
        IBlobContainer<Documents> docs,
        IBlobContainer<Summaries> summary
        )
    {
        _converter = converter;
        _documentTypes = documentTypes;
        _logger = logger;
        _docs = docs;
        _summary = summary;
    }

    /// <summary>
    /// Downloads the specified file.
    /// </summary>
    /// <param name="file">The path to the file.</param>
    [HttpGet("{*file}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Download(string file) =>
        await _docs.GetContentAsync(file) switch
        {
            null => NotFound(),
            ContentReference blob => File(blob.Content, blob.ContentType, blob.FileName)
        };

    /// <summary>
    /// Retrieves the text of the specified file.
    /// </summary>
    /// <param name="file">The path to the file.</param>
    [HttpGet("{*file}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Text(string file) =>
        await _docs.GetContentAsync(file) switch
        {
            null => NotFound(),
            ContentReference blob => File(
                await ConvertToAsync(blob.Content, blob.ContentType, "text/plain"),
                "text/plain",
                Path.ChangeExtension(blob.FileName, ".txt"))
        };

    /// <summary>
    /// Retrieves the html of the specified file.
    /// </summary>
    /// <param name="file">The path to the file.</param>
    [HttpGet("{*file}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Html(string file) =>
        await _docs.GetContentAsync(file) switch
        {
            null => NotFound(),
            ContentReference blob => File(
                await ConvertToAsync(blob.Content, blob.ContentType, "text/html"),
                "text/html",
                Path.ChangeExtension(blob.FileName, ".html"))
        };

    /// <summary>
    /// Retrieves the pdf of the specified file.
    /// </summary>
    /// <param name="file">The path to the file.</param>
    [HttpGet("{*file}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Pdf(string file) =>
        await _docs.GetContentAsync(file) switch
        {
            null => NotFound(),
            ContentReference blob => File(
                await ConvertToAsync(blob.Content, blob.ContentType, "application/pdf"),
                "application/pdf",
                Path.ChangeExtension(blob.FileName, ".pdf"))
        };

    private async Task<Stream> ConvertToAsync(Stream source, string sourceType, string destinationType)
    {
        var ms = new MemoryStream();
        await _converter.ConvertAsync(source, sourceType, ms, destinationType);
        ms.Position = 0;
        return ms;
    }

    /// <summary>
    /// Retrieves the summary of the specified file.
    /// </summary>
    /// <param name="file">The path to the file.</param>
    [HttpGet("{*file}")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType((int)HttpStatusCode.NotFound)]
    public async Task<IActionResult> Summary(string file) =>
        await _docs.GetContentAsync(file) switch
        {
            null => NotFound(),
            ContentReference blob => File(blob.Content, blob.ContentType, blob.FileName)
        };

    /// <summary>
    /// Upload file content
    /// </summary>
    /// <param name="content">upload file content</param>
    /// <param name="file"></param>
    /// <param name="sourceContentType">optionally overload the provided MIME Type</param>
    /// <returns></returns>
    [HttpPost("{*file}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(IFormFile content, string file, string? sourceContentType = null)
    {
        //Note: https://github.com/domaindrivendev/Swashbuckle.AspNetCore#handle-forms-and-file-uploads

        await Task.Yield();
        _logger.LogDebug("Start Upload");
        if (ModelState.IsValid)
        {
            _logger.LogDebug("Upload Ok");
            await _docs.StoreContentAsync(new()
            {
                Content = content.OpenReadStream(),
                ContentType = content.ContentType,
                FileName = file ?? content.FileName,
            });
            return Ok();
        }

        _logger.LogDebug("Upload BadRequest");
        return BadRequest(ModelState);
    }

    /// <summary>
    /// Document Converter
    /// </summary>
    /// <param name="content">upload file content</param>
    /// <param name="targetContentType">define the target MIME type</param>
    /// <param name="sourceContentType">optionally overload the provided MIME Type</param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Convert(IFormFile content, string targetContentType, string? sourceContentType = null)
    {
        // https://github.com/domaindrivendev/Swashbuckle.AspNetCore#handle-forms-and-file-uploads
        using var source = new MemoryStream();
        await content.CopyToAsync(source);
        source.Position = 0;

        var destination = new MemoryStream();
        await _converter.ConvertAsync(source, sourceContentType ?? content.ContentType, destination, targetContentType);
        destination.Position = 0;

        var type = _documentTypes.GetByContentType(targetContentType);

        var targetFile = type != null ? Path.ChangeExtension(content.FileName, type.FileExtensions.FirstOrDefault()) : null;

        return File(destination, targetContentType, fileDownloadName: targetFile);
    }
}
