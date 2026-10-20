using OoBDev.System.Math;
using OoBDev.System.Security.Cryptography;
using OoBDev.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Small framework utilities as endpoints: hashing, message signing and vector distance.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ToolboxController(IServiceProvider services) : ControllerBase
{
    private static readonly string[] Hashes = ["md5", "sha256", "sha512"];
    private static readonly string[] Hmacs = ["hmac-256", "hmac-512", "hmac3-256", "hmac3-512"];

    /// <summary>Lists the supported algorithm keys.</summary>
    /// <returns>Hash and HMAC keys.</returns>
    [HttpGet("algorithms")]
    public object Algorithms() => new { hashes = Hashes, hmacs = Hmacs };

    /// <summary>Hashes text.</summary>
    /// <param name="model">The algorithm and text.</param>
    /// <returns>The hash.</returns>
    [HttpPost("hash")]
    public ActionResult<ToolResultModel> Hash([FromBody] HashRequestModel model)
    {
        if (!Hashes.Contains(model.Algorithm))
            return Problem(title: "Unknown algorithm", detail: $"Use one of: {string.Join(", ", Hashes)}.", statusCode: 400);
        var hash = services.GetRequiredKeyedService<IHash>(model.Algorithm);
        return new ToolResultModel { Algorithm = model.Algorithm, Result = hash.GetHash(model.Value) };
    }

    /// <summary>Signs a message with a shared secret.</summary>
    /// <param name="model">The algorithm, secret and message.</param>
    /// <returns>The encoded signature.</returns>
    [HttpPost("hmac")]
    public ActionResult<ToolResultModel> Hmac([FromBody] HmacRequestModel model)
    {
        if (!Hmacs.Contains(model.Algorithm))
            return Problem(title: "Unknown algorithm", detail: $"Use one of: {string.Join(", ", Hmacs)}.", statusCode: 400);
        var calculator = services.GetRequiredKeyedService<IHMACCalculator>(model.Algorithm);
        return new ToolResultModel { Algorithm = model.Algorithm, Result = calculator.CalculateAndEncode(model.Secret, model.Message) };
    }

    /// <summary>Compares two vectors under every distance metric.</summary>
    /// <param name="model">The two vectors; they must have the same length.</param>
    /// <returns>The four distances.</returns>
    [HttpPost("vector-distance")]
    public ActionResult<VectorDistanceResponseModel> VectorDistance([FromBody] VectorDistanceRequestModel model)
    {
        if (model.A.Count == 0 || model.A.Count != model.B.Count)
            return Problem(title: "Vectors must have the same, non-zero length", statusCode: 400);
        var a = Vector.Create(model.A);
        var b = Vector.Create(model.B);
        return new VectorDistanceResponseModel
        {
            Cosine = a.Distance(b, VectorDistanceMetrics.Cosine) ?? double.NaN,
            Euclidean = a.Distance(b, VectorDistanceMetrics.Euclidean) ?? double.NaN,
            DotProduct = a.Distance(b, VectorDistanceMetrics.DotProduct) ?? double.NaN,
            Manhattan = a.Distance(b, VectorDistanceMetrics.Manhattan) ?? double.NaN,
        };
    }
}
