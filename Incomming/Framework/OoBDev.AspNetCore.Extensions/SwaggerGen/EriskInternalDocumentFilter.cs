using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace OoBDev.AspNetCore.Extensions.SwaggerGen;

public class OoBDevInternalDocumentFilter : IDocumentFilter
{
    private IWebHostEnvironment _env;
    public OoBDevInternalDocumentFilter(IWebHostEnvironment env)
    {
        _env = env;
    }
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        // Comment out the following line to see the EriskInternal endpoints
#if DEBUG
        if (!(new[] { "Local", "Development" }.Any(e => _env.EnvironmentName.Contains(e))))
#endif
        {
            foreach (var item in context.ApiDescriptions)
            {
                var actionDescriptor = (ControllerActionDescriptor)item.ActionDescriptor;
                if (actionDescriptor.MethodInfo.GetCustomAttributes<OoBDevInternalAttribute>().Any() ||
                    actionDescriptor.ControllerTypeInfo.GetCustomAttribute<OoBDevInternalAttribute>() != null)
                {
                    var key = "/" + item.RelativePath?.TrimEnd('/').Replace("{version}", item.GetApiVersion()?.ToString());

                    swaggerDoc.Paths.Remove(key);

                    if (actionDescriptor.ControllerTypeInfo.GetCustomAttribute<OoBDevInternalAttribute>() != null)
                    {
                        var controllerToRemove = actionDescriptor.ControllerName;
                        foreach (OpenApiTag tag in swaggerDoc.Tags)
                        {
                            if (tag.Name.Equals(controllerToRemove))
                            {
                                swaggerDoc.Tags.Remove(tag);
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
