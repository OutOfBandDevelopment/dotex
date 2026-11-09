namespace OoBDev.DevTerm.Configuration;

/// <summary>How the Stream Monitor's capture list is ordered.</summary>
public enum StreamCaptureSort
{
    /// <summary>Oldest first - the monitor's own order.</summary>
    Oldest,
    Newest,
    Largest,
    Kind,
    Device,
}
