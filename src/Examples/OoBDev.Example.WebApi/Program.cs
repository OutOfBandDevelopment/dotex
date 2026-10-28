using OoBDev.AspNetCore.JwtAuthentication.OpenApi;
using OoBDev.AspNetCore.Mvc;
using OoBDev.Common;
using OoBDev.Common.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace OoBDev.WebApi;

/// <summary>
/// primary entry point
/// </summary>
public static class Program
{
    /// <summary>
    /// primary entry point
    /// </summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;

        // Add example application services 
        services.AddApplicationServices();

        var identityProvider = Enum.TryParse<IdentityProviders>(
            Environment.GetEnvironmentVariable("IDENTITY_PROVIDER"), ignoreCase: true, out var ip) ? ip :
            IdentityProviders.None;
        var authProvider = identityProvider != IdentityProviders.None ? $"{identityProvider}:" : "";

        // Add internal services
        services.TryAllCommonExtensions(
            builder.Configuration,
            systemBuilder: new()
            {
            },
            aspNetBuilder: new()
            {
                RequireApplicationUserId = false,
                RequireAuthenticatedByDefault = identityProvider != IdentityProviders.None,
            },
            jwtBuilder: new()
            {
                JwtBearerConfigurationSection = authProvider + nameof(JwtBearerOptions),
                OAuth2OpenApiConfigurationSection = authProvider + nameof(OAuth2OpenApiOptions),
            },
            identityBuilder: new()
            {
                IdentityProvider = identityProvider,
            },
            externalBuilder: new()
            {
            },
            hostingBuilder: new()
            {
                //TODO: add a run profile that disable hosting
                DisableMailKit = true, //TODO: this is not supported at this time so just skip it ...skipHosting,
                //DisableMessageQueueing = true,  // skipHosting,
            });

        // The vector store: SQL Server when configured, otherwise in memory (replaces the Qdrant default)
        services.RemoveAll<OoBDev.Search.Semantic.IVectorStoreProviderFactory>();
        if (!string.IsNullOrWhiteSpace(builder.Configuration["SqlVectorStoreOptions:ConnectionString"]))
            OoBDev.Data.Vectors.SqlStore.ServiceCollectionExtensions.TryAddSqlVectorStore(services, builder.Configuration, "SqlVectorStoreOptions");
        else
            services.AddSingleton<OoBDev.Search.Semantic.IVectorStoreProviderFactory, OoBDev.WebApi.Services.InMemoryVectorStoreProviderFactory>();

        // Extra sentence models for the compare screen; downloaded only when first used
        OoBDev.SBert.AllMpnetBaseV2.ServiceCollectionExtensions.TryAddAllMpnetBaseV2Services(services, builder.Configuration, "AllMpnetBaseV2");
        OoBDev.SBert.NomicEmbedTextV1_5.ServiceCollectionExtensions.TryAddNomicEmbedTextV1_5Services(services, builder.Configuration, "NomicEmbedTextV1_5");

        // CLIP image embeddings; the model is only downloaded when first resolved
        OoBDev.Onnx.ImageEmbeddings.Skia.ServiceCollectionExtensions.TryAddSkiaImageDecoder(services);
        OoBDev.Vision.ClipVitB32.ServiceCollectionExtensions.TryAddClipVitB32Services(services, builder.Configuration, "ClipVitB32");

        // Add services to the container.
        services.AddLogging(builder => builder
            .AddConsole()
#if DEBUG
            .SetMinimumLevel(LogLevel.Debug)
#endif
        );

        builder.Services.AddControllers();

        // CGI programs (see cgi-src/ and README.Example.WebApi.md); their {program}.openapi.json files appear in the "all" OpenAPI document
        builder.Configuration["OoBDev:Cgi:ScriptRoot"] = Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["OoBDev:Cgi:ScriptRoot"] ?? "cgi-bin");
        OoBDev.AspNetCore.Cgi.ServiceCollectionExtensions.TryAddCgiServices(services, builder.Configuration, "OoBDev:Cgi");
        OoBDev.AspNetCore.Cgi.OpenApi.ServiceCollectionExtensions.TryAddCgiOpenApi(services, builder.Configuration, "OoBDev:Cgi:OpenApi");

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        // OpenAPI documents at /openapi/{document}.json and the Scalar API reference at /scalar;
        // AsyncAPI documents at /asyncapi/{document}.json and the viewer at /asyncapi/{document}
        if (app.Environment.IsDevelopment())
        {
            app.MapApiReference();
            app.MapAsyncApi();
        }

        app.UseHttpsRedirection();

        app.UseAllCommonMiddleware(
            middlewareBuilder: new()
            {
            });

        app.UseAuthentication();
        app.UseAuthorization();
        OoBDev.AspNetCore.Cgi.ServiceCollectionExtensions.UseCgi(app);

        app.MapControllers();

        app.Run();
    }
}
