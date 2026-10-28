using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Cgi.OpenApi.Tests;

[TestClass]
public class CgiOpenApiDocumentTransformerTests
{
    private const string Description = """
        {
          "openapi": "3.1.0",
          "info": { "title": "greet", "version": "1.0.0" },
          "paths": {
            "/": { "get": { "responses": { "200": { "description": "page" } } } },
            "/{id}": { "get": { "tags": [ "Own" ], "responses": { "200": { "description": "one" } } } }
          }
        }
        """;

    private string _root = string.Empty;

    [TestInitialize]
    public void Initialize()
    {
        _root = Path.Combine(Path.GetTempPath(), "cgi-openapi-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_root, recursive: true);

    private CgiOpenApiDocumentTransformer CreateTransformer() => new(
        Options.Create(new CgiOptions
        {
            ScriptRoot = _root,
            Handlers = { [".exe"] = string.Empty, [".cgi"] = string.Empty },
        }),
        Options.Create(new CgiOpenApiOptions()),
        NullLogger<CgiOpenApiDocumentTransformer>.Instance);

    private Task Run(OpenApiDocument document) => CreateTransformer().TransformAsync(document, null!, default);

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Transform_DescriptionBesideProgram_AddsPathsUnderPrefix()
    {
        // Stage
        File.WriteAllText(Path.Combine(_root, "greet.cgi"), "#!/bin/sh");
        File.WriteAllText(Path.Combine(_root, "greet.openapi.json"), Description);
        var document = new OpenApiDocument();

        // Test
        await Run(document);

        // Assert
        CollectionAssert.AreEquivalent(new[] { "/cgi-bin/greet.cgi", "/cgi-bin/greet.cgi/{id}" }, document.Paths.Keys.ToArray());
        Assert.IsTrue(document.Paths["/cgi-bin/greet.cgi"].Operations!.ContainsKey(HttpMethod.Get));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Transform_OperationWithoutTags_GetsTheProgramName()
    {
        // Stage
        File.WriteAllText(Path.Combine(_root, "greet.exe"), string.Empty);
        File.WriteAllText(Path.Combine(_root, "greet.openapi.json"), Description);
        var document = new OpenApiDocument();

        // Test
        await Run(document);

        // Assert
        var untagged = document.Paths["/cgi-bin/greet.exe"].Operations![HttpMethod.Get];
        Assert.AreEqual("greet", untagged.Tags!.Single().Name);
        var tagged = document.Paths["/cgi-bin/greet.exe/{id}"].Operations![HttpMethod.Get];
        Assert.AreEqual("Own", tagged.Tags!.Single().Name);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Transform_DescriptionWithoutProgram_IsSkipped()
    {
        // Stage
        File.WriteAllText(Path.Combine(_root, "ghost.openapi.json"), Description);
        var document = new OpenApiDocument();

        // Test
        await Run(document);

        // Assert
        Assert.IsTrue(document.Paths == null || document.Paths.Count == 0);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Transform_InvalidDescription_IsSkippedWithoutFailing()
    {
        // Stage
        File.WriteAllText(Path.Combine(_root, "greet.exe"), string.Empty);
        File.WriteAllText(Path.Combine(_root, "greet.openapi.json"), "{ not json");
        var document = new OpenApiDocument();

        // Test
        await Run(document);

        // Assert
        Assert.IsTrue(document.Paths == null || document.Paths.Count == 0);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Transform_PathAlreadyInDocument_KeepsTheExistingOne()
    {
        // Stage
        File.WriteAllText(Path.Combine(_root, "greet.exe"), string.Empty);
        File.WriteAllText(Path.Combine(_root, "greet.openapi.json"), Description);
        var existing = new OpenApiPathItem();
        var document = new OpenApiDocument { Paths = new OpenApiPaths { ["/cgi-bin/greet.exe"] = existing } };

        // Test
        await Run(document);

        // Assert
        Assert.AreSame(existing, document.Paths["/cgi-bin/greet.exe"]);
        Assert.IsTrue(document.Paths.ContainsKey("/cgi-bin/greet.exe/{id}"));
    }
}
