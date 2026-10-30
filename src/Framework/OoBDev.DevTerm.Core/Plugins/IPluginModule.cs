using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.DevTerm;

/// <summary>
/// The entry point of a plugin assembly: registers whatever it contributes (<c>IPresenter</c>s, an <c>IDeviceAdapter</c>
/// and its options) the same way the built-in <c>AddXyz</c> extensions do.
/// </summary>
public interface IPluginModule
{
    void ConfigureServices(IServiceCollection services);
}
