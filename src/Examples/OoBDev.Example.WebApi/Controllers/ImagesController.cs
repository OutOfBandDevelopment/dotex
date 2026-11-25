using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Vision.ClipVitB32;
using OoBDev.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// In-process image embeddings and zero-shot labels with CLIP ViT-B/32. The model is downloaded on first use,
/// so the first request can take a while.
/// </summary>
[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class ImagesController(IServiceProvider services) : ControllerBase
{
    private const long MaxBytes = 8 * 1024 * 1024;

    /// <summary>Ranks the labels by how well each describes the image.</summary>
    /// <param name="file">The image (PNG, JPEG, WebP, ...), up to 8 MB.</param>
    /// <param name="labels">Comma-separated candidate labels.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The labels with probabilities, best first.</returns>
    [HttpPost("classify")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<List<ImageLabelModel>>> Classify(IFormFile file, [FromForm] string labels, CancellationToken cancellationToken)
    {
        var list = (labels ?? "").Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (list.Count < 2 || list.Count > 50)
            return Problem(title: "Invalid labels", detail: "Give 2 to 50 comma-separated labels.", statusCode: StatusCodes.Status400BadRequest);
        if (file is null || file.Length == 0 || file.Length > MaxBytes)
            return Problem(title: "Invalid image", detail: "Upload an image of up to 8 MB.", statusCode: StatusCodes.Status400BadRequest);

        try
        {
            var classifier = services.GetRequiredKeyedService<ZeroShotImageClassifier>(VisionGlobals.ClipVitB32Key);
            var bytes = await ReadAsync(file, cancellationToken);
            var result = await classifier.ClassifyAsync(new DataContent(bytes, file.ContentType), list, list.Count, cancellationToken);
            return result.Select(r => new ImageLabelModel { Label = r.Label, Probability = r.Probability }).ToList();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Problem(title: "The image could not be classified", detail: ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
        }
    }

    /// <summary>Embeds the image and describes the vector.</summary>
    /// <param name="file">The image, up to 8 MB.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <returns>The vector length, its norm and the first values.</returns>
    [HttpPost("embed")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ImageEmbeddingModel>> Embed(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0 || file.Length > MaxBytes)
            return Problem(title: "Invalid image", detail: "Upload an image of up to 8 MB.", statusCode: StatusCodes.Status400BadRequest);
        try
        {
            var generator = services.GetRequiredKeyedService<IEmbeddingGenerator<DataContent, Embedding<float>>>(VisionGlobals.ClipVitB32Key);
            var bytes = await ReadAsync(file, cancellationToken);
            var embeddings = await generator.GenerateAsync([new DataContent(bytes, file.ContentType)], cancellationToken: cancellationToken);
            var v = embeddings[0].Vector.ToArray();
            return new ImageEmbeddingModel { Length = v.Length, Norm = Math.Sqrt(v.Sum(x => (double)x * x)), First = v.Take(8).ToList() };
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Problem(title: "The image could not be embedded", detail: ex.Message, statusCode: StatusCodes.Status422UnprocessableEntity);
        }
    }

    private static async Task<byte[]> ReadAsync(IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        using var buffer = new global::System.IO.MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        return buffer.ToArray();
    }
}
