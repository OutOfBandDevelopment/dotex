using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.SemanticKernel;
using OoBDev.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Lists the Semantic Kernel plug-in functions registered with the framework and invokes one with text arguments,
/// without a model in the loop.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[ApplicationRight("toolbox.use")]
public class KernelPlugInsController([FromKeyedServices(KernelGlobal.Name)] Kernel kernel) : ControllerBase
{
    /// <summary>Lists every registered plug-in function.</summary>
    /// <returns>The functions with their parameters.</returns>
    [HttpGet]
    public IEnumerable<KernelFunctionModel> List() =>
        kernel.Plugins.SelectMany(p => p).Select(f => new KernelFunctionModel
        {
            PlugIn = f.PluginName ?? string.Empty,
            Name = f.Name,
            Description = f.Description,
            Parameters = f.Metadata.Parameters.ToDictionary(x => x.Name, x => x.Description),
        });

    /// <summary>Invokes one function with text arguments.</summary>
    /// <param name="model">The plug-in, function and arguments.</param>
    /// <param name="cancellationToken">Cancels the call.</param>
    /// <returns>The result as text.</returns>
    [HttpPost("invoke")]
    public async Task<ActionResult<ToolResultModel>> Invoke([FromBody] KernelInvokeRequestModel model, CancellationToken cancellationToken)
    {
        if (!kernel.Plugins.TryGetFunction(model.PlugIn, model.Function, out var function))
            return Problem(title: "Unknown function", detail: $"{model.PlugIn}.{model.Function} is not registered.", statusCode: 404);

        var arguments = new KernelArguments();
        foreach (var (key, value) in model.Arguments)
            arguments[key] = value;
        try
        {
            var result = await kernel.InvokeAsync(function, arguments, cancellationToken);
            return new ToolResultModel { Algorithm = $"{model.PlugIn}.{model.Function}", Result = result.ToString() };
        }
        catch (Exception ex) when (ex is KernelException or ArgumentException or FormatException)
        {
            return Problem(title: "The function failed", detail: ex.Message, statusCode: 400);
        }
    }
}
