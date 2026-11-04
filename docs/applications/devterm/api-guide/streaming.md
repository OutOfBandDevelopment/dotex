# DevTerm web host: streaming channels

Generated from the application's AsyncAPI document by `ApiGuideTests`; do not edit by hand.

**Document:** dev-term web host streams 1.0.0

Every channel needs the host token (Bearer header, devterm_auth cookie or ?token=). A read-only token may receive but not send.

## Channels

### events `/api/events`

Server-Sent Events (text/event-stream). Each subscriber has a 256-event queue that drops its oldest.

Messages: `connectionOpened`, `connectionClosed`, `lineEvent`

### mainSession `/ws`

Text WebSocket for the shared session. A frame received = one typed line to send; a frame sent = one output or status line (an '!' prefix marks an error).

Messages: `line`

### openedSession `/ws/{id}`

The same protocol for one connection opened from the project file.

- Parameter `id`: The id returned by POST /api/connections

Messages: `line`

## Operations

- `receiveEvents`: receive on `events`
- `receiveLine`: receive on `mainSession`
- `sendLine`: send on `mainSession`

## Messages

### connectionClosed

- Name: `connection-closed`
- Payload: object with `id` (string)

### connectionOpened

- Name: `connection-opened`
- Payload: object with `id` (string), `name` (string)

### line

- Name: `line`
- Content type: `text/plain`
- Payload: string

### lineEvent

- Name: `line`
- Payload: object with `id` (string), `text` (string)

