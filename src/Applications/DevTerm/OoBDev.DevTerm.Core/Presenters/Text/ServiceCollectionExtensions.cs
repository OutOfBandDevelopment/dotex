using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.DevTerm.Presenters.Text;

/// <summary>Registration for the built-in text and numeric-base presenters.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the ASCII, UTF-8, hex, decimal, octal and binary presenters as transient <see cref="IPresenter"/>s
    /// (each session gets fresh state). Configure <see cref="AsciiPresenterOptions"/> to change the maximum line length.
    /// </summary>
    public static IServiceCollection TryAddTextPresenters(this IServiceCollection services)
    {
        services.AddOptions<AsciiPresenterOptions>().Validate(o => o.MaxLineLength >= 0, "MaxLineLength must be 0 (unbounded) or positive.").ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IPresenter, AsciiPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IPresenter, Utf8Presenter>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IPresenter, HexPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IPresenter, DecimalPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IPresenter, OctalPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IPresenter, BinaryPresenter>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IPresenter, KeyValuePresenter>());
        return services;
    }
}
