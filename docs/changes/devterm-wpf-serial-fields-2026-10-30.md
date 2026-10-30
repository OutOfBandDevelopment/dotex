# DevTerm WPF serial fields in the connection editor (2026-10-30)

`ConnectionEditorWindow` shows baud, data bits, parity, stop bits and flow control (handshake) for serial and RFC 2217 transports, bound to the existing `ConnectionEditorViewModel` properties (`IsSerialLikeTransport` drives the visibility).

- Verified: the project builds; the bindings are XAML only, so no new unit tests. The window was not viewed running.
- Left: per-transport fields for the other transports, device pickers and the profile-button refresh.
