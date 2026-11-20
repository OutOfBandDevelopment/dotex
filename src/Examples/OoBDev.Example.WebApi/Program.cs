using OoBDev.AspNetCore.JwtAuthentication.OpenApi;
using OoBDev.AspNetCore.EntityFrameworkCore;
using OoBDev.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        ConfigureServices(builder);

        var app = builder.Build();
        ConfigurePipeline(app);
        app.Run();
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        // Add example application services 
        services.AddApplicationServices();

        var identityProvider = Enum.TryParse<IdentityProviders>(
            Environment.GetEnvironmentVariable("IDENTITY_PROVIDER"), ignoreCase: true, out var ip) ? ip :
            IdentityProviders.None;
        var disableHosting = string.Equals(Environment.GetEnvironmentVariable("DISABLE_HOSTING"), "true", StringComparison.OrdinalIgnoreCase);
        var authProvider = identityProvider != IdentityProviders.None ? $"{identityProvider}:" : "";

        // Add internal services
        services.TryAllCommonExtensions(
            builder.Configuration,
            systemBuilder: new()
            {
            },
            aspNetBuilder: new()
            {
                ApplicationUserIdClaim = null,
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
                // DISABLE_HOSTING=true (launch profile "http (no hosting)") runs the API without its hosted services
                DisableCommunications = disableHosting,
                DisableMessageQueueing = disableHosting,
                DisableVectorHosting = disableHosting,
            });

        ConfigureVectorsAndModels(builder);
        builder.Services.AddTransient<OoBDev.WebApi.Services.TemplateCatalog>();
        builder.Services.Configure<OoBDev.WebApi.Models.QueueChannelsOptions>(builder.Configuration.GetSection("MessageQueue"));
        builder.Services.Configure<OoBDev.WebApi.Models.DataAccessOptions>(builder.Configuration.GetSection("OoBDev:DataAccess"));
        builder.Services.AddTransient<OoBDev.WebApi.Services.ReadOnlyQueryRunner>();
        builder.Services.AddTransient<OoBDev.WebApi.Services.QueueChannelCatalog>();
        builder.Services.AddSingleton<OoBDev.WebApi.Services.ReceivedMessageLog>();
        builder.Services.AddTransient<OoBDev.WebApi.Services.CgiProgramCatalog>();
        builder.Services.AddSingleton<OoBDev.WebApi.Services.EndpointRightCatalog>();
        builder.Services.AddSingleton<OoBDev.WebApi.Services.UiDefinitionCatalog>();
        builder.Services.AddSingleton<OoBDev.WebApi.Services.UiDefinitionEvaluator>();
        builder.Services.TryAddSingleton<OoBDev.AspNetCore.Authorization.IApplicationRightChecker, OoBDev.AspNetCore.Authorization.ApplicationRightChecker>();

        // Add services to the container.
        services.AddLogging(builder => builder
            .AddConsole()
#if DEBUG
            .SetMinimumLevel(LogLevel.Debug)
#endif
        );

        services.TryAddRoleRightMapping(builder.Configuration);
        ConfigureRightAudit(builder);
        builder.Services.AddControllers();

        // CGI programs (see cgi-src/ and README.Example.WebApi.md); their {program}.openapi.json files appear in the "all" OpenAPI document
        builder.Configuration["OoBDev:Cgi:ScriptRoot"] = Path.Combine(builder.Environment.ContentRootPath, builder.Configuration["OoBDev:Cgi:ScriptRoot"] ?? "cgi-bin");
        OoBDev.AspNetCore.Cgi.ServiceCollectionExtensions.TryAddCgiServices(services, builder.Configuration, "OoBDev:Cgi");
        OoBDev.AspNetCore.Cgi.OpenApi.ServiceCollectionExtensions.TryAddCgiOpenApi(services, builder.Configuration, "OoBDev:Cgi:OpenApi");

    }

    // The durable right-check trail, only when OoBDev:RightAudit:ConnectionString is set (SQL Server); the screen says "not configured" otherwise.
    private static void ConfigureRightAudit(WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration["OoBDev:RightAudit:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        builder.Services.TryAddEntityFrameworkRightAudit(builder.Configuration, (_, options) => options.UseSqlServer(connectionString));
        builder.Services.AddRightAuditLog();
    }

    private static void ConfigureVectorsAndModels(WebApplicationBuilder builder)
    {
        var services = builder.Services;

        // The vector store: SQL Server when configured, otherwise in memory (replaces the Qdrant default)
        services.RemoveAll<OoBDev.Search.Semantic.IVectorStoreProviderFactory>();
        services.RemoveAll<OoBDev.Search.Semantic.IVectorStoreCollectionCatalog>();
        if (!string.IsNullOrWhiteSpace(builder.Configuration["SqlVectorStoreOptions:ConnectionString"]))
            OoBDev.Data.Vectors.SqlStore.ServiceCollectionExtensions.TryAddSqlVectorStore(services, builder.Configuration, "SqlVectorStoreOptions");
        else
        {
            services.AddSingleton<OoBDev.Search.Semantic.IVectorStoreProviderFactory, OoBDev.WebApi.Services.InMemoryVectorStoreProviderFactory>();
            services.AddSingleton<OoBDev.Search.Semantic.IVectorStoreCollectionCatalog, OoBDev.WebApi.Services.InMemoryVectorStoreCollectionCatalog>();
        }

        // Extra sentence models for the compare screen; downloaded only when first used
        OoBDev.SBert.AllMpnetBaseV2.ServiceCollectionExtensions.TryAddAllMpnetBaseV2Services(services, builder.Configuration, "AllMpnetBaseV2");
        OoBDev.SBert.NomicEmbedTextV1_5.ServiceCollectionExtensions.TryAddNomicEmbedTextV1_5Services(services, builder.Configuration, "NomicEmbedTextV1_5");

        // CLIP image embeddings; the model is only downloaded when first resolved
        OoBDev.Onnx.ImageEmbeddings.Skia.ServiceCollectionExtensions.TryAddSkiaImageDecoder(services);
        OoBDev.Vision.ClipVitB32.ServiceCollectionExtensions.TryAddClipVitB32Services(services, builder.Configuration, "ClipVitB32");

        // Count estimated LLM usage (calls, tokens) per model for every completion provider registered above
        OoBDev.AI.ServiceCollectionExtensions.TryAddEstimatedLlmUsage(services);
    }

    private static void ConfigurePipeline(WebApplication app)
    {
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
        if (!string.IsNullOrWhiteSpace(app.Configuration["OoBDev:RightAudit:ConnectionString"]))
        {
            using (var scope = app.Services.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<IDbContextFactory<RightAuditDbContext>>().CreateDbContext())
                context.Database.EnsureCreated();
            app.MapRightAuditQuery(["audit.read"]);
        }
    }
}
