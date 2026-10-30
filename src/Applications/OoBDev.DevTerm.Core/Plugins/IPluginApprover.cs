using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace OoBDev.DevTerm;

/// <summary>Asks the user whether an out-of-process plugin may run. A front end supplies one; with none, only already-remembered plugins run.</summary>
public interface IPluginApprover
{
    PluginApprovalChoice Ask(PluginApprovalRequest request);
}
