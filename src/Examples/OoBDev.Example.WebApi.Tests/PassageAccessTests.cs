using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Authorization;
using OoBDev.System.Security.Claims;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Models;
using OoBDev.WebApi.Services;
using System.Linq;
using System.Security.Claims;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class PassageAccessTests
{
    private static readonly RagPassage[] Passages =
    [
        new() { Id = "open", Text = "public text" },
        new() { Id = "hr", Text = "salary bands", RequiredRight = "hr.read" },
        new() { Id = "ops", Text = "runbook", RequiredRight = "ops.read" },
    ];

    private static ClaimsPrincipal Signed(params string[] rights) =>
        new(new ClaimsIdentity(rights.Select(r => new Claim(CommonClaims.ApplicationRight, r)), "test"));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void VisibleTo_Anonymous_SeesOnlyUnrestrictedPassages() =>
        CollectionAssert.AreEqual(new[] { "open" }, PassageAccess.VisibleTo(Passages, new ClaimsPrincipal(new ClaimsIdentity()), new ApplicationRightChecker()).Select(p => p.Id).ToArray());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void VisibleTo_HoldsOneRight_SeesThatPassageInOrder() =>
        CollectionAssert.AreEqual(new[] { "open", "hr" }, PassageAccess.VisibleTo(Passages, Signed("hr.read"), new ApplicationRightChecker()).Select(p => p.Id).ToArray());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void VisibleTo_HoldsAllRights_SeesEverything() =>
        Assert.HasCount(3, PassageAccess.VisibleTo(Passages, Signed("hr.read", "ops.read"), new ApplicationRightChecker()));
}
