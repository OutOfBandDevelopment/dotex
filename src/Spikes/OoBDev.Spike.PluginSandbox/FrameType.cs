namespace OoBDev.Spike.PluginSandbox;

/// <summary>What a frame is. Values are wire constants and never change within a major version.</summary>
public enum FrameType : byte
{
    /// <summary>Asks for a <see cref="Response"/> or <see cref="Error"/>; its sequence is what the answer's ack names.</summary>
    Request = 1,

    /// <summary>Answers a request; the ack field holds the request's sequence.</summary>
    Response = 2,

    /// <summary>One-way notice, such as <c>lifecycle.ready</c>.</summary>
    Event = 3,

    /// <summary>Liveness probe; a reply is a heartbeat with the ack set.</summary>
    Heartbeat = 4,

    /// <summary>The host asks the plugin to finish and exit.</summary>
    Shutdown = 5,

    /// <summary>Failure answer to a request; the payload describes it.</summary>
    Error = 6,
}
