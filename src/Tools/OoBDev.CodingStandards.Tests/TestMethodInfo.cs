using System.Collections.Generic;

namespace OoBDev.CodingStandards.Tests;

/// <summary>A <c>[TestMethod]</c> and the categories declared directly on it.</summary>
/// <param name="Name">The method name.</param>
/// <param name="Categories">Categories on the method itself.</param>
internal sealed record TestMethodInfo(string Name, IReadOnlyList<string> Categories);
