# USBTMC hardware tests (2026-10-29)

Added `RealHardwareUsbTmcTests` to `OoBDev.System.IO.UsbTmc.Tests` (2 `Hardware` tests, ported in spirit from dev-term's real-hardware transport tests): four back-to-back `*IDN?` queries, and close then reopen followed by a query. They use the first USBTMC device that discovery finds and end `Assert.Inconclusive` when none is attached. No device was attached on this run, so both reported Skipped; they have not passed against hardware.
