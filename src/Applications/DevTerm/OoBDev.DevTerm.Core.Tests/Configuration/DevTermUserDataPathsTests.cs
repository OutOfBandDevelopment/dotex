using System;
using System.Collections.Generic;
using System.IO;
using OoBDev.System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm;

using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Configuration.Discovery;
using OoBDev.TestUtilities;
namespace OoBDev.DevTerm.Configuration.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class DevTermUserDataPathsTests
{
    [TestCategory(TestCategories.BugRegression)]
    [TestMethod]
    [TestCategory("Unit")]
    public void ResolveManifestDirectory_WithARootedNamePointingAtARealDirectory_ReturnsNullInsteadOfThatDirectory()
    {
        // Regression test for bug 012: ResolveManifestDirectory passed an imported profile's
        // ManifestName straight to Path.Combine. Path.Combine ignores its first argument entirely when
        // the second is rooted, so a rooted ManifestName pointing at any existing directory used to
        // resolve to that directory directly - escaping both the user's and the app's manifest
        // folders. Pointing at Path.GetTempPath() (guaranteed to exist) proves the escape actually
        // worked, rather than merely asserting on a path that happens not to exist either way. See
        // docs/bugs/resolved/012-profile-names-not-validated.md.
        using var folder = new TempDirectoryHandle();
        var outsideDirectory = folder.DirectoryPath;

        Assert.IsNull(DevTermUserDataPaths.ResolveManifestDirectory(outsideDirectory));
    }
}
