namespace OoBDev.DevTerm.Wpf;

/// <summary>Creates the session view models the shell opens.</summary>
public interface ISessionViewModelFactory
{
    /// <summary>Creates a session over the loopback adapter with the ASCII presenter.</summary>
    SessionViewModel CreateLoopback();
}
