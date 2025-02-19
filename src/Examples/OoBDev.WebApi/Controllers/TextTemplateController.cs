using OoBDev.System.Text.Templating;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

    IEnumerable<IFileType> fileTypes
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

    // /// <summary>
    // /// Queries text templates.
    // /// </summary>
    //[HttpPost()]
    //[AllowAnonymous]
    //public IQueryable<TextTemplateModel> Query() => provider.Query();

    // /// <summary>
    // /// Saves a text template.
    // /// </summary>
    //[HttpPost()]
    //[AllowAnonymous]
    //public async Task<string> Save([FromBody] TextTemplateModel model) =>
    //    await provider.SaveAsync(model);

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
                FileDownloadName = $"{context.TemplateName}-{DateTimeOffset.Now:yyyyMMddHHmmss}{context.TargetFileExtension}"
            };
    }
}
