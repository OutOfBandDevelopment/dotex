using System;

namespace OoBDev.AsyncApi;

/// <summary>
/// A channel and message type that the application sends. Senders are open generics, so the document cannot find them; register each with <see cref="ServiceCollectionExtensions.AddAsyncApiSendMessage{TChannel, TMessage}"/>.
/// </summary>
/// <param name="Channel">The channel type given to <c>IMessageQueueSender&lt;TChannel&gt;</c>.</param>
/// <param name="Message">The message type that is sent.</param>
public sealed record AsyncApiSendMessage(Type Channel, Type Message);
