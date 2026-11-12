using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Security;
using OoBDev.TestUtilities;

namespace OoBDev.System.Tests.Security;

[TestClass]
public class SandboxPathTests
{
    private static readonly string _sandbox = Path.Combine(Path.GetTempPath(), "sandbox-root");

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void EnsureSafePath_RelativeInside_ResolvesAgainstSandboxNotCurrentDirectory()
    {
        // Test
        var path = SandboxPath.EnsureSafePath(_sandbox, Path.Combine("sub", "file.txt"));

        // Assert
        Assert.AreEqual(Path.GetFullPath(Path.Combine(_sandbox, "sub", "file.txt")), Path.GetFullPath(path!));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void EnsureSafePath_AbsoluteInside_IsAccepted()
    {
        // Stage
        var inside = Path.Combine(_sandbox, "a", "b.txt");

        // Test
        var path = SandboxPath.EnsureSafePath(_sandbox, inside);

        // Assert
        Assert.AreEqual(Path.GetFullPath(inside), Path.GetFullPath(path!));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void EnsureSafePath_SandboxItself_IsAccepted()
    {
        // Test
        var path = SandboxPath.EnsureSafePath(_sandbox, ".");

        // Assert
        Assert.AreEqual(Path.GetFullPath(_sandbox).TrimEnd(Path.DirectorySeparatorChar), Path.GetFullPath(path!).TrimEnd(Path.DirectorySeparatorChar));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void EnsureSafePath_ParentTraversal_Throws()
    {
        // Test / Assert
        Assert.Throws<InvalidOperationException>(() => SandboxPath.EnsureSafePath(_sandbox, Path.Combine("..", "other", "x.txt")));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void EnsureSafePath_AbsoluteOutside_Throws()
    {
        // Test / Assert
        Assert.Throws<InvalidOperationException>(() => SandboxPath.EnsureSafePath(_sandbox, Path.Combine(Path.GetTempPath(), "elsewhere", "x.txt")));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void EnsureSafePath_SiblingWithSamePrefix_Throws()
    {
        // Test / Assert
        Assert.Throws<InvalidOperationException>(() => SandboxPath.EnsureSafePath(_sandbox, _sandbox + "-evil" + Path.DirectorySeparatorChar + "x.txt"));
    }
}
