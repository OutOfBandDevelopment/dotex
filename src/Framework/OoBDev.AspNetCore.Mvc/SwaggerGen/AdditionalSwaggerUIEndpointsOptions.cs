using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Linq;

namespace OoBDev.AspNetCore.Mvc.SwaggerGen;

/// <summary>
/// SwaggerUI Extension to enable grouping controller/actions by assembly
/// </summary>
public class AdditionalSwaggerUIEndpointsOptions(
    IActionDescriptorCollectionProvider provider
        ) : IConfigureOptions<SwaggerUIOptions>
{
    /// <summary>
    /// Configures SwaggerUI options to enable grouping of controller/actions by assembly.
    /// </summary>
    /// <param name="options">The SwaggerUI options to be configured.</param>
    public void Configure(SwaggerUIOptions options)
    {
        options.SwaggerEndpoint("/swagger/all/swagger.json", "All");

        if (provider != null)
        {
            foreach (var group in provider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
                .Select(c => c.ControllerTypeInfo.Assembly)
                .Distinct()
                .Select(s => s.GetName().Name))
            {
                options.SwaggerEndpoint($"/swagger/{group}/swagger.json", group);
            }
        }

        options.ShowExtensions();
    }
}
