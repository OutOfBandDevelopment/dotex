using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>A function a Semantic Kernel plug-in offers.</summary>
public class KernelFunctionModel
{
    /// <summary>Gets or sets the plug-in name.</summary>
    public required string PlugIn { get; set; }

    /// <summary>Gets or sets the function name.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets the description the model sees.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the parameter names with their descriptions.</summary>
    public IDictionary<string, string?> Parameters { get; set; } = new Dictionary<string, string?>();
}

/// <summary>A request to invoke one plug-in function.</summary>
public class KernelInvokeRequestModel
{
    /// <summary>Gets or sets the plug-in name.</summary>
    public required string PlugIn { get; set; }

    /// <summary>Gets or sets the function name.</summary>
    public required string Function { get; set; }

    /// <summary>Gets or sets the argument values by parameter name.</summary>
    public IDictionary<string, string> Arguments { get; set; } = new Dictionary<string, string>();
}
