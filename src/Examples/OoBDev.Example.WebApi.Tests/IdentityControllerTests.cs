using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.AspNetCore.Authorization;
using OoBDev.Keycloak.Identity;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Controllers;
using OoBDev.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class IdentityControllerTests
{
    private sealed class Directory : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var url = request.RequestUri!.PathAndQuery;
            var body = url.EndsWith("/token", StringComparison.Ordinal) ? """{"access_token":"t","expires_in":60}"""
                : url.Contains("/role-mappings/", StringComparison.Ordinal) ? """[{"name":"editor"}]"""
                : url.Contains("/users?", StringComparison.Ordinal) ? """[{"id":"2","username":"zed","enabled":false},{"id":"1","username":"Ann","firstName":"Ann","lastName":"Lee","email":"a@x.test","enabled":true}]"""
                : """[{"name":"editor"},{"name":"viewer"}]""";
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") });
        }
    }

    private static IdentityController Controller()
    {
        var options = Options.Create(new KeycloakIdentityOptions { BaseUrl = "http://kc.test", Realm = "demo" });
        var reader = new KeycloakDirectoryReader(new HttpClient(new Directory()), options, TimeProvider.System);
        var mapper = new Mock<IRoleRightMapper>();
        mapper.Setup(m => m.MapRights(It.IsAny<IEnumerable<string>>()))
            .Returns((IEnumerable<string> roles) => roles.Contains("editor") ? new HashSet<string> { "docs.write", "docs.read" } : new HashSet<string>());
        return new IdentityController(reader, mapper.Object);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Users_Listed_OrderedWithRolesAndRights()
    {
        // Stage
        var controller = Controller();

        // Test
        var result = await controller.Users();

        // Assert
        var users = (IReadOnlyList<IdentityUserModel>)((OkObjectResult)result.Result!).Value!;
        CollectionAssert.AreEqual(new[] { "Ann", "zed" }, users.Select(u => u.UserName).ToArray());
        Assert.AreEqual("Ann Lee", users[0].DisplayName);
        CollectionAssert.AreEqual(new[] { "docs.read", "docs.write" }, users[0].Rights);
        CollectionAssert.AreEqual(new[] { "editor" }, users[1].Roles);
        Assert.IsFalse(users[1].Enabled);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Roles_Listed_ShowTheRightsEachGrants()
    {
        // Stage
        var controller = Controller();

        // Test
        var result = await controller.Roles();

        // Assert
        var roles = (IReadOnlyList<IdentityRoleModel>)((OkObjectResult)result.Result!).Value!;
        Assert.HasCount(2, roles);
        CollectionAssert.AreEqual(new[] { "docs.read", "docs.write" }, roles.Single(r => r.Name == "editor").Rights);
        Assert.IsEmpty(roles.Single(r => r.Name == "viewer").Rights);
    }
}
