using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>The values entered so far, by control id.</summary>
public sealed class UiEvaluateRequestModel
{
    /// <summary>Control id to text value (toggles use <c>true</c> and <c>false</c>).</summary>
    public Dictionary<string, string> Values { get; init; } = [];
}
