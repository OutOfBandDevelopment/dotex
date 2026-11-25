using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Authorization;
using OoBDev.AspNetCore.Mvc;
using OoBDev.System.Security.Claims;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace OoBDev.AspNetCore.Tests.Authorization;

[TestClass]
public class RightAuditLogTests
{
    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Entries.Add((logLevel, formatter(state, exception)));
    }

    private static ClaimsPrincipal User(params Claim[] claims) => new(new ClaimsIdentity(claims, "test"));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Check_Allowed_LogsInformationWithSubjectAndRights()
    {
        var log = new ListLogger<AuditingApplicationRightChecker>();
        var checker = new AuditingApplicationRightChecker(new ApplicationRightChecker(), log);
        var user = User(new Claim(CommonClaims.NameIdentifier, "u-1"), new Claim(CommonClaims.ApplicationRight, "orders.read"));

        var result = checker.Check(user, ["orders.read"]);

        Assert.AreEqual(RightCheckResult.Allowed, result);
        Assert.HasCount(1, log.Entries);
        Assert.AreEqual(LogLevel.Information, log.Entries[0].Level);
        StringAssert.Contains(log.Entries[0].Message, "u-1");
        StringAssert.Contains(log.Entries[0].Message, "orders.read");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Check_Forbidden_LogsWarningAndKeepsTheResult()
    {
        var log = new ListLogger<AuditingApplicationRightChecker>();
        var checker = new AuditingApplicationRightChecker(new ApplicationRightChecker(), log);

        var result = checker.Check(User(new Claim(CommonClaims.NameIdentifier, "u-2")), ["orders.admin"]);

        Assert.AreEqual(RightCheckResult.Forbidden, result);
        Assert.AreEqual(LogLevel.Warning, log.Entries[0].Level);
        StringAssert.Contains(log.Entries[0].Message, "Forbidden");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Check_Unauthenticated_LogsAnonymous()
    {
        var log = new ListLogger<AuditingApplicationRightChecker>();
        var checker = new AuditingApplicationRightChecker(new ApplicationRightChecker(), log);

        var result = checker.Check(new ClaimsPrincipal(new ClaimsIdentity()), ["orders.read"]);

        Assert.AreEqual(RightCheckResult.Unauthenticated, result);
        StringAssert.Contains(log.Entries[0].Message, "anonymous");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AddRightAuditLog_WrapsTheRegisteredChecker()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IApplicationRightChecker, ApplicationRightChecker>();

        using var provider = services.AddRightAuditLog().BuildServiceProvider();

        Assert.IsInstanceOfType<AuditingApplicationRightChecker>(provider.GetRequiredService<IApplicationRightChecker>());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AddRightAuditLog_WithoutAChecker_Throws() =>
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddRightAuditLog());
}
