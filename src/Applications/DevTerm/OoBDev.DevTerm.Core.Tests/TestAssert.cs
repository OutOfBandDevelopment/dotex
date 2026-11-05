using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DevTerm.Tests;

internal static class TestAssert
{
    public static void AreSequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual, string? message = null) =>
        Assert.IsTrue(expected.SequenceEqual(actual), message ?? $"Expected [{string.Join(", ", expected)}] but was [{string.Join(", ", actual)}].");
}
