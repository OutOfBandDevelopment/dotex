# OoBDev.DevTerm.Devices.Demo

A sample device plugin: a control panel for the loopback transport with an LED toggle, a level slider and an Apply button that sends `SET LED=<0|1> LEVEL=<n>`. It exists so every front end's panel flow can be tested without hardware; the Web, WPF and Tui user-flow tests load it from `plugins/demo`.
