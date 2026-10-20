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
