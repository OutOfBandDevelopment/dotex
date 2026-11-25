using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Guides.Tests;

[TestCategory(TestCategories.Unit)]
[TestClass]
public sealed class ApiGuideRendererTests
{
    private const string OpenApiDocument = """
        {
          "info": { "title": "Demo", "version": "1.0" },
          "paths": {
            "/b": { "post": { "summary": "Make b", "requestBody": { "content": { "application/json": {} } }, "responses": { "201": { "description": "Created" } } } },
            "/a": { "get": { "summary": "Read a", "parameters": [ { "name": "id", "in": "query", "required": true, "schema": { "type": "string" } } ], "responses": { "200": { "description": "Ok", "content": { "text/plain": {} } }, "404": { "description": "Missing" } } } }
          }
        }
        """;

    private const string AsyncApiDocument = """
        {
          "info": { "title": "Events", "version": "2.0", "description": "Device events." },
          "channels": { "readings": { "address": "devices/{id}/readings", "description": "Readings", "parameters": { "id": { "description": "Device id" } } } }
        }
        """;

    [TestMethod]
    public void OpenApi_TwoPaths_ListsOperationsSortedByPath()
    {
        // Stage

        // Test
        var guide = ApiGuideRenderer.OpenApi(OpenApiDocument, "Demo REST");

        // Assert
        StringAssert.StartsWith(guide, "# Demo REST\n");
        Assert.IsLessThan(guide.IndexOf("### POST `/b`", StringComparison.Ordinal), guide.IndexOf("### GET `/a`", StringComparison.Ordinal));
        StringAssert.Contains(guide, "- `id` (query, string, required)");
        StringAssert.Contains(guide, "Request body: `application/json`");
        StringAssert.Contains(guide, "- `200` Ok (`text/plain`)");
    }

    [TestMethod]
    public void OpenApi_PermissionsExtension_ShowsAccessAndRights()
    {
        // Stage
        const string document = """
            { "paths": {
              "/open": { "get": { "x-permissions": { "anonymous": true }, "responses": {} } },
              "/locked": { "delete": { "x-permissions": { "anonymous": false, "right": [ "Write", "Admin" ] }, "responses": {} } },
              "/plain": { "get": { "responses": {} } } } }
            """;

        // Test
        var guide = ApiGuideRenderer.OpenApi(document, "Demo");

        // Assert
        StringAssert.Contains(guide, "### GET `/open`\n\nAccess: anonymous\n\n");
        StringAssert.Contains(guide, "Access: authenticated; rights: `Admin`, `Write`\n\n");
        Assert.DoesNotContain("Access:", guide[guide.IndexOf("`/plain`", StringComparison.Ordinal)..]);
    }

    [TestMethod]
    public void OpenApi_SameDocument_SameBytes()
    {
        // Stage

        // Test
        var first = ApiGuideRenderer.OpenApi(OpenApiDocument, "Demo REST");
        var second = ApiGuideRenderer.OpenApi(OpenApiDocument, "Demo REST");

        // Assert
        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public void AsyncApi_Channel_ShowsAddressAndParameters()
    {
        // Stage

        // Test
        var guide = ApiGuideRenderer.AsyncApi(AsyncApiDocument, "Demo streaming");

        // Assert
        StringAssert.Contains(guide, "### readings `devices/{id}/readings`");
        StringAssert.Contains(guide, "- Parameter `id`: Device id");
        StringAssert.Contains(guide, "Device events.");
    }

    [TestMethod]
    public void WriteIfChanged_RewrittenWithSameContent_LeavesTheFileAlone()
    {
        // Stage
        using var folder = new TempDirectoryHandle();
        var path = Path.Combine(folder.DirectoryPath, "sub", "guide.md");

        // Test
        var first = ApiGuideRenderer.WriteIfChanged(path, "a\r\nb");
        var second = ApiGuideRenderer.WriteIfChanged(path, "a\nb");
        var third = ApiGuideRenderer.WriteIfChanged(path, "a\nc");

        // Assert
        Assert.IsTrue(first);
        Assert.IsFalse(second);
        Assert.IsTrue(third);
        Assert.AreEqual("a\nc", File.ReadAllText(path));
    }

}
