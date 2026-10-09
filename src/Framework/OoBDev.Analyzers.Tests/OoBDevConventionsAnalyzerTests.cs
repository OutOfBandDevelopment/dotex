using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Analyzers.Tests;

[TestClass]
public class OoBDevConventionsAnalyzerTests
{
    private static async Task<string[]> RunAsync(string source)
    {
        var references = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
            .Split(Path.PathSeparator)
            .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p));

        var compilation = CSharpCompilation.Create(
            "Tests",
            [CSharpSyntaxTree.ParseText(source)],
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var diagnostics = await compilation
            .WithAnalyzers(ImmutableArray.Create<DiagnosticAnalyzer>(new OoBDevConventionsAnalyzer()))
            .GetAnalyzerDiagnosticsAsync();

        return diagnostics.Select(d => d.Id).OrderBy(i => i, StringComparer.Ordinal).ToArray();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("var d = System.DateTime.UtcNow;")]
    [DataRow("var d = System.DateTime.Now;")]
    [DataRow("var d = System.DateTimeOffset.UtcNow;")]
    public async Task Analyze_ClockProperty_ReportsOOB0001(string statement)
    {
        var ids = await RunAsync($"class C {{ void M() {{ {statement} }} }}");

        CollectionAssert.AreEqual(new[] { "OOB0001" }, ids);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Analyze_TimeProvider_ReportsNothing()
    {
        var ids = await RunAsync("class C { void M(System.TimeProvider t) { var d = t.GetUtcNow(); } }");

        Assert.AreEqual(0, ids.Length);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("\"Json\"", true)]
    [DataRow("\"my_key\"", true)]
    [DataRow("\"double--hyphen\"", true)]
    [DataRow("\"json\"", false)]
    [DataRow("\"hmac3-256\"", false)]
    public async Task Analyze_KeyedRegistration_ChecksKebabCase(string key, bool reported)
    {
        var source = $$"""
            using Microsoft.Extensions.DependencyInjection;
            using Microsoft.Extensions.DependencyInjection.Extensions;
            class S { }
            class C { void M(IServiceCollection services) { services.TryAddKeyedSingleton<S, S>({{key}}); } }
            """;

        var ids = await RunAsync(source);

        CollectionAssert.AreEqual(reported ? new[] { "OOB0002" } : Array.Empty<string>(), ids);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Analyze_UnrelatedKeyedMethod_ReportsNothing()
    {
        var ids = await RunAsync("class C { void AddKeyedThing(string k) { } void M() { AddKeyedThing(\"Not_Kebab\"); } }");

        Assert.AreEqual(0, ids.Length);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("using Serilog;")]
    [DataRow("using Autofac.Extensions;")]
    public async Task Analyze_BannedUsing_ReportsOOB0003(string directive)
    {
        var ids = await RunAsync(directive + "\nclass C { }");

        Assert.IsTrue(ids.Contains("OOB0003"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("a", true)]
    [DataRow("a-b-1", true)]
    [DataRow("", false)]
    [DataRow("-a", false)]
    [DataRow("a-", false)]
    [DataRow("a--b", false)]
    [DataRow("A", false)]
    [DataRow("a_b", false)]
    public void IsKebabCase_Value_MatchesRule(string value, bool expected)
    {
        Assert.AreEqual(expected, OoBDevConventionsAnalyzer.IsKebabCase(value));
    }
}
