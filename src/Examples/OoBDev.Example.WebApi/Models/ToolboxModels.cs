using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>Text to hash with a named algorithm.</summary>
public class HashRequestModel
{
    /// <summary>Gets or sets the algorithm key: md5, sha256 or sha512.</summary>
    public required string Algorithm { get; set; }

    /// <summary>Gets or sets the text to hash.</summary>
    public required string Value { get; set; }
}

/// <summary>Message to sign with a shared secret.</summary>
public class HmacRequestModel
{
    /// <summary>Gets or sets the calculator key: hmac-256, hmac-512, hmac3-256 or hmac3-512.</summary>
    public required string Algorithm { get; set; }

    /// <summary>Gets or sets the shared secret.</summary>
    public required string Secret { get; set; }

    /// <summary>Gets or sets the message to sign.</summary>
    public required string Message { get; set; }
}

/// <summary>A computed text result.</summary>
public class ToolResultModel
{
    /// <summary>Gets or sets the algorithm key that was used.</summary>
    public required string Algorithm { get; set; }

    /// <summary>Gets or sets the result text.</summary>
    public required string Result { get; set; }
}

/// <summary>Two vectors to compare.</summary>
public class VectorDistanceRequestModel
{
    /// <summary>Gets or sets the first vector.</summary>
    public required List<double> A { get; set; }

    /// <summary>Gets or sets the second vector.</summary>
    public required List<double> B { get; set; }
}

/// <summary>Distance between two vectors under every supported metric.</summary>
public class VectorDistanceResponseModel
{
    /// <summary>Gets or sets the cosine distance (1 minus cosine similarity).</summary>
    public double Cosine { get; set; }

    /// <summary>Gets or sets the Euclidean distance.</summary>
    public double Euclidean { get; set; }

    /// <summary>Gets or sets the dot product.</summary>
    public double DotProduct { get; set; }

    /// <summary>Gets or sets the Manhattan distance.</summary>
    public double Manhattan { get; set; }
}

/// <summary>An arithmetic expression with optional named values.</summary>
public class CalculateRequestModel
{
    /// <summary>Gets or sets the expression, for example <c>(A + 2) * 3 ^ 2</c> (variables start with a capital letter).</summary>
    public required string Expression { get; set; }

    /// <summary>Gets or sets values for the variables used in the expression.</summary>
    public Dictionary<string, decimal> Variables { get; set; } = [];
}

/// <summary>The outcome of evaluating an expression.</summary>
public class CalculateResponseModel
{
    /// <summary>Gets or sets the result.</summary>
    public decimal Result { get; set; }

    /// <summary>Gets or sets the optimized form of the expression.</summary>
    public required string Optimized { get; set; }

    /// <summary>Gets or sets the variable names found in the expression.</summary>
    public required List<string> Variables { get; set; }
}

/// <summary>Request for a time based one-time code.</summary>
public class OtpRequestModel
{
    /// <summary>Gets or sets the Base32 secret; a new one is generated when empty.</summary>
    public string? Secret { get; set; }

    /// <summary>Gets or sets a code to validate against the secret.</summary>
    public string? Code { get; set; }
}

/// <summary>A one-time code for a secret.</summary>
public class OtpResponseModel
{
    /// <summary>Gets or sets the secret used.</summary>
    public required string Secret { get; set; }

    /// <summary>Gets or sets the current code.</summary>
    public required string Code { get; set; }

    /// <summary>Gets or sets the otpauth URI for authenticator apps.</summary>
    public required string Uri { get; set; }

    /// <summary>Gets or sets whether the supplied code is valid, when one was supplied.</summary>
    public bool? Valid { get; set; }
}
