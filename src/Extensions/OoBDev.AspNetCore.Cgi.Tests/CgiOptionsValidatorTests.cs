using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace OoBDev.AspNetCore.Cgi.Tests;

[TestClass]
public class CgiOptionsValidatorTests
{
    private sealed class FakeEnvironment(string? webRoot) : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = webRoot!;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ApplicationName { get; set; } = "test";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public string EnvironmentName { get; set; } = "Test";
    }

    private static string Fixtures => Path.Combine(AppContext.BaseDirectory, "Fixtures");

    [TestMethod]
    [TestCategory("Unit")]
    public void Validate_RootUnderStaticRoot_Fails()
    {
        // Stage
        var validator = new CgiOptionsValidator(new FakeEnvironment(AppContext.BaseDirectory));

        // Test
        var result = validator.Validate(null, new CgiOptions { ScriptRoot = Fixtures });

        // Assert
        Assert.IsTrue(result.Failed);
        StringAssert.Contains(result.FailureMessage, "static file root");
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Validate_RootEqualToStaticRoot_Fails()
    {
        // Stage
        var validator = new CgiOptionsValidator(new FakeEnvironment(Fixtures));

        // Test
        var result = validator.Validate(null, new CgiOptions { ScriptRoot = Fixtures });

        // Assert
        Assert.IsTrue(result.Failed);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Validate_RootBesideStaticRoot_Succeeds()
    {
        // Stage
        var validator = new CgiOptionsValidator(new FakeEnvironment(Path.Combine(AppContext.BaseDirectory, "Fixtures-web")));

        // Test
        var result = validator.Validate(null, new CgiOptions { ScriptRoot = Fixtures });

        // Assert
        Assert.IsTrue(result.Succeeded);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Validate_NoStaticRoot_Succeeds()
    {
        // Stage
        var validator = new CgiOptionsValidator(new FakeEnvironment(null));

        // Test
        var result = validator.Validate(null, new CgiOptions { ScriptRoot = Fixtures });

        // Assert
        Assert.IsTrue(result.Succeeded);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Validate_MissingRoot_Fails()
    {
        // Stage
        var validator = new CgiOptionsValidator(new FakeEnvironment(null));

        // Test
        var result = validator.Validate(null, new CgiOptions { ScriptRoot = Path.Combine(Fixtures, "missing") });

        // Assert
        Assert.IsTrue(result.Failed);
        StringAssert.Contains(result.FailureMessage, "does not exist");
    }
}
