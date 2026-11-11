# OoBDev.EventSourcing.Hosting

`services.TryAddEventRelayHost()` runs the `EventRelay` (event store to message queue, see `OoBDev.EventSourcing`) as a hosted service. It loops while events are waiting and pauses `OoBDev:EventSourcing:Relay:PollInterval` (default 1 second) when there are none; a failed pass is logged and retried. Run one host per relay name.
