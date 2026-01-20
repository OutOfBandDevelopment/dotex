using System;

namespace OoBDev.System.Security;
public class EnvironmentUserAccessor : ICurrentUserAccessor
{
    public string? UserName => $"{Environment.UserName}@{Environment.UserDomainName}";
}
