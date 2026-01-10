using System;

namespace OoBDev.Common;

public class CurrentUserAccessor : ICurrentUserAccessor
{
    private string? _currentUser;

    public string? UserName
    {
        get => _currentUser ?? $"{Environment.UserName}@{Environment.UserDomainName}";
        set => _currentUser = value;
    }
}
