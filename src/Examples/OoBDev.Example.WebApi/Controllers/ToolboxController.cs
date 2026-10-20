using OoBDev.System.ExpressionCalculator.Expressions;
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

    /// <summary>Evaluates an arithmetic expression.</summary>
    /// <param name="model">The expression and variable values.</param>
    /// <returns>The result, optimized form and variable names.</returns>
    [HttpPost("calculate")]
    public ActionResult<CalculateResponseModel> Calculate([FromBody] CalculateRequestModel model)
    {
        try
        {
            var expression = model.Expression.ParseAsExpression<decimal>();
            var names = expression.GetDistinctVariableNames().ToList();
            var missing = names.Where(n => !model.Variables.ContainsKey(n)).ToList();
            if (missing.Count > 0)
                return Problem(title: "Missing values", detail: $"Provide a value for: {string.Join(", ", missing)}.", statusCode: 400);
            return new CalculateResponseModel
            {
                Result = expression.Evaluate(model.Variables.Select(v => (v.Key, v.Value))),
                Optimized = expression.Optimize().ToString() ?? "",
                Variables = names,
            };
        }
        catch (Exception ex) when (ex is not OutOfMemoryException)
        {
            return Problem(title: "Could not evaluate the expression", detail: ex.Message, statusCode: 400);
        }
    }

    /// <summary>Generates or checks a time based one-time code (the demo returns the secret; real systems never do).</summary>
    /// <param name="model">An optional secret and an optional code to validate.</param>
    /// <returns>The secret, current code and authenticator URI.</returns>
    [HttpPost("otp")]
    public ActionResult<OtpResponseModel> Otp([FromBody] OtpRequestModel model)
    {
        var otp = new OneTimeCode();
        var secret = string.IsNullOrWhiteSpace(model.Secret) ? otp.GenerateSecret() : model.Secret.Trim();
        try
        {
            return new OtpResponseModel
            {
                Secret = secret,
                Code = otp.GetToken(secret),
                Uri = otp.GetUri(secret, "OoBDev Example", "demo"),
                Valid = string.IsNullOrWhiteSpace(model.Code) ? null : otp.IsValid(secret, model.Code.Trim()),
            };
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException)
        {
            return Problem(title: "The secret is not valid Base32", detail: ex.Message, statusCode: 400);
        }
    }
}
