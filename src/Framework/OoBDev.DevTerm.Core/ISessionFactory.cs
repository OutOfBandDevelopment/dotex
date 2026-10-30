using OoBDev.System.IO;
using OoBDev.System.IO.Pipelines;
namespace OoBDev.DevTerm;

/// <summary>Creates a <see cref="Session"/> over a transport and a presenter pipeline.</summary>
public interface ISessionFactory
{
    /// <summary>Binds <paramref name="transport"/> to <paramref name="pipeline"/>.</summary>
    Session Create(IDeviceAdapter transport, Pipeline pipeline);
}
