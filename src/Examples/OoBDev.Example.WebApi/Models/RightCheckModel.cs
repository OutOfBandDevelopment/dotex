namespace OoBDev.WebApi.Models;

/// <summary>The result of checking the current user against a list of rights.</summary>
public class RightCheckModel
{
    /// <summary>Gets or sets the rights that were asked for.</summary>
    public string[] Required { get; set; } = [];

    /// <summary>Gets or sets the rights the current user holds.</summary>
    public string[] Held { get; set; } = [];

    /// <summary>Gets or sets the outcome: <c>Allowed</c>, <c>Forbidden</c> or <c>Unauthenticated</c>.</summary>
    public required string Result { get; set; }
}
