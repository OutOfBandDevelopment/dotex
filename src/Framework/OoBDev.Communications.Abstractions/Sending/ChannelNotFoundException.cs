using System;

namespace OoBDev.Communications.Abstractions;

/// <summary>The named channel does not exist (a configuration error).</summary>
public class ChannelNotFoundException : InvalidOperationException
{
    /// <summary>Creates the exception.</summary>
    /// <param name="channelName">The missing channel.</param>
    public ChannelNotFoundException(string channelName) : base($"Channel '{channelName}' was not found.") => ChannelName = channelName;

    /// <summary>Gets the missing channel name.</summary>
    public string ChannelName { get; }
}
