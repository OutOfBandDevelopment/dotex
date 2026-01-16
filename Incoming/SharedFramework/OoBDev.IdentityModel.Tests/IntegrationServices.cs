using OoBDev.IdentityModel.Contracts;
using OoBDev.IdentityModel.Extensions;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using OoBDev.Toolkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Claims;

namespace OoBDev.IdentityModel.Tests
{
    public static class IntegrationServices
    {
        public static IServiceCollection GetServices(this TestContext testContext) =>  new ServiceCollection()
            .AddDebugTestConfigurations(
                ("ApplicationDb:ConnectionString", "Server=(localdb)\\ApplicationDb;Database=ApplicationDb;"),
                ("ApplicationCoreDb:ConnectionString", "Server=(localdb)\\ApplicationDb;Database=ApplicationDb;"),
                ("ApplicationAttendanceDb:ConnectionString", "Server=(localdb)\\ApplicationDb;Database=ApplicationDb;"),
                ("ApplicationSisDb:ConnectionString", "Server=(localdb)\\ApplicationDb;Database=ApplicationDb;"),
                ("ApplicationReadDb:ConnectionString", "Server=(localdb)\\ApplicationDb;Database=ApplicationDb;")
            )
            .AddTransient<IUserSession>(sp => null)
            .AddTransient<ClaimsPrincipal>(sp => new ClaimsPrincipal())
            .AddDebugTestServices(testContext)
            .AddIdentityModelExtensions()
            .AddToolkitServices()
            .AddTestLoggingServices(testContext)
            ;

        public static T GetService<T>(this TestContext testContext, IServiceCollection services = null) => (services ?? testContext.GetServices())
            .BuildServiceProvider()
            .GetService<T>()
            ;
    }
}