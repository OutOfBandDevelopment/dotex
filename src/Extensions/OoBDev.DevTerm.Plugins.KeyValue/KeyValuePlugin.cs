using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Plugins.KeyValue;

/// <summary>An example plugin: one presenter, "keyvalue", that decodes <c>name=value [unit]</c> lines into a <see cref="StructuredMessage"/>.</summary>
public sealed class KeyValuePlugin : IPluginModule
{
    public void ConfigureServices(IServiceCollection services) => services.AddTransient<IPresenter, KeyValuePresenter>();
}
