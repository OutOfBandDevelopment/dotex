# OoBDev.DerEE.De5000

DER EE DE-5000 LCR meter: 17-byte measurement packet read from a stream (no command side).

## Description

**Status: stub.** The project builds and holds the provider key only; nothing is decoded yet. Source: dev-term `Devices.De5000` (new in dotex). Design: [DeviceDecoding](../../../docs/design/DeviceDecoding/README.md), plan: [device-decoding-migration-plan.md](../../../docs/migration/device-decoding-migration-plan.md).

Implements `IDeviceDefinitionReceiver<T>` and `IDeviceDefinitionTransmitter<T>` from `OoBDev.System.Abstractions`; bytes arrive through the existing adapters, so the package never opens a port.

## Not yet verified on hardware

Everything: no behaviour is ported yet.