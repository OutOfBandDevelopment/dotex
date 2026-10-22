using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.AspNetCore.Mvc.OpenApi;
using OoBDev.System.ComponentModel;
using OoBDev.TestUtilities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Tests.OpenApi;

[TestClass]
public class ApiInfoDocumentTransformerTests
{
    private static async Task<OpenApiInfo?> TransformAsync(string documentName, params IVersionProvider[] versions)
    {
        var descriptors = new Mock<IActionDescriptorCollectionProvider>();
        descriptors.Setup(p => p.ActionDescriptors).Returns(new ActionDescriptorCollection(new List<ActionDescriptor>(), 1));
        var document = new OpenApiDocument();
        var context = new OpenApiDocumentTransformerContext
        {
            DocumentName = documentName,
            DescriptionGroups = [],
            ApplicationServices = null!,
        };

        await new ApiInfoDocumentTransformer(descriptors.Object, versions).TransformAsync(document, context, CancellationToken.None);

        return document.Info;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Transform_AllDocumentWithVersionProvider_UsesItsTitleVersionAndDescriptionAsync()
    {
        // Stage
        var version = new Mock<IVersionProvider>();
        version.SetupGet(v => v.Title).Returns("Orders API");
        version.SetupGet(v => v.Version).Returns("2.3.4");
        version.SetupGet(v => v.Description).Returns("Places orders");

        // Test
        var info = await TransformAsync("all", version.Object);

        // Assert
        Assert.AreEqual("Orders API", info?.Title);
        Assert.AreEqual("2.3.4", info?.Version);
        Assert.AreEqual("Places orders", info?.Description);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Transform_AssemblyDocumentWithoutController_FallsBackToDocumentNameAsync()
    {
        // Test
        var info = await TransformAsync("Some.Assembly");

        // Assert
        Assert.AreEqual("Some.Assembly", info?.Title);
        Assert.AreEqual("v0.0.0.0", info?.Version);
    }
}
