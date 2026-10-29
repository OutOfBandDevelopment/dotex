using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Transports.Tcp;

public sealed class TcpTransportOptionsValidator : IValidateOptions<TcpTransportOptions>
{
    public ValidateOptionsResult Validate(string? name, TcpTransportOptions options)
    {
        if (options.Port is < 1 or > 65535)
        {
            return ValidateOptionsResult.Fail("TCP 'Port' must be between 1 and 65535.");
        }

        if (options.Mode == TcpTransportMode.Client && string.IsNullOrWhiteSpace(options.Host))
        {
            return ValidateOptionsResult.Fail("TCP client mode requires 'Host' to be set.");
        }

        return ValidateOptionsResult.Success;
    }
}
