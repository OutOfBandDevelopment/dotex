namespace OoBDev.System.Sandbox;

/// <summary>The application rights a caller's context must carry for a brokered operation, on top of the plugin's own grant.</summary>
public static class BrokerRights
{
    /// <summary>Read a file in a granted folder.</summary>
    public const string FileRead = "files.read";

    /// <summary>Write a file in a read-write granted folder.</summary>
    public const string FileWrite = "files.write";

    /// <summary>Fetch from a granted host.</summary>
    public const string Fetch = "net.fetch";
}
