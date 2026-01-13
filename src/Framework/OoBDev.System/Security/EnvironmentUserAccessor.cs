using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.System.Security;
public class EnvironmentUserAccessor : ICurrentUserAccessor
{
    public string? UserName => $"{Environment.UserName}@{Environment.UserDomainName}";
}
