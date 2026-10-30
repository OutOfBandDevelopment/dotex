# WPF software flow control toggle (2026-10-30)

## What changed

- `SessionViewModel.SoftwareFlowControl` and `HasSoftwareFlowControl` read and set `TcpEndpointDeviceAdapter.SoftwareFlowControl` on the live connection; other transports report false.
- The WPF View menu has a checkable "Software flow control (XON/XOFF)" item bound to the selected session, enabled only for TCP sessions (same behaviour as the Tui and Web hosts).
- Two Unit tests in `SessionViewModelTests` (loopback stays off and unavailable; TCP toggles the adapter and raises a change); the class passes (10 tests).

## Not done

- The menu item was not viewed in a running window.
