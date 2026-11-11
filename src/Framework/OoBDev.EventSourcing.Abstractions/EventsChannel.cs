namespace OoBDev.EventSourcing;

/// <summary>
/// Channel marker for the event stream: the relay sends through <c>IMessageQueueSender&lt;EventsChannel&gt;</c> and
/// projections receive through <c>IMessageQueueHandler&lt;EventsChannel, EventRecord&gt;</c>.
/// </summary>
public sealed class EventsChannel
{
}
