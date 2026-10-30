using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.DevTerm.Plugins.Sample;

/// <summary>The smallest possible plugin: one presenter, "sample", that reports how many bytes each read held.</summary>
public sealed class SamplePlugin : IPluginModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services) => services.AddTransient<IPresenter, ByteCountPresenter>();
}
