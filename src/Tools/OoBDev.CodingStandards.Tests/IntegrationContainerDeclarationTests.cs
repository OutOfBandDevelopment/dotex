using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.CodingStandards.Tests;

/// <summary>
/// An Integration or DevLocal test that talks to a service of the Docker stack must declare its container
/// (<c>[IntegrationTestMethod("oobd-test-...")]</c> or <c>IntegrationContainers.EnsureAsync</c>), so the harness starts it
/// and a missing stack ends the test Inconclusive instead of failing with a connection error.
/// </summary>
[TestCategory("Unit")]
[TestClass]
public sealed class IntegrationContainerDeclarationTests
{
    private static readonly Regex _tierCategory = new(@"TestCategory\((TestCategories\.)?""?(Integration|DevLocal)", RegexOptions.Compiled);

    private static readonly Regex _stackProperty = new(
        @"(GetRequiredProperty|GetPropertyOrDefault)[^;]*""(TIKA|SQL|MONGODB|RABBITMQ|REDIS|POSTGRES|PGVECTOR|OPENSEARCH|QDRANT|DOCLING|CLAMAV|SMTP|MOTO|SQS|SERVICEBUS|KEYCLOAK|SBERT|OLLAMA|MOSQUITTO|MQTT|SER2NET|LOOPBACK|AZURITE|AZURE_STORAGE|OTEL|SONARQUBE|NEO4J)[A-Z0-9_]*""",
        RegexOptions.Compiled);

    private static readonly Regex _declaration = new(@"IntegrationTestMethod|IntegrationContainers\.EnsureAsync", RegexOptions.Compiled);

    /// <summary>Files that use a stack-like property name for something that is not a stack container. Never add to it without a reason.</summary>
    private static readonly string[] _exempt = [];

    [TestMethod]
    public void StackServiceTests_DeclareTheirContainers()
    {
        // Stage / Test
        var problems = Problems(SourceFiles(Path.Combine(RepositoryRoot.Find(), "src")));

        // Assert
        Assert.IsEmpty(problems, "Integration/DevLocal tests that use a stack service without declaring the container:\n" + string.Join('\n', problems));
    }

    [TestMethod]
    public void Detector_FlagsAnUndeclaredStackTest_AndAcceptsADeclaredOne()
    {
        // Stage
        var undeclared = "[TestCategory(TestCategories.Integration)] void T() { var h = TestContext.GetRequiredProperty<string>(\"REDIS_HOST\"); }";
        var declared = "[IntegrationTestMethod(\"oobd-test-redis\")] [TestCategory(TestCategories.Integration)] void T() { var h = TestContext.GetRequiredProperty<string>(\"REDIS_HOST\"); }";
        var unit = "[TestCategory(TestCategories.Unit)] void T() { var h = TestContext.GetPropertyOrDefault(\"REDIS_HOST\", \"x\"); }";

        // Test / Assert
        Assert.IsTrue(IsUndeclared(undeclared));
        Assert.IsFalse(IsUndeclared(declared));
        Assert.IsFalse(IsUndeclared(unit));
    }

    private static bool IsUndeclared(string text) =>
        _tierCategory.IsMatch(text) && _stackProperty.IsMatch(text) && !_declaration.IsMatch(text);

    private static List<string> Problems(IEnumerable<string> files) =>
        [.. files.Where(f => !_exempt.Contains(Path.GetFileName(f), StringComparer.Ordinal) && IsUndeclared(File.ReadAllText(f)))];

    private static IEnumerable<string> SourceFiles(string root) =>
        Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                     && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                     && !f.EndsWith("IntegrationContainerDeclarationTests.cs", StringComparison.Ordinal));
}
