# Feature request: USB parallel-port bridge as a small latched GPIO

- **Status:** declined (2026-11-20, owner: not wanted now; reopen by moving the file back to `request/`)
- **Requested by:** owner (bench session, 2026-11-20)
- **Date:** 2026-11-20
- **Area:** `src/HardwareDevices` (a new device package, for example `OoBDev.Prolific.Pl2305`) and a USB transport

## Need

A Prolific PL2305 USB-to-parallel bridge (`USB\VID_067B&PID_2305`, DB25) is on the bench. The owner would like to use it as a few digital outputs and inputs, with external TTL logic (a latch such as a 74HC574) holding the outputs and conditioning the inputs. There is no other GPIO device on the bench.

## Proposed behavior

A device package that writes bytes to the data pins D0 to D7, with the chip's automatic /STROBE pulse as the latch clock, and reads the printer status lines as inputs. Host side: a WinUSB or libusb transport that sends the bulk-out data and the printer-class control requests. It is a latched output port with a few polled inputs, not a bit-timed GPIO.

## Acceptance criteria

- Given a byte, when written, then D0 to D7 show it and /STROBE pulses once (checked with a meter or a latch).
- Given the status request, when read, then the BUSY, ACK, PAPER-OUT, SELECT and ERROR lines map to named input bits.
- Given no device, when opened, then the package reports it as not present and does not hang.
- Given the adapter on the Windows printer driver, when the package opens it, then it says plainly that WinUSB is needed and the Windows printer port is lost.

## Existing code checked

- No USB bulk or control transport in `src/Framework`; `OoBDev.System.IO.UsbTmc` and `OoBDev.System.IO.UsbHids` are the neighbours but speak other classes.
- `OoBDev.Velleman.K8055` is the existing USB GPIO device and the model for a device package.

## Bench findings (2026-11-20, read only, nothing changed on the machine)

- Windows reports it as "IEEE-1284 Controller" (revision `0202`), bound to the Microsoft USB printing driver, status healthy.
- Compatible ids include `Class_07 & SubClass_01 & Prot_01`: the USB printer class, protocol 1 (unidirectional, from the USB printer class specification as the author remembers it).
- Not read: endpoint list and full descriptors (no USB descriptor tool installed), and whether the port-status request works on this unit.
- Not verified: the output voltage swing (3.3 V or 5 V); measure before connecting logic.

## Security and operations

Binding WinUSB takes the device away from the Windows printer stack. Printer ports have no output protection, so nothing should drive against the data pins. Cost: one USB device, no network.

## Priority

- **Impact:** a handful of latched outputs and polled inputs for bench experiments.
- **Needed by:** no date
- **Workaround today:** the Velleman K8055 for general digital I/O.

## Open questions

- Does the unit answer the printer-class port-status request, and is any of /AUTOFD, /INIT, /SELECT-IN individually drivable?
- Is the data bus really output only? The unidirectional protocol suggests so.
- WinUSB (Zadig) or a printer-class API for the control requests?

## Triage notes

2026-11-20: declined by the owner for now, filed straight to `rejected/` so the bench findings are kept. Reopen when the owner wants the hardware project; start with a read-only descriptor dump (USBView or USB Device Tree Viewer).
