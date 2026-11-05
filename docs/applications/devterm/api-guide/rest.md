# DevTerm web host: REST API

Generated from the application's OpenAPI document by `ApiGuideTests`; do not edit by hand.

**Document:** OoBDev.DevTerm.Web | v1 1.0.0

## Operations

### GET `/`

Responses:

- `200` OK

### GET `/api/connections`

The extra connections currently open

Responses:

- `200` OK

### POST `/api/connections`

Open a project connection as its own session; 404 unknown name, 403 read-only

Parameters:

- `name` (query, string, required)

Responses:

- `200` OK

### DELETE `/api/connections/{id}`

Close an opened connection; 204 whether or not it was open, 403 read-only

Parameters:

- `id` (path, string, required)

Responses:

- `200` OK

### GET `/api/devices`

Attached serial, HID and USBTMC devices

Responses:

- `200` OK

### GET `/api/discover`

Network devices found by the LXI, mDNS and SSDP probes (?seconds=1..10, default 3)

Parameters:

- `seconds` (query)

Responses:

- `200` OK

### GET `/api/monitor/captures/{n}`

A Stream Monitor capture's bytes (index into the oldest-first list); 404 unknown

Parameters:

- `n` (path, integer, required)

Responses:

- `200` OK

### GET `/api/panel`

Responses:

- `200` OK

### GET `/api/project`

The project file's connections (name and description only)

Responses:

- `200` OK

### PUT `/api/project/connections/{name}`

Create or replace a project connection from profile JSON; 400 with the reason when invalid or there is no --project

Parameters:

- `name` (path, string, required)

Responses:

- `200` OK

### DELETE `/api/project/connections/{name}`

Remove a project connection; 204 whether or not it existed, 403 read-only

Parameters:

- `name` (path, string, required)

Responses:

- `200` OK

### GET `/api/project/export`

The open connections (or the saved project) as a dev-term project file; 404 when there are none, 403 read-only

Parameters:

- `name` (query, string)

Responses:

- `200` OK

### POST `/api/project/import`

Add a project file's connections to the host's; 400 when none could be imported, 403 read-only

Responses:

- `200` OK

### POST `/api/session/connect`

Connect the host's own session (a no-op when open or when no connection is configured); 403 read-only

Responses:

- `200` OK

### POST `/api/session/disconnect`

Disconnect the host's own session; a send while closed is refused until Connect. 403 read-only

Responses:

- `200` OK

### POST `/api/session/profile`

Switch the host's own session to a saved profile (closes the old connection, connects the new one); 404 unknown, 403 read-only

Parameters:

- `name` (query, string, required)

Responses:

- `200` OK

### GET `/api/sessions/{id}`

A session's send formats, current format and log file; 404 unknown id

Parameters:

- `id` (path, string, required)

Responses:

- `200` OK

### GET `/api/sessions/{id}/log`

Download the session's current log file; 404 when not logging

Parameters:

- `id` (path, string, required)

Responses:

- `200` OK

### POST `/api/sessions/{id}/logging`

Start (enabled=true) or stop logging a session to ~/.dev-term/logs; 404 unknown id, 403 read-only

Parameters:

- `id` (path, string, required)
- `enabled` (query, boolean, required)

Responses:

- `200` OK

### GET `/api/sessions/{id}/panels`

The device panels available for a session's connection (the Device menu); 404 unknown id

Parameters:

- `id` (path, string, required)

Responses:

- `200` OK

### GET `/api/sessions/{id}/panels/{panel}`

A device panel's form and its latest indicator values; 404 unknown session or panel

Parameters:

- `id` (path, string, required)
- `panel` (path, string, required)

Responses:

- `200` OK

### POST `/api/sessions/{id}/panels/{panel}/invoke`

Run a control on a device panel; 403 read-only, 404 unknown session or panel

Parameters:

- `id` (path, string, required)
- `panel` (path, string, required)

Request body: `application/json`

Responses:

- `200` OK

### POST `/api/sessions/{id}/parser`

Change a session's send format; 400 unknown format, 404 unknown id, 403 read-only

Parameters:

- `id` (path, string, required)
- `name` (query, string, required)

Responses:

- `200` OK

### POST `/api/sessions/{id}/xonxoff`

Turn XON/XOFF software flow control on or off (TCP only); 400 for other transports, 404 unknown id, 403 read-only

Parameters:

- `id` (path, string, required)
- `enabled` (query, boolean, required)

Responses:

- `200` OK

### GET `/api/status`

State and description of the shared session

Responses:

- `200` OK

### GET `/asyncapi`

Responses:

- `200` OK

### GET `/asyncapi.json`

Responses:

- `200` OK

