using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.Identity.Jws;
using OoBDev.System;
using OoBDev.System.Codecs;
using OoBDev.System.ExpressionCalculator.Expressions;
using OoBDev.System.Math;
using OoBDev.System.Security.Cryptography;
using OoBDev.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Small framework utilities as endpoints: hashing, message signing and vector distance.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApplicationRight("toolbox.use")]
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

    /// <summary>Converts text with Morse code, Roman numerals, Base32 or packed BCD (hex).</summary>
    /// <param name="model">The codec, direction and text.</param>
    /// <returns>The converted text.</returns>
    [HttpPost("codec")]
    public ActionResult<ToolResultModel> Codec([FromBody] CodecRequestModel model)
    {
        if (model.Value.Length > 10_000)
            return Problem(title: "The text is too long", detail: "At most 10000 characters.", statusCode: 400);
        var key = model.Codec.Trim().ToLowerInvariant();
        try
        {
            string? result = key switch
            {
                "morse" => model.Decode ? new MorseCode().Decode(model.Value) : new MorseCode().Encode(model.Value.ToUpperInvariant()),
                "roman" => model.Decode
                    ? new RomanNumeral().Convert(model.Value.Trim().ToUpperInvariant()).ToString(global::System.Globalization.CultureInfo.InvariantCulture)
                    : new RomanNumeral().Convert(int.Parse(model.Value.Trim(), global::System.Globalization.CultureInfo.InvariantCulture)),
                "base32" => model.Decode ? Encoding.UTF8.GetString(new Base32Codec().Decode(model.Value.Trim())) : new Base32Codec().Encode(Encoding.UTF8.GetBytes(model.Value)),
                "bcd" => model.Decode ? BcdEx.AsBCD((ReadOnlySpan<byte>)Convert.FromHexString(model.Value.Trim())).ToString(global::System.Globalization.CultureInfo.InvariantCulture) : ToBcdHex(model.Value.Trim()),
                _ => null,
            };
            return result is null
                ? Problem(title: "Unknown codec", detail: "Use morse, roman, base32 or bcd.", statusCode: 400)
                : new ToolResultModel { Algorithm = key, Result = result };
        }
        catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException or InvalidOperationException or KeyNotFoundException)
        {
            return Problem(title: "The text cannot be converted", detail: ex.Message, statusCode: 400);
        }
    }
private static string ToBcdHex(string digits)    {        if (digits.Length == 0 || digits.Length > 18 || !digits.All(char.IsAsciiDigit))            throw new ArgumentException("Give 1 to 18 decimal digits.", nameof(digits));        var bytes = new byte[(digits.Length + 1) / 2];        BcdEx.ToBCD(long.Parse(digits, global::System.Globalization.CultureInfo.InvariantCulture), bytes);        return Convert.ToHexString(bytes);    }

    /// <summary>Signs a JSON payload as an HS256 token.</summary>
    /// <param name="model">The payload and the shared secret.</param>
    /// <returns>The compact token.</returns>
    [HttpPost("jws/sign")]
    public ActionResult<ToolResultModel> JwsSign([FromBody] JwsSignRequestModel model)
    {
        var key = Encoding.UTF8.GetBytes(model.Secret);
        if (key.Length < JsonWebSignature.MinimumHs256KeyBytes)
            return Problem(title: "The secret is too short", detail: $"Use at least {JsonWebSignature.MinimumHs256KeyBytes} bytes.", statusCode: 400);
        try
        {
            return new ToolResultModel { Algorithm = JsonWebSignature.Hs256, Result = JsonWebSignature.SignHs256("{\"alg\":\"HS256\",\"typ\":\"JWT\"}", model.Payload, key) };
        }
        catch (Exception ex) when (ex is ArgumentException)
        {
            return Problem(title: "Could not sign the payload", detail: ex.Message, statusCode: 400);
        }
    }

    /// <summary>Reads the header and payload of a compact token and checks an HS256 signature when a secret is given.</summary>
    /// <param name="model">The token and an optional secret.</param>
    /// <returns>The decoded parts and the verification result.</returns>
    [HttpPost("jws/read")]
    public ActionResult<JwsReadResponseModel> JwsRead([FromBody] JwsReadRequestModel model)
    {
        var parts = model.Token.Trim().Split('.');
        if (parts.Length != 3 || !TryDecode(parts[0], out var header) || !TryDecode(parts[1], out var payload))
            return Problem(title: "Not a compact token", detail: "Expected three base64url parts separated by dots.", statusCode: 400);
        bool? verified = string.IsNullOrEmpty(model.Secret) ? null : JsonWebSignature.TryVerifyHs256(model.Token.Trim(), Encoding.UTF8.GetBytes(model.Secret), out _);
        return new JwsReadResponseModel { Header = header, Payload = payload, Verified = verified };
    }

    private static bool TryDecode(string part, out string text)
    {
        text = string.Empty;
        if (!Base64Url.IsValid(part))
            return false;
        text = Encoding.UTF8.GetString(Base64Url.DecodeFromChars(part));
        return true;
    }
}
