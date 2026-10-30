# Z-Wave Hardware tests: open the adapter (2026-10-30)

The three `OoBDev.ZWave.Tests` Hardware tests timed out against the real Z-Stick on COM13: `StreamDevice` expects an opened adapter and the test helper `ZWaveStick` never opened `SerialPortDeviceAdapter`, and its 100 ms port read timeout would also have ended the read loop. The helper now opens the adapter and leaves the read timeout infinite (disposing closes the port and ends the read).

Verification: `dotnet test Extensions/OoBDev.ZWave.Tests --settings .runsettings --filter TestCategory=Hardware` passed 3 of 3 against the plugged-in stick. Also run with the new bench settings: Radex One (3 passed); Zoom H4n passed 1 of 1 once `ZOOM_H4N_SERIAL_PORT` was set to COM3.
