using OoBDev.AspNetCore.Mvc.Filters;
using OoBDev.System.ComponentModel;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OoBDev.AspNetCore.Mvc.SwaggerGen;

/// <summary>
/// SwaggerGen extensions to enable presenting permissions, application versions and XMLDocs
/// </summary>
public class AdditionalSwaggerGenEndpointsOptions(
    IActionDescriptorCollectionProvider provider,
    ILogger<AdditionalSwaggerGenEndpointsOptions> log,
    IEnumerable<IVersionProvider> versions
        ) : IConfigureOptions<SwaggerGenOptions>
{
    /// <summary>
    /// Configures SwaggerGen options with additional features such as presenting permissions, application versions, and XMLDocs.
    /// </summary>
    /// <param name="options">The SwaggerGen options to be configured.</param>
    public void Configure(SwaggerGenOptions options)
    {
        options.OperationFilter<ApplicationPermissionsApiFilter>();

        var controllerAssemblies = provider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
            .Select(c => c.ControllerTypeInfo.Assembly)
            .Distinct();

        var composedVersions = (from v in versions.Reverse()
                                select new
                                {
                                    v.Title,
                                    v.Description,
                                    v.Version,
                                    v.Assembly,
                                }).Concat(from v in controllerAssemblies.Concat(new[]
                                {
                                    Assembly.GetEntryAssembly(),
                                    Assembly.GetCallingAssembly(),
                                    Assembly.GetExecutingAssembly(),
                                })
                                          select new
                                          {
                                              Title = (string?)null,
                                              Description = (string?)null,
                                              Version = (string?)null,
                                              Assembly = v,
                                          });

        var fileVersions = from i in composedVersions
                           let a = i.Assembly
                           let an = a?.GetName()
                           let fvi = a == null ? null : FileVersionInfo.GetVersionInfo(a.Location)
                           select new
                           {
                               Name = new[] { i.Title, fvi.ProductName, an?.Name }.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i)),
                               Version = new[] { i.Version, fvi.FileVersion, an.Version?.ToString() }.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i)),
                               Description = new[] { i.Description, fvi.Comments }.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i)),
                           };

        var selected = fileVersions.FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.Name));

        options.DocInclusionPredicate((name, desc) => name == "all" || name == desc.GroupName);
        options.CustomSchemaIds(ResolveSchemaType); // https://wegotcode.com/microsoft/swagger-fix-for-dotnetcore/

        options.SwaggerDoc("all", new OpenApiInfo
        {
            Title = selected?.Name ?? "Unknown",
            Version = selected?.Version ?? "v0.0.0.0",
            Description = selected?.Description,
        });

        foreach (var group in provider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
            .Select(c => c.ControllerTypeInfo.Assembly)
            .Distinct())
        {
            var groupName = group.GetName().Name;
            options.SwaggerDoc(groupName, new OpenApiInfo
            {
                Title = groupName,
                Version = group.GetName().Version?.ToString() ?? "v0.0.0.0",
            });
        }

        var commentFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
        foreach (var file in commentFiles)
        {
            try
            {
                log.LogWarning($"Loading comments from \"{{{nameof(file)}}}\"", Path.GetFileName(file));
                options.IncludeXmlComments(file);
            }
            catch (Exception e)
            {
                log.LogWarning($"{{{nameof(file)}}}: {{{nameof(e.Message)}}}", Path.GetFileName(file), e.Message);
            }
        }
    }

    private string ResolveSchemaType(Type type)
    {
        return type.IsGenericType
            ? $"{type.Namespace}.{type.Name.Split('`')[0]}-{string.Join("_", type.GetGenericArguments().Select(ResolveSchemaType))}"
            : $"{type.Namespace}.{type.Name}";
    }
}
