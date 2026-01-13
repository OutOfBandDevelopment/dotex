using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.System.Security;
public interface ICurrentUserAccessor
{
    string? UserName { get; }
}
