using System.Collections.Generic;

namespace OoBDev.CodingStandards.Tests;

/// <summary>One <c>[TestClass]</c> found in a source file: its class-level categories and the categories added on each test method.</summary>
/// <param name="File">Repository-relative path of the source file.</param>
/// <param name="Name">The class name.</param>
/// <param name="ClassCategories">Categories on the class itself.</param>
/// <param name="Methods">Each test method with the categories on it.</param>
internal sealed record TestClassInfo(string File, string Name, IReadOnlyList<string> ClassCategories, IReadOnlyList<TestMethodInfo> Methods);
