namespace OoBDev.Communications.Abstractions;

/// <summary>A named route (protocol plus provider plus settings) that messages are sent through.</summary>
public interface IChannel
{
    /// <summary>Gets the unique channel name, for example <c>support-email</c>.</summary>
    string Name { get; }

    /// <summary>Gets the protocol, for example <c>email</c> or <c>sms</c>.</summary>
    string Protocol { get; }

    /// <summary>Gets the provider key, for example <c>sendgrid</c>.</summary>
    string Provider { get; }
}
