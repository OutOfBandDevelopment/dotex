using OoBDev.System.Net.Mime;
using OoBDev.System.Text.Templating;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Controller for handling text template operations.
/// </summary>
/// <remark>
/// Initializes a new instance of the <see cref="TextTemplateController"/> class.
/// </remark>
/// <param name="engine">The template engine for processing text templates.</param>
/// <param name="fileTypes">The collection of file types supported by the controller.</param>
[ApiController]
[Route("api/[controller]/[action]")]
public class TextTemplateController(
    //ITextTemplateProvider provider,
    ITemplateEngine engine,

    IEnumerable<IFileType> fileTypes,
    TimeProvider timeProvider,
    TemplateCatalog catalog
    ) : ControllerBase
{

    /// <summary>
    /// Gets the list of supported template file types.
    /// </summary>
    /// <returns>An enumeration of supported file types.</returns>
    [HttpPost()]
    [AllowAnonymous]
    public IEnumerable<FileType> SupportedTemplates() =>
        fileTypes.Cast<FileType>().Where(ct => ct.IsTemplateType);

    /// <summary>
    /// Lists the template files in the template folder.
    /// </summary>
    /// <returns>The templates, ordered by name.</returns>
    [HttpGet()]
    [AllowAnonymous]
    public IReadOnlyList<TemplateInfoModel> List() => catalog.List();

    /// <summary>
    /// Reads the source text of a listed template.
    /// </summary>
    /// <param name="file">The file name from the list.</param>
    /// <returns>The template and its text, or not found.</returns>
    [HttpGet()]
    [AllowAnonymous]
    public ActionResult<TemplateDetailModel> Read(string file) =>
        catalog.Read(file) is { } detail ? detail : NotFound();

    /// <summary>
    /// Renders a draft template text with sample data. Nothing is saved.
    /// </summary>
    /// <param name="model">The draft text, the engine, the target extension and the data.</param>
    /// <returns>The rendered text, or a problem when no engine can render the draft.</returns>
    [HttpPost()]
    [AllowAnonymous]
    public async Task<IActionResult> Preview([FromBody] TemplatePreviewRequestModel model)
    {
        var templateType = string.IsNullOrWhiteSpace(model.TemplateContentType) ? ContentTypesExtensions.Text.HandlebarsTemplate : model.TemplateContentType;
        var target = fileTypes.FirstOrDefault(t => !t.IsTemplateType && string.Equals(t.Extension, model.TargetExtension, StringComparison.OrdinalIgnoreCase));
        var bytes = Encoding.UTF8.GetBytes(model.Text);
        var context = new TemplateContext
        {
            TemplateName = "preview",
            TemplateContentType = templateType,
            TemplateFileExtension = string.Empty,
            TemplateSource = null!,
            TemplateReference = "preview",
            OpenTemplate = _ => new MemoryStream(bytes),
            TargetContentType = target?.ContentType ?? "text/plain",
            TargetFileExtension = model.TargetExtension,
        };

        using var output = new MemoryStream();
        try
        {
            if (!await engine.ApplyAsync(context, model.Data ?? new JsonObject(), output))
            {
                return Problem(title: "Cannot render", detail: $"No template engine handles '{templateType}'.", statusCode: StatusCodes.Status400BadRequest);
            }
        }
        catch (Exception ex)
        {
            return Problem(title: "Template error", detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
        }

        return Content(Encoding.UTF8.GetString(output.ToArray()), "text/plain");
    }

    /// <summary>
    /// Applies a text template with the specified name and data.
    /// </summary>
    /// <param name="templateName">The name of the text template to apply.</param>
    /// <param name="data">The JSON data used for template processing.</param>
    /// <returns>An action result containing the processed template content as a downloadable file.</returns>
    [HttpPost()]
    [AllowAnonymous]
    public async Task<IActionResult> Apply(string templateName, [FromBody] JsonNode? data = default)
    {
        var ms = new MemoryStream();
        var context = await engine.ApplyAsync(templateName, data ?? new JsonObject(), ms);
        return context == null
            ? NotFound()
            : new FileContentResult(ms.ToArray(), context.TargetContentType)
            {
                FileDownloadName = $"{context.TemplateName}-{timeProvider.GetLocalNow():yyyyMMddHHmmss}{context.TargetFileExtension}"
            };
    }
}
