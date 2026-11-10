namespace OoBDev.DevTerm.Wpf;

/// <summary>Asks which TCP port to listen on for incoming peers.</summary>
public interface IListenPeersDialog
{
    /// <summary>Asks for the port.</summary>
    /// <returns>The port (0 picks a free one), or null when the user cancels.</returns>
    int? AskPort();

    /// <summary>Asks for the UDP port to bind.</summary>
    /// <returns>The port (0 picks a free one), or null when the user cancels.</returns>
    int? AskUdpPort();
}
